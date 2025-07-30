using System;
using System.Collections;
using Pool;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utilities.Pool {
    public class Pool{
        private readonly ArrayList _pool = new ArrayList();
        private readonly int _enlargeSteps;

        private GameObject pool;

        private readonly System.Type _type;
        private readonly GameObject _prefab;
        
        private readonly bool _dontDestroy;

        public Pool(string poolName, GameObject prefab, System.Type component, int size = 100, int enlargeSteps = 50, bool dontDestroy = true) {
            this._type = component;
            this._enlargeSteps = enlargeSteps;
            this._dontDestroy = dontDestroy;
            this._prefab = prefab;

            _createPoolForFirstTime(poolName, dontDestroy, size);
        }
        public T GetObject<T>(Vector3 position) {
            foreach (GameObject obj in _pool) {
                if (!obj.activeSelf) { 
                    InitObjects(obj, position);
                    return obj.gameObject.GetComponent<T>();
                }
            }

            GameObject newObj = _addPoolObject();
            InitObjects(newObj, position);
            EnlargePool(_enlargeSteps-1);
            
            return newObj.GetComponent<T>();
        }

        public void RemoveObject<T>(GameObject gameObject) {
            if (_pool.Contains(gameObject)) {
                _pool.Remove(gameObject);
            }
        }
        public void EmptyPool() {
            Object.Destroy(pool);
        }

        public void ClearPool() {
            foreach (GameObject  obj in _pool) {
                if(obj.activeSelf)obj.SetActive(false);
            }
        }

        public Boolean IsPoolPermanent() {
            return this._dontDestroy;
        }
        private void _createPoolForFirstTime(string poolName, bool dontDestroy, int size) {
            pool = new GameObject(poolName);
            if (dontDestroy) pool.AddComponent<DontDestroyOnLoad>();
            EnlargePool(size);
        }
        
        private void EnlargePool(int size) {
            for (int i = 0; i < size; i++) {
                _addPoolObject();
            }
        }

        private GameObject _addPoolObject() {
            GameObject obj = Object.Instantiate(_prefab, pool.transform, true);
            if(_type!=null)obj.AddComponent(_type);
            obj.SetActive(false);
            _pool.Add(obj);
            return obj;
        }

        private void InitObjects(GameObject obj, Vector3 position) {
            obj.transform.position = position;
            obj.SetActive(true);
        }
        
    }
}