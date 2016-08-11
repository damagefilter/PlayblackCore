using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Decorator;
using System;

namespace Playblack.BehaviourTree.Execution.Task.Decorator {

    public class ExecutionRepeat : ExecutionDecorator {
        private ExecutionTask child;

        protected ExecutionTask Child {
            get {
                return this.child;
            }
        }

        public ExecutionRepeat(ModelTask modelTask, IBTExecutor executor, ExecutionTask parent) :
            base(modelTask, executor, parent) {
            if (!(modelTask is ModelRepeat)) {
                throw new ArgumentException("The ModelTask must subclass "
                        + typeof(ModelRepeat) + " but it inherits from "
                        + modelTask.GetType());
            }
        }

        protected override void RestoreState(DataContext context) {
            // does nothing
        }

        protected override void InternalSpawn() {
            this.child = ((ModelDecorator)this.ModelTask).GetChild().CreateExecutor(this.BTExecutor, this);
            this.child.Spawn(this.GetGlobalContext());
        }

        protected override TaskStatus InternalTick() {
            var childStatus = this.child.GetStatus();
            // Since we're repeating, do it again and spawn a new one
            if (childStatus != TaskStatus.RUNNING) {
                this.child = ((ModelDecorator)this.ModelTask).GetChild().CreateExecutor(this.BTExecutor, this);
                this.child.Spawn(this.GetGlobalContext());
            }
            return TaskStatus.RUNNING;
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