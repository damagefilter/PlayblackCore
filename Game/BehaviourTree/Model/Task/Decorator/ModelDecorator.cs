using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Model.Task.Decorator {

    public abstract class ModelDecorator : ModelTask {

        public ModelDecorator(ModelTask guard, params ModelTask[] child) :
            base(guard, child) {
        }

        public ModelDecorator() :
            base() {
        }

        public ModelTask GetChild() {
            return this.Children[0];
        }
    }
}