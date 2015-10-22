using System;
using UnityEngine;
using Playblack.Savegame;

namespace Playblack.Logic {
    /// <summary>
    /// The base of all entity types.
    /// As a design note: Try to avoid entity hierarchies deeper than 3 levels.
    /// Try separating logic into components as much as possible.
    /// Generally though, there shouldn'T be more than one entity on a prefab or gameobject
    /// </summary>
    [SaveableComponent]
    public class Entity : MonoBehaviour {

        [SerializeField]private string sceneName;

        [SaveableField(SaveField.FIELD_STRING)]
        public string SceneName {
            get {
                if (string.IsNullOrEmpty(sceneName)) {
                    sceneName = gameObject.name;
                }
                return sceneName;
            }
            set {
                this.sceneName = value;
                gameObject.name = value;
            }
        }

        [SaveableField(SaveField.FIELD_VECTOR_POSITION)]
        public Vector3 Position {
            get {
                return this.transform.position;
            }
            set {
                this.transform.position = value;
            }
        }
    }
}

