using Playblack.BehaviourTree;

namespace PlayBlack.Editor.Sequencer.Renderers {
    /// <summary>
    /// Helper for copy / cut / paste operations. in sequencer editor.
    /// </summary>
    public static class OperatorClipboard {
        private static UnityBtModel currentContent;
        private static bool wasPasted;

        public static bool TryStore(UnityBtModel model) {
            if (!wasPasted && currentContent != null) {
                return false;
            }

            wasPasted = false;
            currentContent = model;
            return true;
        }

        public static void ForceStore(UnityBtModel model) {
            wasPasted = false;
            currentContent = model;
        }

        public static UnityBtModel Paste() {
            wasPasted = true;
            return currentContent.Copy();
        }

        public static bool HasContent() {
            return currentContent != null;
        }
    }
}
