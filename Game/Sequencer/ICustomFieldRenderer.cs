namespace PlayBlack.Sequencer {
    /// <summary>
    /// Used in sequencer to render custom fields with custom data.
    /// </summary>
    public interface ICustomFieldRenderer {
        /// <summary>
        /// Draws a field and returns the value of the operation.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        object DrawFieldEditor(object data);

        /// <summary>
        /// Returns a formatted display value.
        /// This is for user-land only.
        /// Is used in code view display to substitute placeholders with variable names for instance.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string GetEditorDisplayValue(object data);

        /// <summary>
        /// Get the data from a given string value.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        object GetDataFromString(string input);
    }
}