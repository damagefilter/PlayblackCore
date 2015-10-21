using System;
using ProtoBuf;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Playblack.Logic;

namespace Playblack.Csp {
    [ProtoContract]
    [Serializable]
    public class OutputEventListener {
        // Only serializable via unity for scene defaults
        // otherwise this is re-written by the ConnectInputs method
        [SerializeField]
        public List<SignalProcessor> matchedProcessors;

        [ProtoMember(1)]
        public string method;

        [ProtoMember(2)]
        public string param;

        [ProtoMember(3)]
        public float delay;

        [ProtoMember(4)]
        public string handlerName;

        public void Execute(string component) {
            if (matchedProcessors != null) {
                for (int i = 0; i < matchedProcessors.Count; ++i) {
                    var func = matchedProcessors[i].GetInputFunc(method, component);
                    if (func == null) {
                        Debug.LogWarning(method + " is not a declared input func on " + matchedProcessors[i].GetType().Name);
                    }
                    if (delay > 0) {
                        ThreadManager.Instance.StartCoroutine(ExecuteDelayed(func));
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
        public void ConnectSignalProcessors() {
            this.matchedProcessors = new List<SignalProcessor>();
            if (string.IsNullOrEmpty(this.handlerName)) {
                return;
            }
            matchedProcessors.AddRange(SignalProcessorTracker.Instance.GetByName(this.handlerName));
        }


        public bool HasParameter(string component) {
            if (matchedProcessors == null) {
                return false;
            }
            if (string.IsNullOrEmpty(method)) {
                return false;
            }
            if (matchedProcessors == null || matchedProcessors.Count <= 0) {
                return false;
            }
            try {
                return matchedProcessors[0].GetInputFunc(method, component).HasParameter();
            }
            catch (Exception) {
                return false;
            }

        }
    }
}

