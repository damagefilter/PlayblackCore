using System;
using System.Collections.Generic;
using ProtoBuf;
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

        public void Invoke(string component) {
            if (registeredListeners == null) {
                return; // happens in a state after savegame load (deserialization)
            }
            for (int i = 0; i < registeredListeners.Count; ++i) {
                if (registeredListeners[i].matchedHandlers == null) {
                    continue; // happens in a state after savegame load (deserialization)
                }
                registeredListeners[i].Execute(component);
            }
        }

        public void AttachInput(string sceneName, string inputMethod, string parameter, float executionDelay) {
            var l = new OutputEventListener() {
                handlerName = sceneName,
                method = inputMethod,
                param = parameter,
                delay = executionDelay
            };
            l.ConnectInputs();
            registeredListeners.Add(l);
        }

        public void AttachInput() {
            this.AttachInput(null, null, null, 0.0f);
        }

        public void DetachAtIndex(int index) {
            if (registeredListeners.Count > index && index >= 0) {
                registeredListeners.RemoveAt(index);
            }
        }
    }
}
