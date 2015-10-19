using System;

namespace Playblack.BehaviourTree.Exceptions {
    class SpawnException : Exception {
        public SpawnException(string msg) : base(msg) { }
        public SpawnException(string msg, Exception cause) : base(msg, cause) { }
    }
}
