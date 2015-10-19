using UnityEngine;
using Playblack.Savegame.Model;
using FastMember;
using Playblack.EventSystem;
using Playblack.EventSystem.Events;
using System;

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
        public string UUID {
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
                var componentBlock = new ComponentDataBlock(components[i].GetType().ToString());
                // Find all fields and properties that need saving.
                var accessor = TypeAccessor.Create(components[i].GetType());
                var memberSet = accessor.GetMembers();
                for (int j = 0; j < memberSet.Count; ++j) {
                    if (!memberSet[j].IsDefined(typeof(SaveableFieldAttribute))) {
                        continue;
                    }
                    var attribs = memberSet[j].Type.GetCustomAttributes(typeof(SaveableFieldAttribute), true);
                    SaveableFieldAttribute a = (SaveableFieldAttribute)attribs[0];
                    switch (a.fieldType) {
                        case SaveField.FIELD_COLOR:
                            componentBlock.AddColor(memberSet[j].Name, (Color)accessor[components[i], memberSet[j].Name]);
                            break;
                        case SaveField.FIELD_FLOAT:
                            componentBlock.AddFloat(memberSet[j].Name, (float)accessor[components[i], memberSet[j].Name]);
                            break;
                        case SaveField.FIELD_INT:
                            componentBlock.AddInt(memberSet[j].Name, (int)accessor[components[i], memberSet[j].Name]);
                            break;
                        case SaveField.FIELD_PROTOBUF_OBJECT:
                            componentBlock.AddProtoObject(memberSet[j].Name, accessor[components[i], memberSet[j].Name]);
                            break;
                        case SaveField.FIELD_SIMPLE_OBJECT:
                            componentBlock.AddSimpleObject(memberSet[j].Name, accessor[components[i], memberSet[j].Name]);
                            break;
                        case SaveField.FIELD_STRING:
                            componentBlock.AddString(memberSet[j].Name, (string)accessor[components[i], memberSet[j].Name]);
                            break;
                        case SaveField.FIELD_VECTOR_POSITION:
                            componentBlock.AddVector(memberSet[j].Name, (Vector3)accessor[components[i], memberSet[j].Name]);
                            break;
                    }
                }
                goBlock.AddComponentData(componentBlock);
            }
            hook.SceneData.AddGameObjectData(goBlock);
        }

        public void Restore(GameObjectDataBlock data, bool addComponents) {
            this.uuid = data.UUID;
            this.assetPath = data.AssetPath;
            this.assetBundle = data.AssetBundle;
            for (int i = 0; i < data.ComponentList.Count; ++i) {
                Component component = null;
                var componentType = Type.GetType(data.ComponentList[i].ComponentName);
                if (!typeof(Component).IsAssignableFrom(componentType)) {
                    Debug.LogError("Restore error: " + componentType + " was expected to be a subtype of Component but isn't! Not restoring data.");
                    continue; // Not a component :(
                }
                if (addComponents) {
                    component = this.gameObject.AddComponent(componentType);
                }
                else {
                    component = this.gameObject.GetComponent(componentType);
                }

                var accessor = TypeAccessor.Create(componentType);
                var memberSet = accessor.GetMembers();
                for (int j = 0; j < memberSet.Count; ++j) {
                    if (!memberSet[j].IsDefined(typeof(SaveableFieldAttribute))) {
                        continue;
                    }
                    var attribs = memberSet[j].Type.GetCustomAttributes(typeof(SaveableFieldAttribute), true);
                    SaveableFieldAttribute a = (SaveableFieldAttribute)attribs[0];
                    switch (a.fieldType) {
                        case SaveField.FIELD_COLOR:
                            accessor[component, memberSet[j].Name] = data.ComponentList[i].ReadColor(memberSet[j].Name);
                            break;
                        case SaveField.FIELD_FLOAT:
                            accessor[component, memberSet[j].Name] = data.ComponentList[i].ReadFloat(memberSet[j].Name);
                            break;
                        case SaveField.FIELD_INT:
                            accessor[component, memberSet[j].Name] = data.ComponentList[i].ReadInt(memberSet[j].Name);
                            break;
                        case SaveField.FIELD_PROTOBUF_OBJECT:
                            accessor[component, memberSet[j].Name] = data.ComponentList[i].ReadProtoObject(memberSet[j].Name, memberSet[j].Type);
                            break;
                        case SaveField.FIELD_SIMPLE_OBJECT:
                            accessor[component, memberSet[j].Name] = data.ComponentList[i].ReadSimpleObject(memberSet[j].Name);
                            break;
                        case SaveField.FIELD_STRING:
                            accessor[component, memberSet[j].Name] = data.ComponentList[i].ReadString(memberSet[j].Name);
                            break;
                        case SaveField.FIELD_VECTOR_POSITION:
                            accessor[component, memberSet[j].Name] = data.ComponentList[i].ReadVector(memberSet[j].Name);
                            break;
                    }
                }
            }
        }
    }
}
