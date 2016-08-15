using Playblack.Csp;
using Playblack.Pooling;
using System;
using UnityEditor;
using UnityEngine;

namespace Playblack.Editor.Csp {

    /// <summary>
    /// Shows a list of existing  connections outgoing from the given signal processor.
    /// Also offers functionality to create new connections, edit / remove existing ones.
    /// </summary>
    public class CspConnectorWindowOverview : EditorWindow {
        private GenericObjectPoolMap<string, SignalDataCache> dataCache = new GenericObjectPoolMap<string, SignalDataCache>(5, 8);
        private const int listFieldSize = 150;

        private SignalProcessor processor;
        private string[] outputs;

        // NOTE: the editor window has all these output managing and not the SIgnalDataCache objects
        // because if they were we'd have a lot of redundant data flying around the memory.
        // Just saying
        private int GetOutputIndex(string outputName) {
            return Array.IndexOf(this.outputs, outputName);
        }

        private string GetOutputName(int index) {
            if (index < this.outputs.Length && index >= 0) {
                return this.outputs[index];
            }
            return null;
        }

        public void Prepare(SignalProcessor processor) {
            if (processor == null) {
                Debug.Log("Passed SignalProcessor was null or empty...");
                return;
            }
            this.processor = processor;

            this.outputs = new string[processor.Outputs.Count];
            for (int i = 0; i < processor.Outputs.Count; ++i) {
                this.outputs[i] = processor.Outputs[i].Name;
            }
            this.dataCache.Clear();
        }

        public void OnGUI() {
            if (processor == null) {
                EditorGUILayout.HelpBox("No Signal Processor is selected.", MessageType.Info);
            }
            else {
                DrawExistingConnections();
            }
        }

        private void DrawExistingConnections() {
            // Draws connections for each output:

            // read outputs button (with warning)
            // list of existing outputs that are wired up with edit button (opens popup)
            // form to add new output
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.HelpBox(
                    "Output: The signal that is raised from one of the GameObjects components.\n" +
                    "Target: The target GameObject with a SignalProcessor on it\n" +
                    "Input: The method that is called on the target object.\n" +
                    "Parameters: If Input allows parameters, specify them here\n" +
                    "Delay: Should be called with this delay in seconds.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Output", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("Target", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("With Input", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("Parameters", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("Delay", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("Delete", GUILayout.Width(listFieldSize));
                }
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < processor.Outputs.Count; ++i) {
                    var outp = processor.Outputs[i];
                    DrawOutput(ref outp);
                    processor.Outputs[i] = outp;
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                DrawAddForm();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawOutput(ref OutputFunc output) {
            int toRemove = -1;
            for (int i = 0; i < output.Listeners.Count; ++i) {
                string cacheKey = processor.name + output.Listeners[i].targetProcessorName;
                SignalDataCache data = null;
                if (!dataCache.Has(cacheKey)) {
                    data = new SignalDataCache(processor, output.Listeners[i]);
                    dataCache.Add(cacheKey, data);
                }
                else {
                    data = dataCache.Get(cacheKey);
                    if (data.GetComponentList() == null || data.GetComponentList().Length == 0) {
                        // stale data through scene switches and such. rebuild.
                        Debug.Log("Rebuilding cached data for " + cacheKey);
                        dataCache.Remove(cacheKey);
                        data = new SignalDataCache(processor, output.Listeners[i]);
                        dataCache.Add(cacheKey, data);
                    }
                }
                
                dataCache.PutBack(cacheKey); // cause if not, next time we get a null back. Should think about not using the InUse stuff
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(output.Name, GUILayout.Width(listFieldSize));
                    // Target processors
                    string oldTarget = output.Listeners[i].targetProcessorName;
                    string newTarget = EditorGUILayout.TextField(oldTarget, GUILayout.Width(listFieldSize));
                    if (oldTarget != newTarget) {
                        dataCache.Remove(cacheKey); // uncache junk.
                        output.Listeners[i].targetProcessorName = newTarget; // will be re-cached and processed next time
                        Debug.Log("Target changed!");
                        return;
                    }
                    if (data == null) {
                        EditorGUILayout.LabelField("Target is null", GUILayout.Width(listFieldSize));
                        EditorGUILayout.LabelField("No parameter on null target", GUILayout.Width(listFieldSize));
                        EditorGUILayout.LabelField("No delay on null target", GUILayout.Width(listFieldSize));
                    }
                    else if (data.GetComponentList() == null || data.GetComponentList().Length == 0) {
                        // That means we need to rebuild it since it's all empty!
                        EditorGUILayout.LabelField("No input data on target", GUILayout.Width(listFieldSize));
                        EditorGUILayout.LabelField("No parameter on input", GUILayout.Width(listFieldSize));
                        EditorGUILayout.LabelField("No target, no delay", GUILayout.Width(listFieldSize));
                    }
                    else if (output.Listeners[i] == null) {
                        EditorGUILayout.LabelField("No output listeners target", GUILayout.Width(listFieldSize));
                        EditorGUILayout.LabelField("No parameter on input", GUILayout.Width(listFieldSize));
                        EditorGUILayout.LabelField("No target, no delay", GUILayout.Width(listFieldSize));
                    }
                    else {
                        // Select which input. needs 2 dropdowns because of component selection.

                        // Input on processors: Component selection
                        int componentIndex = EditorGUILayout.Popup(data.GetComponentIndex(output.Listeners[i].component), data.GetComponentList(), GUILayout.Width(listFieldSize / 2.05f));
                        string componentName = data.GetComponentName(componentIndex);
                        if (componentName != output.Listeners[i].component) {
                            output.Listeners[i].component = componentName;
                            var inMethods = data.GetInputList(componentName);
                            if (inMethods == null || inMethods.Length == 0) {
                                output.Listeners[i].method = null; // Reset method, the old one will very likely not apply anymore
                            }
                            else {
                                output.Listeners[i].method = inMethods[0]; // Take first we know
                            }
                        }
                        
                        if (string.IsNullOrEmpty(componentName)) {
                            EditorGUILayout.LabelField("Invalid component", GUILayout.Width(listFieldSize));
                        }
                        else {
                            // Input on processors: Input Method selection
                            var currentIndex = (output.Listeners[i].method != null ? data.GetInputIndex(componentName, output.Listeners[i].method) : 0);
                            int inputIndex = EditorGUILayout.Popup(currentIndex, data.GetInputList(componentName), GUILayout.Width(listFieldSize / 2.05f));
                            string inputName = data.GetInputName(componentName, inputIndex);
                            if (inputName != output.Listeners[i].method) {
                                output.Listeners[i].method = inputName;
                            }

                            if (output.Listeners[i].HasParameter(componentName)) {
                                output.Listeners[i].param = EditorGUILayout.TextField(output.Listeners[i].param, GUILayout.Width(listFieldSize));
                            }
                            else {
                                EditorGUILayout.LabelField("No parameter on input", GUILayout.Width(listFieldSize));
                            }
                            output.Listeners[i].delay = EditorGUILayout.FloatField(output.Listeners[i].delay, GUILayout.Width(listFieldSize));
                        }
                    }

                    if (GUILayout.Button("X", GUILayout.Width(listFieldSize))) {
                        // Schedule listener for removal
                        toRemove = i;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (toRemove >= 0) {
                    output.DetachAtIndex(i);
                    toRemove = -1;
                }
            }
        }

        private OutputConfig outputCfg;

        private void DrawAddForm() {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(listFieldSize * 3)); // half the space of the things above will do
            {
                EditorGUILayout.BeginVertical();
                {
                    int newOutputIndex = EditorGUILayout.Popup("On Output", outputCfg.outputIndex, this.outputs);
                    if (newOutputIndex != outputCfg.outputIndex) {
                        outputCfg.outputIndex = newOutputIndex;
                    }

                    outputCfg.targetName = EditorGUILayout.TextField("Target Name", outputCfg.targetName);
                    if (GUILayout.Button("Add")) {
                        string outputName = GetOutputName(outputCfg.outputIndex);
                        for (int i = 0; i < processor.Outputs.Count; ++i) {
                            if (processor.Outputs[i].Name == outputName) {
                                processor.Outputs[i].AttachInput(outputCfg.targetName, null, null, 0f, processor);
                                outputCfg = new OutputConfig(); // reset stuffs
                                break;
                            }
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Use this button if outputs seem to be missing or wrong", MessageType.Info);
                    if (GUILayout.Button("Re-Scan outputs")) {
                        var result = EditorUtility.DisplayDialog("Re-Scan outputs", "Do you really want to do this? Connections MIGHT be lost during the process!", "I do", "Better not");
                        if (result) {
                            processor.ReadOutputs();
                            this.outputs = new string[processor.Outputs.Count];
                            for (int i = 0; i < processor.Outputs.Count; ++i) {
                                this.outputs[i] = processor.Outputs[i].Name;
                            }
                        }
                    }
                    EditorUtility.SetDirty(processor);
                    if (GUILayout.Button("Force-Save")) {
                        EditorApplication.SaveScene();
                        
                        Debug.Log("Saved");
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    internal struct OutputConfig {
        public int outputIndex;
        public string targetName;
        public SignalDataCache cache;

        public void GenerateCacheData(SignalProcessor localProcessor, OutputEventListener l) {
            cache = new SignalDataCache(localProcessor, l);
        }
    }
}