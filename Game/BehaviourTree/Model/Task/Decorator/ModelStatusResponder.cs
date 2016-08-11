using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Execution.Task.Decorator;
using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Model.Task.Decorator {

    [ModelDataDescriptor("Run Conditional", DescriptorType.LOGIC, 3, typeof(ExecutionStatusResponder))]
    [ChildDescriptor("Test Condition of:", 0, 1)]
    [ChildDescriptor("Success Case:", 1, 2)]
    [ChildDescriptor("Fail Case:", 2, 3)]
    public class ModelStatusResponder : ModelDecorator {

        public ModelStatusResponder(ModelTask guard, params ModelTask[] children) : base(guard, children) {
        }

        public ModelStatusResponder() : base() {
        }

        public override ExecutionTask CreateExecutor(IBTExecutor btExecutor, ExecutionTask parent) {
            return new ExecutionStatusResponder(this, btExecutor, parent);
        }
    }
}