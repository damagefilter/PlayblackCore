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

        /// <summary>
        /// Simply renders an TRenderType with the current indent level +1
        /// </summary>
        /// <param name="sequencerPart"></param>
        void RenderSingle(TRenderType sequencerPart);


        /// <summary>
        /// Renders a whole list of sequencer parts one after the other.
        /// Useful if you want to render a child list
        /// </summary>
        /// <param name="sequencerList"></param>
        void RenderList(IList<TRenderType> sequencerList);

        void RenderAddOperatorButton(TRenderType referenceObject);

        /// <summary>
        /// Draws the set of buttons for one operator that should be displayed
        /// in the pseudo code view (the sequence)
        /// </summary>
        /// <param name="referenceObject"></param>
        /// <param name="label"></param>
        void RenderEditOperatorButton(TRenderType referenceObject, TRenderType referenceParentObject, IOperatorRenderer<TRenderType> operatorRenderer, string label);

    }
}
