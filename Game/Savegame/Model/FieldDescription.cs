using ProtoBuf;

namespace Playblack.Savegame.Model {

    /// <summary>
    /// Describes the contents of a field inside a save structure.
    /// </summary>
    [ProtoContract]
    public class FieldDescription {

        /// <summary>
        /// Field type info must be cast to the SaveField enum in order to make sense.
        /// Conversely, to correctly store this you must pass the int representation of a SaveField.
        /// </summary>
        [ProtoMember(100)]
        public int fieldType;

        /// <summary>
        /// Generally the name of the field inside the class we want to save.
        /// </summary>
        [ProtoMember(200)]
        public string fieldName;

        /// <summary>
        /// The byte[] serialized contents of the specified field.
        /// This is always a byte array but it can be either a protobuf byte array or a system-serialized byte array.
        /// The fieldType information has the required information in order to properly read this.
        /// </summary>
        [ProtoMember(300)]
        public byte[] fieldContent;

        public FieldDescription() {
            // Protobuf ctor
        }

        public FieldDescription(int type, string name, byte[] contents) {
            fieldType = type;
            fieldName = name;
            fieldContent = contents;
        }
    }
}