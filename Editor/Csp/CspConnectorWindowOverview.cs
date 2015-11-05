using System;
using UnityEngine;
using UnityEditor;
using Playblack.Csp;
using Playblack.Pooling;

namespace Playblack.Editor.Csp {
    /// <summary>
    /// Shows a list of existing  connections outgoing from the given signal processor.
    /// Also offers functionality to create new connections, edit / remove existing ones.
    /// </summary>
    public class CspConnectorWindowOverview : EditorWindow {

        private static GenericObjectPoolMap<string, SignalDataCache> dataCache = new GenericObjectPoolMap<string, SignalDataCache>(5, 8);
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
                EditorGUILayout.HelpBox("Use this button only if outputs seem to be missing or wrong!", MessageType.Info);
                if (GUILayout.Button("Re-Scan outputs")) {
                    var result = EditorUtility.DisplayDialog("Re-Scan outputs", "Do you really want to do this? Connections MIGHT be lost during the process!", "I do", "Better not");
                    if (result) {
                        processor.ReadOutputs();
                    }
                }
                EditorGUILayout.HelpBox(
                    "[b]Output: [/b]The signal that is raised from one of the GameObjects components.\n" +
                    "[b]Target: [/b]The target GameObject with a SignalProcessor on it\n" +
                    "[b]Input: [/b]The method that is called on the target object.\n" +
                    "[b]Parameters: [/b]If Input allows parameters, specify them here\n" +
                    "[b]Delay: [/b]Should be called with this delay in seconds.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Output", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("Target", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("With Input", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("Parameters", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("Delay", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("Edit", GUILayout.Width(listFieldSize));
                    EditorGUILayout.LabelField("Delete", GUILayout.Width(listFieldSize));
                }
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < processor.Outputs.Count; ++i) {
                    var outp = processor.Outputs[i];
                    DrawOutput(ref outp); // TODO: Will that do or must we re-assign shit?
                }
                DrawAddForm();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawOutput(ref OutputFunc output) {
            for (int i = 0; i < output.Listeners.Count; ++i) {
                string cacheKey = processor.name + output.Listeners[i].processorName; // this is local processor and target processor names
                SignalDataCache data = null;
                if (!dataCache.Has(cacheKey)) {
                    data = new SignalDataCache(processor, output.Listeners[i]);
                    dataCache.Add(cacheKey, data);
                }
                else {
                    data = dataCache.Get(cacheKey);
                }
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(output.Name, GUILayout.Width(listFieldSize));
                    // Target processors
                    string oldTarget = output.Listeners[i].processorName;
                    string newTarget = EditorGUILayout.TextField(output.Listeners[i].processorName, GUILayout.Width(listFieldSize));
                    if (oldTarget != newTarget) {
                        dataCache.Remove(oldTarget); // uncache junk.
                        output.Listeners[i].processorName = newTarget; // will be re-cached and processed next time
                    }
                    // Select which input. needs 2 dropdowns because of component selection.
                    EditorGUILayout.BeginVertical();
                    {
                        // Input on processors: Component selection
                        int componentIndex = EditorGUILayout.Popup(data.GetComponentIndex(output.Listeners[i].component), data.GetComponentList(), GUILayout.Width(listFieldSize));
                        string componentName = data.GetComponentName(componentIndex);
                        if (componentName != output.Listeners[i].component) {
                            output.Listeners[i].component = componentName;
                            output.Listeners[i].method = null; // Reset method, the old one will very likely not apply anymore
                        }

                        // Input on processors: Input Method selection
                        int inputIndex = EditorGUILayout.Popup(data.GetInputIndex(componentName, output.Listeners[i].method), data.GetInputList(componentName), GUILayout.Width(listFieldSize));
                        string inputName = data.GetInputName(componentName, inputIndex);
                        if (inputName != output.Listeners[i].method) {
                            output.Listeners[i].method = inputName;
                        }
                    }
                    EditorGUILayout.EndVertical();

                    output.Listeners[i].param = EditorGUILayout.TextField(output.Listeners[i].param, GUILayout.Width(listFieldSize));
                    output.Listeners[i].delay = EditorGUILayout.FloatField(output.Listeners[i].delay, GUILayout.Width(listFieldSize));
                    if (GUILayout.Button("Edit", GUILayout.Width(listFieldSize))) {
                        // Open Edit popup
                    }

                    if (GUILayout.Button("X", GUILayout.Width(listFieldSize))) {
                        // Schedule listener for removal
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private OutputConfig outputCfg;
        private void DrawAddForm() {
            EditorGUILayout.BeginVertical();
            {
                int newOutputIndex = EditorGUILayout.Popup("On Output", outputCfg.outputIndex, this.outputs);
                if (newOutputIndex != outputCfg.outputIndex) {
                    outputCfg.outputIndex = newOutputIndex;
                }
                outputCfg.targetName = EditorGUILayout.TextField("Target Name", outputCfg.targetName);
                if (GUILayout.Button("Add")) {
                    for (int i = 0; i < processor.Outputs.Count; ++i) {
                        processor.Outputs[i].AttachInput(outputCfg.targetName, null, null, 0f);
                    }
                }
            }
            EditorGUILayout.EndVertical();

        }

        
    }

    struct OutputConfig {
        public int outputIndex, componentIndex, inputIndex;
        public string targetName;
        public SignalDataCache cache;

        public void GenerateCacheData(SignalProcessor localProcessor, OutputEventListener l) {
            cache = new SignalDataCache(localProcessor, l);
        }
    }


}

