using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Events.EventsLayout {
    public class BasicEventChannel: ScriptableObject {
        private readonly List<Action> _callbacks = new List<Action>();
        private Action<List<Action>> _handleCallback;

        public void AddCallBackHandler(Action<List<Action>> handleCallback) {
            _handleCallback = handleCallback;
        }
        public void RaiseEvent() {
            List<Action> actionsCopy = new List<Action>(_callbacks);
            if(_handleCallback!=null)_handleCallback(actionsCopy);
            else {
                Debug.LogError("empty callback handler");
            }
        }

        public virtual void Subscribe([NotNull] Action callback) {
            _callbacks.Add(callback);
        }

        public void Unsubscribe([NotNull] Action callback) {
            _callbacks.Remove(callback);
        }
    }
}