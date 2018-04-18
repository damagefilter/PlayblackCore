namespace PlayBlack.Editor.Sequencer.Renderers {

    public interface IOperatorRenderer<TRenderType> {

        /// <summary>
        /// Renders something comlex
        /// </summary>
        void RenderEditorWindowView(ISequencerRenderer<TRenderType> sequenceRenderer);

        /// <summary>
        /// Renders the pseudo code part of this sequence part.
        /// That's the overview thing where you can click to edit the sequence
        /// or add/edit parts of the sequence
        /// </summary>
        void RenderCodeView(ISequencerRenderer<TRenderType> sequenceRenderer);

        /// <summary>
        /// Set the subject that you want to render with this sequence part renderer
        /// </summary>
        /// <param name="subject"></param>
        void SetSubjects(params TRenderType[] subject);

        void UpdateCodeView();

        /// <summary>
        /// Returns the primary rendering subject
        /// </summary>
        /// <returns></returns>
        TRenderType GetSubjectToRender();
    }
}