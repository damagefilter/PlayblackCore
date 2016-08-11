using System;

namespace Playblack.Pooling {

    public class MaxCapacityReachedException : Exception {

        public MaxCapacityReachedException(string msg) : base(msg) {
        }

        public MaxCapacityReachedException(String msg, Exception cause) : base(msg, cause) {
        }
    }
}