using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Composite;
using System;
using System.Collections.Generic;

namespace Playblack.BehaviourTree.Execution.Task.Composite {

    /// <summary>
    /// Executes everything within the sequence until fail or terminate is received or sequence has ended.
    /// </summary>
    public class ExecutionSequence : ExecutionComposite {

        /// <summary>
        /// list index of active child
        /// </summary>
        private int activeChildIndex;

        /// <summary>
        /// Reference to active child
        /// </summary>
        private ExecutionTask activeChild;

        /// <summary>
        /// List of all children of this selector
        /// </summary>
        private IList<ModelTask> children;

        public ExecutionSequence(ModelTask modelTask, IBTExecutor executor, ExecutionTask parent)
            : base(modelTask, executor, parent) {
            if (!(modelTask is ModelSequence)) {
                throw new ArgumentException("The ModelTask must subclass "
                + typeof(ModelSequence) + " but it inherits from "
                + modelTask.GetType().BaseType);
            }
        }

        protected override void RestoreState(DataContext context) {
            // nothing to do
        }

        protected override void InternalSpawn() {
            this.activeChildIndex = 0;
            this.children = this.ModelTask.Children;
            this.activeChild = this.children[this.activeChildIndex].CreateExecutor(this.BTExecutor, this);
            this.activeChild.Spawn(this.GetGlobalContext());
        }

        protected override TaskStatus InternalTick() {
            TaskStatus childStatus = this.activeChild.GetStatus();

            if (childStatus == TaskStatus.RUNNING) {
                return TaskStatus.RUNNING;
            }
            else if (childStatus == TaskStatus.FAILURE || childStatus == TaskStatus.TERMINATED) {
                return TaskStatus.FAILURE;
            }
            else {
                if (this.activeChildIndex == this.children.Count - 1) {
                    return TaskStatus.SUCCESS;
                }
                else {
                    this.activeChildIndex++;
                    this.activeChild = this.children[this.activeChildIndex].CreateExecutor(this.BTExecutor, this);
                    this.activeChild.Spawn(this.GetGlobalContext());
                    return TaskStatus.RUNNING;
                }
            }
        }

        protected override void InternalTerminate() {
            this.activeChild.Terminate();
        }

        protected override DataContext StoreState() {
            return null;
        }

        protected override DataContext StoreTerminationState() {
            return null;
        }
    }
}