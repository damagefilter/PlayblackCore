using Playblack.BehaviourTree.Execution.Core;
using System.Collections.Generic;

namespace Playblack.BehaviourTree.Model.Core {
    public abstract class ModelTask {
        /// <summary>
        /// The children.
        /// </summary>
        protected IList<ModelTask> children;

        /// <summary>
        /// Get the Children of this ModelTask, if there are any.
        /// </summary>
        public IList<ModelTask> Children {
            get {
                return this.children;
            }
            set {
                this.children = value;
            }
        }

        /// <summary>
        /// The guard of the ModelTask.
        /// It can be null, in which case it will be evaluated to true
        /// TODO: What does this actually do?
        /// </summary>
        private ModelTask guard;

        /// <summary>
        /// The guard for this model task.
        /// May be null.
        /// </summary>
        public ModelTask Guard {
            get {
                return this.guard;
            }
            set {
                this.guard = value;
            }
        }

        /// <summary>
        /// The position of the model task in the behaviour tree
        /// </summary>
        private Position position;

        /// <summary>
        /// The position of the model task in the behaviour tree
        /// </summary>
        public Position TreePosition {
            get {
                return this.position;
            }
        }

        public DataContext Context {
            get;
            set;
        }

        public ModelTask(ModelTask guard, params ModelTask[] children) {
            this.guard = guard;
            this.children = new List<ModelTask>();
            foreach (ModelTask t in children) {
                this.children.Add(t);
            }
            this.position = new Position();
        }
        // reflection ctor
        public ModelTask() {
            this.children = new List<ModelTask>();
            this.position = new Position();
        }

        /// <summary>
        /// This method computes the positions of all the tasks of the behaviour tree
        /// whose root is this node. After calling this method, the positions of all
        /// the tasks below this one will be available and accessible through
        /// {@link #getPosition()}.
        /// 
        /// It is important to note that, when calling this method, this task is
        /// considered to be the root of the behaviour tree, so its position will be
        /// set to an empty sequence of moves, with no offset, and the positions of
        /// the tasks below it will be computed from it.
        /// </summary>
        public void ComputePositions() {
            // assume this node is the root of the tree
            this.position = new Position(new LinkedList<int>());

            /*
             * Set the position of all of the children of this task and recursively
             * compute the position of the rest of the tasks.
             */
            for (int i = 0; i < this.children.Count; ++i) {
                ModelTask currentChild = this.children[i];
                Position currentChildPos = new Position(this.position);
                currentChildPos.AddMove(i);
                currentChild.position = currentChildPos;
                RecursiveComputePositions(currentChild);
            }
        }

        private void RecursiveComputePositions(ModelTask t) {

            for (int i = 0; i < t.children.Count; ++i) {
                ModelTask currentChild = t.children[i];
                Position currentChildPos = new Position(t.position);
                currentChildPos.AddMove(i);
                currentChild.position = currentChildPos;
                RecursiveComputePositions(currentChild);
            }
        }

        /// <summary>
        /// Takes the internal data context values and populates the fields of this
        /// ModelTask with them...
        /// </summary>
        protected void SetValuesFromContext() {

        }

        /// <summary>
        /// Creates a suitable ExecutionTask that will be able to run this ModelTask
        /// through the management of a BTExecutor.
        /// </summary>
        /// <param name="btExecutor"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public abstract ExecutionTask CreateExecutor(IBTExecutor btExecutor, ExecutionTask parent);
    }
}
