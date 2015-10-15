using UnityEngine;
using System.Collections;

namespace Playblack.Savegame {
    /// <summary>
    /// A saveable component.
    /// This can be attached to prefabs and gameobjects that should be manageable by 
    /// the savegame system.
    /// </summary>
    public class Saveable : MonoBehaviour {

        [Tooltip("If non-empty the save manager will request this asset from the asset manager before restoring data.")]
        [SerializeField] private string assetPath;

        /// <summary>
        /// Used to identify this saveable and the gameobject it is attached to in the scene.
        /// </summary>
        private string uuid;
        public string UUIDS {
            get {
                return uuid;
            }
        }
        void Awake() {
            this.uuid = ((GetInstanceID() + Time.time) * UnityEngine.Random.Range(1f, 1024f)).ToString();
            // TODO: Throw event to register this at the SaveManager
        }
    }
}
