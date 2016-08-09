using Playblack.Sequencer;
using PlayBlack.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace PlayBlack.Editor.Sequencer {

    [CustomEditor(typeof(SequenceExecutor))]
    public class SequencerInspector : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            if (target == null) {
                return;
            }
            base.OnInspectorGUI();
            SequenceExecutor sequencer = target as SequenceExecutor;
            if (GUILayout.Button("Open Sequencer Settings")) {
                var window = GenericPopupWindow.Popup<SequencerEditorWindow>();
                window.SetData(sequencer);
            }
        }
    }
}
