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
        private bool doNotTick;

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
            this.decoratedExecutor.AddTaskListener(this);
            this.decoratedExecutor.Spawn(this.GetGlobalContext());
            this.successModel = this.ModelTask.Children[1];
            this.failModel = this.ModelTask.Children[2];
            if (successModel == null && failModel == null) {
                doNotTick = true;
                throw new ArgumentException("At least one condition must be handled by a model in StatusResponder! Both were null!");
            }
        }

        protected override TaskStatus InternalTick() {
            if (doNotTick) {
                return TaskStatus.FAILURE;
            }
            var childStatus = this.decoratedExecutor.GetStatus();
            if (childStatus == TaskStatus.RUNNING) {
                return TaskStatus.RUNNING;
            }
            else {
                if (executingCondition == null) {
                    if (childStatus == TaskStatus.FAILURE || childStatus == TaskStatus.TERMINATED) {
                        if (failModel == null) {
                            return childStatus;
                        }
                        executingCondition = failModel.CreateExecutor(this.BTExecutor, this);
                    }
                    else {
                        if (successModel == null) {
                            return TaskStatus.SUCCESS;
                        }
                        executingCondition = successModel.CreateExecutor(this.BTExecutor, this);
                    }
                    executingCondition.AddTaskListener(this);
                    executingCondition.Spawn(this.GetGlobalContext());
                    return TaskStatus.RUNNING;
                }
                else {
                    return this.executingCondition.GetStatus();
                }
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

        public override void OnChildStatusChanged(TaskEvent e) {
            this.Tick();
        }
    }
}
