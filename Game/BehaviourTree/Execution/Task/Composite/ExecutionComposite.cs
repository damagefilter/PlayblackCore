using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Composite;
using System;

namespace Playblack.BehaviourTree.Execution.Task.Composite {
    public abstract class ExecutionComposite : ExecutionTask {

        public ExecutionComposite(ModelTask modelTask, IBTExecutor executor, ExecutionTask parent)
            : base(modelTask, executor, parent) {
            if (!(typeof(ModelComposite).IsAssignableFrom(modelTask.GetType()))) {
                    throw new ArgumentException("The ModelTask " + modelTask.GetType() + " must subclass "
                    + typeof(ModelComposite) + " but it inherits from "
                    + modelTask.GetType().BaseType);
                }
        }
    }
}
