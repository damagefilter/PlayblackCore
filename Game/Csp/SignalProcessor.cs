using System;
using UnityEngine;
using System.Collections.Generic;
using Playblack.Savegame;

namespace Playblack.Csp {

    public delegate void SimpleSignal();
    public delegate void ParameterSignal(string param);

    /// <summary>
    /// Tracks and manages signals coming in and out of the gameobject this handler is attached to.
    /// </summary>
    [DisallowMultipleComponent]
    [SaveableComponent]
    public class SignalProcessor : MonoBehaviour {
        /// <summary>
        /// Signals this Entity can receive and process.
        /// This list represents volatile information and needs to be rebuilt
        /// manually each time a signal processor is instantiated
        /// </summary>
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
            if (inputFuncs == null) {
                this.RebuildInputs();
            }
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
        /// Call this to rebuild the input funcs.
        /// This needs to be called everytime the signalprocessor object is instantiated.
        /// The data is used in OutputEvent. It will look in a SignalProcessors input-list
        /// for things to call when an output is fired.
        /// </summary>
        public void RebuildInputs() {
            var components = GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i) {
                var methods = components[i].GetType().GetMethods();
                for (int j = 0; j < methods.Length; ++j) {
                    if (!methods[j].IsDefined(typeof(InputFuncAttribute), true)) {
                        continue;
                    }
                    try {
                        var attrib = (InputFuncAttribute)methods[j].GetCustomAttributes(typeof(InputFuncAttribute), true)[0];
                        if (attrib.WithParameter) {
                            var del = (ParameterSignal)Delegate.CreateDelegate(typeof(ParameterSignal), methods[i], attrib.MethodName);
                            DefineInputFunc(attrib.DisplayName, methods[j].GetType().ToString(), del);
                        }
                        else {
                            var del = (SimpleSignal)Delegate.CreateDelegate(typeof(SimpleSignal), methods[i], attrib.MethodName);
                            DefineInputFunc(attrib.DisplayName, methods[j].GetType().ToString(), del);
                        }
                    }
                    catch (Exception e) {
                        Debug.LogError(
                            "You defined an inputfunc on " + methods[j].GetType() + " that is not applicable to the CSP.\n" + 
                            e.Message
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Reads all known outputs from all components on the game object.
        /// Beware: Calling this causes data loss on outputs that got renamed!
        /// </summary>
        public void ReadOutputs() {
            var newOutputList = new List<string>();

            var components = GetComponents<Component>();
            // First: Read out all outputs
            for (int i = 0; i < components.Length; ++i) {
                var t = components[i].GetType();
                if (!t.IsDefined(typeof(OutputAwareAttribute), true)) {
                    continue;
                }
                // Maybe find something that doesn't require 3 nested loops ...
                var attribs = (OutputAwareAttribute[])t.GetCustomAttributes(typeof(OutputAwareAttribute), true);
                for (int j = 0; j < attribs.Length; ++j) {
                    newOutputList.AddRange(attribs[j].OutputGetter);
                }
            }
            if (outputs != null) {
                var toRemove = new List<OutputFunc>();
                // Clear out things that do not exist anymore.
                for (int i = 0; i < outputs.Count; ++i) {
                    if (!newOutputList.Contains(outputs[i].Name)) {
                        // Doesn't exist anymore.
                        toRemove.Add(outputs[i]);
                    }
                }
                for (int i = 0; i < toRemove.Count; ++i) {
                    outputs.Remove(toRemove[i]);
                }
            }
            else {
                outputs = new List<OutputFunc>();
            }
            for (int i = 0; i < newOutputList.Count; ++i) {
                DefineOutput(newOutputList[i]);
            }
        }
        
        /// <summary>
        /// Links an editor callback name to a method inside this entity.
        /// This data is exposed for the I/O system.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name = "component"></param>
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
        /// <param name = "component"></param>
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
            for (int i = 0; i < outputs.Count; ++i) {
                if (outputs[i].Name == name) {
                    Debug.LogWarning(name + " is already declared as output. Calls to it will be grouped into the existing version!");
                    return;
                }
            }
            this.outputs.Add(new OutputFunc(name));
        }
        
        /// <summary>
        /// Makes the signal handler fire an output with the given name and the given component.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name = "component"></param>
        protected void FireOutput(string name, string component) {
            foreach (OutputFunc output in outputs) {
                if (output.Name == name) {
                    output.Invoke(component);
                    return;
                }
            }
        }

        /// <summary>
        /// Runs through all known output signals and hooks up the linking information to real gameobjects in the scene.
        /// This usually must be called after loading a game or to clean up linking information.
        /// TODO: In Savegames we need an event that is thrown after the scene is rebuilt completely.
        /// Only then can we reliably re-connect all signals because there is not enough tracking information
        /// before that point.
        /// </summary>
        public void ConnectSignals() {
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
                    outputs[i].Listeners[j].ConnectSignalProcessors();
                }
            }
        }

        #region Unity Related
        public void Awake() {
            SignalProcessorTracker.Instance.Track(this);
            ReadOutputs(); //Must happen
        }

        public void OnDestroy() {
            SignalProcessorTracker.Instance.Untrack(this);
        }
        #endregion
    }
}

