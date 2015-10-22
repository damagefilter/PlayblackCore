using Playblack.BehaviourTree.Exceptions;
using Playblack.BehaviourTree.Execution.Core.Events;
using Playblack.BehaviourTree.Model.Core;
using System.Collections.Generic;

namespace Playblack.BehaviourTree.Execution.Core {
    public abstract class ExecutionTask : ITaskListener {
        private ModelTask modelTask;
        public ModelTask ModelTask {
            get {
                return this.modelTask;
            }
        }
        private IBTExecutor executor;
        public IBTExecutor BTExecutor {
            get {
                return this.executor;
            }
        }
        private DataContext globalContext;

        private DataContext localContext;

        private ICollection<ITaskListener> listeners;

        private TaskStatus status;

        private bool 
            // Flag if this can (and should) be spawned
            spawnable, 
            // Flag if this can be ticked (has been spawned
            tickable, 
            // are we terminated?
            terminated;

        /// <summary>
        /// Parent, may be null if this is a root node.
        /// </summary>
        private ExecutionTask parent;

        private Position position;

        public ExecutionTask(ModelTask modelTask, IBTExecutor executor, ExecutionTask parent) {
            this.modelTask = modelTask;
            this.executor = executor;
            this.listeners = new LinkedList<ITaskListener>();
            this.spawnable = true;
            this.tickable = false;
            this.status = TaskStatus.UNINITIALISED;

            if (parent == null) {
                this.position = new Position();
            }
            else {
                this.position = new Position(parent.position);
                this.position.AddMove(GetMove());
                this.parent = parent;
            }
        }

        /// <summary>
        /// Handles the general spawning of this task.
        /// Throws SpawnException if this task was already spawned.
        /// </summary>
        /// <param name="globalContext"></param>
        public void Spawn(DataContext context) {
            if (!this.spawnable) {
                throw new SpawnException("The task cannot be spawned. It already was spawned.");
            }
            this.localContext = this.modelTask.Context;
            this.globalContext = context;
            this.spawnable = false;
            this.tickable = true;
            this.status = TaskStatus.RUNNING;
            // TODO: Request to insert this into the tickables now?
            // the original states that it should go into the openTasks list but it's never used and probably leaking
            //this.executor.
            DataContext previousState = this.executor.GetTaskState(this.position);
            this.RestoreState(previousState);
            this.executor.RequestTickableInsertion(this);
            // Do the specific spawning thing
            InternalSpawn();
        }

        /// <summary>
        /// Does the ticking and updating of this task each time it is called and returns the status.
        /// Throws a TickException if this task cannot be ticked.
        /// </summary>
        /// <returns></returns>
        public TaskStatus Tick() {
            if (!this.tickable) {
                throw new NotTickableException("The task cannot be ticked. It must be spawned first!");
            }
            if (this.terminated) {
                return TaskStatus.TERMINATED; // terminated tasks do nothing
            }

            var newStatus = this.InternalTick();
            if (!ValidateInternalTickStatus(newStatus)) {
                throw new IllegalReturnStatusException(newStatus + " cannot be returned by ExecutionTask.InternalTick()");
            }
            this.status = newStatus;

            if (newStatus != TaskStatus.RUNNING) {
                DataContext taskState = StoreState();
                this.executor.SetTaskState(this.position, taskState);
                this.executor.RequestTickableRemoval(this);

                FireTaskEvent(newStatus);
            }
            return newStatus;
        }

        public TaskStatus GetStatus() {
            return this.status;
        }

        public DataContext GetGlobalContext() {
            return this.globalContext;
        }

        public DataContext GetLocalContext() {
            return this.localContext;
        }

        public void AddTaskListener(ITaskListener listener) {
            this.listeners.Add(listener);
        }

        public void RemoveTaskListener(ITaskListener listener) {
            this.listeners.Remove(listener);
        }

        public void Terminate() {
            if (!this.tickable) {
                throw new NotTickableException("Cannot terminate a task that was never spawned.");
            }
            if (!this.terminated) {
                this.terminated = true;
                this.status = TaskStatus.TERMINATED;
                this.executor.RequestTickableRemoval(this);
                var taskContext = this.StoreTerminationState();
                this.executor.SetTaskState(this.position, taskContext);
            }
        }

        #region to implement
        protected abstract void RestoreState(DataContext context);
        // description copied from jbt sources
        /**
        * This is the method that carries out the actual spawning process of the
        * ExecutionTask. Sublclasses must define it, since the spawning process
        * varies depending on the type of the task.
        * <p>
        * <code>internalSpawn()</code> is called from the {@link #spawn(IContext)}
        * method. When <code>internalSpawn()</code> is called, the globalContext of the
        * task is already accessible through {@link #getContext()}, so it can be
        * used by the task.
        * <p>
        * <code>internalSpawn()</code> is the method that creates all the structure
        * of interconnected tasks (ExecutionTask) that are necessary to run this
        * task. RootModels subclass is spawned in a different way. For instance, when a
        * sequence task is spawned, it has to spawn its first child, but when a
        * parallel task is spawned, it has to spawn all of its children.
        * <p>
        * An ExecutionTask contains a reference to a ModelTask which is trying to
        * run. When <code>internalSpawn()</code> is called, it has to create,
        * according to the semantics of the task, new ExecutionTask objects for the
        * children of the ModelTask.
        * <p>
        * For instance, let us suppose that there is a ModelSequence class
        * subclassing ModelTask. The ExecutionTask associated to ModelSequence is
        * ExecutionSequence. An ExecutionSequence has a reference to the
        * ModelSequence it is running. When ExecutionSequence is spawned, it has to
        * create, <i>according to the semantics of the task, new ExecutionTask
        * objects for the children of the ModelTask</i>. In this case it means that
        * the first child of the sequence should also be spawned (in a recursive
        * manner). Therefore, what the ExecutioSequence has to is to take the first
        * child of the ModelSequence (let us call it <i>child</i>), which will be a
        * ModelTask. Then, it will have to create the appropriate ExecutionTask for
        * <i>child</i>, by calling <i>child.createExecutor()</i>. Finally, it will
        * have to call the <code>spawn()</code> method recursively on the
        * ExecutionTask returned by <i>child.createExecutor()</i>. Other tasks
        * would behave differently. For instance, when an ExecutionParallel
        * (associated to a ModelParallel) is spawned, it has to create an
        * ExecutionTask for all of the children of its ModelParallel, and
        * recursively spawn every single one of them.
        * <p>
        * Leaf tasks (also known as <i>low level task</i>, since they usually -but
        * not always- perform a game-dependent process), such as actions and
        * conditions, are spawned in a different way. They do not recursively spawn
        * any child, since they have none. When a leaf task is spawned, it should
        * start the execution of the process associated to the node. Keep in mind
        * that low level tasks may perform long processes that require several
        * ticks in order to complete. It is in this method that those processes
        * start (maybe in independent threads).
        * <p>
        * It should be noted, however, that many processes may be instantaneous, so
        * they may complete even within the <code>internalSpawn()</code> method.
        * Nevertheless, in these cases the BT should not evolve, reason why the
        * termination notification to its parent is carried out in the
        * <code>tick()</code> method, probably in the next AI cycle. If
        * <code>spawn()</code> were allowed to notify parents when the task
        * terminates, then a single call to <code>spawn()</code> may take too long
        * to complete due to the uninterrupted evolution of the tree, which is
        * something that has to be avoided.
        * <p>
        * An important part of the spawning process is to decide if the
        * ExecutionTask will enter the list of tickable nodes of the BTExecutor.
        * Only tasks that request to be inserted into that list are ticked at every
        * game AI cycle (when {@link BTExecutor#tick()} is called). In order to
        * request it, the task has to call
        * {@link #requestInsertionIntoTickableList()}. In general, all leaf tasks
        * should be ticked every cycle, since the progress of parent tasks depends
        * on the termination of their children. However, non-leaf tasks may also
        * need ticking. For instance, the dynamic priority list task needs to
        * constantly receive ticks, since it has to check its children's guards all
        * the time -the dynamic priority list can evolve not only because of the
        * termination of the currently active child, but also because of the
        * reevaluation of guards-. In general, if the only way of making a task
        * evolve is through the notification of termination from one or several of
        * its children, then the task should not be in the list of tickable nodes.
        * On the other hand, if a task can evolve because of factors other than the
        * termination of one or several of its children, then it should request to
        * be inserted into the list of tickable nodes.
        */
        protected abstract void InternalSpawn();
        // description copied from jbt sources
        /**
        * <code>internalTick()</code> is the method that actually carries out the
        * ticking process of an ExecutionTask. Subclasses must define it, since the
        * ticking process varies depending on the type of the task.
        * <p>
        * <code>internalTick()</code> is called from the {@link #tick()} method.
        * When it is called, it must assume that the task has already been spawned
        * ({@link #spawn(IContext)}) and that the globalContext of the task is already
        * accessible through {@link #getContext()}.
        * <p>
        * <code>internalTick()</code> is the method that is used to update an
        * ExecutionTask. Behaviour trees are driven by ticks, which means that they
        * only evolve when they are ticked (otherwise put, behaviour trees are
        * given CPU time only when they are ticked). <code>internalTick()</code> is
        * the method that implements the ticking process of the task. Therefore,
        * when it is called, and according to the semantics of the task, it will
        * have to do some processes to make the task go on. This processes may
        * include spawning other children or even terminating currently running
        * children.
        * <p>
        * For instance, let us suppose that there is a ModelSequence class
        * subclassing ModelTask. The ExecutionTask associated to ModelSequence is
        * ExecutionSequence. An ExecutionSequence has a reference to the
        * ModelSequence it is running. When ExecutionSequence is ticked, it has to
        * update the task <i>according to the semantics of the task</i>. In this
        * case it means that it has to analyze the current status of the current
        * active child (through {@link #getStatus()}). If the child is still
        * running, the ticking process just does nothing, since the sequence cannot
        * go on unless the current child finishes. Nevertheless, if the child has
        * successfully finished, the ExecutionSequence will have to spawn the next
        * task of the sequence. In order to do so, the ExecutionSequence will
        * access it through its ModelSequence. A new ExecutionTask will be created
        * for the next child of the ModelSequence (via the
        * <code>ModelTask.createExecutor()</code>) method, and then it will be
        * spawned (in this case, <code>internalTick()</code> will return
        * {@link Status#RUNNING}). However, if the child has not finished
        * successfully, the sequence has to be aborted, so the ticking process will
        * just return the failure status code {@link Status#FAILURE} (from the
        * outside, the <code>tick()</code> method will catch this termination code
        * and, as a result, it will fire a TaskEvent to notify the parent of the
        * ExecutionSequence).
        * <p>
        * The ticking process of the ExecutionParallel task is very different. When
        * <code>internalTick()</code> is called, the ExecutionParallel has to check
        * the current status of all of its children. If one of them has failed,
        * then all the children must be terminated, and the failure code
        * {@link Status#FAILURE} must be returned. If all of its children have
        * successfully finished, then the ExecutionParallel will just return
        * {@link Status#SUCCESS}. Otherwise, it will return {@link Status#RUNNING}.
        * <p>
        * Leaf tasks (<i>low level task</i>), such as actions and conditions, are
        * ticked in a different way. They do not have to analyze the termination
        * status of any child, since they have none. When a leaf task is ticked, it
        * should check the termination status of the process associated to the
        * task, and return a termination status accordingly. <b>It should be noted
        * that when a task has been terminated ( {@link #terminate()}),
        * <code>tick()</code> does nothing</b>.
        * <p>
        * It should be noted that when a task has been terminated (
        * {@link #terminate()}), <code>tick()</code> does nothing. In particular,
        * <code>tick()</code> will not call <code>internalTick()</code>.
        * <b>Therefore, it can be assumed that if <code>internalTick()</code> is
        * called, then this task has not been terminated</b>, so the implementation
        * of this method should not even consider other cases.
        * <p>
        * An important aspect of this method is that, even though it returns an
        * Status object, only certain return values are allowed. In particular,
        * only {@link Status#SUCCESS}, {@link Status#FAILURE} and
        * {@link Status#RUNNING} can be returned.
        * 
        * @return the status of the task after being ticked.
        */
        protected abstract TaskStatus InternalTick();

        protected abstract void InternalTerminate();
        // description copied from jbt sources
        /**
         * This method stores the persistent state of an ExecutionTask. Some tasks
         * need to keep some information throughout the execution of the tree.
         * <p>
         * Some tasks in BTs are persistent in the sense that, after finishing, if
         * they are spawned again, they remember past information. Take for example
         * the "limit" task. A "limit" task allows to run its child node only a
         * certain number of times (for example, 5). After being spawned, it has to
         * remember how many times it has been run so far, so that, once the
         * threshold is exceeded, it fails.
         * <p>
         * The problem here is that tasks are destroyed when they leave the list of
         * tickable tasks. Thus, if the task needs to be used again, a new instance
         * for the task must be created, which, of course, will not remember past
         * information since it is a new object. This method is used for storing
         * information that needs to be used in the future when the task gets
         * created again. In particular, this method is called in the
         * {@link #tick()} function just after noticing that the task has finished
         * (when <code>internalTick()</code> returns a termination status). By doing
         * so, the task stores its state as soon as possible just in case it needs
         * to be spawned immediately afterwards.
         * <p>
         * This method must return the information it needs to remember in a a
         * {@link ITaskState} object, which must be comprehensible by the
         * {@link #restoreState(ITaskState)}, that is, it is
         * <code>restoreState()</code> that knows how to restore the state of the
         * task by reading the information that <code>storeState()</code> returns.
         * <p>
         * This method is called when the task finishes, so its implementation
         * should take into account that it will be called only when
         * {@link #internalTick()} returns a Status different from
         * {@link Status#RUNNING}.
         * <p>
         * This method may return null if the task does not need to store any state
         * information for future use.
         * 
         * @return an ITaskState object with the persistent state information of the
         *         task, for future use. The returned ITaskState must be readable by
         *         <code>restoreState()</code>.
         */
        protected abstract DataContext StoreState();

        protected abstract DataContext StoreTerminationState();
        #endregion

        private static bool ValidateInternalTickStatus(TaskStatus status) {
            return !(status == TaskStatus.TERMINATED || status == TaskStatus.UNINITIALISED);
        }

        private int GetMove() {
            if (this.parent != null) {
                ICollection<ModelTask> parentsChildren = this.parent.ModelTask.Children;
                IEnumerator<ModelTask> iterator = parentsChildren.GetEnumerator();
                ModelTask thisModelTask = this.ModelTask;

                for (int i = 0; i < parentsChildren.Count; ++i) {
                    iterator.MoveNext();
                    if (iterator.Current == thisModelTask) {
                        return i;
                    }
                }
            }
            return 0;
        }

        private void FireTaskEvent(TaskStatus newStatus) {
            foreach (ITaskListener l in this.listeners) {
                l.OnChildStatusChanged(new TaskEvent(this, newStatus, this.GetStatus()));
            }
        }

        public abstract void OnChildStatusChanged(TaskEvent e);
    }
}
