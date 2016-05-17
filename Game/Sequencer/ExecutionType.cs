using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playblack.Sequencer {

    [Serializable]
    public enum ExecutionType {
        /// <summary>
        /// start if sequence entity was triggered to do so.
        /// Continues each tick.
        /// </summary>
        TRIGGER_PARALLEL,

        /// <summary>
        /// Begin execution when the Entity receives the Start() callback.
        /// Continues on each tick.
        /// </summary>
        ON_START_PARALLEL,

        /// <summary>
        /// Start when entity is triggered and execute only once
        /// </summary>
        TRIGGER_ONCE,

        /// <summary>
        /// Start when start callback is received and execute only once.
        /// </summary>
        ON_START_ONCE
    }
}
