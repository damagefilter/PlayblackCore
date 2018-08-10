using Playblack.Savegame;
using UnityEditor;
using UnityEngine;

namespace PlayBlack.Editor.Savegame {
    [CustomEditor(typeof(UniqueId))]
    public class UniqueIdInspector : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            if (target == null) {
                return;
            }
            base.OnInspectorGUI();
            if (GUILayout.Button("Re-assign UID")) {
                UniqueId id = target as UniqueId;
                if (id != null) {
                    id.CreateUid();
                }
            }
        }
    }
}
