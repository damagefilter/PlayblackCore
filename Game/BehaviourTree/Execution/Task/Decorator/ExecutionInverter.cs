using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Execution.Core.Events;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Decorator;
using System;

namespace Playblack.BehaviourTree.Execution.Task.Decorator {
    public class ExecutionInverter : ExecutionDecorator {

        private ExecutionTask child;

        public ExecutionInverter(ModelTask modelTask, IBTExecutor executor, ExecutionTask parent) : base(modelTask, executor, parent) {
            if (!(modelTask is ModelInverter)) {
                throw new ArgumentException("The ModelTask must subclass "
                        + typeof(ModelInverter) + " but it inherits from "
                        + modelTask.GetType());
            }
        }

        protected override void RestoreState(DataContext context) {
            // nothing, I guess
        }

        protected override void InternalSpawn() {
            this.child = ((ModelDecorator)this.ModelTask).GetChild().CreateExecutor(this.BTExecutor, this);
            this.child.Spawn(this.GetGlobalContext());
        }

        protected override TaskStatus InternalTick() {
            var childStatus = this.child.GetStatus();
            if (childStatus == TaskStatus.RUNNING) {
                return TaskStatus.RUNNING;
            }
            else if (childStatus == TaskStatus.FAILURE || childStatus == TaskStatus.TERMINATED) {
                return TaskStatus.SUCCESS;
            }
            else {
                return TaskStatus.FAILURE;
            }
        }

        protected override void InternalTerminate() {
            this.child.Terminate();
        }

        protected override DataContext StoreState() {
            return null;
        }

        protected override DataContext StoreTerminationState() {
            return null;
        }
    }
}
