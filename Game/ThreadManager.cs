using System;
using UnityEngine;

namespace Playblack {
    public class ThreadManager : MonoBehaviour {
        private static ThreadManager _instance;
        private static object _lock = new object();
        
        public static ThreadManager Instance {
            get {
                if (applicationIsQuitting) {
                    Debug.LogWarning("[Singleton] Instance " + typeof(ThreadManager) +
                        " already destroyed on application quit." +
                        "Won't create again - returning null.");
                    return null;
                }
                
                lock (_lock) {
                    if (_instance == null) {
                        _instance = (ThreadManager)FindObjectOfType(typeof(ThreadManager));
                        
                        if (_instance == null) {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<ThreadManager>();
                            singleton.name = "ThreadPool";
                            
                            DontDestroyOnLoad(singleton);
                            
                            Debug.Log("An instance of " + typeof(ThreadManager) + 
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
    }
}

