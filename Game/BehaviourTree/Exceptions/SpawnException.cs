using System;

namespace Playblack.BehaviourTree.Exceptions {

    internal class SpawnException : Exception {

        public SpawnException(string msg) : base(msg) {
        }

        public SpawnException(string msg, Exception cause) : base(msg, cause) {
        }
    }
}