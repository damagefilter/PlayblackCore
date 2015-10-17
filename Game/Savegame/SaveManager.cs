using UnityEngine;
using Playblack.Savegame.Model;
using FastMember;
using Playblack.EventSystem;
using Playblack.EventSystem.Events;

namespace Playblack.Savegame {
    /// <summary>
    /// The SaveManager.
    /// This can be attached to prefabs and gameobjects that should be manageable by 
    /// the savegame system.
    /// </summary>
    public class SaveManager : MonoBehaviour {

        [Tooltip("If non-empty the save manager will look in this asset bundle for the asset found under assetPath.")]
        [SerializeField] private string assetBundle;

        [Tooltip("If non-empty the save manager will request this asset from the asset manager before restoring data.")]
        [SerializeField] private string assetPath;

        private string uuid;
        public string UUIDS {
            get {
                return uuid;
            }
        }
        void Awake() {
            this.uuid = ((GetInstanceID() + Time.time) * UnityEngine.Random.Range(1f, 1024f)).ToString();
            EventDispatcher.Instance.Register<GameSavingEvent>(OnSave);
        }

        void OnDestroy() {
            EventDispatcher.Instance.Unregister<GameSavingEvent>(OnSave);
        }

        /// <summary>
        /// Event calback. Collects all the things in saveable components into a GameObjectDataBlock.
        /// This is the second-most level from the top.
        /// The GameObjectDataBlock will then go into a list which will finally represent the savegame of a scene.
        /// </summary>
        public void OnSave(GameSavingEvent hook) {
            GameObjectDataBlock goBlock = new GameObjectDataBlock(uuid, assetBundle, assetPath);
            var components = GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i) {
                // Ignore components that are not to be saved.
                if (!components[i].GetType().IsDefined(typeof(SaveableComponentAttribute), true)) {
                    continue;
                }
                var componentBlock = new ComponentDataBlock();
                // Find all fields and properties that need saving.
                var accessor = TypeAccessor.Create(components[i].GetType());
                var memberSet = accessor.GetMembers();
                for (int j = 0; j < memberSet.Count; ++j) {
                    if (!memberSet[i].IsDefined(typeof(SaveableFieldAttribute))) {
                        continue;
                    }
                    var attribs = memberSet[i].Type.GetCustomAttributes(typeof(SaveableFieldAttribute), true);
                    SaveableFieldAttribute a = (SaveableFieldAttribute)attribs[0];
                    switch (a.fieldType) {
                        case SaveField.FIELD_COLOR:
                            componentBlock.AddColor(memberSet[i].Name, (Color)accessor[components[i], memberSet[i].Name]);
                            break;
                        case SaveField.FIELD_FLOAT:
                            componentBlock.AddFloat(memberSet[i].Name, (float)accessor[components[i], memberSet[i].Name]);
                            break;
                        case SaveField.FIELD_INT:
                            componentBlock.AddInt(memberSet[i].Name, (int)accessor[components[i], memberSet[i].Name]);
                            break;
                        case SaveField.FIELD_PROTOBUF_OBJECT:
                            componentBlock.AddProtoObject(memberSet[i].Name, accessor[components[i], memberSet[i].Name]);
                            break;
                        case SaveField.FIELD_SIMPLE_OBJECT:
                            componentBlock.AddSimpleObject(memberSet[i].Name, accessor[components[i], memberSet[i].Name]);
                            break;
                        case SaveField.FIELD_STRING:
                            componentBlock.AddString(memberSet[i].Name, (string)accessor[components[i], memberSet[i].Name]);
                            break;
                        case SaveField.FIELD_VECTOR_POSITION:
                            componentBlock.AddVector(memberSet[i].Name, (Vector3)accessor[components[i], memberSet[i].Name]);
                            break;
                    }
                }
                goBlock.AddComponentData(componentBlock);
            }
            hook.SceneData.AddGameObjectData(goBlock);
        }
    }
}
