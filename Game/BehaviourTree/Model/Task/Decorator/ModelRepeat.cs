
using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Execution.Task.Decorator;
using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Model.Task.Decorator {
    [ModelDataDescriptor("Repeat", DescriptorType.LOGIC, 1, typeof(ExecutionRepeat))]
    [ChildDescriptor("default", 0)]
    public class ModelRepeat : ModelDecorator {

        public ModelRepeat(ModelTask guard, ModelTask child) : base(guard, child) { }
        public ModelRepeat() : base() { }


        public override ExecutionTask CreateExecutor(IBTExecutor btExecutor, ExecutionTask parent) {
            return new ExecutionRepeat(this, btExecutor, parent);
        }
    }
}
