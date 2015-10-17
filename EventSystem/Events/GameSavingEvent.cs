using Playblack.Savegame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
