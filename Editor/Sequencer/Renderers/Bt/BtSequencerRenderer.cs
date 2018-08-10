using Playblack.BehaviourTree;
using PlayBlack.Editor.Windows;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Playblack.Sequencer;
using PlayBlack.Editor.Extensions;

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

        private bool isDirty;

        /// <summary>
        /// Dirty flag is used in the sequencereditorwindow to check if model tree needs updating.
        /// the UpdateModelTree method will then take care of all else
        /// </summary>
        public bool IsDiry {
            get {
                return isDirty;
            }
            set {
                isDirty = value;
            }
        }

        private EditorWindow hostWindow;

        public BtSequencerRenderer(EditorWindow hostWindow) {
            this.corruptedModels = new List<UnityBtModel>();
            if (editorSkin == null) {
                editorSkin = Resources.Load<GUISkin>("SequenceEditorLight");
            }

            this.hostWindow = hostWindow;
        }

        #region Rendering Process

        public int IndentLevel {
            get;
            set;
        }

        public DefaultOperatorRenderer OperatorRenderer {
            get {
                return operatorRenderer;
            }

            set {
                this.operatorRenderer = value;
            }
        }

        public SerializedObject SerializedSequenceExecutor { get; set; }
        public SequenceExecutor SequenceExecutorObject { get; set; }

        private DefaultOperatorRenderer operatorRenderer = new DefaultOperatorRenderer();

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
                    hostWindow.Repaint();
                }
                corruptedModels.Clear();
                isDirty = true;
            }
        }

        private void DoScheduledReorders(UnityBtModel m) {
            if (m == null) {
                // That happens when a model has a default set of children but they are null
                // because they are unused, for example in the choices sequence.
                // There are 4 children but not all of them are actually something useful
                return;
            }
            if (m.NeedsReorders) {
                isDirty = true;
                m.ProcessReorders();
                hostWindow.Repaint();
            }
            // Check the children if they need reordering.
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
                window.SetNoOverride(false);
            });
        }

        public void RenderAddOperatorButton(UnityBtModel referenceObject, int insertIndex) {
            DrawButton("<>", () => {
                var window = GenericPopupWindow.Popup<OperatorSelector>();
                window.SetRelativeRootModel(referenceObject, insertIndex);
                window.SetNoOverride(false);
            });
        }

        public void RenderEditOperatorButton(string label, UnityBtModel referenceObject, UnityBtModel referenceParentObject, IOperatorRenderer<UnityBtModel> operatorRenderer) {
            EditorGUILayout.BeginHorizontal();
            {
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
                        isDirty = true;
                    }
                    if (GUILayout.Button("dn", EditorSkin.button, GUILayout.Width(25))) {
                        int newIndex = referenceParentObject.children.IndexOf(referenceObject) + 1;
                        referenceParentObject.ScheduleChildReorder(referenceObject, newIndex);
                        isDirty = true;
                    }
                    // TODO: Also do copy?
                    if (GUILayout.Button("ct", EditorSkin.button, GUILayout.Width(25))) {
                        // cut this operator out.
                        // if operator clipboard has content, its gonna be available in the selector window.
                        // so you can insert new things or cut / paste like that.
                        // sames goes for the default "add operator" button.
                        var copyOfRefObj = referenceObject.Copy();
                        if (!OperatorClipboard.TryStore(copyOfRefObj)) {
                            var result = EditorUtility.DisplayDialog(
                                "Clipboard content override",
                                "There is something in the clipboard you have cut / copied but not pasted yet. You wanna override?",
                                "Yes, override", "No, wait!");
                            if (result) {
                                OperatorClipboard.ForceStore(copyOfRefObj);
                                referenceParentObject.NullChild(referenceObject);
                                isDirty = true;
                            }
                        }
                        else {
                            OperatorClipboard.ForceStore(copyOfRefObj);
                            referenceParentObject.NullChild(referenceObject);
                            isDirty = true;
                        }
                    }

                    // Works only on things without a fixed child structure which
                    // is indicated by proposedchildren being negative.
                    // That is to retain data integrity
                    if (referenceParentObject.GetProposedNumChildren() < 0) {
                        if (GUILayout.Button("/\\", EditorSkin.button, GUILayout.Width(25))) {
                            // insert above
                            var window = GenericPopupWindow.Popup<OperatorSelector>();
                            window.SetRelativeRootModel(referenceParentObject, referenceParentObject.children.IndexOf(referenceObject));
                            window.SetNoOverride(true);
                        }
                        if (GUILayout.Button("\\/", EditorSkin.button, GUILayout.Width(25))) {
                            // insert below
                            var window = GenericPopupWindow.Popup<OperatorSelector>();
                            window.SetRelativeRootModel(referenceParentObject, referenceParentObject.children.IndexOf(referenceObject) + 1);
                            window.SetNoOverride(true);
                        }
                    }
                    if (GUILayout.Button("x", EditorSkin.button, GUILayout.Width(18))) {
                        if (!referenceParentObject.NullChild(referenceObject)) {
                            Debug.LogError("Failed to remove a model from its parent (but it is in there!)" + referenceObject.ModelClassName);
                        }
                        else {
                            Debug.Log("Nulled child.");
                            isDirty = true;
                        }
                    }
                }
                EditorGUI.indentLevel = indent;
            }
            EditorGUILayout.EndHorizontal();
        }

        public void UpdateSerializedModelTree() {
            Undo.RecordObject(SequenceExecutorObject, "Serializing behaviour tree");
            isDirty = false;
//            int currentArraySize = SerializedSequenceExecutor.FindProperty("serializedModelTree.Array.size").intValue;
            SequenceExecutorObject.SerializeModelTree(); // Force update of the model tree data here
//            var modelTree = SequenceExecutorObject.GetSerializedModelTree();
//            int newArraySize = modelTree.Length;
//            if (newArraySize != currentArraySize) {
//                SerializedSequenceExecutor.FindProperty("serializedModelTree.Array.size").intValue = newArraySize;
//            }
//
//            var arrayProp = SerializedSequenceExecutor.FindProperty("serializedModelTree.Array");
//            for (int i = 0; i < newArraySize; i++) {
//                arrayProp.FindPropertyRelative($"data[{i}]").intValue = modelTree[i];
//                //SerializedSequenceExecutor.FindProperty(string.Format("serializedModelTree.Array.data[{0}]", i)).intValue = modelTree[i];
//            }
            
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
