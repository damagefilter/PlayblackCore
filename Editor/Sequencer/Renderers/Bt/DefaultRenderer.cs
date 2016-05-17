using Playblack.BehaviourTree;
using Fasterflect;
using UnityEditor;
using UnityEngine;
using PlayBlack.Editor.Windows;
using System.Collections.Generic;

namespace PlayBlack.Editor.Sequencer.Renderers.Bt {
    public class DefaultRenderer : IOperatorRenderer<UnityBtModel> {

        protected UnityBtModel modelToRender;
        protected UnityBtModel parentModel;
        private bool fieldsAreFolded = false;
        private Vector2 scrollPos;

        private void ResetToDefaults() {
            scrollPos = Vector2.zero;
            fieldsAreFolded = false;
        }
        public virtual void RenderCodeView(ISequencerRenderer<UnityBtModel> sequenceRenderer) {
            var structure = modelToRender.GetChildStructure();
            int indent = sequenceRenderer.IndentLevel;
            sequenceRenderer.RenderEditOperatorButton(modelToRender.DisplayName, modelToRender, parentModel, this);
            sequenceRenderer.IndentLevel += 10;
            var r = new DefaultRenderer(); // used to render children. Effectively causes each operator to have one renderer
            foreach (var kvp in structure) {
                // in the default list we ignore insert indexes and just render the whole list
                if (kvp.Name == "default") {
                    for (int i = 0; i < modelToRender.children.Count; ++i) {
                        if (modelToRender.children[i] == null) {
                            // FIXME: Shouldn't be happening here but if it does, it should probably be fixed
                            continue;
                        }
                        // Draw this child
                        r.SetSubjects(modelToRender.children[i], modelToRender);
                        r.ResetToDefaults(); // Reset scrollpos and folded stuff to avoid confusion
                        r.RenderCodeView(sequenceRenderer);
                    }
                    // A button to add more operators
                    sequenceRenderer.RenderAddOperatorButton(modelToRender);
                    break;
                }
                sequenceRenderer.RenderOperatorDummyButton(kvp.Name);
                sequenceRenderer.IndentLevel += 10;

                if (modelToRender.children.Count <= kvp.InsertIndex || modelToRender.children.Count == 0) {
                    sequenceRenderer.RenderAddOperatorButton(modelToRender, kvp.InsertIndex);
                }
                else {
                    r.SetSubjects(modelToRender.children[kvp.InsertIndex], modelToRender);
                    r.ResetToDefaults(); // Reset scrollpos and folded stuff to avoid confusion
                    r.RenderCodeView(sequenceRenderer);
                }
            }
        }

        public virtual void RenderEditorWindowView(ISequencerRenderer<UnityBtModel> sequenceRenderer) {
            this.fieldsAreFolded = EditorGUILayout.Foldout(this.fieldsAreFolded, "Field Configurations");
            if (this.fieldsAreFolded) {

                // Draw the Header Information
                EditorGUILayout.BeginHorizontal();
                {
                    // EditorGUILayout.LabelField("Value Type", GUILayout.Width(100));
                    EditorGUILayout.LabelField("Var Name", GUILayout.Width(150));
                    EditorGUILayout.LabelField("Var Value", GUILayout.Width(150));
                }
                EditorGUILayout.EndHorizontal();

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(300), GUILayout.Width(460));
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
                                    bool boolVal = (bool)data.Value;
                                    data.Value = EditorGUILayout.Toggle(boolVal, GUILayout.Width(150));
                                    break;
                                case Playblack.BehaviourTree.ValueType.FLOAT:
                                    float floatVal = (float)data.Value;
                                    data.Value = EditorGUILayout.FloatField(floatVal, GUILayout.Width(150));
                                    break;
                                case Playblack.BehaviourTree.ValueType.INT:
                                    int intVal = (int)data.Value;
                                    data.Value = EditorGUILayout.IntField(intVal, GUILayout.Width(150));
                                    break;
                                case Playblack.BehaviourTree.ValueType.STRING:
                                    data.Value = EditorGUILayout.TextField(data.UnityValue, GUILayout.Width(150), GUILayout.Height(150));
                                    break;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }
                }
                EditorGUILayout.EndScrollView();
                
            }
            this.DrawRemoveButton();
        }

        public void SetSubjects(params UnityBtModel[] subjects) {
            this.modelToRender = subjects[0];
            if (subjects.Length > 1) {
                this.parentModel = subjects[1];
            }
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
