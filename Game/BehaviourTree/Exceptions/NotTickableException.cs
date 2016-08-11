using System;

namespace Playblack.BehaviourTree.Exceptions {

    internal class NotTickableException : Exception {

        public NotTickableException(string msg) : base(msg) {
        }

        public NotTickableException(string msg, Exception cause) : base(msg, cause) {
        }
    }
}