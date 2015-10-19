
using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Model.Task.Leaf {
    public abstract class ModelAction : ModelLeaf {

        public ModelAction(ModelTask guard) : base(guard) { }
        public ModelAction() : base() { }
    }
}
