using System;

namespace Playblack.Sequencer {

    [Serializable]
    public enum ExecutionType {

        /// <summary>
        /// The Sequence can only be started via trigger input
        /// Continues with fixed update intervals in a coroutine.
        /// </summary>
        TRIGGER,

        /// <summary>
        /// Begin execution when the Sequencer receives the Start() callback.
        /// Continues with fixed update intervals in a coroutine.
        /// </summary>
        AUTO,
    }
}