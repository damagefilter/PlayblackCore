using System.Collections.Generic;
using UnityEngine;

namespace PlayBlack.Editor.Sequencer {
    public class OperatorNamespaceRegister : ScriptableObject {

        #region Unity and Static Stuff
        private static OperatorNamespaceRegister _instance;
        public static OperatorNamespaceRegister Instance {
            get {
                if (_instance == null) {
#if UNITY_EDITOR
                    if (!File.Exists("Assets/Resources/OperatorNamespaceRegister.asset")) {
                        var reg = ScriptableObject.CreateInstance<OperatorNamespaceRegister>();
                        AssetDatabase.CreateAsset(reg, "Assets/Resources/OperatorNamespaceRegister.asset");
                        AssetDatabase.SaveAssets();
                    }
#endif
                    _instance = Resources.Load<OperatorNamespaceRegister>("OperatorNamespaceRegister");
                }
                return _instance;
            }
        }

        [SerializeField]
        private List<string> aiModelNamespaces;
        public List<string> AiNamespaces {
            get {
                return aiModelNamespaces;
            }
        }

        [SerializeField]
        private List<string> sequencerModelNamespaces;
        public List<string> SequenceNamespaces {
            get {
                return sequencerModelNamespaces;
            }
        }


        [HideInInspector]
        private byte[] serializedRendererMap;

        public OperatorNamespaceRegister() {
            this.aiModelNamespaces = new List<string>() {
                "Playblack.Game.BehaviourTree.Ai"
            };

            this.sequencerModelNamespaces = new List<string>() {
                "Playblack.Game.BehaviourTree.Model"
            };
        }
        #endregion
    }
}
