using ProtoBuf;
using System;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace Playblack.BehaviourTree {
    /**
    * Loosely typed serializable value field thing for setting arbitrary data in unity editor.
    * Is used to assign values to anything that isn't ConVars
    */

    [Serializable]
    [ProtoContract]
    public struct ValueField {

        [SerializeField]
        [ProtoMember(1)]
        private string name;

        public string Name {
            get {
                return this.name;
            }
            set {
                this.name = value;
            }
        }

        [SerializeField]
        [ProtoMember(2)]
        private string unityValue;

        public string UnityValue {
            get {
                return unityValue;
            }
        }

        private object cachedValue;

        public object Value {
            get {
                if (cachedValue != null) {
                    return cachedValue;
                }
                if (unityValue != null) {
                    switch (varType) {
                        case ValueType.BOOL:
                            cachedValue = bool.Parse(unityValue);
                            break;

                        case ValueType.FLOAT:
                            cachedValue = float.Parse(unityValue);
                            break;

                        case ValueType.INT:
                            cachedValue = int.Parse(unityValue);
                            break;

                        case ValueType.ENUM:
                            cachedValue = Enum.Parse(systemType, unityValue);
                            break;

                        case ValueType.STRING:
                        case ValueType.TEXT:
                            cachedValue = unityValue;
                            break;
                        default:
                            Debug.LogWarning("ValueField: No known ValueType given, returning null as concrete value for type: " + varType);
                            break;
                    }

                    return cachedValue;
                }
                return null;
            }
            set {
                if (value != null) {
                    this.unityValue = value.ToString();
                }
                else {
                    this.unityValue = null;
                }

                cachedValue = null;
            }
        }

        [SerializeField]
        [ProtoMember(3)]
        private ValueType varType;

        public ValueType Type {
            get {
                return varType;
            }
            set {
                varType = value;
            }
        }

        [SerializeField]
        [ProtoMember(4)]
        private Type systemType;

        public Type SystemType {
            get {
                return systemType;
            }
            set {
                systemType = value;
            }
        }

        public ValueField(string pName, object value) {
            name = pName;
            cachedValue = value;
            varType = ValueTypeFromSystemType(value.GetType(), pName);
            systemType = value.GetType();
            unityValue = null;
            Value = value;
        }

        public ValueField(ValueField toCopy) {
            name = toCopy.name;
            varType = toCopy.varType;
            systemType = toCopy.systemType;
            unityValue = toCopy.unityValue;
            cachedValue = null; // don't copy to avoid having cross references here since the values have been boxed.
        }

        private static ValueType ValueTypeFromSystemType(Type t, string fieldName) {
            if (t.IsEnum) {
                return ValueType.ENUM;
            }

            if (t == typeof(string)) {
                return ValueType.STRING;
            }

            if (t == typeof(int)) {
                return ValueType.INT;
            }

            if (t == typeof(float)) {
                return ValueType.FLOAT;
            }

            if (t == typeof(bool)) {
                return ValueType.BOOL;
            }

            Debug.LogError("Could not determine a valuetype for " + t + " on field " + fieldName);
            return ValueType.STRING;
        }
    }
}
