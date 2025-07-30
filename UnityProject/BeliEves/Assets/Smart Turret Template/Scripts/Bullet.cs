using System.Collections;
using System.Collections.Generic;
using Events;
using UnityEngine;
using Utilities.Events.EventsLayout;

public class Bullet : MonoBehaviour {
    public float damage = 10f;

    public bool ExplodeOnImpact = false;
    public float explosionForce = 10f;
    public GameObject explosionPrefab;
    private EventTakeDamage eventTakeDamage;
    private bool hasCollided = false;  // Flag to prevent multiple collisions

    private void OnCollisionEnter(Collision collision) {
        if (hasCollided) return;  // Exit if collision has already been handled

        hasCollided = true;  // Mark that we've collided

        if (ExplodeOnImpact) {
            Explode();
        }
        // Check if the collided object has the appropriate component
        if (collision.gameObject.TryGetComponent<Player.Player>(out Player.Player player)) {
            if (eventTakeDamage == null) eventTakeDamage = (EventTakeDamage)EventBroker.TryToAddEventChannel("takeDamageEvent", ScriptableObject.CreateInstance<EventTakeDamage>());
            eventTakeDamage.UpdateValue(player.gameObject, damage);

            Destroy(gameObject);
        }

        // Destroy the bullet after hitting something
        Destroy(gameObject);
    }

    private void Explode() {
        // Instantiate the explosion prefab
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

    }

    public void SetDamage(float damage) {
        this.damage = damage;
    }
}
