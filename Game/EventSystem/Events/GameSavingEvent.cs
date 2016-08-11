using Playblack.Savegame.Model;

namespace Playblack.EventSystem.Events {

    public class GameSavingEvent : Event<GameSavingEvent> {

        public SceneDataBlock SceneData {
            get;
            private set;
        }

        public GameSavingEvent(string sceneName) {
            this.SceneData = new SceneDataBlock(sceneName);
        }
    }
}