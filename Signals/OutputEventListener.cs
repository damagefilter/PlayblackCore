using System;
using ProtoBuf;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Playblack.Logic;

namespace Playblack.Signals {
    [ProtoContract]
    [Serializable]
    public class OutputEventListener {
        // Only serializable via unity for scene defaults
        // otherwise this is re-written by the ConnectInputs method
        [SerializeField]
        public List<SignalHandler> matchedHandlers;

        [ProtoMember(1)]
        public string method;

        [ProtoMember(2)]
        public string param;

        [ProtoMember(3)]
        public float delay;

        [ProtoMember(4)]
        public string handlerName;

        public void Execute(string component) {
            if (matchedHandlers != null) {
                for (int i = 0; i < matchedHandlers.Count; ++i) {
                    var func = matchedHandlers[i].GetInputFunc(method, component);
                    if (func == null) {
                        Debug.LogWarning(method + " is not a declared input func on " + matchedHandlers[i].GetType().Name);
                    }
                    if (delay > 0) {
                        ThreadPool.Instance.StartCoroutine(ExecuteDelayed(func));
                    }
                    else {
                        Invoke(func);
                    }
                }
            }
        }

        private IEnumerator ExecuteDelayed(InputFunc func) {
            yield return new WaitForSeconds(delay);
            Invoke(func);
        }

        private void Invoke(InputFunc func) {
            func.Invoke(param);
        }

        /// <summary>
        /// Called after savegame loading, to restore the entity reference.
        /// </summary>
        public void ConnectInputs() {
            this.matchedHandlers = new List<SignalHandler>();
            if (string.IsNullOrEmpty(this.handlerName)) {
                return;
            }
            matchedHandlers.AddRange(SignalHandlerTracker.Instance.GetByName(this.handlerName));
        }


        public bool HasParameter(string component) {
            if (matchedHandlers == null) {
                return false;
            }
            if (string.IsNullOrEmpty(method)) {
                return false;
            }
            if (matchedHandlers == null || matchedHandlers.Count <= 0) {
                return false;
            }
            try {
                return matchedHandlers[0].GetInputFunc(method, component).HasParameter();
            }
            catch (Exception) {
                return false;
            }

        }
    }
}

