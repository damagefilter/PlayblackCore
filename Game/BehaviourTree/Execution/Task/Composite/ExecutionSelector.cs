using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Composite;
using System;
using System.Collections.Generic;

namespace Playblack.BehaviourTree.Execution.Task.Composite {

    /// <summary>
    /// Returns after the first successful child a success status
    /// </summary>
    public class ExecutionSelector : ExecutionComposite {

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

        public ExecutionSelector(ModelTask modelTask, IBTExecutor executor, ExecutionTask parent)
            : base(modelTask, executor, parent) {
            if (!(modelTask is ModelSelector)) {
                throw new ArgumentException("The ModelTask must subclass "
                + typeof(ModelSelector) + " but it inherits from "
                + modelTask.GetType().BaseType);
            }
        }

        protected override void RestoreState(DataContext context) {
            // does nothing
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
            else if (childStatus == TaskStatus.SUCCESS) {
                return TaskStatus.SUCCESS;
            }
            else {
                if (this.activeChildIndex == this.children.Count - 1) {
                    return TaskStatus.FAILURE;
                }
                else {
                    this.activeChildIndex++;
                    if (this.activeChild != null) {
                    }
                    this.activeChild = this.children[this.activeChildIndex].CreateExecutor(this.BTExecutor, this);
                    this.activeChild.Spawn(this.GetGlobalContext());
                    return TaskStatus.RUNNING;
                }
            }
        }

        protected override DataContext StoreState() {
            // not doing a thing
            return null;
        }

        protected override DataContext StoreTerminationState() {
            // not doing a thing
            return null;
        }

        protected override void InternalTerminate() {
            this.activeChild.Terminate();
        }
    }
}