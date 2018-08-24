using System;

namespace Playblack.Savegame {
    public class SaveGameException : Exception {
        public SaveGameException(string msg) : base(msg) {
        }

        public SaveGameException(string msg, Exception cause) : base(msg, cause) {
        }
    }
}
