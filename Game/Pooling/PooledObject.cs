using System;

namespace Playblack.Pooling {
    /// <summary>
    /// Represents an object in the object pool.
    /// It has an indicator of how frequently it is used which should be set by the object pool.
    /// By this indicator it can be determined if objects should be removed from a pool.
    /// 
    /// This behaviour comes up in dynamic object pools where the exact contents are fed
    /// in by someone other than the pool itself.
    /// 
    /// </summary>
    public class PooledObject<T> {
        public float UsageValue {
            get;
            set;
        }

        public T Object {
            get;
            set;
        }

        public PooledObject(T obj) {
            Object = obj;
            UsageValue = 0f;
        }

        public PooledObject(T obj, float initUsageValue) {
            Object = obj;
            UsageValue = initUsageValue;
        }
    }
}

