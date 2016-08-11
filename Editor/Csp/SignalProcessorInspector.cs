using Playblack.Csp;
using UnityEditor;
using UnityEngine;

namespace Playblack.Editor.Csp {

    [CustomEditor(typeof(SignalProcessor))]
    internal class SignalProcessorInspector : UnityEditor.Editor {
        private static CspConnectorWindowOverview edWindow;

        public override void OnInspectorGUI() {
            if (GUILayout.Button("Open Outputs")) {
                if (edWindow == null) {
                    edWindow = EditorWindow.CreateInstance<CspConnectorWindowOverview>();
                }
                if (target == null) {
                    Debug.LogError("Target for sigproc is empty!");
                }
                edWindow.Prepare((SignalProcessor)target);
                edWindow.Show();
            }
        }
    }
}