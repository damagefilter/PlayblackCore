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
        private SequenceExecutionType _executionType;

        public SequenceExecutionType executionType {
            get {
                return _executionType;
            }
            set {
                _executionType = value;
            }
        }

        private IBTExecutor executor;

        private bool wasTriggered;

        private bool ranOnce;

        public IBTExecutor GetExecutor() {
            return _commands.GetExecutor(new DataContext(contextData), actor);
        }

        public override void Start() {
            this.executor = this.GetExecutor();
            if (executionType == SequenceExecutionType.ON_START_ONCE || executionType == SequenceExecutionType.ON_START_PARALLEL) {
                this.executor.Tick();
            }
        }

        public void Update() {
            if ((executionType == SequenceExecutionType.ON_START_PARALLEL) || (executionType == SequenceExecutionType.TRIGGER_PARALLEL && wasTriggered)) {
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

        private void TriggerExecution() {
            this.wasTriggered = true;
            if (executionType == SequenceExecutionType.TRIGGER_ONCE) {
                if (this.executor == null) {
                    this.executor = this.GetExecutor();
                }
                this.executor.Tick();
                ranOnce = true;
            }
            this.FireOutput("OnExecutionTrigger");
        }

        private void ResetSequencer() {
            if (this.executor == null) {
                return; // nothing to reset
            }
            this.executor.Reset();
        }
    }
}

