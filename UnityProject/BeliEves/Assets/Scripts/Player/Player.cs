using System;
using System.Collections;
using System.Collections.Generic;
using Controller.Animations;
using Events;
using Events.EventsLayout;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Events.EventsLayout;
using Utilities.Input;

namespace Player {
    public class Player : MonoBehaviour {
        [SerializeField] protected float longPressRequiredTime = 0.5f;

        private BasicEventChannel _lightAttackEvent;
        private BasicEventChannel _heavyAttackEvent;
        private BasicEventChannel _specialAttackEvent;

        protected EventBodySwitch BodySwitchChannel;
        private InventoryEventChannel _bodySaveChannel;

        public GameObject lightAttackColliderGameObject;
        public GameObject heavyAttackColliderGameObject;
        public GameObject specialAttackColliderGameObject;

        public bool isAttackActive = false;
        public bool LightCooldown = false;
        public bool HeavyCooldown = false;
        public bool SpecialCooldown = false;

        protected float pressStartTime;

        //playerInput
        private PlayerInput _playerInput;
        private InputAction _lightAttackAction;
        protected InputAction HeavyAttackAction;
        protected InputAction SpecialAttackAction;
        private InputAction _eveReleaseAction;

        private EventWithBool _movementEnabledEvent;
        private Coroutine _StunPlayerCoroutine;
        private EventWithVector3 _deathEvent;
        private bool _isDead;
        protected void Start() {
            _isDead = false;
            lightAttackColliderGameObject = this.transform.Find("LightAttackCollider").gameObject;
            lightAttackColliderGameObject.SetActive(false);
            heavyAttackColliderGameObject = this.transform.Find("HeavyAttackCollider").gameObject;
            heavyAttackColliderGameObject.SetActive(false);
            specialAttackColliderGameObject = this.transform.Find("SpecialAttackCollider").gameObject;
            specialAttackColliderGameObject.SetActive(false);
            _deathEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("deathEvent", ScriptableObject.CreateInstance<EventWithVector3>());
            _deathEvent.Subscribe(() => _isDead = true);

            _movementEnabledEvent = (EventWithBool)EventBroker.TryToAddEventChannel("movementEnableEvent", ScriptableObject.CreateInstance<EventWithBool>());

            //initializePlayerInput
            _playerInput = FindObjectOfType<PlayerInput>();
            SetupPlayerInput();
        }

        private void SetupPlayerInput() {
            _lightAttackAction = _playerInput.actions["LightAttack"];
            HeavyAttackAction = _playerInput.actions["HeavyAttack"];
            SpecialAttackAction = _playerInput.actions["SpecialAttack"];
            _eveReleaseAction = _playerInput.actions["EveRelease"];
        }

        protected void Update() {
            if (_lightAttackAction.WasPressedThisFrame() && !isAttackActive && !LightCooldown) {
                if (_lightAttackEvent == null) _lightAttackEvent = EventBroker.GetEventChannel("lightAttackTriggerEvent");
                _lightAttackEvent.RaiseEvent();
            }
            else if (HeavyAttackAction.WasPressedThisFrame() && !isAttackActive && !HeavyCooldown) {
                if (_heavyAttackEvent == null) _heavyAttackEvent = EventBroker.GetEventChannel("heavyAttackTriggerEvent");
                _heavyAttackEvent.RaiseEvent();
            }
            else if (SpecialAttackAction.WasPressedThisFrame() && !isAttackActive && !SpecialCooldown) {
                if (_specialAttackEvent == null) _specialAttackEvent = EventBroker.GetEventChannel("specialAttackTriggerEvent");
                _specialAttackEvent.RaiseEvent();
            }

            if (_eveReleaseAction.WasPressedThisFrame()) pressStartTime = Time.time;
            if (_eveReleaseAction.WasReleasedThisFrame()) {
                float pressDuration = Time.time - pressStartTime;

                if (pressDuration < longPressRequiredTime) {
                    ReleaseEve();
                    AnimationController.Instance.ChangeState("Idle");
                }
                else SaveRobot();

                pressStartTime = 0f; // Reset the press start time.
            }
        }

        private void SaveRobot() {
            if (BodySwitchChannel == null) BodySwitchChannel = (EventBodySwitch)EventBroker.GetEventChannel("bodySwitch");
            if (_bodySaveChannel == null) _bodySaveChannel = (InventoryEventChannel)EventBroker.GetEventChannel("bodySave");
            _bodySaveChannel.UpdateValue(BodySwitchChannel.bodyType);
        }

        protected virtual void ReleaseEve() { }

        public void StunPlayer(float time = 0) {
            if (_StunPlayerCoroutine != null) StopCoroutine(_StunPlayerCoroutine);
            _movementEnabledEvent.UpdateValue(false);
            _StunPlayerCoroutine = StartCoroutine(StunPlayerCoroutine(time));

            //lock input
            var actions = new List<string> {
                "Move", "Jump", "Run",
                "lightAttack", "HeavyAttack", "SpecialAttack",
                "EveRelease", "Perry"
            };
            InputEnabler.DisableActions(actions, _playerInput);
        }

        private IEnumerator StunPlayerCoroutine(float time) {
            yield return new WaitForSeconds(time);
            _movementEnabledEvent.UpdateValue(true);

            //unlock input
            var actions = new List<string> {
                "Move", "Jump", "Run",
                "lightAttack", "HeavyAttack", "SpecialAttack",
                "EveRelease", "Perry"
            };
            if (!_isDead)
                InputEnabler.EnableActions(actions, _playerInput);
        }
    }
}