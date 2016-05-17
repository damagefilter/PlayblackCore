
using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Execution.Task.Decorator;
using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Model.Task.Decorator {

    [ModelDataDescriptor("Invert Result", DescriptorType.LOGIC, 1, typeof(ExecutionInverter))]
    [ChildDescriptor("default", 0)]
    public class ModelInverter : ModelDecorator {

        public ModelInverter(ModelTask guard, ModelTask child) : base(guard, child) { }
        public ModelInverter() : base() { }

        public override ExecutionTask CreateExecutor(IBTExecutor btExecutor, ExecutionTask parent) {
            return new ExecutionInverter(this, btExecutor, parent);
        }
    }
}
