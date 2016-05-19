﻿using System;
using UnityEngine;
using System.Collections.Generic;
using Playblack.BehaviourTree.Execution.Core;
using Playblack.Csp;
using Playblack.BehaviourTree;

namespace Playblack.Sequencer {
    /// <summary>
    /// Executes a behaviour tree of some description.
    /// </summary>
    [OutputAware("OnExecutionFinish", "OnExecutionTrigger")]
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
            if (sequenceContainer.TypeOfExecution == ExecutionType.ON_START_ONCE || sequenceContainer.TypeOfExecution == ExecutionType.ON_START_PARALLEL) {
                this.executor.Tick();
            }
        }

        public void Update() {
            if ((sequenceContainer.TypeOfExecution == ExecutionType.ON_START_PARALLEL) || (sequenceContainer.TypeOfExecution == ExecutionType.TRIGGER_PARALLEL && wasTriggered)) {
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
            if (sequenceContainer.TypeOfExecution == ExecutionType.TRIGGER_ONCE) {
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

