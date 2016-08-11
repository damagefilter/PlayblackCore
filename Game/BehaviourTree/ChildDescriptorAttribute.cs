using System;

namespace Playblack.BehaviourTree {

    /// <summary>
    /// Describes one child of a model, which index it should appear in and under which label.
    /// This does not describe the type that the child will become as that's up to the designer
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ChildDescriptorAttribute : System.Attribute {

        /// <summary>
        /// The label under which this child can be added.
        /// Label is an extra button under which, with an indent, the child option will be placed.
        /// If a label exists for multiple children, an unordered list is assumed.
        /// That means the insertIndex is ignored and children are added to the child list
        /// in the order they are added / set to the sequencer editor.
        ///
        /// A name of "default" will add the child add button directly under the label for the
        /// parent operator without a label. Good for lists with no particular use or order
        /// </summary>
        private string name;

        /// <summary>
        /// The index to insert this child into its parents children list.
        /// This is the internal index only used to get the expected order straight,
        /// the executir will use to gather its children, if any.
        ///
        /// -1 means that multiple children can be attached under this label.
        /// Use on sequences and selectors.
        /// </summary>
        private int insertIndex;

        /// <summary>
        /// In which order to display this child?
        /// </summary>
        private int displayDelta;

        public string Name {
            get {
                return name;
            }
        }

        public int InsertIndex {
            get {
                return insertIndex;
            }
        }

        public int DisplayDelta {
            get {
                return displayDelta;
            }
        }

        public ChildDescriptorAttribute(string name) {
            this.name = name;
            this.insertIndex = -1;
        }

        public ChildDescriptorAttribute(string name, int insertIndex) {
            this.name = name;
            this.insertIndex = insertIndex;
        }

        public ChildDescriptorAttribute(string name, int insertIndex, int displayDelta) {
            this.name = name;
            this.insertIndex = insertIndex;
            this.displayDelta = displayDelta;
        }
    }
}