﻿using Playblack.BehaviourTree;
using Playblack.BehaviourTree.Model.Core;
using PlayBlack.Editor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlayBlack.Editor.Sequencer {
    public class OperatorSelector : GenericPopupWindow {

        /// <summary>
        /// The model to add a new child (behaviour) to
        /// </summary>
        private UnityBtModel model;

        /// <summary>
        /// Used to indicate where in the model the children should go, at which index
        /// </summary>
        private int insertIndex = -1;

        private Vector2 scrollPos;

        private bool showAi = false;

        private List<Type> knownAiModels;

        private List<Type> knownSequenceModels;

        public override string GetTitle() {
            return "Operator Selector";
        }

        public override void InternalInit() {
            var m2r = OperatorNamespaceRegister.Instance;
            // This properly reads all the model types from everything in and below the Assets.src.ai.behaviourtree namespace
            var aiTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                    .Where(t => {
                        bool typeValid = t.IsClass && !t.IsAbstract;
                        bool nameValid = false;
                        for (int i = 0; i < m2r.AiNamespaces.Count; ++i) {
                            nameValid |= t.FullName.StartsWith(m2r.AiNamespaces[i]);
                        }
                        return typeValid && nameValid && t.IsSubclassOf(typeof(ModelTask));
                    });
            this.knownAiModels = aiTypes.ToList();

            var sequenceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                    .Where(t => {
                        bool typeValid = t.IsClass && !t.IsAbstract;
                        bool nameValid = false;
                        for (int i = 0; i < m2r.SequenceNamespaces.Count; ++i) {
                            nameValid |= t.FullName.StartsWith(m2r.SequenceNamespaces[i]);
                        }
                        return typeValid && nameValid && t.IsSubclassOf(typeof(ModelTask));
                    });
            this.knownSequenceModels = sequenceTypes.ToList();
        }

        public void OnGUI() {
            showAi = EditorGUILayout.Toggle("Show AI Operators", showAi);
            ShowOptions(showAi);
        }

        private void ShowOptions(bool showAi) {
            if (this.knownAiModels == null || this.knownSequenceModels == null) {
                EditorGUILayout.HelpBox("Window wasn't initialised. Reopen it ...?", MessageType.Error);
                return;
            }
            var models = (showAi ? this.knownAiModels : this.knownSequenceModels);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            {
                EditorGUILayout.BeginVertical();
                {
                    foreach (var t in models) {
                        if (GUILayout.Button(t.Name)) {
                            var newModel = CreateNewModel();
                            newModel.ModelClassName = t.ToString();
                            var proposedFields = newModel.GetProposedFields();
                            if (proposedFields != null) {
                                newModel.contextData.AddRange(proposedFields);
                            }
                            Close();
                        }
                    }

                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private UnityBtModel CreateNewModel() {
            UnityBtModel newModel = null;
            if (insertIndex >= 0) {
                newModel = UnityBtModel.NewInstance(model, new UnityBtModel(), insertIndex);
            }
            else {
                newModel = UnityBtModel.NewInstance(model, new UnityBtModel());
            }
            return newModel;
        }

        /// <summary>
        /// If new sequences are added via this sequence selector, they'll be attached as children to the given model.
        /// Hence the name relative root model.
        /// </summary>
        /// <param name="model"></param>
        public void SetRelativeRootModel(UnityBtModel model) {
            this.model = model;
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
            this.model = model;
            this.insertIndex = insertIndex;
            this.InternalInit();
        }
    }
}
