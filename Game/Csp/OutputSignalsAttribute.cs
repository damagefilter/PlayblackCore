using System;

namespace Playblack.Csp {
    /// <summary>
    /// Can be used to make fired output signals of any component known.
    /// </summary>
    public class OutputSignalsAttribute : System.Attribute {
        string[] outputs;

        public OutputSignalsAttribute(params string[] outputs) {
            this.outputs = outputs;
        }
    }
}

