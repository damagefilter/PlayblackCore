using System;
using UnityEngine;
using Playblack.EventSystem.Events;
using System.IO;
using Playblack.Savegame.Model;
using System.IO.Compression;

namespace Playblack.Savegame {
    /// <summary>
    /// This handles saving and restoring of scene data.
    /// This is not a saveable thing.
    /// </summary>
    public class SaveState : MonoBehaviour {
        [SerializeField] private string saveName;

        public string SaveName {
            get {
                return saveName;
            }
            set {
                this.saveName = value;
            }
        }

        public void IssueSave() {
            var hook = new GameSavingEvent(Application.loadedLevel);
            hook.Call();
            // loads scene data
            using (MemoryStream sceneDataStream = new MemoryStream(DataSerializer.SerializeProtoObject(hook.SceneData))) {
                // Holds final zipped file data bytes.
                var sceneDataBytes = sceneDataStream.ToArray();
                using (MemoryStream fileStream = new MemoryStream()) {
                    // zip will write compressed sceneDataBytes into the FileStream
                    using (GZipStream zip = new GZipStream(fileStream, CompressionMode.Compress)) {
                        zip.Write(sceneDataBytes, 0, sceneDataBytes.Length);
                        fileStream.Position = 0;
                        // And fileSream will be dumped into a file.
                        // And this should avoid any crazy-ass file-locking issues.
                        File.WriteAllBytes(Application.persistentDataPath + "/" + saveName + ".save", fileStream.ToArray());
                    }
                }
            }
        }
    }
}

