using System;

namespace Playblack.BehaviourTree {

    /// <summary>
    /// Tack on fields to expose them in the sequencer parts editor
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EditableFieldAttribute : System.Attribute {
        private string defaultUnityValue = null;
        private string fieldName;

        public EditableFieldAttribute(string fieldName) {
            this.fieldName = fieldName;
        }

        public string DefaultUnityValue {
            get {
                return defaultUnityValue;
            }
        }

        public string FieldName {
            get {
                return fieldName;
            }
        }
    }
}
