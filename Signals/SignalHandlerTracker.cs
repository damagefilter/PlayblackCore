using System;
using UnityEngine;
using System.Collections.Generic;

namespace Playblack.Signals {
    /// <summary>
    /// Tracks all signal handlers in the scene so they can find each other
    /// for connecting their inputs.
    /// </summary>
    public class SignalHandlerTracker : MonoBehaviour {
        private Dictionary<string, SignalHandler> trackedHandlers;
        private Dictionary<string, SignalHandler> preTrackedHandlers;
    }
}

