using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Model.Core;
using Playblack.BehaviourTree.Model.Task.Leaf;
using System;

namespace Playblack.BehaviourTree.Execution.Task.Leaf {

    public abstract class ExecutionLeaf : ExecutionTask {

        public ExecutionLeaf(ModelTask modelTask, IBTExecutor executor, ExecutionTask parent) :
            base(modelTask, executor, parent) {
            if (!(modelTask is ModelLeaf)) {
                throw new ArgumentException("The ModelTask must subclass "
                        + typeof(ModelLeaf) + " but it inherits from "
                        + modelTask.GetType());
            }
        }
    }
}