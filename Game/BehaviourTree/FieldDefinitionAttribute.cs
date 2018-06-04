using System;

namespace Playblack.BehaviourTree {

    public class FieldDefinitionAttribute : System.Attribute {
        private string defaultUnityValue = null;
        private ValueType fieldValueType;
        private string displayName;
        private Type typeInfo; // used for enums currently to create the correct enum dropdown in editor window.

        public string DefaultUnityValue {
            get {
                return defaultUnityValue;
            }
        }

        public string DisplayName {
            get {
                return displayName;
            }
        }

        public Type TypeInfo {
            get {
                return typeInfo;
            }
        }

        public ValueType FieldValueType {
            get {
                return fieldValueType;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fieldName">The name of the field in the executor class to address</param>
        /// <param name="displayName">The corresponding display name. It MUST be unique within the set of fields on a class!!! IMPORTANT</param>
        /// <param name="valueType">The data type of the described field. Others than those in ValueType are not supported!</param>
        public FieldDefinitionAttribute(string displayName, ValueType valueType) {
            this.displayName = displayName;
            this.fieldValueType = valueType;
        }
        public FieldDefinitionAttribute(string displayName, ValueType valueType, Type typeInfo) {
            this.displayName = displayName;
            this.fieldValueType = valueType;
            this.typeInfo = typeInfo;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fieldName">The name of the field in the executor class to address</param>
        /// <param name="displayName">The corresponding display name. It MUST be unique within the set of fields on a class!!! IMPORTANT</param>
        /// <param name="valueType">The data type of the described field. Others than those in ValueType are not supported!</param>
        /// <param name="defaultValue">Provide a string representation of the default value that is to be set</param>
        public FieldDefinitionAttribute(string displayName, ValueType valueType, string defaultValue) {
            this.displayName = displayName;
            this.defaultUnityValue = defaultValue;
            this.fieldValueType = valueType;
        }
    }
}
