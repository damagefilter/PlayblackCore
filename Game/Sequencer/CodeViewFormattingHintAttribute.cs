namespace Playblack.Sequencer {

    /// <summary>
    /// Can be attached to Models to describe in the editor view
    /// how the model should look.
    /// </summary>
    public class CodeViewFormattingHintAttribute : System.Attribute {

        public string Format {
            get;
            private set;
        }

        /// <summary>
        /// Describe a formatting. You can use model field names as placeholders,
        /// which will be replaced with the data from the context.
        /// May look like this "{speaker}:{message}"
        /// or "Searching for {target} in radius {radius}"
        /// </summary>
        /// <param name="format"></param>
        public CodeViewFormattingHintAttribute(string format) {
            this.Format = format;
        }
    }
}