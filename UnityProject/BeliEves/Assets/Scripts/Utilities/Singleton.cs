using UnityEngine;

namespace Utilities
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance;

        protected virtual void Awake()
        {
            if (Instance == null) Instance = this as T;
            else Destroy(gameObject);
        }
    }
}