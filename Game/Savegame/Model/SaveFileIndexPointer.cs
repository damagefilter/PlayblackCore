using ProtoBuf;

namespace Playblack.Savegame.Model {
    [ProtoContract]
    public class SaveFileIndexPointer {
        [ProtoMember(100)]
        public long startOffset;
        [ProtoMember(200)]
        public int chunkLength;
        [ProtoMember(300)]
        public string dataId;

        public SaveFileIndexPointer() {
            // protobuf ctor
        }

        public SaveFileIndexPointer(string dataId, long startOffset, int chunkLength) {
            this.dataId = dataId;
            this.startOffset = startOffset;
            this.chunkLength = chunkLength;
        }
    }
}
