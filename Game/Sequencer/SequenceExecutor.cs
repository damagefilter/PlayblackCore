using Playblack.BehaviourTree;
using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Composite;
using Playblack.Csp;
using Playblack.Extensions;
using Playblack.Savegame;
using Playblack.Savegame.Model;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Playblack.Sequencer {

    /// <summary>
    /// Executes a behaviour tree of some description.
    /// </summary>
    [OutputAware("OnExecutionFinish", "OnExecutionTrigger", "OnExecutionTerminated")]
    [SaveableComponent]
    public class SequenceExecutor : MonoBehaviour, ISerializationCallbackReceiver {

        [SerializeField]
        [SaveableField(SaveField.FIELD_PROTOBUF_OBJECT)]
        private List<ValueField> globalDataContext;

        // in deployment this is only used on initial scene load
        // So do not sore that in the save game
        [SerializeField]
        [HideInInspector]
        private byte[] serializedModelTree;

        // This is restored when loading a savegame and is ideally the
        // original model tree.
        [SaveableField(SaveField.FIELD_PROTOBUF_OBJECT)]
        private UnityBtModel rootModel;

        public UnityBtModel RootModel {
            get {
                return rootModel;
            }
        }

        [SerializeField]
        private ExecutionType executionType;

        public ExecutionType TypeOfExecution {
            get {
                return executionType;
            }
            set {
                executionType = value;
            }
        }

        // To make the enum known to the savegame
        [SaveableField(SaveField.FIELD_PRIMITIVE)]
        private int SaveExecutionType {
            get {
                return (int)executionType;
            }
            set {
                executionType = (ExecutionType)value;
            }
        }

        // internally used to store the global context of the BT in a save game.
        // The executor cannot be persisted (doesn't need to be either) but
        // when we just persist its root context, we can pass it back in and have the state as it was after saving
        [SaveableField(SaveField.FIELD_PROTOBUF_OBJECT)]
        private DataContext GlobalContext {
            get {
                return executor != null ? executor.GetRootContext() : null;
            }
            set {
                // This is after loading a save game so just create the executor
                GetExecutor(value);
            }
        }

        // Means this cannot be re-created via savegames without a valid prefab
        [SerializeField]
        [Tooltip("The object on which this sequencer is acting (for instance the object that handles AI movement or such thing.")]
        private UnityEngine.Object actor;

        private IBTExecutor executor;

        private bool running;

        public SequenceExecutor() {
            this.rootModel = UnityBtModel.NewInstance(null, new UnityBtModel(), typeof(ModelSequence).ToString());
        }

        public IBTExecutor GetExecutor(DataContext overrideContext = null) {
            // recycle this for performance reasons but also to
            // persist the global context
            if (this.executor != null) {
                if (executor.GetStatus() != TaskStatus.UNINITIALISED) {
                    executor.Terminate();
                    executor.Reset();
                }
                if (overrideContext != null) {
                    this.executor.SetRootContext(overrideContext);
                }
                return this.executor;
            }
            else {
                return this.GetExecutor(overrideContext != null ? overrideContext : new DataContext(globalDataContext), actor);
            }
        }

        public IBTExecutor GetExecutor(DataContext context, UnityEngine.Object actor) {
            context["actor"] = actor;

            var root = rootModel.Model;
            RecursiveLoadModelTree(rootModel, root);
            // TODO: Fetch an implementation from a factory
            return new CachingBtExecutor(root, context);
        }

        public void Start() {
            this.executor = this.GetExecutor();
            if (this.TypeOfExecution == ExecutionType.AUTO) {
                running = true;
                StartCoroutine("TickExecutorParallel");
            }
        }

        /// <summary>
        /// Instead of update we use this.
        /// It's started as coroutine making the BT more or less parallel
        /// and also saves the overhead of calling Update from engine code.
        /// </summary>
        /// <returns></returns>
        private IEnumerator TickExecutorParallel() {
            while (running) {
                var status = this.executor.GetStatus();
                if (status == TaskStatus.SUCCESS || status == TaskStatus.FAILURE) {
                    Debug.Log("Terminating Sequence Executor. Status is " + status);
                    this.executor.Terminate();
                    this.FireOutput("OnExecutionFinish");
                    running = false;
                }
                else {
                    this.executor.Tick();
                    yield return 0;
                }
            }
        }

        [InputFunc("TriggerExecution")]
        public void TriggerExecution() {
            if (this.TypeOfExecution == ExecutionType.TRIGGER) {
                if (this.executor == null) {
                    this.executor = this.GetExecutor();
                }
                if (!running) {
                    running = true;
                    StartCoroutine("TickExecutorParallel");
                    this.FireOutput("OnExecutionTrigger");
                }
            }
        }

        [InputFunc("ResetSequencer")]
        public void ResetSequencer() {
            if (this.executor == null) {
                return; // nothing to reset
            }
            this.executor.Reset();
        }

        [InputFunc("TerminateBT", DisplayName = "Terminate")]
        public void TerminateBT() {
            if (this.executor == null) {
                return; // nothing to reset
            }
            this.executor.Terminate();
            StopCoroutine("TickExecutorParallel");
            running = false;
            this.FireOutput("OnExecutionTerminated");
        }

        private void RecursiveLoadModelTree(UnityBtModel current, ModelTask root) {
            var childQueue = new Dictionary<ModelTask, UnityBtModel>();
            if (current.children != null && current.children.Count > 0) {
                foreach (var btModelChild in current.children) {
                    if (btModelChild == null || btModelChild.ModelClassName == null) {
                        continue;
                    }
                    var modelTask = btModelChild.Model;
                    root.Children.Add(modelTask);

                    if (btModelChild.children.Count > 0) {
                        childQueue.Add(modelTask, btModelChild);
                    }
                }
                foreach (var kvp in childQueue) {
                    RecursiveLoadModelTree(kvp.Value, kvp.Key);
                }
            }
        }

        public void OnAfterDeserialize() {
            using (var ms = new MemoryStream(serializedModelTree)) {
                ms.Position = 0;
                this.rootModel = DataSerializer.DeserializeProtoObject<UnityBtModel>(ms.ToArray());
            }
        }

        public void OnBeforeSerialize() {
            this.serializedModelTree = DataSerializer.SerializeProtoObject(this.rootModel);
        }
    }
}