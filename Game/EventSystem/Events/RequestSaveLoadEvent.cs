using UnityEngine;

namespace Playblack.EventSystem.Events {
    /// <summary>
    /// Throw this to get the game state loaded under the given save with the given data id.
    /// When the event call returns you will have access to the coroutine that
    /// loads the game data via the LoadingProcess property.
    /// </summary>
    public class RequestSaveLoadEvent : Event<RequestSaveLoadEvent> {
        public string SaveName {
            get;
            private set;
        }

        public string DataId {
            get;
            private set;
        }

        public Coroutine LoadingProcess {
            get;
            set;
        } 

        public RequestSaveLoadEvent(string saveName, string dataId) {
            this.SaveName = saveName;
            this.DataId = dataId;
        }
    }
}
