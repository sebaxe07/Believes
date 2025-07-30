using System.Collections;
using System;
using Events;
using UnityEngine;
using Utilities.Events.EventsLayout;

namespace Controller.Attack.AgileAttack.GrapplingIdentifiers {
    public class GrapplingAnchorPoint : MonoBehaviour {
        private Coroutine _moveCoroutine;

        private MoveTowardsEvent _moveTowardsMoveTowardsEvent;
        private bool _isGrappling = false;

        public void DoAnchorPoint(float moveSpeed, float maxDistance, Action endAction, Rigidbody rb, GameObject draggablePlatform) {
            if (_isGrappling) {
                return;
            }
            
            _isGrappling = true;
            if (_moveTowardsMoveTowardsEvent == null) _moveTowardsMoveTowardsEvent = (MoveTowardsEvent)EventBroker.TryToAddEventChannel("movePlayerTowardsEvent", ScriptableObject.CreateInstance<MoveTowardsEvent>());

            Vector3 destination = transform.position;
            if (Vector3.Distance(destination, rb.transform.position) > maxDistance) destination = rb.transform.position + (destination - rb.transform.position).normalized * maxDistance; ;

            if (draggablePlatform == null) _moveTowardsMoveTowardsEvent.UpdateValue(destination, moveSpeed, true, new Action(() => NewEndAction(endAction, draggablePlatform)));
            else {
                _moveTowardsMoveTowardsEvent.UpdateValue(destination, moveSpeed, false, new Action(() => NewEndAction(endAction, draggablePlatform)));
            }
        }

        public bool Grappling() {
            return _isGrappling;
        }
        private void NewEndAction(Action endAction, GameObject draggablePlatform) {
            endAction();
            _isGrappling = false;

            if (draggablePlatform != null) draggablePlatform.transform.parent = null;
        }
    }
}