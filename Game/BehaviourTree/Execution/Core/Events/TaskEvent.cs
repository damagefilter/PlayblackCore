namespace Playblack.BehaviourTree.Execution.Core.Events {

    public class TaskEvent {

        /// <summary>
        /// New task state
        /// </summary>
        public TaskStatus NewStatus {
            get;
            private set;
        }

        /// <summary>
        /// Previous task state
        /// </summary>
        public TaskStatus PreviousStatus {
            get;
            private set;
        }

        /// <summary>
        /// Source of the event, where it was thrown
        /// </summary>
        public ExecutionTask Source {
            get;
            private set;
        }

        public TaskEvent(ExecutionTask source, TaskStatus newStatus, TaskStatus oldStatus) {
            this.NewStatus = newStatus;
            this.PreviousStatus = oldStatus;
            this.Source = source;
        }
    }
}