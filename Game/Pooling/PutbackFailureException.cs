using System;

namespace Playblack.Pooling {

    public class PutbackFailureException : Exception {

        public PutbackFailureException(string msg) : base(msg) {
        }

        public PutbackFailureException(String msg, Exception cause) : base(msg, cause) {
        }
    }
}