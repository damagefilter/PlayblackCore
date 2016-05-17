using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayBlack.Editor.Sequencer.Renderers {

    /// <summary>
    /// Renders a list of ISequencerPartRenderers into a list of interactive buttons
    /// so a sequence can be created / edited etc.
    /// 
    /// Also provides a rudimentary API for nested sequencer parts to be rendered
    /// properly without the sequencerparts knowing their exact context.
    /// </summary>
    public interface ISequencerRenderer<TRenderType> {

        int IndentLevel {
            get;
            set;
        }

        void RenderAddOperatorButton(TRenderType referenceObject);

        void RenderAddOperatorButton(TRenderType referenceObject, int insertIndex);

        /// <summary>
        /// Draws the set of buttons for one operator that should be displayed
        /// in the pseudo code view (the sequence)
        /// </summary>
        /// <param name="label"></param>
        /// <param name="referenceObject"></param>
        /// <param name="referenceParentObject"></param>
        /// <param name="operatorRenderer"></param>
        /// <param name="indenLevel"></param>
        void RenderEditOperatorButton(string label, TRenderType referenceObject, TRenderType referenceParentObject, IOperatorRenderer<TRenderType> operatorRenderer);

        /// <summary>
        /// Draws a dummy button with a label.
        /// Use for structuring the pseudo code window.
        /// </summary>
        /// <param name="label"></param>
        void RenderOperatorDummyButton(string label);

    }
}
