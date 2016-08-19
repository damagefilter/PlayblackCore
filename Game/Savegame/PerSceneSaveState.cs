﻿using Playblack.EventSystem.Events;
using Playblack.Savegame.Model;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            var hook = new GameSavingEvent(SceneManager.GetActiveScene().name);
            hook.Call();
            string fileName = Application.persistentDataPath + "/" + SaveName + ".save";
            SaveFile sf = null;
            // save file already exists.
            if (File.Exists(fileName)) {
                sf = DataSerializer.DeserializeProtoObject<SaveFile>(ZipTools.DecompressBytes(File.ReadAllBytes(fileName)));
            }
            else {
                sf = new SaveFile(SaveName);
            }

            sf.Add(hook.SceneData);
            using (MemoryStream saveFileData = new MemoryStream(DataSerializer.SerializeProtoObject(sf))) {
                // Holds final zipped file data bytes.
                saveFileData.Position = 0;
                File.WriteAllBytes(fileName, ZipTools.CompressBytes(saveFileData.ToArray()));
            }
        }

        public IEnumerator RestoreSave(string dataId) {
            // Can use GO.Find here, this point in code has no requirement to be super fast.
            // (That also means we don't need to track them all)
            var managers = UnityEngine.Object.FindObjectsOfType<SaveManager>();
            Debug.Log("Clearing scene. Removing  " + managers.Length + " managed game objects");
            for (int i = 0; i < managers.Length; ++i) {
                // SaveManager handled objects need to be destroyed
                // as they will all be re-created from the save file.
                UnityEngine.Object.DestroyImmediate(managers[i].gameObject);
            }
            SaveFile file = null;
            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(Application.persistentDataPath + "/" + SaveName + ".save"))) {
                ms.Position = 0;
                file = DataSerializer.DeserializeProtoObject<SaveFile>(ZipTools.DecompressBytes(ms.ToArray()));
            }
            var sceneData = (SceneDataBlock)file.Get(dataId);
            Debug.Log("We have  " + sceneData.SceneObjects.Count + " objects to restore!");
            for (int i = 0; i < sceneData.SceneObjects.Count; ++i) {
                if (sceneData.SceneObjects[i].MustLoadAsset) {
                    // load async, wait for finish, then continue
                    yield return CreateGameObjectFromAsset(sceneData.SceneObjects[i]);
                }
                else {
                    CreateNewGameObject(sceneData.SceneObjects[i]);
                }
            }
            Debug.Log("All done, raising SaveGameLoadedEvent");
            // because we're waiting for all async tasks to be done, we can savely fire the loading-done event here.
            new SaveGameLoadedEvent(SaveName).Call();
        }

        public void CreateNewGameObject(GameObjectDataBlock dataBlock) {
            Debug.Log("Instantiating Raw object " + dataBlock.AssetPath);
            var go = new GameObject();
            var saveMan = go.AddComponent<SaveManager>();
            saveMan.Restore(dataBlock, true);
        }

        /// <summary>
        /// Creates the game object from assetbundle or resources path.
        /// This call may be async in which case it will return the coroutine it works on.
        /// </summary>
        /// <returns>The game object from asset.</returns>
        /// <param name="dataBlock">Data block.</param>
        public Coroutine CreateGameObjectFromAsset(GameObjectDataBlock dataBlock) {
            var hook = (RequestAssetEvent)new RequestAssetEvent(dataBlock.AssetBundle, dataBlock.AssetPath, (UnityEngine.Object loadedAsset) => {
                Debug.Log("Instantiating asset " + dataBlock.AssetPath);
                var go = (GameObject)UnityEngine.Object.Instantiate(loadedAsset);
                Debug.Log("Restoring " + dataBlock.AssetPath);
                Debug.Assert(go);
                var sm = go.GetComponent<SaveManager>();
                Debug.Assert(sm);
                sm.Restore(dataBlock, false);
            }).Call();
            return hook.AssetLoadingProcess;
        }
    }
}