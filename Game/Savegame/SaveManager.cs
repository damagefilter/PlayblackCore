using Fasterflect;
using Playblack.EventSystem;
using Playblack.EventSystem.Events;
using Playblack.Savegame.Model;
using System;
using UnityEngine;

namespace Playblack.Savegame {

    /// <summary>
    /// The SaveManager.
    /// This can be attached to prefabs and gameobjects that should be manageable by
    /// the savegame system.
    /// </summary>
    public class SaveManager : MonoBehaviour {

        [Tooltip("If non-empty the save manager will assume the resource under assetPath is from the specified assetbundle name")]
        [SerializeField]
        private string assetBundle;

        [Tooltip("Specifies a resource or a path in a specified assetBundle under which the managed GO can be found. Leave empty for raw GO regeneration")]
        [SerializeField]
        private string assetPath;

        private string uuid;

        public string UUID {
            get {
                return uuid;
            }
        }

        private void Awake() {
            this.uuid = ((GetInstanceID() + Time.time) * UnityEngine.Random.Range(1f, 1024f)).ToString();
            EventDispatcher.Instance.Register<GameSavingEvent>(OnSave);
        }

        private void OnDestroy() {
            EventDispatcher.Instance.Unregister<GameSavingEvent>(OnSave);
        }

        /// <summary>
        /// Event calback. Collects all the things in saveable components into a GameObjectDataBlock.
        /// This is the second-most level from the top.
        /// The GameObjectDataBlock will then go into a list which will finally represent the savegame of a scene.
        /// </summary>
        public void OnSave(GameSavingEvent hook) {
            GameObjectDataBlock goBlock = new GameObjectDataBlock(uuid, gameObject.name, assetBundle, assetPath);
            var components = GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i) {
                // Ignore components that are not to be saved.
                if (!components[i].GetType().IsDefined(typeof(SaveableComponentAttribute), true)) {
                    continue;
                }
                var componentBlock = new ComponentDataBlock(components[i].GetType().ToString(), components[i].GetType().Assembly.GetName().Name);
                // Find all fields and properties that need saving.
                var type = components[i].GetType();
                var memberSet = type.FieldsAndPropertiesWith(typeof(SaveableFieldAttribute));
                for (int j = 0; j < memberSet.Count; ++j) {
                    SaveableFieldAttribute a = memberSet[j].Attribute<SaveableFieldAttribute>();
                    switch (a.fieldType) {
                        case SaveField.FIELD_COLOR:
                            componentBlock.AddColor(memberSet[j].Name, (Color)components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                            break;

                        case SaveField.FIELD_FLOAT:
                            componentBlock.AddFloat(memberSet[j].Name, (float)components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                            break;
                        case SaveField.FIELD_BOOL:
                            componentBlock.AddBoolean(memberSet[j].Name, (bool)components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                            break;

                        case SaveField.FIELD_INT:
                            componentBlock.AddInt(memberSet[j].Name, (int)components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                            break;

                        case SaveField.FIELD_PROTOBUF_OBJECT:
                            componentBlock.AddProtoObject(memberSet[j].Name, components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                            break;

                        case SaveField.FIELD_SIMPLE_OBJECT:
                            componentBlock.AddSimpleObject(memberSet[j].Name, components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                            break;

                        case SaveField.FIELD_STRING:
                            componentBlock.AddString(memberSet[j].Name, (string)components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                            break;

                        case SaveField.FIELD_VECTOR_POSITION:
                            componentBlock.AddVector(memberSet[j].Name, (Vector3)components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                            break;
                        default:
                            // In case we have new data types and forgot to add it here for processing
                            Debug.LogError(memberSet[j].Name + " is of unhandled data type " + a.fieldType);
                            break;
                    }
                }
                goBlock.AddComponentData(componentBlock);
            }
            hook.SceneData.AddGameObjectData(goBlock);
        }

        public void Restore(GameObjectDataBlock data, bool addComponents) {
            this.uuid = data.UUID;
            gameObject.name = data.SceneName;
            this.assetPath = data.AssetPath;
            this.assetBundle = data.AssetBundle;
            for (int i = 0; i < data.ComponentList.Count; ++i) {
                Component component = null;
                var componentType = Type.GetType(data.ComponentList[i].ComponentName + "," + data.ComponentList[i].AssemblyName);
                Debug.Log("Restoring component type " + data.ComponentList[i].ComponentName + " from assembly " + data.ComponentList[i].AssemblyName);
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

                var memberSet = componentType.FieldsAndPropertiesWith(typeof(SaveableFieldAttribute));
                for (int j = 0; j < memberSet.Count; ++j) {
                    SaveableFieldAttribute a = memberSet[j].Attribute<SaveableFieldAttribute>();
                    switch (a.fieldType) {
                        case SaveField.FIELD_COLOR:
                            component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadColor(memberSet[j].Name), Flags.InstanceAnyVisibility);
                            break;

                        case SaveField.FIELD_FLOAT:
                            component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadFloat(memberSet[j].Name), Flags.InstanceAnyVisibility);
                            break;

                        case SaveField.FIELD_INT:
                            component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadInt(memberSet[j].Name), Flags.InstanceAnyVisibility);
                            break;
                        case SaveField.FIELD_BOOL:
                            component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadBool(memberSet[j].Name), Flags.InstanceAnyVisibility);
                            break;

                        case SaveField.FIELD_PROTOBUF_OBJECT:
                            component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadProtoObject(memberSet[j].Name, memberSet[j].Type()), Flags.InstanceAnyVisibility);
                            break;

                        case SaveField.FIELD_SIMPLE_OBJECT:
                            component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadSimpleObject(memberSet[j].Name), Flags.InstanceAnyVisibility);
                            break;

                        case SaveField.FIELD_STRING:
                            component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadString(memberSet[j].Name), Flags.InstanceAnyVisibility);
                            break;

                        case SaveField.FIELD_VECTOR_POSITION:
                            component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadVector(memberSet[j].Name), Flags.InstanceAnyVisibility);
                            break;
                        default:
                            // In case we have new data types and forgot to add it here for processing
                            Debug.LogError(memberSet[j].Name + " is of unhandled data type " + a.fieldType);
                            break;
                    }
                }
            }
        }
    }
}