using Playblack.BehaviourTree.Model.Core;
using System;
using System.Collections.Generic;

namespace Playblack.BehaviourTree.Execution.Core {
    /// <summary>
    /// Executes a tree structure by caching all active execution tasks.
    /// This way we don't need to traverse the whole tree on each tick.
    /// </summary>
    public class CachingBtExecutor : IBTExecutor {

        private ModelTask rootModel;

        private ExecutionTask executionBT;

        private IList<ExecutionTask> tickableTasks;
        private IList<ExecutionTask> tickableTasksInsertionQueue;
        private IList<ExecutionTask> tickableTasksDeletionQueue;
        // TODO: Add collection of execution interrupters for interrupter models

        /// <summary>
        /// States of the tasks of the tree that is being run by this BTExecutor.
        /// States are indexed by Position. These are the positions of the ExecutionTask in the execution tree.
        /// These positions are unique sp each node in the execution tree can be unambigously referenced by such position.
        /// Note that this map does not strore the states of nodes of the guards of the tree that is bein run.
        /// </summary>
        private Dictionary<Position, DataContext> taskStates;

        /// <summary>
        /// Global data globalContext
        /// </summary>
        private DataContext context;

        private bool isInitialised;

        public CachingBtExecutor(ModelTask modelBT, DataContext context) {
            if (modelBT == null) {
                throw new NullReferenceException("Input ModelTask is null, but must not!");
            }
            if (context == null) {
                throw new NullReferenceException("Input DataContext is null but must not!");
            }
            this.rootModel = modelBT;
            this.rootModel.ComputePositions();
            this.context = context;
            this.tickableTasks = new List<ExecutionTask>();
            this.tickableTasksDeletionQueue = new List<ExecutionTask>();
            this.tickableTasksInsertionQueue = new List<ExecutionTask>();
            this.taskStates = new Dictionary<Position, DataContext>();
        }

        public CachingBtExecutor(ModelTask modelBT) {
            if (modelBT == null) {
                throw new NullReferenceException("Input ModelTask is null, but must not!");
            }

            this.rootModel = modelBT;
            this.context = new DataContext();
            this.tickableTasks = new List<ExecutionTask>();
            this.tickableTasksDeletionQueue = new List<ExecutionTask>();
            this.tickableTasksInsertionQueue = new List<ExecutionTask>();
            this.taskStates = new Dictionary<Position, DataContext>();
        }

        
        public void Reset() {
            this.isInitialised = false;
            // Makes sure GetStatus returns UNINITIALISED next time
            this.executionBT = null;
            this.tickableTasks.Clear();
            this.tickableTasksDeletionQueue.Clear();
            this.tickableTasksInsertionQueue.Clear();
        }

        public void Tick() {
            TaskStatus currentStatus = this.GetStatus();
            if (currentStatus == TaskStatus.RUNNING || currentStatus == TaskStatus.UNINITIALISED) {
                this.ProcessQueues();
                if (!this.isInitialised) {
                    this.executionBT = this.rootModel.CreateExecutor(this, null);
                    this.executionBT.Spawn(this.context);
                    this.isInitialised = true;
                }
                else {
                    for (int i = 0; i < tickableTasks.Count; ++i) {
                        tickableTasks[i].Tick();
                    }
                }
            }
        }

        public void Terminate() {
            if (this.executionBT != null) {
                this.executionBT.Terminate();
            }
        }

        public ModelTask GetBehaviourTree() {
            return this.rootModel;
        }

        public TaskStatus GetStatus() {
            if (this.executionBT == null) {
                return TaskStatus.UNINITIALISED;
            }
            return this.executionBT.GetStatus();
        }

        public DataContext GetRootContext() {
            return this.context;
        }

        public DataContext GetTaskState(Position pos) {
            if (this.taskStates.ContainsKey(pos)) {
                return this.taskStates[pos];
            }
            return null;
        }


        public void SetTaskState(Position pos, DataContext taskState) {
            this.taskStates[pos] = taskState;
        }

        public void RequestTickableInsertion(ExecutionTask task) {
            tickableTasksInsertionQueue.Add(task);
        }

        public void RequestTickableRemoval(ExecutionTask task) {
            tickableTasksDeletionQueue.Add(task);
        }

        private void ProcessQueues() {
            foreach (ExecutionTask task in tickableTasksInsertionQueue) {
                this.tickableTasks.Add(task);
            }

            foreach (ExecutionTask task in tickableTasksDeletionQueue) {
                this.tickableTasks.Remove(task);
            }
            tickableTasksInsertionQueue.Clear();
            tickableTasksDeletionQueue.Clear();
        }
    }
}
