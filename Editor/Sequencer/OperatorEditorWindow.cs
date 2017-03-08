using Playblack.BehaviourTree;
using Playblack.Sequencer;
using PlayBlack.Editor.Sequencer.Renderers;
using PlayBlack.Editor.Windows;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PlayBlack.Editor.Sequencer {

    /// <summary>
    /// Renders the window where you can edit one operaot in the sequence.
    /// Called from within peusocode editor when user presses a button.
    /// </summary>
    public class OperatorEditorWindow : GenericPopupWindow {

        public IOperatorRenderer<UnityBtModel> OperatorRenderer {
            get; set;
        }

        public ISequencerRenderer<UnityBtModel> SequencerRenderer {
            get; set;
        }

        public SerializedObject SerializedSequenceExecutor { get; set; }
        public SequenceExecutor SequenceExecutorObject { get; set; }

        public override string GetTitle() {
            return "Operator Editor";
        }

        public override void InternalInit() {
            // otImplementedException();
        }

        public void OnGUI() {
            if (this.OperatorRenderer != null) {
                this.OperatorRenderer.RenderEditorWindowView(SequencerRenderer);
            }
        }

        public void OnDestroy() {
            // Called when closed
            this.OperatorRenderer.GetSubjectToRender().UpdateCodeViewDisplay();
            int currentArraySize = SerializedSequenceExecutor.FindProperty("serializedModelTree.Array.size").intValue;
            SequenceExecutorObject.SerializeModelTree(); // Force update of the model tree data here
            int newArraySize = SequenceExecutorObject.SerializedModelTree.Length;
            if (newArraySize != currentArraySize)
                SerializedSequenceExecutor.FindProperty("serializedModelTree.Array.size").intValue = newArraySize;

            for (int i = 0; i < newArraySize; i++) {
                SerializedSequenceExecutor.FindProperty(string.Format("serializedModelTree.Array.data[{0}]", i)).intValue = SequenceExecutorObject.SerializedModelTree[i];
            }
            EditorUtility.SetDirty(SequenceExecutorObject);
        }
    }
}