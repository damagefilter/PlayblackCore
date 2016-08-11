using System;

namespace Playblack.BehaviourTree.Exceptions {

    internal class IllegalReturnStatusException : Exception {

        public IllegalReturnStatusException(string msg) : base(msg) {
        }

        public IllegalReturnStatusException(string msg, Exception cause) : base(msg, cause) {
        }
    }
}