using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playblack.Pooling {
    public class MaxCapacityReachedException : Exception {
        public MaxCapacityReachedException(string msg) : base(msg) { }

        public MaxCapacityReachedException(String msg, Exception cause) : base(msg, cause) { }
    }
}
