using System;
using Events;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Events.EventsLayout;

namespace Controller.Attack.AgileAttack.GrapplingIdentifiers {
    public class NpcDraggable : Draggable {
        private EventAttackHit _attackHit;
        private float _NavSpeed;
        public override void Dragged(Vector3 destination, float safeOffset, float moveSpeed, float maxDistance, Action endAction) {
            if (_moveCoroutine != null) return;

            if (_attackHit == null) _attackHit = (EventAttackHit)EventBroker.TryToAddEventChannel("attackHitEvent", ScriptableObject.CreateInstance<EventAttackHit>());
            _attackHit.UpdateValue(null, this.gameObject);


            if (Vector3.Distance(transform.position, destination) > maxDistance) {
                Vector3 direction = (destination - transform.position).normalized;
                destination = transform.position + direction * maxDistance;
            }

            if (gameObject.TryGetComponent(out NavMeshAgent navAgent)) {
                _NavSpeed = navAgent.speed;
                navAgent.updatePosition = false;
            }

            destination = destination - (destination - transform.position).normalized * safeOffset;
            _moveCoroutine = StartCoroutine(MoveCoroutine(destination, moveSpeed, () => {
                endAction();
                navAgent.nextPosition = transform.position;
                navAgent.updatePosition = true;
            }));


        }
        public override void RestartDragging(Vector3 destination, float safeOffset, float moveSpeed, float maxDistance, Action endAction) {
            StopCoroutine(_moveCoroutine);

            if (Vector3.Distance(transform.position, destination) > maxDistance) {
                Vector3 direction = (destination - transform.position).normalized;
                destination = transform.position + direction * maxDistance;
            }

            _NavSpeed = gameObject.GetComponent<NavMeshAgent>().speed;
            NavMeshAgent navAgent = gameObject.GetComponent<NavMeshAgent>();

            navAgent.updatePosition = false;

            destination = destination - (destination - transform.position).normalized * safeOffset;
            _moveCoroutine = StartCoroutine(MoveCoroutine(destination, moveSpeed, () => {
                endAction();

                navAgent.nextPosition = transform.position;
                navAgent.updatePosition = true;

            }));
        }
    }
}