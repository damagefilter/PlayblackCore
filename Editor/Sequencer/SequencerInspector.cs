using Playblack.Sequencer;
using PlayBlack.Editor.Sequencer.Renderers.Bt;
using PlayBlack.Editor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace PlayBlack.Editor.Sequencer {

    [CustomEditor(typeof(SequenceExecutor))]
    public class SequencerInspector : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            SequenceExecutor trigger = target as SequenceExecutor;
            if (GUILayout.Button("Open Sequencer Settings")) {
                var window = GenericPopupWindow.Popup<SequencerEditorWindow>();
                window.SetSequencer(trigger.SequenceCommands);
            }
        }
    }
}
