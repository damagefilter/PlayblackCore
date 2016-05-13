using System;
using System.Reflection;
using System.Collections.Generic;
using Playblack.BehaviourTree.Model.Core;
using Playblack.Extensions;
using ProtoBuf;
using UnityEngine;
using Fasterflect;

namespace Playblack.BehaviourTree {

    /// <summary>
    /// Represents a protocontract model of a behaviour tree.
    /// This is usually the data model that gets created in the editor
    /// and is read and managed during runtime.
    /// Now, why add this extra layer?
    /// Because Unity-serialisation itself has only a limited nesting level (of 7).
    /// But behaviour trees may get far more complex and deep than that.
    /// Additionally, it's good to have them compatible with the savegame system.
    /// That's why there is this extra layer of model data.
    /// </summary>
    [ProtoContract]
    public class UnityBtModel {
        [ProtoMember(1)]
        public bool enable = false;

        [ProtoMember(2)]
        public List<ValueField> contextData;

        [ProtoMember(3, OverwriteList = true)]
        public List<UnityBtModel> children = new List<UnityBtModel>();

        // requirement here is that a class may have a 0-args constructor
        [ProtoMember(4)]
        private string modelClassName;

        public string ModelClassName {
            get {
                return this.modelClassName;
            }
            set {
                this.modelClassName = value;

                // Update context data if empty
                if (this.contextData == null || this.contextData.Count == 0) {
                    this.contextData = new List<ValueField>(this.GetProposedFields());
                }

                // FIXME: In case a model gets re-assigned at some point, we'd have to
                // either scan context data to avoid data loss or just not allow multiple setting of model class names.
                // Would also need to remove fields not in the new proposed fields list.
                // Seems wasteful.
                /*if (contextData != null || contextData.Count > 0) {
                    for (int i = 0; i < fields.Length; ++i) {
                        var fieldExists = false;
                        for (int j = 0; j < contextData.Count; ++j) {
                            if (contextData[j].Name == fields[i].Name) {
                                fieldExists = true;
                                break;
                            }
                        }
                        if (!fieldExists) {
                            this.contextData.Add(fields[i]);
                        }
                    }
                }
                else {
                    // Nothing there, just dump in all the new data
                    this.contextData = new List<ValueField>(fields);
                }*/
            }
        }

        public Type ModelType {
            get {
                var type = Type.GetType(this.modelClassName);
                if (type != null) {
                    return type;
                }
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) {
                    type = a.GetType(this.modelClassName);
                    if (type != null) {
                        return type;
                    }
                }
                return null;
            }
        }

        [ProtoMember(5)]
        private int numChildren = 0;
        /// <summary>
        /// Used to register certain children within for re-ordering inside the child-list.
        /// This is required in order to avoid concurrent modifications to the child list.
        /// </summary>
        private Dictionary<UnityBtModel, int> scheduledReorders = new Dictionary<UnityBtModel, int>();
        private ModelTask mtInstance;

        public ModelTask Model {
            get {
                if (mtInstance != null) {
                    mtInstance.Context = new DataContext(this.contextData);
                    return mtInstance;
                }
                // First: introspect the class to see if it's one we can work with
                var type = Type.GetType(this.modelClassName);
                //if (!type.IsAssignableFrom(typeof(ModelTask))) { 
                if (!typeof(ModelTask).IsAssignableFrom(type)) {
                    throw new ArgumentException("'" + type + "' is not a ModelTask object. I don't know how to use it. Fix it.");
                }
                var ctor = type.GetConstructor(Type.EmptyTypes);
                // guards and stuff must be set in the method that reads the tree.
                var mt = (ModelTask)ctor.Invoke(new object[] { });
                if (this.contextData != null) {
                    mt.Context = new DataContext(this.contextData);
                }
                this.mtInstance = mt;
                return mt;
            }
        }

        public static UnityBtModel NewInstance(UnityBtModel parent) {
            var model = new UnityBtModel();
            model.children = new List<UnityBtModel>();
            if (parent != null) {
                parent.children.Add(model);
            }
            return model;
        }

        public static UnityBtModel NewInstance(UnityBtModel parent, UnityBtModel model, string modelClassName) {
            model.children = new List<UnityBtModel>();
            model.contextData = new List<ValueField>();
            model.enable = true;
            if (parent != null) {
                parent.children.Add(model);
            }
            model.ModelClassName = modelClassName;
            return model;
        }

        public static UnityBtModel NewInstance(UnityBtModel parent, UnityBtModel model, string modelClassName, int insertIndex) {
            model.children = new List<UnityBtModel>();
            model.contextData = new List<ValueField>();
            model.enable = true;
            if (parent != null) {
                // NOTE: This means the children list must be populated already, at least with nulls!
                if (insertIndex >= parent.children.Count) {
                    parent.children.Resize(insertIndex + 1, null);
                }
                parent.children[insertIndex] = model;
            }
            model.ModelClassName = modelClassName;
            return model;
        }



        public bool RemoveChild(UnityBtModel model) {
            int index = this.children.IndexOf(model);            
            if (index >= 0) {
                var child = children[index];
                children.RemoveAt(index);
                child.ResizeChildren();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets a child at the given index to null
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool NullChild(UnityBtModel model) {
            int index = children.IndexOf(model);
            if (index == -1) {
                return false;
            }
            Debug.Log("Removing child at index " + index);
            children[index] = null;
            return true;
        }

        /// <summary>
        /// Resize the children array to the currently set numChildren
        /// </summary>
        public void ResizeChildren() {

            bool needsRefill = children.Count < numChildren;

            if (needsRefill) {
                var diff = numChildren - children.Count;
                for (int i = 0; i < diff; ++i) {
                    UnityBtModel.NewInstance(this);
                }
            }
            else {
                // Shrinking 
                while (children.Count > numChildren) {
                    var child = children[children.Count - 1];
                    children.RemoveAt(children.Count - 1);
                    // Make sure the now inaccessible children get pruged properly
                    child.ResizeChildren();
                    child = null;
                }
            }
            // just to be sure
            numChildren = children.Count;
        }

        public void ScheduleChildReorder(UnityBtModel child, int newIndex) {
            scheduledReorders.Add(child, newIndex);
        }

        public void ProcessReorders() {
            foreach (var kvp in scheduledReorders) {
                if (kvp.Value >= this.children.Count || kvp.Value < 0) {
                    // This means we're either at the beginning or at the end
                    Debug.Log("Hit one end of a list, not reordering this child. New Index is told to be " + kvp.Value);
                    continue;
                }
                int i = this.children.IndexOf(kvp.Key);
                this.children.RemoveAt(i);
                this.children.Insert(kvp.Value, kvp.Key);
            }
            scheduledReorders.Clear();
        }


        /// <summary>
        /// Get an array with proposed fields for the underlying Model.
        /// These are used for setting defaults in the Sequence editor windows.
        /// This is a list of fields annotated with the EditableFieldAttribute.
        /// If there is no such attribute on any field, an empty array is returned.
        /// </summary>
        /// <returns>The proposed fields.</returns>
        public ValueField[] GetProposedFields() {
            var proposedFields = ModelType.FieldsWith(Flags.AnyVisibility, typeof(EditableFieldAttribute));
            var valueFields = new ValueField[proposedFields.Count];
            for (int i = 0; i < proposedFields.Count; ++i) {
                var newValueField = new ValueField();
                if (proposedFields[i].FieldType == typeof(int)) {
                    newValueField.Type = ValueType.INT;
                }
                else if (proposedFields[i].FieldType == typeof(float)) {
                    newValueField.Type = ValueType.FLOAT;
                }
                else if (proposedFields[i].FieldType == typeof(string)) {
                    newValueField.Type = ValueType.STRING;
                }
                else if (proposedFields[i].FieldType == typeof(bool)) {
                    newValueField.Type = ValueType.BOOL;
                }
                var attrib = proposedFields[i].Attribute<EditableFieldAttribute>();
                newValueField.Name = attrib.FieldName;

                if (attrib.DefaultUnityValue != null) {
                    newValueField.Value = attrib.DefaultUnityValue;
                }
                valueFields[i] = newValueField;
            }
            return valueFields;
        }
    }
}

