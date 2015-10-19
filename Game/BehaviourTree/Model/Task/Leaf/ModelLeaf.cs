using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Model.Task.Leaf {
    public abstract class ModelLeaf : ModelTask {

        public ModelLeaf(ModelTask guard) : base(guard) { }
        public ModelLeaf() : base() { }
    }
}
