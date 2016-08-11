using System;

namespace Playblack.Savegame {

    /// <summary>
    /// Marks any annotated component as saveable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SaveableComponentAttribute : System.Attribute {
    }
}