using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Decorator;
using System;

namespace Playblack.BehaviourTree.Execution.Task.Decorator {
    public abstract class ExecutionDecorator : ExecutionTask {
        public ExecutionDecorator(ModelTask modelTask, IBTExecutor executor, ExecutionTask parent) :
            base(modelTask, executor, parent) {
            if (!(modelTask is ModelDecorator)) {
                throw new ArgumentException("The ModelTask must subclass "
                        + typeof(ModelDecorator) + " but it inherits from "
                        + modelTask.GetType());
            }
        }
    }
}
