using System;
using UnityEngine;
using Playblack.EventSystem.Events;
using System.IO;
using Playblack.Savegame.Model;
using System.IO.Compression;

namespace Playblack.Savegame {
    /// <summary>
    /// Manages all registered save data and stores it in a format that saves
    /// the game state on a per-scene basis.
    /// </summary>
    public class PerSceneSaveState : ISaveState {

        public string SaveName {
            get;
            set;
        }

        public void CreateSave() {
            var hook = new GameSavingEvent(Application.loadedLevelName);
            hook.Call();
            // loads scene data
            using (MemoryStream sceneDataStream = new MemoryStream(DataSerializer.SerializeProtoObject(hook.SceneData))) {
                // Holds final zipped file data bytes.
                File.WriteAllBytes(Application.persistentDataPath + "/" + SaveName + ".save", ZipTools.CompressBytes(sceneDataStream.ToArray()));
            }
        }

        public void RestoreSave() {
            // TODO: Find all SaveManagers in scene and destroy them.
            SceneDataBlock sceneData = null;
            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(Application.persistentDataPath + "/" + SaveName + ".save"))) {
                ms.Position = 0;
                sceneData = DataSerializer.DeserializeProtoObject<SceneDataBlock>(ZipTools.DecompressBytes(ms.ToArray()));
            }
            // TODO: Do Scene Data restoring ...
            for (int i = 0; i < sceneData.SceneObjects.Count; ++i) {
                if (sceneData.SceneObjects[i].MustLoadAsset) {
                    CreateGameObjectFromAsset(sceneData.SceneObjects[i]);
                }
                else {
                    CreateNewGameObject(sceneData.SceneObjects[i]);
                }
            }
        }

        public void CreateNewGameObject(GameObjectDataBlock dataBlock) {
            var go = new GameObject();
            var saveMan = go.AddComponent<SaveManager>();
            saveMan.Restore(dataBlock, true);
        }

        public void CreateGameObjectFromAsset(GameObjectDataBlock dataBlock) {
            new RequestAssetEvent(dataBlock.AssetBundle, dataBlock.AssetPath, (UnityEngine.Object loadedAsset) => {
                var go = (GameObject)UnityEngine.Object.Instantiate(loadedAsset);
                go.GetComponent<SaveManager>().Restore(dataBlock, false);
            }).Call();
        }
    }
}

