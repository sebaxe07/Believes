using System.Collections;
using System.Collections.Generic;
using Events;
using UnityEngine;
using Utilities.Events.EventsLayout;

public class ExplosionDamage : MonoBehaviour {
    public int damageAmount = 10;
    public float explosionForce = 10;
    
    private EventWithVector3 _addMovementEvent;

    private void OnTriggerEnter(Collider other) {
        {
            if (other.TryGetComponent<Player.Player>(out Player.Player npcScript)) {
                var takeDamageEvent = (EventTakeDamage)EventBroker.TryToAddEventChannel("takeDamageEvent", ScriptableObject.CreateInstance<EventTakeDamage>());
                takeDamageEvent.UpdateValue(other.gameObject, damageAmount);

                // Should move the player but does not work
                _addMovementEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("playerMovementEventChannel", ScriptableObject.CreateInstance<EventWithVector3>());
                Vector3 forceDirection = (other.transform.position - transform.position).normalized;
                forceDirection.y = 1;
                _addMovementEvent.UpdateValue(forceDirection * explosionForce);
                
            }
        }
    }
}
