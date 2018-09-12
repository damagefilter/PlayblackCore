using Playblack.Csp;
using UnityEngine;

namespace Playblack.Extensions {

    public static class MonoBehaviourExtensions {
        /// <summary>
        /// OutputAware objects can use this to fire CSP events.
        /// The signal processor on the current gameobject will receive and process it.
        /// NOTE: Deactivated signal processors will not react to this
        /// </summary>
        /// <param name="behaviour">Behaviour.</param>
        /// <param name="outputName">Output name.</param>
        /// <param name="trace"></param>
        public static void FireOutput(this MonoBehaviour behaviour, string outputName, Trace trace = null) {
            var csp = behaviour.gameObject.GetComponent<SignalProcessor>();
            if (csp != null) {
                csp.InternalFireOutput(outputName, trace);
            }
            else {
                Debug.LogError(string.Format("No SignalProcessor on {0} to call output {1}", behaviour.gameObject.name, outputName));
            }
        }

        /// <summary>
        /// Fires a CSP event upwards in the game object hierarchy all CSPs on the current GameObject
        /// and on all ancestors will receive this event.
        /// NOTE: Deactivated signal processors will not react to this
        /// </summary>
        /// <param name="behaviour">Behaviour.</param>
        /// <param name="outputName">Output name.</param>
        public static void FireOutputUpwards(this MonoBehaviour behaviour, string outputName) {
            behaviour.SendMessageUpwards("InternalFireOutput", outputName, SendMessageOptions.DontRequireReceiver);
        }
    }
}
