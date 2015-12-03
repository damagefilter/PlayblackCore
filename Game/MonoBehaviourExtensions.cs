using System;
using UnityEngine;

namespace Playblack {
    public static class MonoBehaviourExtensions {

        /// <summary>
        /// OutputAware objects can use this to fire CSP events.
        /// The signal processor on the current gameobject will receive and process it.
        /// NOTE: Deactivated signal processors will not react to this
        /// </summary>
        /// <param name="behaviour">Behaviour.</param>
        /// <param name="outputName">Output name.</param>
        public static void FireOutput(this MonoBehaviour behaviour, string outputName) {
            behaviour.SendMessage("InternalFireOutput", outputName, SendMessageOptions.DontRequireReceiver);
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

