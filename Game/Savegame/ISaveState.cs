using System;

namespace Playblack.Savegame {
    public interface ISaveState {
        string SaveName {
            get;
            set;
        }
        /// <summary>
        /// Creates the save file for the current game state.
        /// The save file name is determined by the SaveName property.
        /// </summary>
        void CreateSave();

        /// <summary>
        /// Restores the game state that was stored in the save file specified by SaveName.
        /// </summary>
        void RestoreSave();
    }
}

