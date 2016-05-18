using Playblack.BehaviourTree;
using Fasterflect;
using UnityEditor;
using UnityEngine;
using PlayBlack.Editor.Windows;
using System.Collections.Generic;
using System.Linq;

namespace PlayBlack.Editor.Sequencer.Renderers.Bt {
    public class DefaultRenderer : IOperatorRenderer<UnityBtModel> {

        protected UnityBtModel modelToRender;
        protected UnityBtModel parentModel;
        private IList<ChildDescriptorAttribute> childStructure;
        private Vector2 scrollPos;

        private void ResetToDefaults() {
            scrollPos = Vector2.zero;
        }
        public virtual void RenderCodeView(ISequencerRenderer<UnityBtModel> sequenceRenderer) {
            if (modelToRender == null || modelToRender.ModelClassName == null) {
                // This is invalid and happens somehow and I should figure this out at some point.
                parentModel.NullChild(modelToRender); // Do nothing, wait for next frame
                Debug.LogError("modelToRender or ModelClassName is null. Much error!");
                return;
            }
            int indent = sequenceRenderer.IndentLevel;
            sequenceRenderer.RenderEditOperatorButton(modelToRender.DisplayName, modelToRender, parentModel, this);
            sequenceRenderer.IndentLevel += 10;
            var r = new DefaultRenderer(); // used to render children. Effectively causes each operator to have one renderer
            foreach (var kvp in childStructure) {
                // in the default list we ignore insert indexes and just render the whole list then cancel any further rendering
                if (kvp.Name == "default") {
                    for (int i = 0; i < modelToRender.children.Count; ++i) {
                        if (modelToRender.children[i] == null) {
                            // This is absolutely invalid and we need to address it.
                            modelToRender.children.RemoveAt(i);
                            return; // exit early, wait for next frame to display stuff properly
                        }
                        else {
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
                sequenceRenderer.RenderOperatorDummyButton(kvp.Name);
                sequenceRenderer.IndentLevel += 10;
                
                /*if ((modelToRender.children.Count == 0 && (modelToRender.GetProposedNumChildren() > 0 || modelToRender.GetProposedNumChildren() == -1)) && kvp.InsertIndex >= 0) {
                    UnityBtModel.NewInstance(modelToRender, null, null, kvp.InsertIndex);
                }*/

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
                    // Operator already exists, draw that one
                    r.SetSubjects(modelToRender.children[kvp.InsertIndex], modelToRender);
                    r.ResetToDefaults(); // Reset scrollpos and folded stuff to avoid confusion
                    r.RenderCodeView(sequenceRenderer);
                }
            }
            // Set back indent level
            sequenceRenderer.IndentLevel = indent;
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

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(300), GUILayout.Height(460));
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
                                data.Value = EditorGUILayout.TextField(data.UnityValue, GUILayout.Width(150), GUILayout.Height(50));
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

        public void SetSubjects(params UnityBtModel[] subjects) {
            var model = subjects[0];
            if (this.modelToRender != model) {
                if (model != null) { // For deleted models to retain indexes this can be null. Intended
                    this.childStructure = model.GetChildStructure();
                }
                this.modelToRender = model;
            }
            if (subjects.Length > 1) {
                this.parentModel = subjects[1];
            }
        }

        public UnityBtModel GetModelToRender() {
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
