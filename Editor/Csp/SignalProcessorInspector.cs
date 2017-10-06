using Playblack.Csp;
using UnityEditor;
using UnityEngine;

namespace Playblack.Editor.Csp {

    [CustomEditor(typeof(SignalProcessor))]
    internal class SignalProcessorInspector : UnityEditor.Editor {
        private static CspConnectorWindowOverview edWindow;

        public override void OnInspectorGUI() {
            if (GUILayout.Button("Open Outputs")) {
                OpenCspEditorWindow((SignalProcessor)target);
            }
        }

        public static void OpenCspEditorWindow(SignalProcessor target, SignalProcessor previousTarget = null) {
            if (edWindow == null) {
                edWindow = CreateInstance<CspConnectorWindowOverview>();
            }
            else {
                edWindow.Close();
                edWindow = CreateInstance<CspConnectorWindowOverview>();
            }
            if (target == null) {
                Debug.LogError("Target for sigproc is empty!");
                return;
            }
            Debug.Log("Switching target to " + target.name);
            Debug.Log("Previous? " + (previousTarget != null ? previousTarget.name : "none"));
            edWindow.Prepare(target);
            edWindow.SetPreviousTarget(previousTarget);
            edWindow.Show();
        }
    }
}
