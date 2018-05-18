using Playblack.Csp;
using Playblack.Pooling;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Playblack.Editor.Csp {

    /// <summary>
    /// Shows a list of existing  connections outgoing from the given signal processor.
    /// Also offers functionality to create new connections, edit / remove existing ones.
    /// </summary>
    public class CspConnectorWindowOverview : EditorWindow {
        private GenericObjectPoolMap<string, SignalDataCache> dataCache = new GenericObjectPoolMap<string, SignalDataCache>(5, 8);
        private const int ListFieldSize = 150;

        private SignalProcessor processor;
        private SignalProcessor previousTarget;
        private string[] outputs;

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
            Undo.RecordObject(this.processor, "Edit Signal Processor");
        }

        public void SetPreviousTarget(SignalProcessor signalProcessor) {
            this.previousTarget = signalProcessor;
        }

        public void OnGUI() {
            if (processor == null) {
                EditorGUILayout.HelpBox("No Signal Processor is selected.", MessageType.Info);
            }
            else {
                DrawExistingConnections();
            }
        }

        private void InvalidateDataCache() {

            this.dataCache.Clear();
        }

        private void OnDestroy() {
            EditorUtility.SetDirty(processor);
        }

        private void DrawExistingConnections() {
            // Draws connections for each output:

            // read outputs button (with warning)
            // list of existing outputs that are wired up with edit button (opens popup)
            // form to add new output
            EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Button("Previous Target", GUILayout.Width(ListFieldSize))) {
                    if (previousTarget != null) {
                        Selection.activeGameObject = previousTarget.gameObject;
                        SceneView.lastActiveSceneView.LookAt(Selection.activeGameObject.transform.position);
                        SignalProcessorInspector.OpenCspEditorWindow(previousTarget, processor);

                    }
                }
                EditorGUILayout.HelpBox(
                    "Target: The target GameObject with a SignalProcessor on it\n" +
                    "Input: The method that is called on the target object.\n" +
                    "Parameters: If Input allows parameters, specify them here\n" +
                    "Delay: Should be called with this delay in seconds.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Target", GUILayout.Width(ListFieldSize));
                    EditorGUILayout.LabelField("With Input", GUILayout.Width(ListFieldSize));
                    EditorGUILayout.LabelField("Parameters", GUILayout.Width(ListFieldSize));
                    EditorGUILayout.LabelField("Delay", GUILayout.Width(ListFieldSize));
                    EditorGUILayout.LabelField("To Target", GUILayout.Width(ListFieldSize/3f));
                    EditorGUILayout.LabelField("Rebuild Inputs", GUILayout.Width(ListFieldSize/3f));
                    EditorGUILayout.LabelField("Delete", GUILayout.Width(ListFieldSize/3f));
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
            SignalProcessor jumpTarget = null;

            if (output.Listeners.Count == 0) {
                return;
            }
            EditorGUILayout.LabelField(output.Name, EditorStyles.boldLabel, GUILayout.Width(ListFieldSize));
            for (int i = 0; i < output.Listeners.Count; ++i) {
                string cacheKey = processor.name + output.Listeners[i].targetProcessorName;
                SignalDataCache data;
                if (!dataCache.Has(cacheKey)) {
                    data = new SignalDataCache(processor, output.Listeners[i]);
                    dataCache.Add(cacheKey, data);
                }
                else {
                    data = dataCache.Get(cacheKey);
                    if (data.GetComponentList() == null || data.GetComponentList().Length == 0) {
                        // stale data through scene switches and such. rebuild.
                        Debug.Log("Rebuilding cache for " + cacheKey);
                        dataCache.Remove(cacheKey);
                        data = new SignalDataCache(processor, output.Listeners[i]);
                        dataCache.Add(cacheKey, data);
                    }
                }
                
                dataCache.PutBack(cacheKey); // cause if not, next time we get a null back. Should think about not using the InUse stuff
                EditorGUILayout.BeginHorizontal();
                {
                    // Target processors
                    string oldTarget = output.Listeners[i].targetProcessorName;
                    string newTarget = EditorGUILayout.TextField(oldTarget, GUILayout.Width(ListFieldSize));
                    if (oldTarget != newTarget) {
                        dataCache.Remove(cacheKey); // uncache junk.
                        output.Listeners[i].targetProcessorName = newTarget; // will be re-cached and processed next time
                        Debug.Log("Target changed. Rebuilding matched processor list.");
                        output.Listeners[i].FindTargetProcessors(this.processor);
                        return;
                    }
                    if (data == null) {
                        EditorGUILayout.LabelField("Target is null", GUILayout.Width(ListFieldSize));
                        EditorGUILayout.LabelField("No parameter on null target", GUILayout.Width(ListFieldSize));
                        EditorGUILayout.LabelField("No delay on null target", GUILayout.Width(ListFieldSize));
                        EditorGUILayout.LabelField("No target to go to", GUILayout.Width(ListFieldSize/3f));
                        EditorGUILayout.LabelField("Nothing to clear", GUILayout.Width(ListFieldSize/3f));
                    }
                    else if (data.GetComponentList() == null || data.GetComponentList().Length == 0) {
                        // That means we need to rebuild it since it's all empty!
                        EditorGUILayout.LabelField("No input data on target", GUILayout.Width(ListFieldSize));
                        EditorGUILayout.LabelField("No parameter on input", GUILayout.Width(ListFieldSize));
                        EditorGUILayout.LabelField("No target, no delay", GUILayout.Width(ListFieldSize));
                        EditorGUILayout.LabelField("No target to go to", GUILayout.Width(ListFieldSize/3f));
                        EditorGUILayout.LabelField("Nothing to clear", GUILayout.Width(ListFieldSize/3f));
                    }
                    else if (output.Listeners[i] == null) {
                        EditorGUILayout.LabelField("No output listeners target", GUILayout.Width(ListFieldSize));
                        EditorGUILayout.LabelField("No parameter on input", GUILayout.Width(ListFieldSize));
                        EditorGUILayout.LabelField("No target, no delay", GUILayout.Width(ListFieldSize));
                        EditorGUILayout.LabelField("No target to go to", GUILayout.Width(ListFieldSize/3f));
                        EditorGUILayout.LabelField("Nothing to clear", GUILayout.Width(ListFieldSize/3f));
                    }
                    else {
                        // Select which input. needs 2 dropdowns because of component selection.

                        // Input on processors: Component selection
                        int componentIndex = EditorGUILayout.Popup(data.GetComponentIndex(output.Listeners[i].component), data.GetComponentList(), GUILayout.Width(ListFieldSize / 2.05f));
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
                            EditorGUILayout.LabelField("Invalid component", GUILayout.Width(ListFieldSize));
                        }
                        else {
                            // Input on processors: Input Method selection
                            var currentIndex = (output.Listeners[i].method != null ? data.GetInputIndex(componentName, output.Listeners[i].method) : 0);
                            int inputIndex = EditorGUILayout.Popup(currentIndex, data.GetInputList(componentName), GUILayout.Width(ListFieldSize / 2.05f));
                            string inputName = data.GetInputName(componentName, inputIndex);
                            if (inputName != output.Listeners[i].method) {
                                output.Listeners[i].method = inputName;
                            }

                            if (output.Listeners[i].HasParameter(componentName)) {
                                output.Listeners[i].param = EditorGUILayout.TextField(output.Listeners[i].param, GUILayout.Width(ListFieldSize));
                            }
                            else {
                                EditorGUILayout.LabelField("No parameter on input", GUILayout.Width(ListFieldSize));
                            }
                            output.Listeners[i].delay = EditorGUILayout.FloatField(output.Listeners[i].delay, GUILayout.Width(ListFieldSize));
                            if (GUILayout.Button("=>", GUILayout.Width(ListFieldSize/3f))) {
                                if (output.Listeners[i].matchedProcessors.Count > 0) {
                                    jumpTarget = output.Listeners[i].matchedProcessors[0];
                                }
                            }
                            if (GUILayout.Button("C", GUILayout.Width(ListFieldSize/3f))) {
                                if (output.Listeners[i].matchedProcessors.Count > 0) {
                                    var procs = output.Listeners[i].matchedProcessors;
                                    for (int j = 0; j < procs.Count; ++j) {
                                        procs[j].RebuildInputs();
                                    }
                                    dataCache.Clear();
                                }
                            }
                        }
                    }

                    if (GUILayout.Button("X", GUILayout.Width(ListFieldSize/3))) {
                        // Schedule listener for removal
                        toRemove = i;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (toRemove >= 0) {
                output.DetachAtIndex(toRemove);
            }
            if (jumpTarget != null) {
                Selection.activeGameObject = jumpTarget.gameObject;
                SceneView.lastActiveSceneView.LookAt(Selection.activeGameObject.transform.position);
                SignalProcessorInspector.OpenCspEditorWindow(jumpTarget, processor);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        private OutputConfig outputCfg;

        private void DrawAddForm() {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(ListFieldSize * 3)); // half the space of the things above will do
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
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Use this button if the components dropdown is wonky.", MessageType.Info);
                    if (GUILayout.Button("Invalidate Signal Cache")) {
                        var result = EditorUtility.DisplayDialog("Invalidate Signal Cache", "Are you sure?", "Do it!", "Nope, return!");
                        if (result) {
                            InvalidateDataCache();
                        }
                        EditorUtility.SetDirty(processor);
                    }
                    if (GUILayout.Button("Force-Save")) {
                        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

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
