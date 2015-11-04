using System;
using Playblack.Csp;
using System.Collections.Generic;

namespace PlayBlack.Editor.Csp {
    /// <summary>
    /// Helper object to cache signal data for a handler.
    /// Used to speed up the signal editor and make it less inefficient.
    /// </summary>
    public class SignalDataCache {
        private Dictionary<string, string[]> inputMapping; // must be arrays because they are used from the unity GUI which does not work on lists
        private string[] componentList;

        public SignalDataCache(SignalProcessor localProcessor, OutputEventListener listener) {
            if (string.IsNullOrEmpty(listener.processorName)) { // check if target processor name is okay
                return;
            }

            // Generates data.
            // This is a more or lesss expensive task which is why the results are stored for later re-use.
            listener.matchedProcessors.Clear();
            List<string> inputs = new List<string>();
            List<string> outputs = new List<string>();
            var hits = UnityEngine.Object.FindObjectsOfType<SignalProcessor>(); // expensive but it's not used THAT often
            Dictionary<string, List<string>> inputMap = new Dictionary<string, List<string>>();

            for (int i= 0; i < hits.Length; ++i) {
                if (!hits[i].name.StartsWith(listener.processorName, StringComparison.InvariantCulture)) {
                    continue;
                }
                listener.matchedProcessors.Add(hits[i]);
                foreach (var kvp in hits[i].InputFuncs) {
                    if (!inputMap.ContainsKey(kvp.Key)) {
                        inputMap.Add(kvp.Key, new List<string>());
                    }
                    for (int j = 0; j < kvp.Value.Count; ++j) {
                        inputMap[kvp.Key].Add(kvp.Value[j].Name);
                    }
                }
            }

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
        #region Inputs
        /// <summary>
        /// Get the index number of the input name from the given component.
        /// Used for editor popup fields.
        /// </summary>
        /// <returns>The input index.</returns>
        /// <param name="component">Component.</param>
        /// <param name="inputName">Input name.</param>
        public int GetInputIndex(string component, string inputName) {
            return !inputMapping.ContainsKey(component) ? 0 : Array.IndexOf(inputMapping[component], inputName);
        }

        /// <summary>
        /// Get the name of an input based on its index and the given component.
        /// </summary>
        /// <returns>The input name at index.</returns>
        /// <param name="component">Component.</param>
        /// <param name="inputIndex">Input index.</param>
        public string GetInputName(string component, int inputIndex) {
            if (!inputMapping.ContainsKey(component)) {
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
            return !inputMapping.ContainsKey(component) ? null : inputMapping[component];
        }
        #endregion
        #region Components
        public int GetComponentIndex(string component) {
            return Array.IndexOf(this.componentList, component);
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
        #endregion
    }
}

