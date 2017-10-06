using Playblack.BehaviourTree;
using Playblack.Sequencer;
using PlayBlack.Editor.Sequencer.Renderers;
using PlayBlack.Editor.Windows;
using UnityEditor;

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
            // Record the change of the whole thing. Seems a lil wasteful but ... yeah. Huh. Unity.
            // NOTE: This Undo ain't actually working because it acts on the serialized data
            // whereas the stuff displayed in the editors is taken from the current in-memory
            // representation of the model tree (the real thing, that is)
            // BUT: This will scratch the right itch in Unity to make it save the damn thing.
            Undo.RecordObject(SequenceExecutorObject, "Serializing behaviour tree");
            SequenceExecutorObject.SerializeModelTree(); // Force update of the model tree data here
            EditorUtility.SetDirty(SequenceExecutorObject);
        }
    }
}
