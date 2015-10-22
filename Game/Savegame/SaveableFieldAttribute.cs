using System;

namespace Playblack.Savegame {
    /// <summary>
    /// This attribute is used to mark and identify fields 
    /// that should be stored into a savegame.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class SaveableFieldAttribute : System.Attribute {

        public readonly SaveField fieldType;

        public SaveableFieldAttribute(SaveField type) {
            this.fieldType = type;
        }
    }
}

