using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Execution.Core.Events;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Decorator;
using System;

namespace Playblack.BehaviourTree.Execution.Task.Decorator {

    public class ExecutionStatusResponder : ExecutionDecorator {
        private ExecutionTask executingCondition;
        private ExecutionTask decoratedExecutor;

        private ModelTask successModel, failModel;
        private bool doNotTick, failed;

        public ExecutionStatusResponder(ModelTask modelTask, IBTExecutor executor, ExecutionTask parent) : base(modelTask, executor, parent) {
            if (!(modelTask is ModelStatusResponder)) {
                throw new ArgumentException("The ModelTask must subclass "
                        + typeof(ModelStatusResponder) + " but it inherits from "
                        + modelTask.GetType());
            }
        }

        protected override void RestoreState(DataContext context) {
            // nothing, I guess
        }

        protected override void InternalSpawn() {
            this.decoratedExecutor = ((ModelDecorator)this.ModelTask).GetChild().CreateExecutor(this.BTExecutor, this);
            // This saves us, depending on situation, a lot of cpu time when we don't have to check
            // for the decorated executor status and run through a lot of decisions on each tick.
            // It may return running for a lot of ticks so better listen for when it actually changes
            this.decoratedExecutor.AddTaskStatusChangedCallback(this.DecoratedExecutorStatusChange);
            this.decoratedExecutor.Spawn(this.GetGlobalContext());
            // while not ticking return running
            doNotTick = true;
            failed = false; // if failed, return failed
            this.successModel = this.ModelTask.Children[1];
            this.failModel = this.ModelTask.Children[2];
            if (successModel == null && failModel == null) {
                throw new ArgumentException("At least one condition must be handled by a model in StatusResponder! Both were null!");
            }
        }

        private void DecoratedExecutorStatusChange(TaskEvent e) {
            executingCondition = null;
            if ((e.NewStatus == TaskStatus.FAILURE || e.NewStatus == TaskStatus.TERMINATED) && failModel != null) {
                this.executingCondition = failModel.CreateExecutor(this.BTExecutor, this);
            }
            else if (e.NewStatus == TaskStatus.SUCCESS && successModel != null) {
                this.executingCondition = successModel.CreateExecutor(this.BTExecutor, this);
            }

            if (executingCondition != null) {
                executingCondition.Spawn(this.GetGlobalContext());
                doNotTick = false;
                failed = true;
            }
        }

        protected override TaskStatus InternalTick() {
            if (doNotTick) {
                return TaskStatus.RUNNING;
            }
            else if (failed) {
                return TaskStatus.FAILURE;
            }
            else {
                return this.executingCondition.GetStatus();
            }
        }

        protected override void InternalTerminate() {
            if (executingCondition != null) {
                executingCondition.Terminate();
            }
            if (decoratedExecutor != null) {
                decoratedExecutor.Terminate();
            }
        }

        protected override DataContext StoreState() {
            return null;
        }

        protected override DataContext StoreTerminationState() {
            return null;
        }
    }
}