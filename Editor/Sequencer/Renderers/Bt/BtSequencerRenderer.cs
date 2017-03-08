using Playblack.BehaviourTree;
using PlayBlack.Editor.Windows;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Playblack.Sequencer;

namespace PlayBlack.Editor.Sequencer.Renderers.Bt {

    public class BtSequencerRenderer : ISequencerRenderer<UnityBtModel> {

        /// <summary>
        /// List of models that are found to be corrupt during the rendering process.
        /// These will be cleared at the end of the render loop.
        /// This is a list of direct children of the root element only and will
        /// purge the whole tree for the corrupt child.
        /// </summary>
        private List<UnityBtModel> corruptedModels;

        private static GUISkin editorSkin;

        public static GUISkin EditorSkin {
            get {
                return editorSkin;
            }
        }

        public BtSequencerRenderer() {
            this.corruptedModels = new List<UnityBtModel>();
            if (editorSkin == null) {
                editorSkin = Resources.Load<GUISkin>("SequenceEditorLight");
            }
        }

        #region Rendering Process

        public int IndentLevel {
            get;
            set;
        }

        public DefaultRenderer OperatorRenderer {
            get {
                return operatorRenderer;
            }

            set {
                this.operatorRenderer = value;
            }
        }

        public SerializedObject SerializedSequenceExecutor { get; set; }
        public SequenceExecutor SequenceExecutorObject { get; set; }

        private DefaultRenderer operatorRenderer = new DefaultRenderer();

        /// <summary>
        /// Handles rendering of all the things and cleans up messes and rearrangements
        /// of senquence parts and all that good stuff.
        ///
        /// Passed in editedExecutor reference is for efficient saving / undo steps
        /// </summary>
        public void DoRenderLoop() {
            // Important note: We don't render the root model as it is always a sequence.
            // we're just adding to it if appropriate
            var mr = operatorRenderer.GetSubjectToRender();
            operatorRenderer.SetSubjects(mr);
            operatorRenderer.RenderCodeView(this);
            ProcessCorruptedModels(mr);
            DoScheduledReorders(mr);
        }

        private void ProcessCorruptedModels(UnityBtModel rootModel) {
            if (corruptedModels.Count > 0) {
                Debug.Log("Removing corrupted child element(s) from root sequence");
                // Must be done here because otherwise we'll end up with concurrent modification exceptions
                foreach (var corrupted in corruptedModels) {
                    rootModel.children.Remove(corrupted);
                }
                corruptedModels.Clear();
            }
        }

        private void DoScheduledReorders(UnityBtModel m) {
            if (m == null) {
                // That happens when a model has a default set of children but they are null
                // because they are unused, for example in the choices sequence.
                // There are 4 children but not all of them are actually something useful
                return;
            }
            // First change the child list
            m.ProcessReorders();
            // Then run through the child list to see if there are more reorders
            foreach (var child in m.children) {
                DoScheduledReorders(child);
            }
        }

        #endregion Rendering Process

        #region API

        public void RenderAddOperatorButton(UnityBtModel referenceObject) {
            DrawButton("<>", () => {
                var window = GenericPopupWindow.Popup<OperatorSelector>();
                window.SetRelativeRootModel(referenceObject);
            });
        }

        public void RenderAddOperatorButton(UnityBtModel referenceObject, int insertIndex) {
            DrawButton("<>", () => {
                var window = GenericPopupWindow.Popup<OperatorSelector>();
                window.SetRelativeRootModel(referenceObject, insertIndex);
            });
        }

        public void RenderEditOperatorButton(string label, UnityBtModel referenceObject, UnityBtModel referenceParentObject, IOperatorRenderer<UnityBtModel> operatorRenderer) {
            EditorGUILayout.BeginHorizontal();
            {
                var opr = operatorRenderer as DefaultRenderer;
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = this.IndentLevel;
                if (referenceParentObject != null) {
                    // Draw first button with indent
                    DrawButton(label, () => {
                        var window = GenericPopupWindow.Popup<OperatorEditorWindow>();
                        window.OperatorRenderer = operatorRenderer;
                        window.SequencerRenderer = this;
                        // We need to drag this along into the operator editor where it will be saved and set dirty 
                        // when the window is closed, which is the only place where data can be changed so it's a perfect fit
                        window.SequenceExecutorObject = SequenceExecutorObject;
                        window.SerializedSequenceExecutor = SerializedSequenceExecutor;
                    });
                    // Followup buttons come directly after, without extra indents
                    if (GUILayout.Button("up", EditorSkin.button, GUILayout.Width(25))) {
                        int newIndex = referenceParentObject.children.IndexOf(referenceObject) - 1;
                        referenceParentObject.ScheduleChildReorder(referenceObject, newIndex);
                        UpdateSerializedModelTree();
                    }
                    if (GUILayout.Button("dn", EditorSkin.button, GUILayout.Width(25))) {
                        int newIndex = referenceParentObject.children.IndexOf(referenceObject) + 1;
                        referenceParentObject.ScheduleChildReorder(referenceObject, newIndex);
                        UpdateSerializedModelTree();
                    }

                    if (GUILayout.Button("x", EditorSkin.button, GUILayout.Width(18))) {
                        if (referenceParentObject == null) {
                            Debug.LogError("Tried to remove a root object or corrupted model " + referenceObject.ModelClassName);
                            return;
                        }
                        if (!referenceParentObject.NullChild(referenceObject)) {
                            Debug.LogError("Failed to remove a model from its parent (but it is in there!)" + referenceObject.ModelClassName);
                        }
                        else {
                            Debug.Log("Nulled child.");
                            UpdateSerializedModelTree();
                        }
                    }
                }
                EditorGUI.indentLevel = indent;
            }
            EditorGUILayout.EndHorizontal();
        }

        public void UpdateSerializedModelTree() {
            int currentArraySize = SerializedSequenceExecutor.FindProperty("serializedModelTree.Array.size").intValue;
            SequenceExecutorObject.SerializeModelTree(); // Force update of the model tree data here
            int newArraySize = SequenceExecutorObject.SerializedModelTree.Length;
            if (newArraySize != currentArraySize)
                SerializedSequenceExecutor.FindProperty("serializedModelTree.Array.size").intValue = newArraySize;

            for (int i = 0; i < newArraySize; i++) {
                SerializedSequenceExecutor.FindProperty(string.Format("serializedModelTree.Array.data[{0}]", i)).intValue = SequenceExecutorObject.SerializedModelTree[i];
            }
            EditorUtility.SetDirty(SequenceExecutorObject);
        }

        public void RenderOperatorDummyButton(string label) {
            DrawButton(label, null);
        }

        #endregion API

        /// <summary>
        /// Internally draws buttons to achieve proper indenting,
        /// apparently the normal indenting thing doesn't work on GUILayout elements.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="onClick"></param>
        private void DrawButton(string label, Action onClick) {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(this.IndentLevel);
                if (GUILayout.Button(label, EditorSkin.button)) {
                    if (onClick != null) {
                        onClick();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}