using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Execution.Task.Composite;
using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Model.Task.Composite {

    [ModelDataDescriptor("Run Selector", DescriptorType.LOGIC, -1, typeof(ExecutionSelector))]
    [ChildDescriptor("default")]
    public class ModelSelector : ModelComposite {

        public ModelSelector(ModelTask guard, params ModelTask[] children) : base(guard, children) {
        }

        // reflection ctor
        public ModelSelector() : base() { }

        public override ExecutionTask CreateExecutor(IBTExecutor btExecutor, ExecutionTask parent) {
            return new ExecutionSelector(this, btExecutor, parent);
        }
    }
}
