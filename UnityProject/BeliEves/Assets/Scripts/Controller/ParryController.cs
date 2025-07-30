using System.Collections;
using Events;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utilities.Events.EventsLayout;
using CoroutineRunner = Utilities.CoroutineRunner;

namespace Controller {
    public class ParryController {
        private bool _isParrying;
        private bool _perryEnabled;
        private readonly float _perryTime;
        
        private GameObject _player;
        private CoroutineRunner _coroutineRunner;
        private GameObject _parryObject;
        
        //Events
        private readonly EventWithBool _parryEvent;
        private EventWithBool _movementEnabledEvent;
        
        //playerInput
        private readonly PlayerInput _playerInput;
        private InputAction _perryAction;
        public ParryController(float perryTime, PlayerInput playerInput) {
            _perryTime = perryTime;
            
            //initializeEvents
            _parryEvent = (EventWithBool)EventBroker.TryToAddEventChannel("ParryController", ScriptableObject.CreateInstance<EventWithBool>());
            _parryEvent.Subscribe(PerryCallback);
            _parryEvent.eventBool=false;
            _movementEnabledEvent = (EventWithBool)EventBroker.TryToAddEventChannel("movementEnableEvent", _movementEnabledEvent);
            
            //initializePlayerInput
            _playerInput = playerInput;
            SetupPlayerInput();

            _coroutineRunner = CoroutineRunner.Instance;
        }
        
        private void SetupPlayerInput() {
            _perryAction = _playerInput.actions["Perry"];
        }


        public void PerryUpdate() {  //update perry event
            if(!_perryAction.WasPressedThisFrame()|| !_perryEnabled || _isParrying)return;

            _isParrying = true;
            _parryEvent.UpdateValue(_isParrying);
        }

        public void BodySwitch(BodyType bodyType, GameObject player) {
            _player=player;
            _perryEnabled = bodyType != BodyType.Eve;
        }
        
        private void PerryCallback() { //implement parry actions (sound, animation, vfx...)
            if(!_parryEvent.eventBool || !_perryEnabled) return;
            
            //vfx
            var vfx = Resources.Load("Vfx/PerryLight");
            _parryObject = (GameObject)UnityEngine.Object.Instantiate(vfx, _player.transform, true);
            _parryObject.transform.position = _player.transform.position;
            
            //movement
            _movementEnabledEvent.UpdateValue(false);
            
            _coroutineRunner.RunCoroutine(StopPerry(_perryTime));
        }

        private IEnumerator StopPerry(float seconds) {
            yield return new WaitForSeconds(seconds);
            _isParrying = false;
            _parryEvent.UpdateValue(_isParrying);
            
            //vfx
            UnityEngine.Object.Destroy(_parryObject);
            
            //movement
            _movementEnabledEvent.UpdateValue(true);
        }
    }
}