using System;
using Events;
using UnityEngine;
using Utilities.Events.EventsLayout;
using Object = UnityEngine.Object;

// Temporal script for testing damage on player
public class TriggerDamage : MonoBehaviour {
    public int damageAmount = 10;

    private EventTakeDamage _takeDamageEvent;
    protected void DamagePlayer(Collider other) {
        if (_takeDamageEvent == null) _takeDamageEvent = (EventTakeDamage)EventBroker.TryToAddEventChannel("takeDamageEvent", ScriptableObject.CreateInstance<EventTakeDamage>());
        _takeDamageEvent.UpdateValue(other.gameObject, damageAmount);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Player.Player>(out Player.Player npcScript)) {
            DamagePlayer(other);
        }
    }
}
