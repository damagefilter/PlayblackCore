using System.Collections.Generic;
using Fasterflect;
using Playblack.BehaviourTree;
using Playblack.Sequencer;
using UnityEngine;

namespace PlayBlack.Editor.Sequencer {

    /// <summary>
    /// Takes care of the editor logic of displaying a sequencer.
    /// </summary>
    public class ModelDisplayManager {
        private string cachedCodeViewDisplay;
        private readonly Dictionary<int, string> childDisplayCache = new Dictionary<int, string>();

        private readonly UnityBtModel model;
        private List<ChildDescriptorAttribute> childStructure;

        public ModelDisplayManager(UnityBtModel model) {
            this.model = model;
        }

        public string CodeViewDisplay {
            get {
                if (cachedCodeViewDisplay == null) {
                    UpdateCodeViewDisplay();
                }
                return cachedCodeViewDisplay;
            }
        }

        public void UpdateCodeViewDisplay() {
            if (model.contextData == null || model.contextData.Count == 0) {
                cachedCodeViewDisplay = model.DisplayName;
                return;
            }
            var t = model.ModelType;
            if (t == null) {
                // There's nothing yet, use displayname if any
                // otherwise stuff is just null.
                cachedCodeViewDisplay = model.DisplayName;
                return;
            }
            var attrib = t.Attribute<CodeViewFormattingHintAttribute>();
            if (attrib == null) {
                cachedCodeViewDisplay = model.DisplayName;
                return;
            }

            var theString = attrib.Format;
            theString = ReplacePlaceholdersWithContextData(theString);
            cachedCodeViewDisplay = theString;
            this.childDisplayCache.Clear();
        }

        private string ReplacePlaceholdersWithContextData(string theString) {
            foreach (var valueField in model.contextData) {
                string valueString = valueField.UnityValue;
                if (string.IsNullOrEmpty(valueString)) {
                    continue;
                }
                if (!string.IsNullOrEmpty(valueString) && valueString.Length > 50) {
                    valueString = valueString.Substring(0, 47) + "...";
                }
                theString = theString.Replace("{" + valueField.Name + "}", valueString);
            }

            return theString;
        }

        public string GetChildDisplayName(int insertIndex) {
            if (childDisplayCache.ContainsKey(insertIndex)) {
                return childDisplayCache[insertIndex];
            }
            var childs = GetChildStructure();
            bool foundInsertIndex = false;
            for (int i = 0; i < childs.Count; ++i) {
                if (childs[i].InsertIndex == insertIndex) {
                    var theString = ReplacePlaceholdersWithContextData(childs[i].Name);
                    this.childDisplayCache.Add(insertIndex, theString);
                    foundInsertIndex = true;
                    break;
                }
            }
            if (!foundInsertIndex) {
                if (childs.Count < insertIndex && childs.Count > 0) {
                    childDisplayCache.Add(insertIndex, childs[insertIndex].Name);
                }
                else {
                    return "No child descriptor on insert index " + insertIndex;
                }
            }
            return childDisplayCache[insertIndex];
        }

        public IList<ChildDescriptorAttribute> GetChildStructure() {
            if (childStructure != null) {
                return childStructure;
            }
            if (model.ModelClassName == null) {
                // NOTE: So far this seems to be fixed.
                // I'll keep the condition ust in case (20161120)
                Debug.LogError("No classname specified ... Much error!");
                return new List<ChildDescriptorAttribute>(0);
            }
            this.childStructure = new List<ChildDescriptorAttribute>();
            // unity doesn't want the type argument version so we gotta do this instead ...
            var childDescriptors = model.ModelType.Attributes(typeof(ChildDescriptorAttribute));
            for (int i = 0; i < childDescriptors.Count; ++i) {
                this.childStructure.Add((ChildDescriptorAttribute)childDescriptors[i]);

            }

            this.childStructure.Sort((a, b) => {
                if (a.DisplayDelta > b.DisplayDelta) {
                    return -1;
                }
                else if (a.DisplayDelta < b.DisplayDelta) {
                    return 1;
                }
                else {
                    return 0;
                }
            });
            return this.childStructure;
        }
    }
}
