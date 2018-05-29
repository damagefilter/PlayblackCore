using Playblack.BehaviourTree;

namespace PlayBlack.Editor.Sequencer.Renderers {
    /// <summary>
    /// Helper for copy / cut / paste operations. in sequencer editor.
    /// </summary>
    public class OperatorClipboard {
        private UnityBtModel currentContent;
        private bool wasPasted;

        private static OperatorClipboard instance;

        public static void Ensure() {
            if (instance == null) {
                instance = new OperatorClipboard();
            }
        }
        public static bool TryStore(UnityBtModel model) {
            Ensure();
            if (!instance.wasPasted && instance.currentContent != null) {
                return false;
            }

            instance.wasPasted = false;
            instance.currentContent = model;
            return true;
        }

        public static void ForceStore(UnityBtModel model) {
            Ensure();
            instance.wasPasted = false;
            instance.currentContent = model;
        }

        public static UnityBtModel Paste() {
            Ensure();
            instance.wasPasted = true;
            return instance.currentContent.Copy();
        }

        public static bool HasContent() {
            Ensure();
            return instance.currentContent != null;
        }
    }
}
