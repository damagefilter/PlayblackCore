using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Playblack.Pooling {
    public class GameObjectPool {
        private List<PooledObject<GameObject>> pooledObjects;
        private int maxCapacity;

        public int CurrentSize {
            get {
                return pooledObjects.Count;
            }
        }

        public int MaxCapacity {
            get {
                return this.maxCapacity;
            }
        }

        public GameObjectPool(int maxCapacity) {
            this.pooledObjects = new List<PooledObject<GameObject>>(maxCapacity);
            this.maxCapacity = maxCapacity;
        }

        /// <summary>
        /// Adds an object to the pool
        /// </summary>
        /// <param name="obj"></param>
        public void Add(GameObject obj) {
            if (this.pooledObjects.Count >= maxCapacity) {
                throw new MaxCapacityReachedException("Object pool has reached the capacity of " + this.maxCapacity + ". Cannot add object ("+obj.name+") to pool.");
            }

            obj.SetActive(false);
            var po = new PooledObject<GameObject>(obj);
            GameObject.DontDestroyOnLoad(obj); // We will take care of cleaning these up ourselves
            this.pooledObjects.Add(po);
        }

        public GameObject Take() {
            for (int i = 0; i < this.pooledObjects.Count; ++i) {
                if (!this.pooledObjects[i].IsInUse) {
                    this.pooledObjects[i].IsInUse = true;
                    return this.pooledObjects[i].Object;
                }
            }
            return null; // nothing free in pool
        }

        public void PutBack(GameObject obj) {
            for (int i = 0; i < this.pooledObjects.Count; ++i) {
                if (this.pooledObjects[i].IsInUse && this.pooledObjects[i].Object == obj) {
                    obj.SetActive(false);
                    this.pooledObjects[i].IsInUse = false;
                    return;
                }
            }
            throw new PutbackFailureException("Failed to put back game object " + obj.name + ". It was not found in pool.");
        }

        /// <summary>
        /// Clears the object pool of its objects and destroys them
        /// </summary>
        public void Clear() {
            // Mark objects for destruction
            for (int i = 0; i < this.pooledObjects.Count; ++i) {
                GameObject.Destroy(this.pooledObjects[i].Object); 
            }

            // Release ther references for GC
            this.pooledObjects.Clear();
        }
    }
}
