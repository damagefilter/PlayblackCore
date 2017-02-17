using Playblack.BehaviourTree;
using PlayBlack.Editor.Windows;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlayBlack.Editor.Sequencer.Renderers.Bt {

    public class DefaultRenderer : IOperatorRenderer<UnityBtModel> {
        protected UnityBtModel modelToRender;
        protected UnityBtModel parentModel;
        private IList<ChildDescriptorAttribute> childStructure;
        private Vector2 scrollPos;
        private int insertIndex; // used when the model to render is null and we need to add-operator on the correct index

        private void ResetToDefaults() {
            scrollPos = Vector2.zero;
            this.insertIndex = -1;
        }

        public virtual void RenderCodeView(ISequencerRenderer<UnityBtModel> sequenceRenderer) {
            // Happens in the best families.
            // That's when we're processing an empty child slot, which means we wanna add the add-operator button and get out.
            if (modelToRender == null) {
                if (parentModel == null) {
                    // Well that should really not be happening
                    Debug.LogError("Parent model is null, cannot add the add-operator-button");
                    return;
                }
                if (this.insertIndex >= 0) {
                    sequenceRenderer.RenderAddOperatorButton(parentModel, this.insertIndex);
                }
                else {
                    sequenceRenderer.RenderAddOperatorButton(parentModel);
                }
                return;
            }
            int indent = sequenceRenderer.IndentLevel;
            sequenceRenderer.RenderEditOperatorButton(modelToRender.CodeViewDisplay, modelToRender, parentModel, this);
            sequenceRenderer.IndentLevel += 10;

            foreach (var kvp in childStructure) {
                // in the default list we ignore insert indexes and just render the whole list then cancel any further rendering
                if (kvp.Name == "default") {
                    for (int i = 0; i < modelToRender.children.Count; i++) {
                        if (modelToRender.children[i] == null) {
                            // Debug.LogWarning("Found null child element in sequence list. Fixing.");
                            // This is absolutely invalid and we need to address it.
                            // Because in a sequence of things none of these things ought to be null
                            modelToRender.children.RemoveAt(i);
                            // Stuff is shifted down when removing at index, so count from that index again
                            i--;
                            Debug.Log("Removed null child in default list");
                            continue;
                        }
                        else {
                            DefaultRenderer r = null;
                            if (childRenderers.Count > i + 1) {
                                r = childRenderers[i];
                            }
                            else {
                                while (childRenderers.Count < i + 1) { // index is 0-based, count is not, so add one. Just sayin.
                                    childRenderers.Add(new DefaultRenderer());
                                }
                                r = childRenderers[i];
                            }
                            // If child already exists, just draw it
                            r.SetSubjects(modelToRender.children[i], modelToRender);
                            r.ResetToDefaults(); // Reset scrollpos and folded stuff to avoid confusion
                            r.RenderCodeView(sequenceRenderer);
                        }
                    }
                    if (modelToRender.children.Count < modelToRender.GetProposedNumChildren() || modelToRender.GetProposedNumChildren() == -1) {
                        // If there's still room in the list or list is infinite, offer add button
                        sequenceRenderer.RenderAddOperatorButton(modelToRender);
                    }
                    break;
                }

                // Displays a name for structuring purposes.
                sequenceRenderer.RenderOperatorDummyButton(modelToRender.GetChildDisplayName(kvp.InsertIndex));
                sequenceRenderer.IndentLevel += 10;
                // Make sure the index we want to access exists at all.
                if (modelToRender.children.Count < kvp.InsertIndex + 1) { // index is 0-based, count is not, so add one. Just sayin.
                    UnityBtModel.NewInstance(modelToRender, null, null, kvp.InsertIndex);
                }
                // WARNING: This expects that the child list is populated properly already
                // Draws an add-operator button to add a new operator
                if ((modelToRender.children[kvp.InsertIndex] == null)) {
                    sequenceRenderer.RenderAddOperatorButton(modelToRender, kvp.InsertIndex);
                }
                else {
                    DefaultRenderer r = null;
                    if (childRenderers.Count >= kvp.InsertIndex + 1) {
                        r = childRenderers[kvp.InsertIndex];
                    }
                    else {
                        while (childRenderers.Count < kvp.InsertIndex + 1) { // index is 0-based, count is not, so add one. Just sayin.
                            childRenderers.Add(new DefaultRenderer());
                        }
                        r = childRenderers[kvp.InsertIndex];
                    }
                    // Operator already exists, draw that one
                    r.SetSubjects(modelToRender.children[kvp.InsertIndex], modelToRender);
                    r.ResetToDefaults(); // Reset scrollpos and folded stuff to avoid confusion
                    r.SetInsertIndex(kvp.InsertIndex);
                    r.RenderCodeView(sequenceRenderer);
                }
                // Set back indent level
                sequenceRenderer.IndentLevel -= 10;
            }
            sequenceRenderer.IndentLevel = indent;
        }

        private void SetInsertIndex(int insertIndex) {
            this.insertIndex = insertIndex;
        }

        public virtual void RenderEditorWindowView(ISequencerRenderer<UnityBtModel> sequenceRenderer) {
            // Draw the Header Information
            EditorGUILayout.BeginHorizontal();
            {
                // EditorGUILayout.LabelField("Value Type", GUILayout.Width(100));
                EditorGUILayout.LabelField("Var Name", GUILayout.Width(150));
                EditorGUILayout.LabelField("Var Value", GUILayout.Width(150));
            }
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(320), GUILayout.Height(460));
            {
                // Iterate over data fields on context
                for (int i = 0; i < modelToRender.contextData.Count; ++i) {
                    // Skip junk (really shouldn't be happening
                    if (modelToRender.contextData[i] == null) {
                        Debug.LogWarning("Found null Value in operator context at index " + i);
                        continue;
                    }
                    var data = modelToRender.contextData[i];

                    // Draw the given item
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(data.Name, GUILayout.Width(150)); // read only information
                        switch (data.Type) {
                            case Playblack.BehaviourTree.ValueType.BOOL:
                                if (data.Value == null) {
                                    data.Value = EditorGUILayout.Toggle(false, GUILayout.Width(150));
                                }
                                else {
                                    bool boolVal = (bool)data.Value;
                                    data.Value = EditorGUILayout.Toggle(boolVal, GUILayout.Width(150));
                                }
                                break;

                            case Playblack.BehaviourTree.ValueType.FLOAT:
                                if (data.Value == null) {
                                    data.Value = EditorGUILayout.FloatField(0f, GUILayout.Width(150));
                                }
                                else {
                                    float floatVal = (float)data.Value;
                                    data.Value = EditorGUILayout.FloatField(floatVal, GUILayout.Width(150));
                                }
                                break;

                            case Playblack.BehaviourTree.ValueType.INT:
                                if (data.Value == null) {
                                    data.Value = EditorGUILayout.IntField(0, GUILayout.Width(150));
                                }
                                else {
                                    int intVal = (int)data.Value;
                                    data.Value = EditorGUILayout.IntField(intVal, GUILayout.Width(150));
                                }
                                break;

                            case Playblack.BehaviourTree.ValueType.STRING:
                                data.Value = EditorGUILayout.TextField(data.UnityValue, GUILayout.Width(150));
                                break;

                            case Playblack.BehaviourTree.ValueType.TEXT:
                                data.Value = EditorGUILayout.TextArea(data.UnityValue, GUILayout.Width(150), GUILayout.Height(50));
                                break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndScrollView();
            // this.DrawRemoveButton();
        }

        private List<DefaultRenderer> childRenderers;

        public void SetSubjects(params UnityBtModel[] subjects) {
            var model = subjects[0];
            if (this.modelToRender != model) {
                if (model != null) {
                    if (model.ModelClassName != null) {
                        this.childStructure = model.GetChildStructure();
                    }
                    else {
                        // When ModelClassName is null, means we're having a deserialized default element here
                        // which is due to specification limitations in protobuf (it doesn't know null references)
                        // Anyhow, in such case this means what we actually want to render is a null value (empty slot in parent object)
                        model = null;
                    }
                    if (model != null && model.children != null && model.children.Count > 0) {
                        this.childRenderers = new List<DefaultRenderer>(model.children.Count);
                        // Pre-create all renderers for the models children.
                        for (int i = 0; i < model.children.Count; ++i) {
                            this.childRenderers.Add(new DefaultRenderer());
                        }
                    }
                    else {
                        this.childRenderers = new List<DefaultRenderer>(3);
                    }
                }
                this.modelToRender = model;
            }
            if (subjects.Length > 1) {
                this.parentModel = subjects[1];
            }
        }

        public UnityBtModel GetSubjectToRender() {
            return this.modelToRender;
        }

        protected void DrawRemoveButton() {
            if (GUILayout.Button("Delete this")) {
                if (this.parentModel == null) {
                    Debug.LogError("Tried to remove a root object or corrupted model " + this.modelToRender.ModelClassName);
                    return;
                }
                if (!this.parentModel.NullChild(this.modelToRender)) {
                    Debug.LogError("Failed to remove a model from its parent (but it is in there!)" + this.modelToRender.ModelClassName);
                    return;
                }
                // Gets hold of the current oped window and closes it as its renderable object was deleted
                var window = GenericPopupWindow.Popup<OperatorEditorWindow>();
                window.Close();
            }
        }
    }
}