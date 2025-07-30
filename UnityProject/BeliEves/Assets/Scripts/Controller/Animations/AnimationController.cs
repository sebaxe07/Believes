using System;
using Events;
using Events.EventsLayout;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using Utilities.Events.EventsLayout;
using Utilities;

namespace Controller.Animations {
    public class AnimationController : Utilities.Singleton<AnimationController> {
        [SerializeField] Animator anim;

        private BasicEventChannel _lighAttackAnimationEventChannel;
        private BasicEventChannel _heavyAttackAnimationEventChannel;
        private BasicEventChannel _specialAttackAnimationEventChannel;
        private EventWithGameObj _interactionAnimationEventChannel;
        private BasicEventChannel _interactionFinishedEventChannel;
        private BasicEventChannel _knockOutAnimationEventChannel;
        private BasicEventChannel _jumpEventChannel;
        private BasicEventChannel _jumpFinishedEventChannel;

        private EventWithBool _parryEventChannel;

        private EventWithFloat _walkingAnimationEventChannel;

        private EventWithVector3 _deathEvent;

        private int _state = 0;
        private String _publicState = "Idle";
        private GameObject _player;
        private float _speed = 0;
        private float _height = 0;
        private bool _isDead = false;

        /* <EventsDescription>
         * lightAttackAnimationEventChannel: this event is raised when the player triggers a light attack.
         * heavyAttackAnimationEventChannel: this event is raised when the player triggers a heavy attack.
         * specialAttackAnimationEventChannel: this event is raised when the player triggers a special attack.
         * jumpEventChannel: this event is raised when the player triggers a jump.
         * jumpFinishedEventChannel: this event is raised when the player finishes a jump.
         * knockOutAnimationEventChannel: this event is raised when the player is knocked out.
         * interactionFinishedEventChannel: this event is raised when the player finishes an interaction.
         * interactionAnimationEventChannel: this event is raised when the player triggers an interaction.
         * walkingAnimationEventChannel: this event is raised when the player is walking.
         */

        public void Initalize() {
            Start();
        }

        void Start() {
            _isDead = false;
            _lighAttackAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("lightAttackAnimationEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            _lighAttackAnimationEventChannel.Subscribe(() => ChangeState("LightAttack"));

            _heavyAttackAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("heavyAttackAnimationEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            _heavyAttackAnimationEventChannel.Subscribe(() => ChangeState("HeavyAttack"));

            _specialAttackAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("specialAttackAnimationEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            _specialAttackAnimationEventChannel.Subscribe(() => ChangeState("SpecialAttack"));

            _jumpEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("jumpEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
            _jumpEventChannel.Subscribe(() => ChangeState("Jumping"));

            _knockOutAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("knockOutEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            _knockOutAnimationEventChannel.Subscribe(() => ChangeState("Death"));

            _interactionFinishedEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("interactionFinishedEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            _interactionFinishedEventChannel.Subscribe(() => InteractionStateUpdate(false));

            _interactionAnimationEventChannel = (EventWithGameObj)EventBroker.TryToAddEventChannel("interactionEvent", ScriptableObject.CreateInstance<EventWithGameObj>());
            _interactionAnimationEventChannel.Subscribe(() => InteractionStateUpdate(true));

            _walkingAnimationEventChannel = ScriptableObject.CreateInstance<EventWithFloat>();
            _walkingAnimationEventChannel = (EventWithFloat)EventBroker.TryToAddEventChannel("walkingAnimationEvent", _walkingAnimationEventChannel);
            _walkingAnimationEventChannel.Subscribe(SetSpeed);

            _deathEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("deathEvent", ScriptableObject.CreateInstance<EventWithVector3>());
            _deathEvent.Subscribe(() => {
                // Debug.Log("Death Event Animation");
                ChangeState("Death");
            });

            _parryEventChannel = (EventWithBool)EventBroker.TryToAddEventChannel("ParryController", ScriptableObject.CreateInstance<EventWithBool>());
            _parryEventChannel.Subscribe(() => { if (_parryEventChannel.eventBool) ChangeState("Parry"); });



            _player = FindObjectOfType<Player.Player>().gameObject;
            SetGameObject(_player);

            ChangeState("Walking");

        }

        private void InteractionStateUpdate(bool state) {
        }

        private void KnockOutTrigger() {
        }

        private void Update() {
            switch (_state) {
                case -1:
                // Awakening State
                case 0:
                    // Idle, Walking, Running State
                    MovementStateOperations();
                    break;
                case 1:
                    // Jumping State
                    JumpStateOperations();
                    break;
                case 2:
                    // Light attack state
                    LightAttackStateOperations();
                    break;
                case 3:
                    // Heavy attack state
                    HeavyAttackStateOperations();
                    break;
                case 4:
                    // Special attack state
                    SpecialAttackStateOperations();
                    break;
                case 5:
                    // Interaction State
                    break;
                case 6:
                    // Knocked out state
                    DeathStateOperations();
                    break;
                case 7:
                    ParryStateOperations();
                    break;

            }

        }

        private void ParryStateOperations() {

        }

        public void ChangeState(String newState) {
            // Debug.LogError("Change State: " + newState);
            if (_isDead) {
                anim.SetInteger("State", 6);
                // Debug.LogError("Player is dead, cannot change state");
                return;
            }
            if (newState == "Death") {
                // Debug.LogError("Player is dead");
                _isDead = true;
            }
            switch (newState) {
                case "Awakening":
                    _state = -1;
                    _publicState = newState;
                    break;
                case "Idle":
                case "Walking":
                case "Running":
                    _state = 0;
                    _publicState = newState;
                    break;
                case "Jumping":
                    _state = 1;
                    _publicState = newState;
                    break;
                case "LightAttack":
                    _state = 2;
                    _publicState = newState;
                    break;
                case "HeavyAttack":
                    _state = 3;
                    _publicState = newState;
                    break;
                case "SpecialAttack":
                    _state = 4;
                    _publicState = newState;
                    break;
                case "Interacting":
                    _state = 5;
                    _publicState = newState;
                    break;
                case "Death":
                    _state = 6;
                    _publicState = newState;
                    break;
                case "Parry":
                    _state = 7;
                    _publicState = newState;
                    break;
                default:
                    // Debug.LogWarning("Unknown state: " + newState);
                    break;
            }

            anim.SetInteger("State", _state);
        }

        private void JumpStateOperations() {
        }

        private void MovementStateOperations() {
            anim.SetFloat("Speed", _speed);
        }

        private void DeathStateOperations() {
        }

        private void SpecialAttackStateOperations() {
        }

        private void HeavyAttackStateOperations() {
        }

        private void LightAttackStateOperations() {
        }
        public void SetSpeed() {
            _speed = _walkingAnimationEventChannel.value;
        }

        public String GetState() {
            return _publicState;
        }

        public void SetGameObject(GameObject player) {
            //if (anim != null) ChangeState("Death");
            anim = player.GetComponentInChildren<Animator>();
            ChangeState("Idle");
            _player = player;
        }

    }
}