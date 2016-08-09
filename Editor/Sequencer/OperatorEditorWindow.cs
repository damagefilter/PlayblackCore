using Playblack.BehaviourTree;
using Playblack.Sequencer;
using PlayBlack.Editor.Sequencer.Renderers;
using PlayBlack.Editor.Sequencer.Renderers.Bt;
using PlayBlack.Editor.Windows;
using System;
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
            // Dirty hack to force unity into serializing the internal byte array properly
            // by just saving the whole scene .... jeezus christ
            EditorApplication.SaveScene();
        }
    }
}
