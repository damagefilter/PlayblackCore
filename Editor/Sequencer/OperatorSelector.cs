using Fasterflect;
using Playblack.BehaviourTree;
using Playblack.BehaviourTree.Model.Core;
using PlayBlack.Editor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using PlayBlack.Editor.Sequencer.Renderers;
using UnityEditor;
using UnityEngine;

namespace PlayBlack.Editor.Sequencer {

    public class OperatorSelector : GenericPopupWindow {

        /// <summary>
        /// The model to add a new child (behaviour) to
        /// </summary>
        private UnityBtModel rootModel;

        /// <summary>
        /// Used to indicate where in the model the children should go, at which index
        /// </summary>
        private int insertIndex = -1;

        /// <summary>
        /// If true, instead of overriding an operator at the given index,
        /// one will be inserted at the index, causing subsequent operators to be pushed down by one.
        /// </summary>
        private bool noOverride;

        private Vector2 scrollPos;

        /// <summary>
        /// The descriptor type (or operator type) that should be displayed.
        /// </summary>
        private DescriptorType displayedOperators;

        private static List<Type> knownAiOperators;

        private static List<Type> knownLogicOperators;

        private static List<Type> knownGameplayOperators;
        
        private static List<Type> knownGraphicsOperators;
        
        private static List<Type> knownDialogueOperators;

        public override string GetTitle() {
            return "Operator Selector";
        }

        /// <summary>
        /// If true, instead of overriding an operator at the given index,
        /// one will be inserted at the index, causing subsequent operators to be pushed down by one.
        /// </summary>
        /// <param name="noOverride"></param>
        public void SetNoOverride(bool noOverride) {
            this.noOverride = noOverride;
        }

        public override void InternalInit() {
            // TODO: These should go into static variables, it doesn't change that much
            // and should only be generated on change or if they are not generated yet
            // Very expensive this.
            // var m2r = OperatorNamespaceRegister.Instance;
            // This properly reads all the model types from everything in and below the Assets.src.ai.behaviourtree namespace
            FetchOperatorList(false);
        }

        private void FetchOperatorList(bool forceNew) {
            if (knownAiOperators == null || forceNew) {
                var aiTypes = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(t => t.GetTypes())
                                .Where(t => {
                                    bool typeValid = t.IsClass && !t.IsAbstract;
                                    var attr = t.Attribute<ModelDataDescriptorAttribute>();
                                    bool hasDescriptor = attr != null && attr.DescriptorType == DescriptorType.AI;
                                    return typeValid && hasDescriptor && t.IsSubclassOf(typeof(ModelTask));
                                });
                knownAiOperators = aiTypes.ToList();
            }

            if (knownLogicOperators == null || forceNew) {
                var sequenceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                    .Where(t => {
                        bool typeValid = t.IsClass && !t.IsAbstract;
                        var attr = t.Attribute<ModelDataDescriptorAttribute>();
                        bool hasDescriptor = attr != null && attr.DescriptorType == DescriptorType.LOGIC;
                        return typeValid && hasDescriptor && t.IsSubclassOf(typeof(ModelTask));
                    });
                knownLogicOperators = sequenceTypes.ToList();
            }

            if (knownGameplayOperators == null || forceNew) {
                var gameplayType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                    .Where(t => {
                        bool typeValid = t.IsClass && !t.IsAbstract;
                        var attr = t.Attribute<ModelDataDescriptorAttribute>();
                        bool hasDescriptor = attr != null && attr.DescriptorType == DescriptorType.GAMEPLAY;
                        return typeValid && hasDescriptor && t.IsSubclassOf(typeof(ModelTask));
                    });
                knownGameplayOperators = gameplayType.ToList();
            }
            
            if (knownGraphicsOperators == null || forceNew) {
                var gameplayType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(t => {
                        bool typeValid = t.IsClass && !t.IsAbstract;
                        var attr = t.Attribute<ModelDataDescriptorAttribute>();
                        bool hasDescriptor = attr != null && attr.DescriptorType == DescriptorType.GRAPHICS;
                        return typeValid && hasDescriptor && t.IsSubclassOf(typeof(ModelTask));
                    });
                knownGraphicsOperators = gameplayType.ToList();
            }
            
            if (knownDialogueOperators == null || forceNew) {
                var gameplayType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(t => {
                        bool typeValid = t.IsClass && !t.IsAbstract;
                        var attr = t.Attribute<ModelDataDescriptorAttribute>();
                        bool hasDescriptor = attr != null && attr.DescriptorType == DescriptorType.DIALOGUE;
                        return typeValid && hasDescriptor && t.IsSubclassOf(typeof(ModelTask));
                    });
                knownDialogueOperators = gameplayType.ToList();
            }
        }

        public void OnGUI() {
            if (OperatorClipboard.HasContent()) {
                if (GUILayout.Button("Insert from clipboard")) {
                    InsertFromClipboard();
                }
            }
            EditorGUILayout.BeginHorizontal();
            {
                foreach (DescriptorType t in Enum.GetValues(typeof(DescriptorType))) {
                    if (GUILayout.Button("Show " + t.ToString() + " Operators")) {
                        this.displayedOperators = t;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            ShowOptions();
        }

        private void ShowOptions() {
            if (knownAiOperators == null || knownLogicOperators == null || knownGameplayOperators == null ||knownGraphicsOperators == null || knownDialogueOperators == null) {
                EditorGUILayout.HelpBox("Window wasn't initialised. Reopen it ...?", MessageType.Error);
                return;
            }

            List<Type> models = null;// = (showAi ? this.knownAiOperators : this.knownLogicOperators);
            switch (displayedOperators) {
                case DescriptorType.AI:
                    models = knownAiOperators;
                    break;

                case DescriptorType.LOGIC:
                    models = knownLogicOperators;
                    break;

                case DescriptorType.GAMEPLAY:
                    models = knownGameplayOperators;
                    break;
                case DescriptorType.GRAPHICS:
                    models = knownGraphicsOperators;
                    break;
                case DescriptorType.DIALOGUE:
                    models = knownDialogueOperators;
                    break;
            }
            if (models == null) {
                EditorGUILayout.HelpBox("No operators to display ...", MessageType.Warning);
                return;
            }
            EditorGUILayout.LabelField("Showing operators for: " + this.displayedOperators);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            {
                EditorGUILayout.BeginVertical();
                {
                    foreach (var t in models) {
                        if (GUILayout.Button(t.Attribute<ModelDataDescriptorAttribute>().OperatorName)) {
                            CreateNewModel(t.ToString()); // Also takes care of parenting child objects and setting context field data etc etc
                            Close();
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private void CreateNewModel(string className) {
            UnityBtModel newModel = null;
            if (noOverride) {
                newModel = UnityBtModel.NewInstance(className);
                if (insertIndex >= 0) {
                    rootModel.ScheduleChildReorder(newModel, insertIndex);
                }
                else {
                    // force appending
                    rootModel.ScheduleChildReorder(newModel, rootModel.children.Count+1);
                }
            }
            else {
                // NewInstance does all the things we need to ensure data integrity
                if (insertIndex >= 0) {
                    newModel = UnityBtModel.NewInstance(rootModel, new UnityBtModel(), className, insertIndex);
                }
                else {
                    newModel = UnityBtModel.NewInstance(rootModel, new UnityBtModel(), className);
                }
            }
        }

        private void InsertFromClipboard() {
            var newModel = OperatorClipboard.Paste();
            if (insertIndex >= 0) {
                rootModel.ScheduleChildReorder(newModel, insertIndex);
            }
            else {
                // force appending
                rootModel.ScheduleChildReorder(newModel, rootModel.children.Count+1);
            }
        }

        /// <summary>
        /// If new sequences are added via this sequence selector, they'll be attached as children to the given model.
        /// Hence the name relative root model.
        /// </summary>
        /// <param name="model"></param>
        public void SetRelativeRootModel(UnityBtModel model) {
            this.rootModel = model;
            this.InternalInit();
        }

        /// <summary>
        /// If new sequences are added via this sequence selector, they'll be attached as children to the given model.
        /// Hence the name relative root model.
        /// Additionaly you can specify the index at which the new model should be inserted.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="insertIndex"></param>
        public void SetRelativeRootModel(UnityBtModel model, int insertIndex) {
            this.rootModel = model;
            this.insertIndex = insertIndex;
            this.InternalInit();
        }
    }
}
