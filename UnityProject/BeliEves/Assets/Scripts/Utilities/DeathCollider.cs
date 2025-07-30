using System;
using Events;
using Events.EventsLayout;
using UnityEngine;
using Utilities.Events.EventsLayout;

namespace Utilities {
    public class DeathCollider : MonoBehaviour {
        private EventWithVector3 _deathEvent;
        private void Start() {
            _deathEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("deathEvent", ScriptableObject.CreateInstance<EventWithVector3>());
        }

        private void OnTriggerEnter(Collider other) {
            if (other.TryGetComponent<Player.Player>(out var player)) {
                _deathEvent.UpdateValue(Vector3.zero);
            }
        }
    }
}