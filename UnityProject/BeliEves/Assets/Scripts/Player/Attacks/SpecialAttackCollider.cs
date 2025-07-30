using Events;
using Npc;
using UnityEngine;
using Utilities.Events.EventsLayout;

namespace Player {
    public class SpecialAttackCollider : MonoBehaviour {
        private EventAttackHit _attackHit;
        private Enemy _enemy;
        void Start() {
            // Check if the parent object has an Enemy component
            if (_enemy == null) _enemy = GetComponentInParent<Enemy>();

        }

        protected void OnTriggerEnter(Collider other) {
            if (_enemy != null) {
                if (other.TryGetComponent<Player>(out var player)) {
                    _enemy.PlayerHit(player);
                }
            }
            else {

                if (other.TryGetComponent<Npc.Npc>(out var npc) || other.TryGetComponent<Npc.NPCEnemy>(out var enemy)) {
                    if (_attackHit == null) _attackHit = (EventAttackHit)EventBroker.TryToAddEventChannel("attackHitEvent", ScriptableObject.CreateInstance<EventAttackHit>());
                    _attackHit.UpdateValue(transform.parent.gameObject, other.gameObject);

                }
            }

        }
    }
}