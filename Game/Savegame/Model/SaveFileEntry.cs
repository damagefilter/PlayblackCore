using System;
using ProtoBuf;

namespace Playblack.Savegame.Model {
    /// <summary>
    /// This must exist to please ProtoBuf...
    /// </summary>
    [ProtoContract]
    public class SaveFileEntry {
        [ProtoMember(100)]
        public string typeName;

        [ProtoMember(200)]
        public string dataId;

        [ProtoMember(300)]
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
            this.dataId = dblock.DataId;
        }
    }
}
