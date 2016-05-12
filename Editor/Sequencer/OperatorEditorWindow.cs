using Playblack.BehaviourTree;
using PlayBlack.Editor.Sequencer.Renderers;
using PlayBlack.Editor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayBlack.Editor.Sequencer {

    public class OperatorEditorWindow : GenericPopupWindow {

        public IOperatorRenderer<UnityBtModel> Renderer {
            get; set;
        }

        public override string GetTitle() {
            return "Operator Editor";
        }

        public override void InternalInit() {
            throw new NotImplementedException();
        }
    }
}
