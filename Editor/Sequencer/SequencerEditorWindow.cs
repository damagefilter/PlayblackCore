using Playblack.Sequencer;
using PlayBlack.Editor.Sequencer.Renderers.Bt;
using PlayBlack.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace PlayBlack.Editor.Sequencer {

    public class SequencerEditorWindow : GenericPopupWindow {
        private BtSequencerRenderer sequencerWindow;

        private SequenceExecutor subject; // Used to set this dirty to make unity save it
        private SerializedObject serializedSequenceExecutor;
        private Vector2 codeViewScrollPos;

        public override string GetTitle() {
            return "Sequencer";
        }

        public override void InternalInit() {
            //throw new NotImplementedException();
        }

        public void SetData(SequenceExecutor subject, SerializedObject serializedSequenceExecutor) {
            this.subject = subject;
            this.serializedSequenceExecutor = serializedSequenceExecutor;
            this.sequencerWindow = new BtSequencerRenderer();
            this.sequencerWindow.OperatorRenderer = new DefaultRenderer();
            this.sequencerWindow.OperatorRenderer.SetSubjects(subject.RootModel);
        }

        private void DrawSequenceSettings() {
            EditorGUILayout.BeginVertical();
            {
                subject.TypeOfExecution = (ExecutionType)EditorGUILayout.EnumPopup("Execution Mode", subject.TypeOfExecution);
            }
            EditorGUILayout.EndVertical();
        }

        public void OnGUI() {
            EditorGUILayout.BeginHorizontal();
            {
                if (sequencerWindow != null) {
                    this.DrawSequenceSettings();
                    this.codeViewScrollPos = EditorGUILayout.BeginScrollView(this.codeViewScrollPos);
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            sequencerWindow.DoRenderLoop();
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndScrollView();

                    int currentArraySize = serializedSequenceExecutor.FindProperty("serializedModelTree.Array.size").intValue;
                    subject.SerializeModelTree(); // Force update of the model tree data here
                    int newArraySize = subject.SerializedModelTree.Length;
                    if (newArraySize != currentArraySize)
                        serializedSequenceExecutor.FindProperty("serializedModelTree.Array.size").intValue = newArraySize;

                    for (int i = 0; i < newArraySize; i++) {
                        serializedSequenceExecutor.FindProperty(string.Format("serializedModelTree.Array.data[{0}]", i)).intValue = subject.SerializedModelTree[i];
                    }
                    EditorUtility.SetDirty(subject);
                }
                else {
                    EditorGUILayout.HelpBox("Nothing selected", MessageType.Info);
                }
            }
            EditorGUILayout.EndHorizontal();
            Repaint();
        }
    }
}