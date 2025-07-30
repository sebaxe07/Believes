using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using Utilities.Events.EventsLayout;
using Events.EventsLayout;
using Events;
using KinematicCharacterController;
using ScriptableObjects.Movement;
using UnityEngine.InputSystem;


public struct PlayerCharacterInputs {
    public float MoveAxisForward;
    public float MoveAxisRight;
    public Quaternion CameraRotation;
    public bool JumpDown;
    public bool Running;
    public bool ForceSecondJump;
}

namespace Controller.Movement {
    public class MovementController {
        private PlayerMovementHandler _character;

        private readonly UnityEngine.Camera _camera;
        private readonly LayerMask _groundMask;
        private KinematicCharacterMotor _playerKinematicMotor;
        private GameObject _playerGameObject;
        private CinemachineBrain _cinemachineBrain;

        private EventWithGameObj _walkingAnimationEventChannel;
        private EventWithBool _movementEnabledEvent;
        private EventWithVector3 _addMovementEvent;
        private MoveTowardsEvent _moveTowardsMoveTowardsEvent;
        private EventWithFloat _cameraRotationOffsetEvent;
        
        //playerInput
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _runAction;
        
        //camera rotation
        private Quaternion _cameraRotation;
        private bool _inverseRotation;

        public MovementController(ICharacterController character, LayerMask groundMask, GameObject player, PlayerInput playerInput, float cameraRotationOffset, bool inverseRotation) {
            _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            
            _camera = Camera.main;
            _groundMask = groundMask;
            _character = (PlayerMovementHandler)character;
            _inverseRotation = inverseRotation;
            
            if (player.TryGetComponent<KinematicCharacterMotor>(out var playerKinematicMotor)) {
                playerKinematicMotor.enabled = true;
                _playerKinematicMotor = playerKinematicMotor;

            }
            else throw new UnityException("player does not have a KinematicCharacterMotor");
            _playerGameObject = player;

            //initializeEvents
            SetupEvents(cameraRotationOffset);
            
            //initializePlayerInput
            _playerInput = playerInput;
            SetupPlayerInput();
        }

        private void SetupEvents(float cameraRotationOffset) {
            _movementEnabledEvent = ScriptableObject.CreateInstance<EventWithBool>();
            _movementEnabledEvent = (EventWithBool)EventBroker.TryToAddEventChannel("movementEnableEvent", _movementEnabledEvent);
            _movementEnabledEvent.Subscribe(new Action(() => _character.DisableMovement(!_movementEnabledEvent.eventBool)));

            _addMovementEvent = ScriptableObject.CreateInstance<EventWithVector3>();
            _addMovementEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("playerMovementEventChannel", _addMovementEvent);
            _addMovementEvent.Subscribe(new Action(() => _character.AddVelocity(_addMovementEvent.vector)));

            _moveTowardsMoveTowardsEvent = ScriptableObject.CreateInstance<MoveTowardsEvent>();
            _moveTowardsMoveTowardsEvent = (MoveTowardsEvent)EventBroker.TryToAddEventChannel("movePlayerTowardsEvent", _moveTowardsMoveTowardsEvent);
            _moveTowardsMoveTowardsEvent.Subscribe(new Action(() => _character.MoveTowards(_moveTowardsMoveTowardsEvent.goalPosition, _moveTowardsMoveTowardsEvent.time, _moveTowardsMoveTowardsEvent.requestJump, _moveTowardsMoveTowardsEvent.onComplete, _moveTowardsMoveTowardsEvent.goalDirection, _moveTowardsMoveTowardsEvent.coolDown)));

            _cameraRotationOffsetEvent = (EventWithFloat) EventBroker.TryToAddEventChannel("cameraRotationOffsetEvent", ScriptableObject.CreateInstance<EventWithFloat>());
            _cameraRotationOffsetEvent.UpdateValue(cameraRotationOffset);
            
            SetCameraRotation();
        }
        
        private void SetupPlayerInput() {
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
            _runAction = _playerInput.actions["Run"];
        }


        
        public void SetCameraRotation() {
            if(_cinemachineBrain.ActiveVirtualCamera==null)return;
            var cameraRotationOffset = _cameraRotationOffsetEvent.value;
            _cameraRotation = _cinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject.transform.rotation * Quaternion.Euler(0, -cameraRotationOffset, 0);
        }

        public void MovementControlUpdate() {
            HandleCharacterInput();
        }


        private void HandleCharacterInput() {
            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            characterInputs.CameraRotation = _cameraRotation;
            
            // Build the CharacterInputs struct
            characterInputs.MoveAxisForward = _moveAction.ReadValue<Vector2>().y;
            characterInputs.MoveAxisRight = _moveAction.ReadValue<Vector2>().x;
            characterInputs.JumpDown = _jumpAction.WasPressedThisFrame();

            if (_runAction.IsPressed()) characterInputs.Running = true;
            else characterInputs.Running = false;
            
            
            Vector3 cameraForward = _cameraRotation * Vector3.forward;
            Vector3 cameraRight;
            if (_inverseRotation)
                cameraRight = Quaternion.AngleAxis(-90, Vector3.up) * cameraForward;
            else
                cameraRight = Quaternion.AngleAxis(90, Vector3.up) * cameraForward;
            Vector3 targetRotation = (cameraRight * characterInputs.MoveAxisRight) + (cameraForward * characterInputs.MoveAxisForward);
            Vector3 rotation = Vector3.Lerp(_playerGameObject.transform.rotation.eulerAngles, targetRotation, Time.deltaTime);

            _character.SetInputs(ref characterInputs, rotation);    
        }

        public void SetGameObject(GameObject player, BodyType bodyType) {
            _playerGameObject = player;

            _playerKinematicMotor.enabled = false;

            if (player.TryGetComponent<KinematicCharacterMotor>(out var playerKinematicMotor)) {
                _playerKinematicMotor = playerKinematicMotor;
                //playerKinematicMotor.enabled = true;
            }
            else throw new UnityException("player does not have a KinematicCharacterMotor");

            //load desired settings
            ChangeSettings(bodyType);

            //set the correct obj to control
            _character.ChangeMotor(_playerKinematicMotor, _playerGameObject.transform.forward);
        }

        public void StartEveFlight() {
            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();
            characterInputs.MoveAxisForward = 0;
            characterInputs.MoveAxisRight = 0;
            characterInputs.JumpDown = true;
            characterInputs.ForceSecondJump = true;
            
            _character.SetInputs(ref characterInputs);
        }
        public Vector3 GetPlayerDirection()
        {
            return _playerGameObject.transform.forward;
        }

        private void ChangeSettings(BodyType bodyType) {
            KinematicMovementSettings movementSettings;
            _character.ToggleSecondJump(false);
            switch (bodyType) {
                case BodyType.Tank:
                    movementSettings = Resources.Load<KinematicMovementSettings>("Settings/MovementSettings/TankMovementSettings");
                    break;
                case BodyType.Support:
                    movementSettings = Resources.Load<KinematicMovementSettings>("Settings/MovementSettings/SupportMovementSettings");
                    break;
                case BodyType.Agile:
                    movementSettings = Resources.Load<KinematicMovementSettings>("Settings/MovementSettings/AgileKinematicMovementSettings");
                    _character.ToggleSecondJump(true);
                    break;
                default:
                    movementSettings = Resources.Load<KinematicMovementSettings>("Settings/MovementSettings/EveKinematicMovementSettings");
                    break;
            }
            _character.ChangeSettings(movementSettings);
        }

        private (bool success, Vector3 position) GetMousePosition() {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, _groundMask)) {
                // The Raycast hit something, return with the position.
                return (success: true, position: hitInfo.point);
            }
            // The Raycast did not hit anything.
            return (success: false, position: Vector3.zero);
        }
    }
}