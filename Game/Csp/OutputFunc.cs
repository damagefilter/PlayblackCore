using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Playblack.Csp {

    /// <summary>
    /// Defines a signal output and registered listeners.
    ///
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class OutputFunc {

        [SerializeField]
        [ProtoMember(1)]
        protected List<OutputEventListener> registeredListeners; // protobuf needs protected

        public List<OutputEventListener> Listeners {
            get {
                return this.registeredListeners;
            }
        }

        [SerializeField]
        [ProtoMember(2)]
        private string outputName;

        public string Name {
            get {
                return this.outputName;
            }
        }

        // protobuf ctor
        public OutputFunc() {
        }

        public OutputFunc(string outputName) {
            registeredListeners = new List<OutputEventListener>();
            this.outputName = outputName;
        }

        public void Invoke(SignalProcessor caller, Trace trace) {
            if (registeredListeners == null) {
                return; // happens in a state after savegame load (deserialization)
            }
            for (int i = 0; i < registeredListeners.Count; ++i) {
                if (registeredListeners[i].matchedProcessors == null) {
                    continue; // happens in a state after savegame load (deserialization)
                }

                registeredListeners[i].Execute(this, caller, trace);
            }
        }

        public void AttachInput(string sceneName, string inputMethod, string parameter, float executionDelay, SignalProcessor localProcessor) {
            var l = new OutputEventListener() {
                targetProcessorName = sceneName,
                method = inputMethod,
                param = parameter,
                delay = executionDelay
            };
            l.FindTargetProcessors(localProcessor);
            registeredListeners.Add(l);
        }

        public void AttachInput(SignalProcessor localProcessor) {
            this.AttachInput(null, null, null, 0.0f, localProcessor);
        }

        public void DetachAtIndex(int index) {
            if (registeredListeners.Count > index && index >= 0) {
                registeredListeners.RemoveAt(index);
            }
        }
    }
}
