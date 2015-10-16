using System;
using ProtoBuf;
using System.Collections.Generic;
using UnityEngine;

namespace Playblack.Savegame.Model {
    /// <summary>
    /// A block of data representing a saveable component.
    /// </summary>
    [ProtoContract]
    public class DataBlock {
        #region Data Section
        /// <summary>
        /// Identify to which saveable object this save must be applied to
        /// </summary>
        [ProtoMember(100)]
        private string uuid;

        /// <summary>
        /// Should an asset be loaded or should a new go be generated?
        /// </summary>
        [ProtoMember(200)]
        private bool loadAsset;

        /// <summary>
        /// If loadAsset is true, this should be the asset path to look at.
        /// </summary>
        [ProtoMember(300)]
        private string assetPath;

        /// <summary>
        /// The AssetBundle the asset to load can be found in.
        /// Should only be set if an asset is supposed to be loaded.
        /// </summary>
        [ProtoMember(400)]
        private string assetBundle;

        /// <summary>
        /// Savegame version to avoid coflicts
        /// </summary>
        [ProtoMember(500)]
        private int version = 100;

        /// <summary>
        /// List of fields to be saved and or restored.
        /// </summary>
        [ProtoMember(600)]
        List<FieldDescription> saveData;
        #endregion

        public DataBlock(string uuid) {
            loadAsset = false;
            saveData = new List<FieldDescription>();
            this.uuid = uuid;
        }

        public DataBlock(string uuid, string assetPath, string assetBundle) {
            loadAsset = true;
            this.assetBundle = assetBundle;
            this.assetPath = assetPath;
            this.uuid = uuid;
            saveData = new List<FieldDescription>();
        }

        #region adding data
        public void AddInt(string name, int value) {
            var fd = new FieldDescription((int)SaveField.FIELD_INT, name, Serializer.SerializeSimpleObject(value));
        }

        public void AddFloat(string name, float value) {
            var fd = new FieldDescription((int)SaveField.FIELD_FLOAT, name, Serializer.SerializeSimpleObject(value));
        }

        public void AddString(string name, string value) {
            var fd = new FieldDescription((int)SaveField.FIELD_STRING, name, Serializer.SerializeSimpleObject(value));
        }

        public void AddColor(string name, Color value) {
            var fd = new FieldDescription((int)SaveField.FIELD_COLOR, name, Serializer.SerializeSimpleObject(new float[] {
                value.r,
                value.g,
                value.b,
                value.a
            }));
        }

        public void AddVector(string name, Vector3 value) {
            var fd = new FieldDescription((int)SaveField.FIELD_VECTOR_POSITION, name, Serializer.SerializeSimpleObject(new float[] {
                value.x,
                value.y,
                value.z
            }));
        }

        public void AddProtoObject(string name, object value) {
            var fd = new FieldDescription((int)SaveField.FIELD_PROTOBUF_OBJECT, name, Serializer.SerializeProtoObject(value));
        }

        public void AddSimpleObject(string name, object value) {
            var fd = new FieldDescription((int)SaveField.FIELD_SIMPLE_OBJECT, name, Serializer.SerializeSimpleObject(value));
        }
        #endregion
        #region reading data
        public int ReadInt(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return Serializer.DeserializeSimpleObject<int>(saveData[i].fieldContent);
                }
            }
            return 0;
        }

        public float ReadFloat(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return Serializer.DeserializeSimpleObject<float>(saveData[i].fieldContent);
                }
            }
            return 0f;
        }

        public string ReadString(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return Serializer.DeserializeSimpleObject<string>(saveData[i].fieldContent);
                }
            }
            return string.Empty;
        }

        public Color ReadColor(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    float[] data = Serializer.DeserializeSimpleObject<float[]>(saveData[i].fieldContent);
                    return new Color(data[0], data[1], data[2], data[3]);
                }
            }
            return Color.clear;
        }

        public Vector3 ReadVector(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    float[] data = Serializer.DeserializeSimpleObject<float[]>(saveData[i].fieldContent);
                    return new Vector3(data[0], data[1], data[2]);
                }
            }
            return Vector3.zero;
        }

        public T ReadProtoObject<T>(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return Serializer.DeserializeProtoObject<T>(saveData[i].fieldContent);
                }
            }
            return default(T);
        }

        public object ReadProtoObject(string name, Type type) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return Serializer.DeserializeProtoObject(saveData[i].fieldContent, type);
                }
            }
            return null;
        }

        public T ReadSimpleObject<T>(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return Serializer.DeserializeSimpleObject<T>(saveData[i].fieldContent);
                }
            }
            return default(T);
        }

        public object ReadSimpleObject(string name) {
            for (int i = 0; i < saveData.Count; ++i) {
                if (saveData[i].fieldName == name) {
                    return Serializer.DeserializeSimpleObject(saveData[i].fieldContent);
                }
            }
            return null;
        }
        #endregion

        public byte[] ToBytes() {
            return Serializer.SerializeProtoObject(this);
        }
    }
}

