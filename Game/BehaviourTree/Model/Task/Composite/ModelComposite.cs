using Playblack.BehaviourTree.Model.Core;
using System;

namespace Playblack.BehaviourTree.Model.Task.Composite {

    public abstract class ModelComposite : ModelTask {

        public ModelComposite(ModelTask guard, params ModelTask[] children)
            : base(guard, children) {
            if (this.children.Count == 0) {
                throw new ArgumentException("The list of children cannot be empty.");
            }
        }

        // reflection ctor
        public ModelComposite() : base() { }
    }
}