using System;
using UnityEngine;
using System.Collections.Generic;
using Playblack.BehaviourTree.Execution.Core;
using Playblack.Csp;
using Playblack.BehaviourTree;
using System.Collections;

namespace Playblack.Sequencer {
    /// <summary>
    /// Executes a behaviour tree of some description.
    /// </summary>
    [OutputAware("OnExecutionFinish", "OnExecutionTrigger", "OnExecutionTerminated")]
    public class SequenceExecutor : MonoBehaviour {
        [SerializeField]
        private List<ValueField> globalDataContext;

        // Should work with savegame and unity serializer
        [SerializeField]
        private SequenceContainer sequenceContainer;

        public SequenceContainer SequenceCommands {
            get {
                return sequenceContainer;
            }
            set {
                sequenceContainer = value;
            }
        }

        [SerializeField]
        [Tooltip("The object on which this sequencer is acting (for instance the object that handles AI movement or such thing.")]
        private UnityEngine.Object actor;

        private IBTExecutor executor;

        private bool wasTriggered;

        private bool ranOnce;

        public IBTExecutor GetExecutor() {
            return sequenceContainer.GetExecutor(new DataContext(globalDataContext), actor);
        }

        public void Start() {
            this.executor = this.GetExecutor();
            if (sequenceContainer.TypeOfExecution == ExecutionType.AUTO) {
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
            var status = this.executor.GetStatus();
            if (status == TaskStatus.SUCCESS || status == TaskStatus.FAILURE) {
                this.executor.Terminate();
                this.FireOutput("OnExecutionFinish");
                StopCoroutine("TickExecutorParallel");
            }
            else {
                this.executor.Tick();
                yield return new WaitForFixedUpdate();
            }
        }

        [InputFunc("TriggerExecution")]
        private void TriggerExecution() {
            if (this.sequenceContainer.TypeOfExecution == ExecutionType.TRIGGER) {
                if (this.executor == null) {
                    this.executor = this.GetExecutor();
                }
                StartCoroutine("TickExecutorParallel");
                this.FireOutput("OnExecutionTrigger");
            }
        }

        [InputFunc("ResetSequencer")]
        public void ResetSequencer() {
            if (this.executor == null) {
                return; // nothing to reset
            }
            this.executor.Reset();
        }

        [InputFunc("Terminate")]
        public void TerminateBT() {
            if (this.executor == null) {
                return; // nothing to reset
            }
            this.executor.Terminate();
            StopCoroutine("TickExecutorParallel");
            this.FireOutput("OnExecutionTerminated");
        }
    }
}

