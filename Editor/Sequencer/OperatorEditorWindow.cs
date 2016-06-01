using Playblack.BehaviourTree;
using PlayBlack.Editor.Sequencer.Renderers;
using PlayBlack.Editor.Windows;

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
    }
}
