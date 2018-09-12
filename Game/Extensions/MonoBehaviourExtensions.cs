using Playblack.Csp;
using UnityEngine;

namespace Playblack.Extensions {

    public static class MonoBehaviourExtensions {
        /// <summary>
        /// OutputAware objects can use this to fire CSP events.
        /// The signal processor on the current gameobject will receive and process it.
        /// </summary>
        /// <param name="behaviour">Behaviour.</param>
        /// <param name="outputName">Output name.</param>
        public static void FireOutput(this MonoBehaviour behaviour, string outputName) {
            var csp = behaviour.gameObject.GetComponent<SignalProcessor>();
            if (csp != null) {
                // NOTE: CSP being null is a valid situation when we are using a CSP enabled object without the signal processor.
                // in cases where the CSP object has behaviour that doesn't rely on CSP but can be controlled by it.
                csp.FireOutput(outputName);
            }
        }
    }
}
