using Playblack.BehaviourTree.Model.Core;

namespace Playblack.BehaviourTree.Execution.Core {
    public interface IBTExecutor {

        /// <summary>
        /// Resets the Execution state.
        /// </summary>
        void Reset();

        /**
        * This method gives the underlying BT a little amount of time to run.
        * <p>
        * Initially, a IBTExecutor is created to run a particular BT (ModelTask).
        * From then on, the <code>tick()</code> method is called to make the tree
        * evolve.
        * <p>
        * Usually the AI of a game is driven by ticks, which means that
        * periodically the AI is given some time to update its state (it checks the
        * current game state and performs some actions). BTs follow this pattern,
        * so whenever they are ticked, they are given a little amount of time to
        * think and behave as expected. If BTs are not ticked, they do not consume
        * CPU time and they not evolve.
        * <p>
        * By calling this method, the underlying BT will be ticked, so it will
        * think and evolve accordingly.
        * <p>
        * Note that ticking a tree that has already finished should have no effect
        * on the tree.
        */
        void Tick();

        /// <summary>
        /// Terminate execution of the behaviour tree.
        /// This can be called at any time.
        /// </summary>
        void Terminate();

        /// <summary>
        /// Get the Behaviour Tree that is executed from this executor
        /// </summary>
        /// <returns></returns>
        ModelTask GetBehaviourTree();

        /// <summary>
        /// Returns the execution status of the behaviour tree.
        /// It is the status of the root of the tree.
        /// </summary>
        /// <returns></returns>
        TaskStatus GetStatus();

        /// <summary>
        /// Returns the globalContext that was associated to the root node pf the behaviour tree.
        /// This globalContext is also used to run the tree.
        /// </summary>
        /// <returns></returns>
        DataContext GetRootContext();

        /// <summary>
        /// Request that the given task is inserted into the tickable
        /// list of this BT executor.
        /// </summary>
        /// <param name="task"></param>
        void RequestTickableInsertion(ExecutionTask task);

        /// <summary>
        /// Request that the given task is removed from the tickable
        /// list of this BT executor.
        /// </summary>
        /// <param name="task"></param>
        void RequestTickableRemoval(ExecutionTask task);

        /// <summary>
        /// Get the data globalContext for a task at the given position
        /// </summary>
        /// <returns></returns>
        DataContext GetTaskState(Position pos);

        /// <summary>
        /// Add a task state to the list of task states
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="taskState"></param>
        void SetTaskState(Position pos, DataContext taskState);
    }
}
