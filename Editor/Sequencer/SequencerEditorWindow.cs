using Playblack.BehaviourTree;
using Playblack.Sequencer;
using PlayBlack.Editor.Sequencer.Renderers.Bt;
using PlayBlack.Editor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace PlayBlack.Editor.Sequencer {
    public class SequencerEditorWindow : GenericPopupWindow {

        private BtSequencerRenderer sequencerWindow;

        private SequenceContainer sequenceContainer;
        private SequenceExecutor subject; // Used to set this dirty to make unity save it

        public override string GetTitle() {
            return "Sequencer";
        }

        public override void InternalInit() {
            //throw new NotImplementedException();
        }

        public void SetData(SequenceExecutor subject) {
            this.sequenceContainer = subject.SequenceCommands;
            this.subject = subject;
            this.sequencerWindow = new BtSequencerRenderer();
            this.sequencerWindow.OperatorRenderer = new DefaultRenderer();
            this.sequencerWindow.OperatorRenderer.SetSubjects(this.sequenceContainer.RootModel);

        }

        private void DrawSequenceSettings() {
            EditorGUILayout.BeginVertical();
            {
                sequenceContainer.TypeOfExecution = (ExecutionType)EditorGUILayout.EnumPopup("Execution Mode", sequenceContainer.TypeOfExecution);
            }
            EditorGUILayout.EndVertical();
        }

        public void OnGUI() {
            EditorGUILayout.BeginHorizontal();
            {
                if (sequencerWindow != null) {
                    this.DrawSequenceSettings();
                    EditorGUILayout.BeginVertical();
                    {
                        sequencerWindow.DoRenderLoop();
                    }
                    EditorGUILayout.EndVertical();
                }
                else {
                    EditorGUILayout.HelpBox("Nothing selected", MessageType.Info);
                }

            }
            EditorGUILayout.EndHorizontal();
            Repaint();
            if (subject != null) {
                EditorUtility.SetDirty(subject); // Happens when viewing the editor and switching to play mode
            }
            
        }

    }
}
