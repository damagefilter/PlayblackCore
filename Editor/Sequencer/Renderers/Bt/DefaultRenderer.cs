using Playblack.BehaviourTree;
using Fasterflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PlayBlack.Editor.Windows;

namespace PlayBlack.Editor.Sequencer.Renderers.Bt {
    public class DefaultRenderer : IOperatorRenderer<UnityBtModel> {

        protected UnityBtModel modelToRender;
        protected UnityBtModel parentModel;
        private bool fieldsAreFolded = false;
        public virtual void RenderCodeView(ISequencerRenderer<UnityBtModel> sequenceRenderer) {
            
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
