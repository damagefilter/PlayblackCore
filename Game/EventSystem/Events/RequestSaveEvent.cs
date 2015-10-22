namespace Playblack.EventSystem.Events {
    /// <summary>
    /// Throw this to get the game state saved.
    /// May not work when there is no valid SavingBehaviour in the scene.
    /// </summary>
    public class RequestSaveEvent : Event<RequestSaveEvent> {
        public string SaveName {
            get;
            private set;
        }

        public RequestSaveEvent(string saveName) {
            this.SaveName = saveName;
        }
    }
}
