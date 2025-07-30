using System.Collections;
using UnityEngine;

namespace Utilities {
    public class CoroutineRunner : MonoBehaviour {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance {
            get {
                if (_instance == null) {
                    var obj = new GameObject("CoroutineRunner");
                    _instance = obj.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        public void RunCoroutine(IEnumerator coroutine) {
            StartCoroutine(coroutine);
        }

        // Start a coroutine that returns itself
        public Coroutine RunCoroutineAndGet(IEnumerator coroutine) {
            return StartCoroutine(coroutine);
        }

    }
}