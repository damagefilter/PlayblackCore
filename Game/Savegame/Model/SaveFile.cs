using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Playblack.Savegame.Model {

    [ProtoContract]
    public class SaveFile : IDataBlock {

        /// <summary>
        /// This must exist to please ProtoBuf...
        /// </summary>
        [ProtoContract]
        public class SaveFileEntry {

            [ProtoMember(100)]
            public string typeName;

            [ProtoMember(200)]
            public byte[] rawData;

            public IDataBlock DataBlock {
                get {
                    return (IDataBlock)DataSerializer.DeserializeProtoObject(rawData, Type.GetType(typeName, true));
                }
            }

            public SaveFileEntry() {
                // protobuf ctor
            }

            public SaveFileEntry(IDataBlock dblock) {
                this.typeName = dblock.GetType().AssemblyQualifiedName;
                this.rawData = DataSerializer.SerializeProtoObject(dblock);
            }
        }

        [ProtoMember(100)]
        private string saveName;

        [ProtoMember(200)]
        private Dictionary<string, SaveFileEntry> saveData;

        #region IDataBlock implementation

        public string DataId {
            get {
                return saveName;
            }
        }

        #endregion IDataBlock implementation

        public SaveFile() {
            // protobuf compatible ctor
        }

        public SaveFile(string saveName) {
            this.saveName = saveName;
            this.saveData = new Dictionary<string, SaveFileEntry>();
        }

        public void Add(IDataBlock data) {
            if (saveData.ContainsKey(data.DataId)) {
                // Override if exists
                saveData[data.DataId] = new SaveFileEntry(data);
            }
            else {
                // new one otherwise
                saveData.Add(data.DataId, new SaveFileEntry(data));
            }
        }

        public IDataBlock Get(string name) {
            if (this.saveData.ContainsKey(name)) {
                return saveData[name].DataBlock;
            }
            return null;
        }
    }
}