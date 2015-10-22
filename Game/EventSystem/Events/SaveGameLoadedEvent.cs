﻿using Playblack.Savegame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playblack.EventSystem.Events {
    public class SaveGameLoadedEvent : Event<SaveGameLoadedEvent> {

        /// <summary>
        /// The save game we loaded from.
        /// </summary>
        /// <value>The name of the scene.</value>
        public string SaveName {
            get;
            private set;
        }

        public SaveGameLoadedEvent(string saveName) {
            this.SaveName = saveName;
        }
    }
}
