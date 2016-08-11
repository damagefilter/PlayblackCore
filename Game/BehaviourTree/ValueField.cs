using ProtoBuf;
using System;
using UnityEngine;

namespace Playblack.BehaviourTree {
    /**
    * Loosely typed serializable value field thing for setting arbitrary data in unity editor.
    * Is used to assign values to anything that isn't ConVars
    */

    [Serializable]
    [ProtoContract]
    public class ValueField {

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

        public object Value {
            get {
                if (unityValue != null) {
                    switch (varType) {
                        case ValueType.BOOL:
                            return bool.Parse(unityValue);

                        case ValueType.FLOAT:
                            return float.Parse(unityValue);

                        case ValueType.INT:
                            return int.Parse(unityValue);

                        case ValueType.STRING:
                        case ValueType.TEXT:
                            return unityValue;
                    }
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

        public ValueField() {
        }

        public ValueField(string name, string value, ValueType type) {
            this.Name = name;
            this.unityValue = value;
            this.Type = type;
        }
    }
}