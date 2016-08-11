using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playblack.Csp {

    [ProtoContract]
    [Serializable]
    public class OutputEventListener {

        // Only serializable via unity for scene defaults
        // otherwise this is re-written by the ConnectInputs method
        // This keeps a list of processors matching the given processor name
        [SerializeField]
        public List<SignalProcessor> matchedProcessors;

        [ProtoMember(1)]
        public string method;

        [ProtoMember(2)]
        public string param;

        [ProtoMember(3)]
        public float delay;

        /// <summary>
        /// Name of the processor which is supposed to be our target.
        /// </summary>
        [ProtoMember(4)]
        public string targetProcessorName;

        [ProtoMember(5)]
        public string component;

        public void Execute() {
            if (matchedProcessors != null) {
                for (int i = 0; i < matchedProcessors.Count; ++i) {
                    var func = matchedProcessors[i].GetInputFunc(method, component);
                    if (func == null) {
                        Debug.LogWarning(method + " is not a declared input func on " + matchedProcessors[i].GetType().Name);
                    }
                    if (delay > 0) {
                        matchedProcessors[i].StartCoroutine(ExecuteDelayed(func));
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
        public void FindTargetProcessors() {
            this.matchedProcessors = new List<SignalProcessor>();
            if (string.IsNullOrEmpty(this.targetProcessorName)) {
                return;
            }
            // TODO: Check what kind of performance hit this thing has when instantiating loads of objects and how to mitigate
            // without writing a custom tracker. Seems rather redundant
            var hits = UnityEngine.Object.FindObjectsOfType<SignalProcessor>();
            for (int i = 0; i < hits.Length; ++i) {
                if (!hits[i].name.StartsWith(this.targetProcessorName, StringComparison.InvariantCulture)) {
                    continue;
                }
                this.matchedProcessors.Add(hits[i]);
            }
            //matchedProcessors.AddRange(SignalProcessorTracker.Instance.GetByName(this.processorName));
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