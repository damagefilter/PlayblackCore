using System;

namespace Playblack.Signals {

    /// <summary>
    /// Marks an object or component as signal receiver
    /// and makes it implement a method to expose its input signals.
    /// It would be possible to mark input callbacks via annotations,
    /// however, retrieving them directly instead of via reflection appears to be faster.
    /// </summary>
    public interface SignalReceiver {

        /// <summary>
        /// Returns an array of available input functions for this signal receiver.
        /// 
        /// </summary>
        /// <returns>The input funcs.</returns>
        InputFunc[] GetInputFuncs();
    }
}

