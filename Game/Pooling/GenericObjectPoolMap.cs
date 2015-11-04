using System;
using System.Collections.Generic;

namespace Playblack.Pooling {
    /// <summary>
    /// An object pool that pools named objects.
    /// </summary>
    public class GenericObjectPoolMap<TKey, TValue> {
        private Dictionary<TKey, PooledObject<TValue>> pooledObjects;
        private int maxCapacity;

        public GenericObjectPoolMap(int initCapacity, int maxCapacity) {
            pooledObjects = new Dictionary<TKey, PooledObject<TValue>>(initCapacity);
            this.maxCapacity = maxCapacity;
        }

        public TValue Get(TKey key) {
            if (pooledObjects.ContainsKey(key)) {
                var ob = pooledObjects[key];
                ob.UsageValue += 1 / (maxCapacity * pooledObjects.Count);
                return ob.Object;
            }
            return default(TValue);
        }

        public bool Has(TKey key) {
            return pooledObjects.ContainsKey(key);
        }

        public void Add(TKey key, TValue val) {
            if (pooledObjects.Count+1 > maxCapacity) {
                TrimPool();
            }
            pooledObjects.Add(key, new PooledObject<TValue>(val, 1 / (maxCapacity * pooledObjects.Count)));
        }

        public void Remove(TKey key) {
            pooledObjects.Remove(key);
        }

        private void TrimPool() {
            // the init value of the last insertion
            float threshold = 1 / (maxCapacity * (pooledObjects.Count-1));
            while (pooledObjects.Count+1 > maxCapacity) {
                var toRemove = new List<TKey>();
                foreach (var kvp in pooledObjects) {
                    if (kvp.Value.UsageValue <= threshold) {
                        toRemove.Add(kvp.Key);
                    }
                }
                for (int i = 0; i < toRemove.Count; ++i) {
                    pooledObjects.Remove(toRemove[i]);
                }
                threshold *= 2; // increase in case we need to run again (nothign was removed)
            }
        }
    }
}

