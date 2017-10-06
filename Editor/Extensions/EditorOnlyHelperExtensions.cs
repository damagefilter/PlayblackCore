using Fasterflect;
using Playblack.Sequencer;

namespace PlayBlack.Editor.Extensions {
    public static class EditorOnlyHelperExtensions {

        public static byte[] GetSerializedModelTree(this SequenceExecutor seq) {
            return (byte[])seq.GetFieldValue("serializedModelTree");
        }
    }
}
