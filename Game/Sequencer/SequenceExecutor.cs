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
using System;

namespace Playblack.Sequencer {

    /// <summary>
    /// Executes a behaviour tree of some description.
    /// </summary>
    [OutputAware("OnExecutionFinish", "OnExecutionTrigger", "OnExecutionTerminated")]
    [SaveableComponent]
    public class SequenceExecutor : MonoBehaviour {

        [SerializeField]
        [SaveableField(SaveField.FIELD_PROTOBUF_OBJECT)]
        private List<ValueField> globalDataContext;

        // in deployment this is only used on initial scene load
        // So do not sore that in the save game
        [SerializeField]
        [HideInInspector]
        private byte[] serializedModelTree;
#if UNITY_EDITOR
        public byte[] SerializedModelTree {
            get {
                return serializedModelTree;
            }
        }
#endif

        // This is restored when loading a savegame and is ideally the
        // original model tree.
        [SaveableField(SaveField.FIELD_PROTOBUF_OBJECT)]
        private UnityBtModel rootModel;

        public UnityBtModel RootModel {
            get {
                if (rootModel == null) {
                    DeserializeModelTree();
                }
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


        // Means this cannot be re-created via savegames without a valid prefab
        [SerializeField]
        [Tooltip("The object on which this sequencer is acting (for instance the object that handles AI movement or such thing.")]
        private UnityEngine.Object actor;

        private IBTExecutor executor;

        private bool running;

        public SequenceExecutor() {
            //this.rootModel = UnityBtModel.NewInstance(null, new UnityBtModel(), typeof(ModelSequence).ToString());
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
            return this.GetExecutor(overrideContext ?? new DataContext(globalDataContext), actor);
        }

        public IBTExecutor GetExecutor(DataContext context, UnityEngine.Object actor) {
            context["actor"] = actor;
            if (RootModel == null) {
                throw new InvalidOperationException("Tried initializing BTExecutor but root model is null");
            }
            var root = RootModel.Model;
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
            Debug.Log("Entered coroutine. Beginning ticking.");
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
            Debug.Log("Sequence executor coroutine control flow ended.");
        }

        [InputFunc("TriggerExecution")]
        public void TriggerExecution() {
            Debug.Log("Trigger execution");
            if (this.TypeOfExecution == ExecutionType.TRIGGER) {
                Debug.Log("Is trigger. Doing my thing.");
                if (this.executor == null) {
                    Debug.Log("Executor is null. Fetching new one.");
                    this.executor = this.GetExecutor();
                }
                if (!running) {
                    running = true;
                    StartCoroutine("TickExecutorParallel");
                    this.FireOutput("OnExecutionTrigger");
                    Debug.Log("Execution triggered, coroutine started");
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

        public void DeserializeModelTree() {
            using (var ms = new MemoryStream(serializedModelTree)) {
                ms.Position = 0;
                var buffer = ms.ToArray();
                if (buffer != null) {
                    this.rootModel = DataSerializer.DeserializeProtoObject<UnityBtModel>(buffer);
                }
                else {
                    this.rootModel = UnityBtModel.NewInstance(null, new UnityBtModel(), typeof(ModelSequence).ToString());
                }
                if (this.rootModel.ModelClassName == null) {
                    // This happens in corner cases where the serialized bt model didn't have a model class. Rare. Nut needs to be accounted for
                    this.rootModel.ModelClassName = typeof(ModelSequence).ToString();
                }
            }
        }

        public void SerializeModelTree() {
            this.serializedModelTree = DataSerializer.SerializeProtoObject(this.rootModel);
        }
    }
}