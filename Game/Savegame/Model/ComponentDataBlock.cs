using System;
using ProtoBuf;
using System.Collections.Generic;
using UnityEngine;

namespace Playblack.Savegame.Model {
    /// <summary>
    /// A block of data representing one saveable component.
    /// A bunch of these are added to a GameObjectDataBlock to represent the (relevant) state of a full gameobject.
    /// </summary>
    [ProtoContract]
    public class ComponentDataBlock {
        #region Data Section
        /// <summary>
        /// List of fields to be saved and or restored.
        /// </summary>
        [ProtoMember(100)]
        List<FieldDescription> saveData;
        #endregion

        public ComponentDataBlock() {
            saveData = new List<FieldDescription>();
        }

        #region adding data
        public void AddInt(string name, int value) {
            var fd = new FieldDescription((int)SaveField.FIELD_INT, name, DataSerializer.SerializeSimpleObject(value));
        }

        public void AddFloat(string name, float value) {
            var fd = new FieldDescription((int)SaveField.FIELD_FLOAT, name, DataSerializer.SerializeSimpleObject(value));
        }

        public void AddString(string name, string value) {
            var fd = new FieldDescription((int)SaveField.FIELD_STRING, name, DataSerializer.SerializeSimpleObject(value));
        }

        public void AddColor(string name, Color value) {
            var fd = new FieldDescription((int)SaveField.FIELD_COLOR, name, DataSerializer.SerializeSimpleObject(new float[] {
                value.r,
                value.g,
                value.b,
                value.a
            }));
        }

        public void AddVector(string name, Vector3 value) {
            var fd = new FieldDescription((int)SaveField.FIELD_VECTOR_POSITION, name, DataSerializer.SerializeSimpleObject(new float[] {
                value.x,
                value.y,
                value.z
            }));
        }

        public void AddProtoObject(string name, object value) {
            var fd = new FieldDescription((int)SaveField.FIELD_PROTOBUF_OBJECT, name, DataSerializer.SerializeProtoObject(value));
        }

        public void AddSimpleObject(string name, object value) {
            var fd = new FieldDescription((int)SaveField.FIELD_SIMPLE_OBJECT, name, DataSerializer.SerializeSimpleObject(value));
        }
        #endregion
        #region reading data
        public int ReadInt(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<int>(saveData[i].fieldContent);
                }
            }
            return 0;
        }

        public float ReadFloat(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<float>(saveData[i].fieldContent);
                }
            }
            return 0f;
        }

        public string ReadString(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<string>(saveData[i].fieldContent);
                }
            }
            return string.Empty;
        }

        public Color ReadColor(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    float[] data = DataSerializer.DeserializeSimpleObject<float[]>(saveData[i].fieldContent);
                    return new Color(data[0], data[1], data[2], data[3]);
                }
            }
            return Color.clear;
        }

        public Vector3 ReadVector(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    float[] data = DataSerializer.DeserializeSimpleObject<float[]>(saveData[i].fieldContent);
                    return new Vector3(data[0], data[1], data[2]);
                }
            }
            return Vector3.zero;
        }

        public T ReadProtoObject<T>(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeProtoObject<T>(saveData[i].fieldContent);
                }
            }
            return default(T);
        }

        public object ReadProtoObject(string name, Type type) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeProtoObject(saveData[i].fieldContent, type);
                }
            }
            return null;
        }

        public T ReadSimpleObject<T>(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<T>(saveData[i].fieldContent);
                }
            }
            return default(T);
        }

        public object ReadSimpleObject(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject(saveData[i].fieldContent);
                }
            }
            return null;
        }
        #endregion
    }
}
