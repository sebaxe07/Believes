using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Events {
    public class CallbacksHandler : Singleton<CallbacksHandler> {
        public void Start() {
            //DontDestroyOnLoad(this.gameObject);
        }

        public void HandleCallback(List<Action> callbacks) {
            for (int i = 0; i < callbacks.Count; i++) {
                var callback = callbacks[i];
                StartCoroutine(Callback(callback));
            }
        }

        private IEnumerator Callback(Action callback) {
            callback?.Invoke();
            yield return null;
        }
    }
}