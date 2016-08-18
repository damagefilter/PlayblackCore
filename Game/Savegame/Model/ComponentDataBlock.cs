using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Playblack.Savegame.Model {

    /// <summary>
    /// A block of data representing one saveable component.
    /// A bunch of these are added to a GameObjectDataBlock to represent the (relevant) state of a full gameobject.
    /// </summary>
    [ProtoContract]
    public class ComponentDataBlock : IDataBlock {

        #region Data Section

        [ProtoMember(100)]
        private string componentName;

        public string ComponentName {
            get {
                return componentName;
            }
        }

        /// <summary>
        /// List of fields to be saved and or restored.
        /// </summary>
        [ProtoMember(200)]
        private List<FieldDescription> saveData;

        public List<FieldDescription> SaveData {
            get {
                return saveData;
            }
        }

        /// <summary>
        /// Returns the name of the assembly this component comes from.
        /// That is very important because otherwise we cannot
        /// re-create objects that didn't come from a prefab.
        /// </summary>
        [ProtoMember(300)]
        private string assemblyName;

        public string AssemblyName {
            get {
                return assemblyName;
            }
        }

        #endregion Data Section

        #region IDataBlock implementation

        public string DataId {
            get {
                return componentName;
            }
        }

        #endregion IDataBlock implementation

        public ComponentDataBlock() {
            // Protobuf ctor
        }

        public ComponentDataBlock(string componentName, string componentAssembly) {
            saveData = new List<FieldDescription>();
            this.componentName = componentName;
            this.assemblyName = componentAssembly;
        }

        #region adding data

        public void AddBoolean(string name, bool value) {
            var fd = new FieldDescription((int)SaveField.FIELD_BOOL, name, DataSerializer.SerializeSimpleObject(value));
            saveData.Add(fd);
        }
        public void AddInt(string name, int value) {
            var fd = new FieldDescription((int)SaveField.FIELD_INT, name, DataSerializer.SerializeSimpleObject(value));
            saveData.Add(fd);
        }

        public void AddFloat(string name, float value) {
            var fd = new FieldDescription((int)SaveField.FIELD_FLOAT, name, DataSerializer.SerializeSimpleObject(value));
            saveData.Add(fd);
        }

        public void AddString(string name, string value) {
            var fd = new FieldDescription((int)SaveField.FIELD_STRING, name, DataSerializer.SerializeSimpleObject(value));
            saveData.Add(fd);
        }

        public void AddColor(string name, Color value) {
            var fd = new FieldDescription((int)SaveField.FIELD_COLOR, name, DataSerializer.SerializeSimpleObject(new float[] {
                value.r,
                value.g,
                value.b,
                value.a
            }));
            saveData.Add(fd);
        }

        public void AddVector(string name, Vector3 value) {
            var fd = new FieldDescription((int)SaveField.FIELD_VECTOR_POSITION, name, DataSerializer.SerializeSimpleObject(new float[] {
                value.x,
                value.y,
                value.z
            }));
            saveData.Add(fd);
        }

        public void AddProtoObject(string name, object value) {
            var fd = new FieldDescription((int)SaveField.FIELD_PROTOBUF_OBJECT, name, DataSerializer.SerializeProtoObject(value));
            saveData.Add(fd);
        }

        public void AddSimpleObject(string name, object value) {
            var fd = new FieldDescription((int)SaveField.FIELD_SIMPLE_OBJECT, name, DataSerializer.SerializeSimpleObject(value));
            saveData.Add(fd);
        }

        #endregion adding data

        #region reading data

        public int ReadInt(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<int>(saveData[i].fieldContent);
                }
            }
            return 0;
        }

        public bool ReadBool(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<bool>(saveData[i].fieldContent);
                }
            }
            return false;
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

        #endregion reading data
    }
}