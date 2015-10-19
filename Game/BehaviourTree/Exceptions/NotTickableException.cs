using System;

namespace Playblack.BehaviourTree.Exceptions {
    class NotTickableException : Exception {
        public NotTickableException(string msg) : base(msg) { }
        public NotTickableException(string msg, Exception cause) : base(msg, cause) { }
    }
}
