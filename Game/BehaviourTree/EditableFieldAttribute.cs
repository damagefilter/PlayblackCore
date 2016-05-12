using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playblack.BehaviourTree {

    /// <summary>
    /// Tack on fields to expose them in the sequencer parts editor
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EditableFieldAttribute : System.Attribute {
        private string defaultUnityValue = null;

        public string DefaultUnityValue {
            get {
                return defaultUnityValue;
            }
        }
    }
}
