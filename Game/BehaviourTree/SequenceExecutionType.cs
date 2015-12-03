using System;

namespace Playblack.BehaviourTree {
    [Serializable]
    public enum SequenceExecutionType {
        /// <summary>
        /// start if signal processor was triggered to do so.
        /// Continues each tick.
        /// </summary>
        TRIGGER_PARALLEL,

        /// <summary>
        /// Begin execution when the signal processor receives the Start() callback.
        /// Continues on each tick.
        /// </summary>
        ON_START_PARALLEL,

        /// <summary>
        /// Start when signal processor is triggered and execute only once
        /// </summary>
        TRIGGER_ONCE,

        /// <summary>
        /// Start when start callback is received and execute only once.
        /// </summary>
        ON_START_ONCE
    }
}

