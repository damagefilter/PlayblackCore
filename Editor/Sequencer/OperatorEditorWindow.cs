﻿using System;
using Playblack.BehaviourTree;
using Playblack.Sequencer;
using PlayBlack.Editor.Sequencer.Renderers;
using PlayBlack.Editor.Sequencer.Renderers.Bt;
using PlayBlack.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace PlayBlack.Editor.Sequencer {

    /// <summary>
    /// Renders the window where you can edit one operaot in the sequence.
    /// Called from within peusocode editor when user presses a button.
    /// </summary>
    public class OperatorEditorWindow : GenericPopupWindow {

        public IOperatorRenderer<UnityBtModel> OperatorRenderer {
            get; set;
        }

        public ISequencerRenderer<UnityBtModel> SequencerRenderer {
            get; set;
        }

        public SerializedObject SerializedSequenceExecutor { get; set; }
        public SequenceExecutor SequenceExecutorObject { get; set; }

        public override string GetTitle() {
            return "Operator Editor";
        }

        public override void InternalInit() {
            // otImplementedException();
        }

        public void OnGUI() {
            if (this.OperatorRenderer != null) {
                this.OperatorRenderer.RenderEditorWindowView(SequencerRenderer);
            }
        }

        public void OnDestroy() {
            // Record the change of the whole thing. Seems a lil wasteful but ... yeah. Huh. Unity.
            // NOTE: This Undo ain't actually working because it acts on the serialized data
            // whereas the stuff displayed in the editors is taken from the current in-memory
            // representation of the model tree (the real thing, that is)
            // BUT: This will scratch the right itch in Unity to make it save the damn thing.
            OperatorRenderer.UpdateCodeView();
            try {
                (SequencerRenderer as BtSequencerRenderer).IsDirty = true;
            }
            catch(Exception e) {
                Debug.LogError("Oh boy something went wrong when setting the sequencer dirty ...");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

        }
    }
}
