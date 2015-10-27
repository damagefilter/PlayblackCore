using Playblack.BehaviourTree.Model.Core;
using System;
using System.Collections.Generic;

namespace Playblack.BehaviourTree.Execution.Core {
    /// <summary>
    /// Executes a tree structure by caching all active execution tasks.
    /// This way we don't need to traverse the whole tree on each tick.
    /// </summary>
    public class CachingBtExecutor : IBTExecutor {

        private ModelTask modelBT;

        private ExecutionTask executionBT;

        private ICollection<ExecutionTask> tickableTasks;
        private ICollection<ExecutionTask> tickableTasksInsertionQueue;
        private ICollection<ExecutionTask> tickableTasksDeletionQueue;
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
            this.modelBT = modelBT;
            this.modelBT.ComputePositions();
            this.context = context;
            this.tickableTasks = new LinkedList<ExecutionTask>();
            this.tickableTasksDeletionQueue = new LinkedList<ExecutionTask>();
            this.tickableTasksInsertionQueue = new LinkedList<ExecutionTask>();
            this.taskStates = new Dictionary<Position, DataContext>();
        }

        public CachingBtExecutor(ModelTask modelBT) {
            if (modelBT == null) {
                throw new NullReferenceException("Input ModelTask is null, but must not!");
            }

            this.modelBT = modelBT;
            this.context = new DataContext();
            this.tickableTasks = new LinkedList<ExecutionTask>();
            this.tickableTasksDeletionQueue = new LinkedList<ExecutionTask>();
            this.tickableTasksInsertionQueue = new LinkedList<ExecutionTask>();
            this.taskStates = new Dictionary<Position, DataContext>();
        }

        
        public void Reset() {
            this.isInitialised = false;
            // Makes sure GetStatus returns UNINITIALISED next time
            this.executionBT = null;
        }

        public void Tick() {
            TaskStatus currentStatus = this.GetStatus();
            if (currentStatus == TaskStatus.RUNNING || currentStatus == TaskStatus.UNINITIALISED) {
                this.ProcessQueues();
                if (!this.isInitialised) {
                    this.executionBT = this.modelBT.CreateExecutor(this, null);
                    this.executionBT.Spawn(this.context);
                    this.isInitialised = true;
                }
                else {
                    foreach (var t in tickableTasks) {
                        t.Tick();
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
            return this.modelBT;
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

        public void CopyTaskStates(CachingBtExecutor executor) {
            this.taskStates = executor.taskStates;
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
