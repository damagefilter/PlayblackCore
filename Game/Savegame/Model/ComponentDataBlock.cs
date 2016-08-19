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
            var fd = new FieldDescription(name, DataSerializer.SerializeSimpleObject(value));
            saveData.Add(fd);
        }

        public void AddInt(string name, int value) {
            var fd = new FieldDescription(name, DataSerializer.SerializeSimpleObject(value));
            saveData.Add(fd);
        }

        public void AddFloat(string name, float value) {
            var fd = new FieldDescription(name, DataSerializer.SerializeSimpleObject(value));
            saveData.Add(fd);
        }

        public void AddString(string name, string value) {
            var fd = new FieldDescription(name, DataSerializer.SerializeSimpleObject(value));
            saveData.Add(fd);
        }

        public void AddColor(string name, Color value) {
            var fd = new FieldDescription(name, DataSerializer.SerializeSimpleObject(new float[] {
                value.r,
                value.g,
                value.b,
                value.a
            }));
            saveData.Add(fd);
        }

        public void AddVector(string name, Vector3 value) {
            var fd = new FieldDescription(name, DataSerializer.SerializeSimpleObject(new float[] {
                value.x,
                value.y,
                value.z
            }));
            saveData.Add(fd);
        }

        public void AddQuaternion(string name, Quaternion value) {
            var fd = new FieldDescription(name, DataSerializer.SerializeSimpleObject(new float[] {
                value.x,
                value.y,
                value.z,
                value.w
            }));
            saveData.Add(fd);
        }

        public void AddProtoObject(string name, object value) {
            var fd = new FieldDescription(name, DataSerializer.SerializeProtoObject(value));
            saveData.Add(fd);
        }

        public void AddSimpleObject(string name, object value) {
            var fd = new FieldDescription(name, DataSerializer.SerializeSimpleObject(value));
            saveData.Add(fd);
        }

        #endregion adding data

        #region reading data

        /// <summary>
        /// Read a field under the givne name and return it as an integer.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int ReadInt(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<int>(saveData[i].fieldContent);
                }
            }
            return 0;
        }

        /// <summary>
        /// Read a field under the givne name and return it as a boolean.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ReadBool(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<bool>(saveData[i].fieldContent);
                }
            }
            return false;
        }

        /// <summary>
        /// Read a field under the givne name and return it as a float.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public float ReadFloat(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<float>(saveData[i].fieldContent);
                }
            }
            return 0f;
        }

        /// <summary>
        /// Read a field under the givne name and return it as a string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ReadString(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<string>(saveData[i].fieldContent);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Read a field under the givne name and return it as a unity color.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Color ReadColor(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    float[] data = DataSerializer.DeserializeSimpleObject<float[]>(saveData[i].fieldContent);
                    return new Color(data[0], data[1], data[2], data[3]);
                }
            }
            return Color.clear;
        }

        /// <summary>
        /// Read a field under the givne name and return it as a unity vector.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Vector3 ReadVector(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    float[] data = DataSerializer.DeserializeSimpleObject<float[]>(saveData[i].fieldContent);
                    return new Vector3(data[0], data[1], data[2]);
                }
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Read a field under the givne name and return it as unity quaternion.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Quaternion ReadQuaternion(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    float[] data = DataSerializer.DeserializeSimpleObject<float[]>(saveData[i].fieldContent);
                    return new Quaternion(data[0], data[1], data[2], data[3]);
                }
            }
            return Quaternion.identity;
        }

        /// <summary>
        /// Read a field under the givne name and interpret it as protobuf data. Returns an object.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Deserialized the field under the given name as specified type.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T ReadSimpleObject<T>(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return DataSerializer.DeserializeSimpleObject<T>(saveData[i].fieldContent);
                }
            }
            return default(T);
        }

        /// <summary>
        /// Deserialized the field under the given name as raw object.
        /// It'S up to the user how to interpret / cast that object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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