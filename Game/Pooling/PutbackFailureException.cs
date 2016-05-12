using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playblack.Pooling {
    public class PutbackFailureException : Exception {
        public PutbackFailureException(string msg) : base(msg) { }

        public PutbackFailureException(String msg, Exception cause) : base(msg, cause) { }
    }
}
