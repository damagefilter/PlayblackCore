using System;
using UnityEngine;
using System.Collections.Generic;
using Playblack.Savegame;

namespace Playblack.Signals {

    public delegate void SimpleSignal();
    public delegate void ParameterSignal(string param);

    /// <summary>
    /// Tracks and manages signals coming in and out of the gameobject this handler is attached to.
    /// </summary>
    [DisallowMultipleComponent]
    [SaveableComponent]
    public class SignalHandler : MonoBehaviour {
        /// <summary>
        /// Signals this Entity can receive and process.
        /// This list merely represents meta information.
        /// </summary>
        [SaveableField(SaveField.FIELD_PROTOBUF_OBJECT)]
        protected Dictionary<string, List<InputFunc>> inputFuncs;

        public Dictionary<string, List<InputFunc>> InputFuncs {
            get {
                return this.inputFuncs;
            }
        }
        
        [HideInInspector]
        [SerializeField]
        [SaveableField(SaveField.FIELD_PROTOBUF_OBJECT)]
        protected List<OutputFunc> outputs;
        public List<OutputFunc> Outputs {
            get {
                return this.outputs;
            }
            #if UNITY_EDITOR
            set {
                this.outputs = value;
            }
            #endif
        }


        public InputFunc GetInputFunc(string name, string component) {
            if (!inputFuncs.ContainsKey(component)) {
                return null;
            }

            for (int i = 0; i < inputFuncs[component].Count; ++i) {
                if (inputFuncs[component][i].Name == name) {
                    return inputFuncs[component][i];
                }
            }
            return null;
        }
        
        /// <summary>
        /// Links an editor callback name to a method inside this entity.
        /// This data is exposed for the I/O system.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="callback">Callback.</param>
        protected void DefineInputFunc(string name, string component, SimpleSignal callback) {
            if (!inputFuncs.ContainsKey(component)) {
                inputFuncs.Add(component, new List<InputFunc>(5));
            }
            this.inputFuncs[component].Add(new SimpleInputFunc (name, callback));
        }
        
        /// <summary>
        /// Links an editor callback name to a method inside this entity.
        /// This data is exposed for the I/O system.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="callback">Callback.</param>
        protected void DefineInputFunc(string name, string component, ParameterSignal callback) {
            if (!inputFuncs.ContainsKey(component)) {
                inputFuncs.Add(component, new List<InputFunc>(5));
            }
            this.inputFuncs[component].Add(new ParameterInputFunc(name, callback));
        }
        
        /// <summary>
        /// Declares an output event.
        /// InputFunc objects get attached to these events. They are identified by their names.
        /// Previously declared output events cannot be re-declared.
        /// </summary>
        /// <param name="name">Name.</param>
        protected void DefineOutput(string name) {
            foreach (var output in this.outputs) {
                if (output.Name == name) {
                    Debug.LogWarning(name + " is already declared as output. Will not re-declare.");
                    return;
                }
            }
            this.outputs.Add(new OutputFunc(name));
        }
        
        /// <summary>
        /// Calls the event registered by this name on the event dispatcher
        /// </summary>
        /// <param name="name">Name.</param>
        protected void FireOutput(string name, string component) {
            foreach (OutputFunc output in outputs) {
                if (output.Name == name) {
                    output.Invoke(component);
                    return;
                }
            }
        }

        /// <summary>
        /// Takes care of re-wiring the signals.
        /// </summary>
        public void RewireOutputs() {
            if (outputs == null) {
                outputs = new List<OutputFunc>();
                return;
            }
            for (int i = 0; i < outputs.Count; ++i) {
                if (outputs[i].Listeners == null) {
                    continue;
                }
                for (int j = 0; j < outputs[i].Listeners.Count; ++j) {
                    // Finds and re-connects the inputs for signal handlers in the scene
                    outputs[i].Listeners[j].ConnectInputs();
                }
            }
        }
    }
}

