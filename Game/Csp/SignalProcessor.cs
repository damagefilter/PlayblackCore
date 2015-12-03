using System;
using System.Collections.Generic;
using Fasterflect;
using Playblack.EventSystem;
using Playblack.EventSystem.Events;
using Playblack.Pooling;
using Playblack.Savegame;
using UnityEngine;

//#if UNITY_EDITOR
using UnityEditor;

//#endif

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
                if (this.inputFuncs == null) {
                    this.RebuildInputs();
                }
                return this.inputFuncs;
            }
        }

        [HideInInspector]
        [SerializeField]
        [SaveableField(SaveField.FIELD_PROTOBUF_OBJECT)]
        protected List<OutputFunc> outputs;

        public List<OutputFunc> Outputs {
            get {
                if (this.outputs == null) {
                    this.ReadOutputs();
                }
                return this.outputs;
            }
            #if UNITY_EDITOR
            set {
                this.outputs = value;
            }
            #endif
        }

        /// <summary>
        /// This cache is used to speed up the lookups for input funcs on components by remembering
        /// their type and all attached attributes to it.
        /// </summary>
        private static GenericObjectPoolMap<Type, InputFuncAttribute[]> inputFuncCache = new GenericObjectPoolMap<Type, InputFuncAttribute[]>(10, 50);


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
        private void RebuildInputs() {
            var components = GetComponents<Component>();
            if (inputFuncs == null) {
                inputFuncs = new Dictionary<string, List<InputFunc>>();
            }
            else {
                inputFuncs.Clear();
            }

            for (int i = 0; i < components.Length; ++i) {
                var type = components[i].GetType();
                var methods = type.MethodsWith(Flags.InstancePublic, typeof(InputFuncAttribute));
                InputFuncAttribute[] attribs = null;
                if (inputFuncCache.Has(type)) {
                    attribs = inputFuncCache.Get(type);

                }
                else {
                    var attribList = new List<InputFuncAttribute>(methods.Count / 2); // round about this number as init capacity
                    for (int j = 0; j < methods.Count; ++j) {
                        attribList.Add(methods[j].Attribute<InputFuncAttribute>());
                    }
                    attribs = attribList.ToArray();
                    inputFuncCache.Add(type, attribs); // next time a processor sees this component, all the lookup reflection will be spared
                }
                for (int j = 0; j < attribs.Length; ++j) {
                    try {
                        if (attribs[j].WithParameter) {
                            var del = (ParameterSignal)Delegate.CreateDelegate(typeof(ParameterSignal), components[i], attribs[j].MethodName);
                            DefineInputFunc(attribs[j].DisplayName, type.ToString(), del);
                        }
                        else {
                            var del = (SimpleSignal)Delegate.CreateDelegate(typeof(SimpleSignal), components[i], attribs[j].MethodName);
                            DefineInputFunc(attribs[j].DisplayName, type.ToString(), del);
                        }
                    }
                    catch (Exception e) {
                        Debug.LogError(
                            "You defined an inputfunc (" + attribs[j].MethodName + ") on " + type + " that is not applicable to the CSP.\n" +
                            e.Message + "\n" +
                            e.StackTrace
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Reads all known outputs from all components on the game object.
        /// Beware: Calling this causes data loss on outputs that got renamed!
        /// Should be called from an editor only.
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

                var attribs = (OutputAwareAttribute[])t.GetCustomAttributes(typeof(OutputAwareAttribute), true);
                for (int j = 0; j < attribs.Length; ++j) {
                    newOutputList.AddRange(attribs[j].Outputs);
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
            this.inputFuncs[component].Add(new SimpleInputFunc(name, callback));
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
        /// Makes the signal handler fire an output with the given name.
        /// This will trigger all outputs in all components filed under this name.
        /// This will also reach deactivated game objects.
        /// </summary>
        /// <param name="name">Name.</param>
        protected void InternalFireOutput(string name) {
            for (int i = 0; i < outputs.Count; ++i) {
                if (outputs[i].Name == name) {
                    outputs[i].Invoke();
                    return;
                }
            }
        }

        /// <summary>
        /// Runs through all known output signals and hooks up the linking information to real gameobjects in the scene.
        /// This usually must be called after loading a game or to clean up linking information.
        /// 
        /// NOTE: On a clean / default scene this data gets fed in by unity and will be correct.
        /// This is only required after loading a save game.
        /// </summary>
        private void ConnectOutputSignals() {
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
                    outputs[i].Listeners[j].FindTargetProcessors();
                }
            }
        }

        private void OnSaveGameLoaded(SaveGameLoadedEvent hook) {
            RebuildInputs();
            ConnectOutputSignals();
        }

        #region Unity Related

        public void Awake() {
            // SignalProcessorTracker.Instance.Track(this);
            EventDispatcher.Instance.Register<SaveGameLoadedEvent>(OnSaveGameLoaded);
            RebuildInputs();
        }

        public void OnDestroy() {
            // SignalProcessorTracker.Instance.Untrack(this);
            EventDispatcher.Instance.Unregister<SaveGameLoadedEvent>(OnSaveGameLoaded);
        }

        #endregion

        #if UNITY_EDITOR
        public virtual void OnDrawGizmos() {
            if (outputs == null || outputs.Count <= 0) {
                return;
            }

            Handles.color = (Color)new Color32(240, 40, 16, 255);
            Gizmos.color = (Color)new Color32(240, 40, 16, 255);
            var labelPos = this.transform.position;

            labelPos.y += 1f;

            Handles.Label(labelPos, this.name);
            for (int i = 0; i < outputs.Count; ++i) {
                if (outputs[i].Listeners == null) {
                    continue; // can be null after deserialization
                }
//                foreach (var listener in output.Listeners) {
                for (int j = 0; j < outputs[i].Listeners.Count; ++j) {
                    bool needsEntityCleaning = false;
                    if (outputs[i].Listeners[j] == null || outputs[i].Listeners[j].matchedProcessors == null) {
                        continue;
                    }
                    for (int k = 0; k < outputs[i].Listeners[j].matchedProcessors.Count; ++k) {
                        if (outputs[i].Listeners[j].matchedProcessors[k] == null) {
                            needsEntityCleaning = true;
                            continue;
                        }
                        Gizmos.color = (Color)new Color32(240, 40, 16, 255);
                        // Draw a connection line
                        var targetPos = outputs[i].Listeners[j].matchedProcessors[k].transform.position;
                        Gizmos.DrawLine(this.transform.position, targetPos);

                        // Prepare the orientation for a cone cap 
                        // to display in which direction the connection goes
                        var direction = targetPos - this.transform.position;
                        // This may or may not work
                        if (UnityEditor.EditorSettings.defaultBehaviorMode == UnityEditor.EditorBehaviorMode.Mode2D) {
                            direction.z = 0f; // this is only needed for 2D where z is just confusing the direction
                        }

                        var rot = Quaternion.LookRotation(direction);

                        Ray r = new Ray(this.transform.position, direction);
                        var pos = r.GetPoint(direction.magnitude - 0.5f);
                        Handles.ConeCap(0, pos, rot, 0.3f);

                    }
                    if (needsEntityCleaning) {
                        var list = new List<SignalProcessor>();
                        // TODO: Better way to clean null refs?
                        for (int k = 0; k < outputs[i].Listeners[j].matchedProcessors.Count; k++) {
                            if (outputs[i].Listeners[j].matchedProcessors[k] == null) {
                                continue;
                            }
                            list.Add(outputs[i].Listeners[j].matchedProcessors[k]);
                        }

                        outputs[i].Listeners[j].matchedProcessors = list;
                    }
                }
            }
        }
        #endif
    }
}

