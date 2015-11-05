using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Playblack.Csp {
    /// <summary>
    /// Tracks all signal handlers in the scene so they can find each other
    /// for connecting their inputs.
    /// </summary>
    public class SignalProcessorTracker : MonoBehaviour {
        #region Singleton
        private static SignalProcessorTracker _instance;
        private static object _lock = new object();

        public static SignalProcessorTracker Instance {
            get {
                if (applicationIsQuitting) {
                    Debug.LogWarning("[Singleton] Instance " + typeof(SignalProcessorTracker) +
                        " already destroyed on application quit." +
                        "Won't create again - returning null.");
                    return null;
                }

                lock (_lock) {
                    if (_instance == null) {
                        _instance = (SignalProcessorTracker)FindObjectOfType(typeof(SignalProcessorTracker));

                        if (_instance == null) {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<SignalProcessorTracker>();
                            singleton.name = "SignalHandlerTracker";

                            DontDestroyOnLoad(singleton);

                            Debug.Log("An instance of " + typeof(SignalProcessorTracker) +
                                " is needed in the scene, so '" + singleton +
                                "' was created with DontDestroyOnLoad.");
                        }
                    }

                    return _instance;
                }
            }
        }

        private static bool applicationIsQuitting = false;
        /// <summary>
        /// When unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        public void OnDestroy() {
            applicationIsQuitting = true;
        }
        #endregion

        private List<SignalProcessor> trackedHandlers;
        private List<SignalProcessor> preTrackedHandlers;

        private SignalProcessorTracker() {
            trackedHandlers = new List<SignalProcessor>();
            preTrackedHandlers = new List<SignalProcessor>();
        }
        /// <summary>
        /// Starts tracking a signal handler.
        /// </summary>
        /// <param name="handler"></param>
        public void Track(SignalProcessor handler) {
            if (!trackedHandlers.Contains(handler)) {
                this.trackedHandlers.Add(handler);
                if (preTrackedHandlers.Contains(handler)) {
                    preTrackedHandlers.Remove(handler);
                }
            }
            
        }

        /// <summary>
        /// Starts pre-tracking a signal handler.
        /// Once properly tracked a handler is auto-removed from the pre-tracking list.
        /// </summary>
        /// <param name="handler"></param>
        public void PreTrack(SignalProcessor handler) {
            this.preTrackedHandlers.Add(handler);
        }

        /// <summary>
        /// Manually untrack a handler.
        /// This can be called in a handlers OnDestroy() method.
        /// If the handler was already destroyed, is null or is untracked, nothing will happen.
        /// </summary>
        /// <param name="handler"></param>
        public void Untrack(SignalProcessor handler) {
            if (handler != null && trackedHandlers.Contains(handler)) {
                trackedHandlers.Remove(handler);
            }
            if (handler != null && preTrackedHandlers.Contains(handler)) {
                preTrackedHandlers.Remove(handler);
            }
        }

        /// <summary>
        /// Get a list of handlers by their scene name.
        /// If there are multiple handlers with the same scene name, all will be returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<SignalProcessor> GetByName(string name) {
            return trackedHandlers.Where((proc) => {
                return proc.name.StartsWith(name);
            }).ToList();
        }

        /// <summary>
        /// Clears the tracking lists and removes all handlers and their gameObjects from the scene.
        /// </summary>
        public void ClearTrackedHandlers() {
            // First actually destroy all the things.
            for (int i = 0; i < preTrackedHandlers.Count; ++i) {
                Destroy(preTrackedHandlers[i].gameObject);
            }
            for (int i = 0; i < trackedHandlers.Count; ++i) {
                Destroy(trackedHandlers[i].gameObject);
            }
            // Then clear them out.
            this.preTrackedHandlers.Clear();
            this.trackedHandlers.Clear();
        }
    }
}

