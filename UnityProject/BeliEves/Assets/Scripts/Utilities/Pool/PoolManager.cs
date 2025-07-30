using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.Pool {
    public class PoolException : Exception {
        public PoolException(string message) : base(message) {}
    }
    public static class PoolManager {
        private static Dictionary<string, Utilities.Pool.Pool> Pools = new Dictionary<string, Utilities.Pool.Pool>();

        public static void CreateNewPool(string poolName, GameObject prefab, System.Type component, int poolSize = 100, int poolEnlargeSteps = 50, bool dontDestroy = true) {
            if (Pools.ContainsKey(poolName)) {
                //throw new PoolException(poolName);
                return;
            }
            Utilities.Pool.Pool pool = new Utilities.Pool.Pool(poolName, prefab, component, poolSize, poolEnlargeSteps, dontDestroy);
            Pools.Add(poolName, pool);
        }

        public static void DestroyPool(string poolName){
            if (Pools.TryGetValue(poolName, out Utilities.Pool.Pool pool)) pool.EmptyPool();
            Pools.Remove(poolName);
        }
        
        public static T GetPoolObject<T>(string poolName, Vector3 position) {
            if (!Pools.TryGetValue(poolName, out Utilities.Pool.Pool pool)) throw new Exception("no pool");
            return pool.GetObject<T>(position);
        }

        public static void RemovePoolObject(string poolName, GameObject gameObject) {
            if (!Pools.TryGetValue(poolName, out Utilities.Pool.Pool pool)) throw new Exception("no pool");
            pool.RemoveObject<object>(gameObject);
        }

        public static void ClearAllPools() {
            foreach (var pool in Pools) {
                pool.Value.ClearPool();
            }
        }

        public static void ReleasePoolObject(GameObject poolObject) {
            poolObject.SetActive(false);
        }

        public static void RemoveNonPermanentPools() {
            Pools = Pools.Where(poolEntry => poolEntry.Value.IsPoolPermanent()).ToDictionary(poolEntry => poolEntry.Key, poolEntry => poolEntry.Value);
        }
        
    }
}