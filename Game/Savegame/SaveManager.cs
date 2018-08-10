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
    [RequireComponent(typeof(UniqueId))]
    public class SaveManager : MonoBehaviour {

        [Tooltip("If non-empty the save manager will assume the resource under assetPath is from the specified assetbundle name")]
        [SerializeField]
        private string assetBundle;

        [Tooltip("Specifies a resource or a path in a specified assetBundle under which the managed GO can be found. Leave empty for raw GO regeneration")]
        [SerializeField]
        private string assetPath;

        [Tooltip("Do not clear this SaveManagers GO on restore but fill in the data from the save file on it instead.")]
        [SerializeField]
        private bool persistOnRestore;
        
        [Tooltip("If ticked stores data of child components aswell. Useful for prefabs!")]
        [SerializeField]
        private bool storeFullObjectTree;

        private UniqueId uid;

        public string UUID {
            get {
                if (uid == null) {
                    uid = GetComponent<UniqueId>();
                }

                return uid.UId;
            }
        }

        public bool PersistsOnRestore {
            get {
                return this.persistOnRestore;
            }
        }

        private void Awake() {
            // unique enough for saveable stuff. that#s all we need
            GameSavingEvent.Register(OnSave);
        }

        private void OnDestroy() {
            GameSavingEvent.Unregister(OnSave);
        }

        /// <summary>
        /// Event calback. Collects all the things in saveable components into a GameObjectDataBlock.
        /// This is the second-most level from the top.
        /// The GameObjectDataBlock will then go into a list which will finally represent the savegame of a scene.
        /// </summary>
        public void OnSave(GameSavingEvent hook) {
            Debug.Log("Saving " + gameObject.name);
            GameObjectDataBlock goBlock = new GameObjectDataBlock(UUID, gameObject.name, assetBundle, assetPath);
            
            var components = this.storeFullObjectTree ? GetComponentsInChildren<Component>() : GetComponents<Component>();
            
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
                    try {
                        switch (a.fieldType) {
                            case SaveField.FIELD_PRIMITIVE:
                            case SaveField.FIELD_SIMPLE_OBJECT:
                                componentBlock.AddSimpleObject(memberSet[j].Name, components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                                break;
                                
                            case SaveField.FIELD_PROTOBUF_OBJECT:
                                componentBlock.AddProtoObject(memberSet[j].Name, components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                                break;
                                
                            case SaveField.FIELD_COLOR:
                                componentBlock.AddColor(memberSet[j].Name, (Color)components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                                break;

                            case SaveField.FIELD_VECTOR_2:
                                componentBlock.AddVector2(memberSet[j].Name, (Vector2)components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                                break;

                            case SaveField.FIELD_VECTOR_3:
                                componentBlock.AddVector3(memberSet[j].Name, (Vector3)components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                                break;
                                
                            case SaveField.FIELD_QUATERNION:
                                componentBlock.AddQuaternion(memberSet[j].Name, (Quaternion)components[i].TryGetValue(memberSet[j].Name, Flags.InstanceAnyVisibility));
                                break;
                                
                            default:
                                // In case we have new data types and forgot to add it here for processing
                                Debug.LogError(memberSet[j].Name + " is of unhandled data type " + a.fieldType);
                                break;
                        }
                    }
                    catch (Exception e) {
                        Debug.LogError(
                            "Could not store the value from field " + memberSet[j].Name + " on component " + type + ": \n" +
                            e.Message);
                    }
                    
                }
                goBlock.AddComponentData(componentBlock);
            }
            hook.SceneData.AddGameObjectData(goBlock);
        }

        public void Restore(GameObjectDataBlock data, bool addComponents) {
            if (data == null) {
                Debug.LogError("Error restoring a component. Data is null!");
                Destroy(this.gameObject);
                return;
            }
            gameObject.name = data.SceneName;
            this.assetPath = data.AssetPath;
            this.assetBundle = data.AssetBundle;
            if (data.ComponentList == null) {
                Debug.LogError("No components to restore on " + data.SceneName);
                return;
            }
            for (int i = 0; i < data.ComponentList.Count; ++i) {
                Component component;
                var componentType = Type.GetType(data.ComponentList[i].ComponentName + "," + data.ComponentList[i].AssemblyName);
                if (componentType == null) {
                    Debug.LogError("Failed restoring component type " + data.ComponentList[i].ComponentName + " from assembly " + data.ComponentList[i].AssemblyName);
                    continue;
                }
                if (!typeof(Component).IsAssignableFrom(componentType)) {
                    Debug.LogError("Restore error: " + componentType + " was expected to be a subtype of Component but isn't! Not restoring data.");
                    continue; // Not a component :(
                }
                if (addComponents) {
                    component = this.gameObject.AddComponent(componentType);
                }
                else {
                    component = storeFullObjectTree ? this.gameObject.GetComponentInChildren(componentType) : this.gameObject.GetComponent(componentType);
                }
                if (component == null) {
                    Debug.LogError(string.Format("The component {0} does not exist on the GO named {1}. You missed marking the GO persistent or didn't add a prefab path?", componentType, this.name));
                    continue;
                }
                var memberSet = componentType.FieldsAndPropertiesWith(typeof(SaveableFieldAttribute));
                for (int j = 0; j < memberSet.Count; ++j) {
                    SaveableFieldAttribute a = memberSet[j].Attribute<SaveableFieldAttribute>();
                    try {
                        switch (a.fieldType) {
                            // Read them in as simple objects as they can be primitives or arrays of primitives.
                            // Or they can, of course, be simple objects.
                            case SaveField.FIELD_PRIMITIVE:
                            case SaveField.FIELD_SIMPLE_OBJECT:
                                component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadSimpleObject(memberSet[j].Name), Flags.InstanceAnyVisibility);
                                break;
                            case SaveField.FIELD_PROTOBUF_OBJECT:
                                component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadProtoObject(memberSet[j].Name, memberSet[j].Type()), Flags.InstanceAnyVisibility);
                                break;
                            case SaveField.FIELD_VECTOR_2:
                                component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadVector2(memberSet[j].Name), Flags.InstanceAnyVisibility);
                                break;
                            case SaveField.FIELD_VECTOR_3:
                                component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadVector3(memberSet[j].Name), Flags.InstanceAnyVisibility);
                                break;
                            case SaveField.FIELD_QUATERNION:
                                component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadQuaternion(memberSet[j].Name), Flags.InstanceAnyVisibility);
                                break;
                            case SaveField.FIELD_COLOR:
                                component.TrySetValue(memberSet[j].Name, data.ComponentList[i].ReadColor(memberSet[j].Name), Flags.InstanceAnyVisibility);
                                break;
                            default:
                                // In case we have new data types and forgot to add it here for processing
                                Debug.LogError(memberSet[j].Name + " is of unhandled data type " + a.fieldType);
                                break;
                        }
                    }
                    catch (Exception e) {
                        string str = string.Format("Restore failed on GO '{0}' for field '{1}' for component '{2}'\n{3}\nStack:\n{4}", this.name, memberSet[j].Name, componentType, e.Message, e.StackTrace);
                        Debug.LogError(str);
                    }
                    
                }
            }
        }
    }
}
