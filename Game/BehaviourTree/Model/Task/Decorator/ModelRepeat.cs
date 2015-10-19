
using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Execution.Task.Decorator;
using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Model.Task.Decorator {
    public class ModelRepeat : ModelDecorator {

        public ModelRepeat(ModelTask guard, ModelTask child) : base(guard, child) { }
        public ModelRepeat() : base() { }


        public override ExecutionTask CreateExecutor(IBTExecutor btExecutor, ExecutionTask parent) {
            return new ExecutionRepeat(this, btExecutor, parent);
        }
    }
}
