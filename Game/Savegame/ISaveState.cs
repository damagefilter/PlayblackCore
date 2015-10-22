using System;
using System.Collections;

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
        /// This task may be asynchronous, which is why it returns an IEnumerator.
        /// If the task is running synchronous, IEnumerator may be null.
        /// However, you should, merely by the nature of such tasks, always start it as coroutine.
        /// </summary>
        /// <param name="dataId">the identifier which part of the save file you want to load.</param>
        IEnumerator RestoreSave(string dataId);
    }
}

