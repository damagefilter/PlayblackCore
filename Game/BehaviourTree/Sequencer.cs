using System;
using UnityEngine;
using System.Collections.Generic;
using Playblack.BehaviourTree.Execution.Core;
using Playblack.Csp;

namespace Playblack.BehaviourTree {
    /// <summary>
    /// Contains information about a behaviour tree that is to be executed.
    /// Also knows how to work on that tree.
    /// </summary>
    [OutputAware("OnExecutionFinish", "OnExecutionTrigger")]
    public class Sequencer : MonoBehaviour {
        [SerializeField]
        private List<ValueField> contextData;

        // Should work with savegame and unity serializer
        [SerializeField]
        private BtContainer _commands;

        public BtContainer commands {
            get {
                return _commands;
            }
            set {
                _commands = value;
            }
        }

        [SerializeField]
        [Tooltip("The object on which this sequencer is acting (for instance the object that handles AI movement or such thing.")]
        private UnityEngine.Object actor;

        [SerializeField]
        private SequenceExecutionType executionType;

        public SequenceExecutionType ExecutionType {
            get {
                return executionType;
            }
            set {
                executionType = value;
            }
        }

        private IBTExecutor executor;

        private bool wasTriggered;

        private bool ranOnce;

        public IBTExecutor GetExecutor() {
            return _commands.GetExecutor(new DataContext(contextData), actor);
        }

        public void Start() {
            this.executor = this.GetExecutor();
            if (ExecutionType == SequenceExecutionType.ON_START_ONCE || ExecutionType == SequenceExecutionType.ON_START_PARALLEL) {
                this.executor.Tick();
            }
        }

        public void Update() {
            if ((ExecutionType == SequenceExecutionType.ON_START_PARALLEL) || (ExecutionType == SequenceExecutionType.TRIGGER_PARALLEL && wasTriggered)) {
                if (this.executor == null) {
                    this.executor = this.GetExecutor();
                }
                if (this.executor.GetStatus() == TaskStatus.SUCCESS || this.executor.GetStatus() == TaskStatus.FAILURE) {
                    this.executor.Terminate();
                    this.FireOutput("OnExecutionFinish");
                }
                else {
                    this.executor.Tick();
                }
            }
        }

        [InputFunc("TriggerExecution")]
        private void TriggerExecution() {
            this.wasTriggered = true;
            if (ExecutionType == SequenceExecutionType.TRIGGER_ONCE) {
                if (this.executor == null) {
                    this.executor = this.GetExecutor();
                }
                this.executor.Tick();
                ranOnce = true;
            }
            this.FireOutput("OnExecutionTrigger");
        }

        [InputFunc("ResetSequencer")]
        public void ResetSequencer() {
            if (this.executor == null) {
                return; // nothing to reset
            }
            this.executor.Reset();
        }
    }
}

