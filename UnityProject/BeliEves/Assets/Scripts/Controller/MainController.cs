using System;
using System.Collections;
using AimIK.Behaviour;
using Ami.BroAudio;
using Cinemachine;
using Controller.Animations;
using Controller.Attack;
using Controller.Attack.AgileAttack;
using Controller.CameraManager;
using Controller.Movement;
using Controller.Stats;
using Controller.Stats.Bonus;
using Events;
using Managers.CameraManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utilities;
using Utilities.Events.EventsLayout;

namespace Controller {
    public class MainController : Singleton<MainController> {
        [Header("Settings")]
        [SerializeField] private PlayerMovementHandler character;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private InventoryController inventoryController;

        [Header("Camera")]
        [SerializeField] private CinemachineVirtualCamera defaultCamera;
        [SerializeField] private float defaultCameraAngle;
        [SerializeField] private bool inverseCameraRotation;
        
        [Header("perry")]
        [SerializeField] private float perryTime = 1f;

        [Header("Battery Bonus")]
        [SerializeField] private float bonusAmount = 0;
        [SerializeField] private GameObject batteryUI;
        [SerializeField] private SoundID _batteryActivateSound = default;

        private MovementController _movementController;
        private AttackController _attackController;
        private StatsController _statsController;
        private AnimationController _animationController;
        private CamFollow _camFollow;
        private ParryController _parryController;
        private BatteryController _batteryController;

        private EventBodySwitch _bodySwitchChannel;
        private EventWithFloat _defaultCameraAngleEvent;

        //camera rotation
        private CinemachineBrain _cinemachineBrain;

        public void Awake() {
            var player = FindObjectOfType<Player.Player>().gameObject;
            _movementController = new MovementController(character, groundMask, player, GetComponent<PlayerInput>(), defaultCameraAngle, inverseCameraRotation);
            _defaultCameraAngleEvent = (EventWithFloat)EventBroker.TryToAddEventChannel("defaultCameraAngle", ScriptableObject.CreateInstance<EventWithFloat>());
            _camFollow = new CamFollow(player, defaultCamera);
            if (batteryUI != null)
                _batteryController = new BatteryController(bonusAmount, GetComponent<PlayerInput>(), batteryUI, _batteryActivateSound);
        }
        public void Start() {
            //setting up Event
            _bodySwitchChannel = (EventBodySwitch)EventBroker.TryToAddEventChannel("bodySwitch", ScriptableObject.CreateInstance<EventBodySwitch>());
            _bodySwitchChannel.Subscribe(new Action(() => SetGameObject(_bodySwitchChannel.bodyType)));
            
            var player = FindObjectOfType<Player.Player>().gameObject;
            
            _statsController = GetComponent<StatsController>();
            _animationController = AnimationController.Instance;
            _animationController = AnimationController.Instance;
            _animationController.Initalize();
            _parryController = new ParryController(perryTime, GetComponent<PlayerInput>());

            SetGameObject(_bodySwitchChannel.bodyType);
            //set camera rotation
            _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (_cinemachineBrain != null) {
                _cinemachineBrain.m_CameraActivatedEvent.AddListener(CameraSwitchCallback);
            }
        }
        void Update() {
            if (_movementController != null)
                _movementController.MovementControlUpdate();
            if (_parryController != null)
                _parryController.PerryUpdate();
            if (_batteryController != null)
                _batteryController.BatteryUpdate();
        }

        private void CameraSwitchCallback(ICinemachineCamera incomingCamera, ICinemachineCamera outgoingCamera) {
            StartCoroutine(CameraSwitchCallback());
        }
        private IEnumerator CameraSwitchCallback() {
            while (_cinemachineBrain.IsBlending) {
                yield return null;
            }
            _movementController.SetCameraRotation();
        }


        private void SetGameObject(BodyType bodyType) {
            var player = FindObjectOfType<Player.Player>().gameObject;
            _movementController.SetGameObject(player, bodyType);
            inventoryController.BodySwitch(player);
            _camFollow.SetTarget(player);
            _statsController.SetGameObject(bodyType, player);
            _animationController.SetGameObject(player);

            if (_attackController != null) _attackController.DestroyAttackController();

            switch (bodyType) {
                case BodyType.Tank:
                    _attackController = new TankAttackController(_movementController.GetPlayerDirection);
                    break;
                case BodyType.Agile:
                    _attackController = new AgileAttackController(_movementController.GetPlayerDirection);
                    break;
                case BodyType.Support:
                    _attackController = new SupportAttackController(_movementController.GetPlayerDirection);
                    break;
                default:
                    _attackController = new EveAttackController(_movementController.GetPlayerDirection, _movementController.StartEveFlight);
                    break;
            }
            _attackController.SetGameObject(player);
            _parryController.BodySwitch(bodyType, player);
        }
    }
}
