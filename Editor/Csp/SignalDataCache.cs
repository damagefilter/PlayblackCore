using Playblack.Csp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Playblack.Editor.Csp {

    /// <summary>
    /// Helper object to cache signal data for a handler.
    /// Used to speed up the signal editor and make it less inefficient.
    /// </summary>
    public class SignalDataCache {
        private Dictionary<string, string[]> inputMapping; // must be arrays because they are used from the unity GUI which does not work on lists
        private string[] componentList;
        private OutputEventListener listener;

        public SignalDataCache(SignalProcessor localProcessor, OutputEventListener listener) {
            this.listener = listener;
            RecreateDataCache();

        }

        public void RecreateDataCache() {
            // Generates data.
            // This is a more or lesss expensive task which is why the results are stored for later re-use.
            listener.matchedProcessors.Clear();
            // expensive but it's not used THAT often
            Dictionary<string, List<string>> inputMap = new Dictionary<string, List<string>>();
            if (!string.IsNullOrEmpty(listener.targetProcessorName)) { // check if target processor name is okay
                GenerateInputMap(listener, inputMap);
            }

            BindInputMap(inputMap);
        }

        private void BindInputMap(Dictionary<string, List<string>> inputMap) {
            this.inputMapping = new Dictionary<string, string[]>(inputMap.Count);
            componentList = new string[inputMap.Count];
            int k = 0;
            // This is a bit of a waste but apparently it has to happen, after all.
            // Unitys popup elements don't support lists
            foreach (var kvp in inputMap) {
                inputMapping.Add(kvp.Key, kvp.Value.ToArray());
                componentList[k] = kvp.Key;
                ++k;
            }
        }

        private static void GenerateInputMap(OutputEventListener listener, Dictionary<string, List<string>> inputMap) {
            var hits = UnityEngine.Object.FindObjectsOfType<SignalProcessor>();
            for (int i = 0; i < hits.Length; ++i) {
                if (!hits[i].name.StartsWith(listener.targetProcessorName, StringComparison.Ordinal)) {
                    continue;
                }

                listener.matchedProcessors.Add(hits[i]);
                if (hits[i].InputFuncs.Count == 0) {
                    UnityEngine.Debug.LogError("No inputfunc components on processor " + hits[i].name);
                }

                foreach (var kvp in hits[i].InputFuncs) {
                    if (!inputMap.ContainsKey(kvp.Key)) {
                        inputMap.Add(kvp.Key, new List<string>());
                    }

                    for (int j = 0; j < kvp.Value.Count; ++j) {
                        inputMap[kvp.Key].Add(kvp.Value[j].Name);
                    }
                }
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("##### COMPONENTS");
            foreach (var str in componentList) {
                sb.AppendLine(str);
            }
            sb.AppendLine("##### MATCHED PROCESSORS");
            foreach (var kvp in inputMapping) {
                sb.AppendLine(kvp.Key);
            }
            return sb.ToString();
        }

        #region Inputs

        /// <summary>
        /// Get the index number of the input name from the given component.
        /// Used for editor popup fields.
        /// </summary>
        /// <returns>The input index.</returns>
        /// <param name="component">Component.</param>
        /// <param name="inputName">Input name.</param>
        public int GetInputIndex(string component, string inputName) {
            return (component == null || !inputMapping.ContainsKey(component) || inputName == null) ? 0 : Array.IndexOf(inputMapping[component], inputName);
        }

        /// <summary>
        /// Get the name of an input based on its index and the given component.
        /// </summary>
        /// <returns>The input name at index.</returns>
        /// <param name="component">Component.</param>
        /// <param name="inputIndex">Input index.</param>
        public string GetInputName(string component, int inputIndex) {
            if (component == null || !inputMapping.ContainsKey(component)) {
                return null;
            }
            if (inputIndex < inputMapping[component].Length && inputIndex >= 0) {
                return inputMapping[component][inputIndex];
            }
            return null;
        }

        /// <summary>
        /// Get the list of all inputs for the given component.
        ///
        /// </summary>
        /// <returns>The input list.</returns>
        /// <param name="component">Component.</param>
        public string[] GetInputList(string component) {
            return (component == null || !inputMapping.ContainsKey(component)) ? null : inputMapping[component];
        }

        #endregion Inputs

        #region Components

        public int GetComponentIndex(string component) {
            return component == null ? 0 : Array.IndexOf(this.componentList, component);
        }

        public string GetComponentName(int componentIndex) {
            if (componentIndex < componentList.Length && componentIndex >= 0) {
                return componentList[componentIndex];
            }
            return null;
        }

        public string[] GetComponentList() {
            return this.componentList;
        }

        #endregion Components
    }
}
