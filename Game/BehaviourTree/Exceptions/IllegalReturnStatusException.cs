using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playblack.BehaviourTree.Exceptions {
    class IllegalReturnStatusException : Exception {
        public IllegalReturnStatusException(string msg) : base(msg) { }
        public IllegalReturnStatusException(string msg, Exception cause) : base(msg, cause) { }
    }
}
