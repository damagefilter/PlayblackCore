using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Leaf;
using System;

namespace Playblack.BehaviourTree.Execution.Task.Leaf {

    public abstract class ExecutionAction : ExecutionTask {

        public ExecutionAction(ModelTask modelTask, IBTExecutor executor, ExecutionTask parent) :
            base(modelTask, executor, parent) {
            if (!(modelTask is ModelAction)) {
                throw new ArgumentException("The ModelTask must subclass "
                        + typeof(ModelAction) + " but it inherits from "
                        + modelTask.GetType());
            }
        }
    }
}