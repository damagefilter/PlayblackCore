using System.Collections.Generic;
using System.Linq;

namespace Playblack.Pooling {

    /// <summary>
    /// An object pool that pools named objects.
    /// </summary>
    public class GenericObjectPoolMap<TKey, TValue> {
        private readonly Dictionary<TKey, PooledObject<TValue>> pooledObjects;
        private readonly int maxCapacity;
        private bool ignoreInUse;

        public GenericObjectPoolMap(int initCapacity, int maxCapacity) {
            pooledObjects = new Dictionary<TKey, PooledObject<TValue>>(initCapacity);
            this.maxCapacity = maxCapacity;
        }

        // it's a little silly and we might as well remove the in-use part
        // for the current use-cases within this code but it might has "external" use cases.
        // so we'll keep it.
        public void SetIgnoreInUse(bool ignoreInUse) {
            this.ignoreInUse = ignoreInUse;
        }

        public TValue Get(TKey key) {
            if (pooledObjects.ContainsKey(key)) {
                var ob = pooledObjects[key];
                if (ob.IsInUse && !this.ignoreInUse) {
                    return default(TValue);
                }
                ob.IsInUse = true;
                return ob.Object;
            }
            return default(TValue);
        }

        public void Clear() {
            pooledObjects.Clear();
        }

        public bool Has(TKey key) {
            return pooledObjects.ContainsKey(key);
        }

        public void Add(TKey key, TValue val) {
            if (pooledObjects.Count >= maxCapacity) {
                // remove from the end of the dictionary.
                pooledObjects.Remove(pooledObjects.Keys.Last());
                UnityEngine.Debug.LogError("Pool full. Removing last element");
            }
            pooledObjects.Add(key, new PooledObject<TValue>(val));
        }

        public void PutBack(TKey key) {
            if (!Has(key)) {
                throw new PutbackFailureException("The thing to put back does not exist in this dictionary");
            }
            pooledObjects[key].IsInUse = false;
        }

        public void Remove(TKey key) {
            pooledObjects.Remove(key);
        }
    }
}
