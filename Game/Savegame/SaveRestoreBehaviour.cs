using Playblack.EventSystem;
using Playblack.EventSystem.Events;
using System;
using UnityEngine;

namespace Playblack.Savegame {

    public class SaveRestoreBehaviour : MonoBehaviour {

        [Tooltip("Defines the way the game is saved. Defaults to a per-scene save. Loading between different types is not supported and leads to terrible errors!")]
        [SerializeField]
        private string saveTypeImpl = "Playblack.Savegame.PerSceneSaveState";

        private ISaveState saveStateInstance;

        private void Awake() {
            var other = FindObjectOfType<SaveRestoreBehaviour>();
            if (other != null && other != this) {
                Debug.LogWarning("Second SaveRestoreBehaviour was created. Destroying it. Only one allowed");
                Destroy(this); // not the go. might be something else, important on it.
                return;
            }
            saveStateInstance = Activator.CreateInstance(Type.GetType(saveTypeImpl)) as ISaveState;
            if (saveStateInstance == null) {
                Debug.LogError(saveTypeImpl + " is not a valid ISaveState! Cannot save the game.");
            }
            else {
                RequestSaveEvent.Register(OnSaveRequest);
                RequestSaveLoadEvent.Register(OnSavegameLoad);
            }
            DontDestroyOnLoad(this.gameObject);
        }

        private void OnDestroy() {
            RequestSaveEvent.Unregister(OnSaveRequest);
            RequestSaveLoadEvent.Unregister(OnSavegameLoad);
        }

        private void OnSaveRequest(RequestSaveEvent hook) {
            saveStateInstance.SaveName = hook.SaveName;
            saveStateInstance.CreateSave();
        }

        private void OnSavegameLoad(RequestSaveLoadEvent hook) {
            saveStateInstance.SaveName = hook.SaveName;
            hook.LoadingProcess = StartCoroutine(saveStateInstance.RestoreSave(hook.DataId));
        }
    }
}
