using System;
using System.Collections;
using Events;
using Events.EventsLayout;
using Managers.DialogueManager;
using ScriptableObjects.Dialogue;
using UnityEngine;
using Utilities.Events.EventsLayout;
using Utilities.Interactable;

namespace Npc.Tank {
    public class PushableObj : Interactable {
        [SerializeField] Vector3 startPos;
        [SerializeField] Vector3 endPos;
        [SerializeField] float speed;
        [SerializeField] bool allowReverse;
        
        [SerializeField] Vector3 boxSize;
        
        [Header("Dialogue")]
        [SerializeField] private DialogueSequence dialogueSequence;
        
        [SerializeField, Tooltip("Reference to the Dialogue UI Manager")] private DialogueManager dialogueManager;
        
        private GameObject _target;
        private MoveTowardsEvent _moveTowardsEventChannel;
        private BasicEventChannel _lighAttackAnimationEventChannel;
        private GameObject _other;
        
        private Coroutine _routine;
        
        private void Start() {
            _target = transform.parent.gameObject;
            
            _moveTowardsEventChannel = (MoveTowardsEvent)EventBroker.TryToAddEventChannel("movePlayerTowardsEvent", ScriptableObject.CreateInstance<MoveTowardsEvent>());
            _lighAttackAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("lightAttackAnimationEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            
            dialogueSequence.ResetDialogueSequence();
        }

        private void OnDrawGizmos() {
                // Set the Gizmo color (optional)
            Gizmos.color = Color.green;

                // Draw the box at the target's position
            Gizmos.DrawWireCube(startPos, boxSize);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(endPos, boxSize);
        }
        
        protected override void OnTriggerEnter(Collider other) {
            if(!other.gameObject.TryGetComponent<Player.Player>(out var player))return;
            
            if (other.name.Contains("TankRobot") &&
                _target.transform.position != endPos) {
                
                Vector3 moveDir = (startPos - endPos).normalized;
                Vector3 otherToTarget = (other.transform.position - startPos).normalized;

                // Check if the otherToTarget direction is opposite to moveDir
                if (Vector3.Dot(otherToTarget, moveDir) > 0) {
                    this._other = other.gameObject;
                    this.interactable.enabled = true;
                    interactable.ShowHelpText("Press ", " to push away");
                }
            }
            else if (_target.transform.position != endPos) {
                dialogueManager.DisplayDialogue(dialogueSequence);
            }
        }

        protected override void OnTriggerEnterCallback() {
            interactable.ShowHelpText("Press ", " to push away");
        }

        protected override void OnTriggerExitCallback() {
            this._other = null;
            interactable.HideHelpText();
        }

        public override void Interact() {
            if(_target.transform.position != endPos && _other!=null&& _routine==null) _routine = StartCoroutine(MoveCoroutine(_other));
        }


        private IEnumerator MoveCoroutine(GameObject other) {
            var reach = false;
            
            _moveTowardsEventChannel.UpdateValue(_other.transform.position, 0f, false, null, endPos - _other.transform.position,1.2f);
            _lighAttackAnimationEventChannel.RaiseEvent();
            yield return new WaitForSeconds(0.6f);
                
            float journey = 0f;
            while (journey < 1f) {
                
                Collider[] hitColliders = Physics.OverlapBox(_target.transform.position, boxSize / 2f, Quaternion.identity);
                foreach (var hitCollider in hitColliders) {
                    if (hitCollider.CompareTag("StopTank") && hitCollider.gameObject != _target) { 
                        _routine = null;
                        yield break; // Exit the coroutine if a non-trigger collider with the "Stop" tag is detected
                    }
                }
                
                journey += Time.deltaTime * speed / Vector3.Distance(startPos, endPos);
                _target.transform.position = Vector3.Lerp(startPos, endPos, journey);
                if(journey < 1f) reach = true;
                
                yield return null;
            }
            _routine = null;

            if (allowReverse && reach) {
                (endPos, startPos) = (startPos, endPos);//swap end e start pose
            }
        }
        
    }
}