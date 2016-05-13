using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Execution.Task.Composite;
using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Model.Task.Composite {
    [ModelDataDescriptor("Run until FAILURE or end", DescriptorType.LOGIC, typeof(ExecutionSequence))]
    public class ModelSequence : ModelComposite {

        public ModelSequence(ModelTask guard, params ModelTask[] children) : base(guard, children) { }
        // reflection ctor
        public ModelSequence() : base() { }

        public override ExecutionTask CreateExecutor(IBTExecutor btExecutor, ExecutionTask parent) {
            return new ExecutionSequence(this, btExecutor, parent);
        }
    }
}
