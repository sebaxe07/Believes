using System.Collections;
using System;
using UnityEngine;

namespace Controller.Attack.AgileAttack.GrapplingIdentifiers
{
    public class Draggable : MonoBehaviour{
        
        protected Coroutine _moveCoroutine; 
        private Collider _collider;
        private String _colliderTag = "AgileStopDragging";
        
        
        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }


        public virtual void Dragged(Vector3 destination, float safeOffset, float moveSpeed, float maxDistance, Action endAction) {
            if (_moveCoroutine != null)return;
            
            if (Vector3.Distance(transform.position, destination) > maxDistance) {
                Vector3 direction = (destination - transform.position).normalized;
                destination = transform.position + direction * maxDistance;
            }

            destination = destination - (destination - transform.position).normalized * safeOffset;
            destination = new Vector3(destination.x, transform.position.y, destination.z);
            _moveCoroutine = StartCoroutine(MoveCoroutine(destination, moveSpeed, endAction));
        }

        public virtual void RestartDragging(Vector3 destination, float safeOffset, float moveSpeed, float maxDistance, Action endAction) {
            StopCoroutine(_moveCoroutine);
            
            if (Vector3.Distance(transform.position, destination) > maxDistance) {
                Vector3 direction = (destination - transform.position).normalized;
                destination = transform.position + direction * maxDistance;
            }

            destination = destination - (destination - transform.position).normalized * safeOffset;
            destination = new Vector3(destination.x, transform.position.y, destination.z);
            _moveCoroutine = StartCoroutine(MoveCoroutine(destination, moveSpeed, endAction));
        }

        protected IEnumerator MoveCoroutine(Vector3 destination, float moveSpeed, Action endAction) {
            while (Vector3.Distance(transform.position, destination) > 1f) {
                Vector3 direction = (destination - transform.position).normalized;
                float distanceToMove = moveSpeed * Time.deltaTime;

                if (CheckForCollisions(direction, distanceToMove)) {
                    endAction?.Invoke();
                    _moveCoroutine = null;
                    yield break;
                }

                transform.position = Vector3.MoveTowards(transform.position, destination, distanceToMove);
                yield return null;
            }

            transform.position = destination;
            endAction?.Invoke();
            _moveCoroutine = null;
        }

        private bool CheckForCollisions(Vector3 direction, float distance) {
            return Physics.BoxCast(
                _collider.bounds.center,  
                _collider.bounds.extents, 
                direction,                 
                out RaycastHit hit,       
                transform.rotation,        
                distance                 
            ) && hit.collider != _collider && hit.collider.CompareTag(_colliderTag);
        }

    }
}