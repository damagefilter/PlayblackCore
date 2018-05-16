using Playblack.BehaviourTree.Execution.Core;
using Playblack.BehaviourTree.Execution.Task.Composite;
using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Model.Task.Composite {

    [ModelDataDescriptor("Run Sequence", DescriptorType.LOGIC, -1, typeof(ExecutionSequence))]
    [ChildDescriptor("default")]
    public class ModelSequence : ModelComposite {

        public ModelSequence(ModelTask guard, params ModelTask[] children) : base(guard, children) {
        }

        // reflection ctor
        public ModelSequence() : base() { }

        public override ExecutionTask CreateExecutor(IBTExecutor btExecutor, ExecutionTask parent) {
            return new ExecutionSequence(this, btExecutor, parent);
        }
    }
}
