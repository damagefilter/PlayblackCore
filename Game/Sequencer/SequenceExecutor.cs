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
    [OutputAware("OnExecutionFinish", "OnExecutionTrigger", "OnExecutionTerminated", "OnPauseExecution", "OnContinueExecution")]
    [SaveableComponent]
    [DefaultExecutionOrder(-1000)] // Likely puts this before any other script requiring its data. Resolves race conditions in savegames
    public class SequenceExecutor : MonoBehaviour {

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

        public event Action OnSequenceFinished;

        public event Action OnSequenceStarted;
        
        private IBTExecutor executor;

        private bool running;

        private bool isPaused;

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
            if (RootModel == null) {
                throw new InvalidOperationException("Tried initializing BTExecutor but root model is null");
            }
            var root = RootModel.Model;
            RecursiveLoadModelTree(rootModel, root);
            Debug.Log("Creating new bt executor.");
            // TODO: Fetch an implementation from a factory
            var exec = new CachingBtExecutor(root, context);
            exec.Actor = actor;
            return exec;
        }

        public void Start() {
            this.executor = this.GetExecutor();
            if (this.TypeOfExecution == ExecutionType.AUTO) {
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
            running = true;
            isPaused = false;
            var status = this.executor.GetStatus();
            while (!(status == TaskStatus.SUCCESS || status == TaskStatus.FAILURE)) {
                if (!isPaused) {
                    this.executor.Tick();
                    status = this.executor.GetStatus();
                }
                yield return null;
            }
            TerminateExecutorAndFinish();
        }

        private void TerminateExecutorAndFinish() {
            running = false;
            this.executor.Terminate();
            this.executor.Reset();
            this.FireOutput("OnExecutionFinish");
            OnSequenceFinished?.Invoke();
            // Update the serialized / saved global data context with the current state.
            // That will ensure, when this is saved, the context is retained when it's restored.
            var contextUpdate = this.executor.GetRootContext();
            var keys = contextUpdate.GetKeys();
            for (int i = 0; i < keys.Count; ++i) {
                var key = keys[i];
                bool found = false;
                for (int j = 0; j < globalDataContext.Count; ++j) {
                    if (globalDataContext[j].Name == key) {
                        var tmp = globalDataContext[j];
                        tmp.Value = contextUpdate[key];
                        globalDataContext[j] = tmp;
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    var updateValue = contextUpdate.Get(key);

                    // reference types other than swtrings are basically too complex and we don't do them in ValueFields.
                    // If one has accidentally entered the playing field, we skip it.
                    var type = updateValue.Value.GetType();
                    if (!updateValue.Value.GetType().IsValueType && type != typeof(string)) {
                        Debug.LogWarning(string.Format("A reference type ({0}) was found in the context data of a sequencer {1}", updateValue.Value.GetType(), this.gameObject.name));
                        continue;
                    }
                    globalDataContext.Add(new ValueField(updateValue));
                }
            }
        }

        [InputFunc("TriggerExecution")]
        public void TriggerExecution() {
            Debug.Log("Trigger execution");
            if (this.TypeOfExecution == ExecutionType.TRIGGER) {
                if (this.executor == null) {
                    this.executor = this.GetExecutor();
                }
                if (!running) {
                    StartCoroutine("TickExecutorParallel");
                    OnSequenceStarted?.Invoke();
                    this.FireOutput("OnExecutionTrigger");
                }
            }
        }

        [InputFunc("PauseExecution")]
        public void PauseExecution() {
            isPaused = true;
            this.FireOutput("OnPauseExecution");
        }
        
        [InputFunc("ContinueExecution")]
        public void ContinueExecution() {
            isPaused = false;
            this.FireOutput("OnContinueExecution");
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

            StopCoroutine("TickExecutorParallel");
            TerminateExecutorAndFinish();
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
                this.rootModel = DataSerializer.DeserializeProtoObject<UnityBtModel>(buffer);
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
