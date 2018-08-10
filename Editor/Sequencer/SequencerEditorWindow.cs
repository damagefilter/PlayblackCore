using Playblack.Sequencer;
using PlayBlack.Editor.Sequencer.Renderers.Bt;
using PlayBlack.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace PlayBlack.Editor.Sequencer {

    public class SequencerEditorWindow : GenericPopupWindow {
        private BtSequencerRenderer sequencerWindow;

        private SequenceExecutor subject; // Used to set this dirty to make unity save it
        private Vector2 codeViewScrollPos;

        public override string GetTitle() {
            return "Sequencer";
        }

        public override void InternalInit() {
            EditorApplication.playmodeStateChanged += CheckIfDirty;
            //throw new NotImplementedException();
        }

        private void CheckIfDirty() { // need for that playmodeStateChanged callback
            CheckIfDirty(false);
        }

        private void CheckIfDirty(bool ignoreEditorState) {
            if (sequencerWindow == null) {
                Debug.Log("Sequencer Window was destroyed. Cannot check if dirty.");
                return;
            }
            if (ignoreEditorState) {
                if (sequencerWindow.IsDiry) {
                    sequencerWindow.UpdateSerializedModelTree();
                }
            }
            // If we're in playmode don't try saving. It'll just be derping.
            if (EditorApplication.isPlaying) {
                Debug.Log("Editor is playing, not updating data");
                return;
            }
            // if we are not playing but about to switch to it, and if sequencer is dirty, dump the changes before playing!
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying && sequencerWindow.IsDiry) {
                Debug.Log("Updating BT serialized data. Prior to play");
                sequencerWindow.UpdateSerializedModelTree();
            }
            else {
                Debug.Log("Cannot BT update data prior to play. Playmode isorwill: " + (EditorApplication.isPlayingOrWillChangePlaymode) + ", isDirty: " + sequencerWindow.IsDiry);
            }

        }

        public void SetData(SequenceExecutor subject, SerializedObject serializedSequenceExecutor) {
            this.subject = subject;
            if (subject.RootModel == null) {
                Debug.Log("RootModel in sequencer editor window is null... but y tho");
            }
            this.sequencerWindow = new BtSequencerRenderer(this);
            this.sequencerWindow.OperatorRenderer = new DefaultOperatorRenderer();
            this.sequencerWindow.OperatorRenderer.SetSubjects(subject.RootModel);

            // We need to drag this along into the operator editor where it will be saved and set dirty 
            // when the window is closed, which is the only place where data can be changed so it's a perfect fit
            this.sequencerWindow.SerializedSequenceExecutor = serializedSequenceExecutor;
            this.sequencerWindow.SequenceExecutorObject = subject;
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
                }
                else {
                    EditorGUILayout.HelpBox("Nothing selected", MessageType.Info);
                }
            }
            EditorGUILayout.EndHorizontal();
            Repaint();
        }

        private void OnDestroy() {
            EditorApplication.playmodeStateChanged -= CheckIfDirty;
            CheckIfDirty(true);
        }
    }
}
