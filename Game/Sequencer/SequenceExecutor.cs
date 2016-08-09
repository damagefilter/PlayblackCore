using System;
using UnityEngine;
using System.Collections.Generic;
using Playblack.BehaviourTree.Execution.Core;
using Playblack.Csp;
using Playblack.BehaviourTree;
using System.Collections;
using Playblack.BehaviourTree.Model.Core;
using System.IO;
using Playblack.Savegame.Model;
using Playblack.BehaviourTree.Model.Task.Composite;

namespace Playblack.Sequencer {
    /// <summary>
    /// Executes a behaviour tree of some description.
    /// </summary>
    [OutputAware("OnExecutionFinish", "OnExecutionTrigger", "OnExecutionTerminated")]
    public class SequenceExecutor : MonoBehaviour, ISerializationCallbackReceiver {
        [SerializeField]
        private List<ValueField> globalDataContext;

        // Should work with savegame and unity serializer
        [SerializeField]
        [HideInInspector]
        private byte[] serializedModelTree;

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

        [SerializeField]
        [Tooltip("The object on which this sequencer is acting (for instance the object that handles AI movement or such thing.")]
        private UnityEngine.Object actor;

        private IBTExecutor executor;

        private bool running;

        public SequenceExecutor() {
            this.rootModel = UnityBtModel.NewInstance(null, new UnityBtModel(), typeof(ModelSequence).ToString());
        }
        public IBTExecutor GetExecutor() {
            return this.GetExecutor(new DataContext(globalDataContext), actor);
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
        public IBTExecutor GetExecutor(DataContext context, UnityEngine.Object actor) {
            context["actor"] = actor;

            var root = rootModel.Model;
            RecursiveLoadModelTree(rootModel, root);
            // TODO: Fetch an implementation from a factory
            return new CachingBtExecutor(root, context);
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

