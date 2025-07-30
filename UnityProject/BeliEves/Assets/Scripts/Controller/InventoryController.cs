using System;
using System.Collections;
using Ami.BroAudio;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities.Events.EventsLayout;

namespace Controller {
    public class InventoryController : MonoBehaviour {
        //TODO add UI image to display the current saved robot
        [SerializeField] GameObject inventoryPanel;
        [SerializeField] Image inventoryContentImage;
        [SerializeField] protected float longPressRequiredTime = 0.5f;
        private float currentPressTime = 0f;

        private Image _inventoryPanelLoadingImg;
        private GameObject _inventoryPanelText;
        private GameObject _player;

        private InventoryEventChannel _bodySaveChannel;
        private InventoryEventChannel _preSaveChannel;
        private EventBodySwitch _bodySwitchChannel;
        private EventGetStats _getStatsEvent;
        private EventGetStats _setStatsEvent;

        private BodyType _currentSavedBody = BodyType.Eve;
        private float _savedHealth = 0f;
        private float _savedStamina = 0f;


        [Header("Audios")]
        [SerializeField] private SoundID _saveSound;
        [SerializeField] private SoundID _bodySwapSound;

        private IAudioPlayer _audioPlayer;

        //playerInput
        private PlayerInput _playerInput;
        private InputAction _eveReleaseAction;

        public void Awake() {
            //setting up Event
            _preSaveChannel = (InventoryEventChannel)EventBroker.TryToAddEventChannel("PreSave", ScriptableObject.CreateInstance<InventoryEventChannel>());//used from game manager to set the inventory
            _preSaveChannel.Subscribe(new Action(() => PreSaveRobot(_preSaveChannel.bodyType)));
            _bodySaveChannel = (InventoryEventChannel)EventBroker.TryToAddEventChannel("bodySave", ScriptableObject.CreateInstance<InventoryEventChannel>());//used from player when switch robot in the inventory
            _bodySaveChannel.Subscribe(new Action(() => SaveRobot(_bodySaveChannel.bodyType)));

            //get UI element
            _inventoryPanelLoadingImg = inventoryPanel.transform.Find("LoadingImage").GetComponent<Image>();
            _inventoryPanelText = inventoryPanel.transform.Find("SavedRobotType").gameObject;
        }

        public void Start() {
            //setting up Event
            _bodySwitchChannel = (EventBodySwitch)EventBroker.TryToAddEventChannel("bodySwitch", ScriptableObject.CreateInstance<EventBodySwitch>());
            _setStatsEvent = (EventGetStats)EventBroker.TryToAddEventChannel("setStatsEvent", ScriptableObject.CreateInstance<EventGetStats>());
            _getStatsEvent = (EventGetStats)EventBroker.TryToAddEventChannel("getStatsEvent", ScriptableObject.CreateInstance<EventGetStats>());

            //initializePlayerInput
            _playerInput = FindObjectOfType<PlayerInput>();
            SetupPlayerInput();
        }

        public void Update() {
            if (_eveReleaseAction.IsPressed()) {
                currentPressTime += Time.deltaTime; // Increment by the time since the last frame.
                _inventoryPanelLoadingImg.fillAmount = Mathf.Clamp01(currentPressTime / longPressRequiredTime); // Update the loading image.
            }
            else {
                currentPressTime = 0f; // Reset the press time.
                _inventoryPanelLoadingImg.fillAmount = 0f; // Reset the loading image.
            }
        }

        public void BodySwitch(GameObject player) {
            _player = player;
            if (_audioPlayer != null) {
                if (_audioPlayer.IsActive) return;
            }
            BroAudio.Play(_bodySwapSound);
        }
        
        private void PreSaveRobot(BodyType bodyType) {
            _currentSavedBody = bodyType;
            _bodySaveChannel.bodyType = bodyType;

            _savedStamina = _preSaveChannel.savedStamina;
            _savedHealth = _preSaveChannel.savedHealth;
            
            DisplaySavedRobotNameAndImg();
        }

        private void SetupPlayerInput() {
            _eveReleaseAction = _playerInput.actions["EveRelease"];
        }


        private void SaveRobot(BodyType bodyType) {
            if (bodyType == BodyType.Eve && _currentSavedBody == BodyType.Eve) return;
            _audioPlayer = BroAudio.Play(_saveSound);
            var oldBody = _bodySwitchChannel.bodyType;

            Vector3 newPosition = new Vector3(_player.transform.position.x, _player.transform.position.y, _player.transform.position.z);
            Destroy(_player);

            var tmpHealth = _getStatsEvent.currentHealth;
            var tmpStamina = _getStatsEvent.currentStamina;
            
            if (bodyType != BodyType.Eve && _currentSavedBody == BodyType.Eve) {
                var eve = Instantiate(Resources.Load<GameObject>("Prefabs/Eve"), newPosition, Quaternion.identity);
                StartCoroutine(VFXCoroutine(eve, Resources.Load<GameObject>("Vfx/WWEnemyExplosion")));
                _bodySwitchChannel.UpdateValue(BodyType.Eve);
                //_setStatsEvent.UpdateValue(null, 100,100);
            }
            else {
                var robot = LoadRobot(_currentSavedBody, newPosition);
                StartCoroutine(VFXCoroutine(robot, Resources.Load<GameObject>("Vfx/WWEnemyExplosion")));
            }

            _currentSavedBody = _currentSavedBody != oldBody ? bodyType : BodyType.Eve;
            
            
            _savedHealth = tmpHealth;
            _savedStamina = tmpStamina;

            _bodySaveChannel.savedHealth = _savedHealth;
            _bodySaveChannel.savedStamina = _savedStamina;
            
            DisplaySavedRobotNameAndImg();
        }

        private GameObject LoadRobot(BodyType bodyType, Vector3 position) {
            GameObject robot = null;
            switch (bodyType) {
                case BodyType.Agile:
                    robot = Instantiate(Resources.Load<GameObject>("Prefabs/Robots/AgileRobot Player Variant"), position, Quaternion.identity);
                    break;
                case BodyType.Support:
                    robot = Instantiate(Resources.Load<GameObject>("Prefabs/Robots/SupportRobot Player Variant"), position, Quaternion.identity);
                    break;
                case BodyType.Tank:
                    robot = Instantiate(Resources.Load<GameObject>("Prefabs/Robots/TankRobot Player Variant"), position, Quaternion.identity);
                    break;
            }
            _bodySwitchChannel.UpdateValue(bodyType);
            _setStatsEvent.UpdateValue(null, _savedHealth,_savedStamina);
            return robot;
        }

        private void DisplaySavedRobotNameAndImg() {
            if (_currentSavedBody != BodyType.Eve) {
                inventoryContentImage.gameObject.SetActive(true);
                inventoryContentImage.sprite = Resources.Load<Sprite>($"UI/RobotImages/{_currentSavedBody}UI");
                _inventoryPanelText.SetActive(true);
                var text = _inventoryPanelText.GetComponent<TextMeshProUGUI>();
                text.text = _bodySaveChannel.bodyType.ToString();
            }
            else {
                inventoryContentImage.gameObject.SetActive(false);
                _inventoryPanelText.SetActive(false);
            }
        }

        private IEnumerator VFXCoroutine(GameObject inputGameObject, GameObject vfx) {
            // Instantiate the VFX as a child of the inputGameObject
            GameObject go = Instantiate(vfx, inputGameObject.transform, true);

            // Calculate the base position of the inputGameObject
            Collider collider = inputGameObject.GetComponent<Collider>();
            var position = new Vector3(
                collider.bounds.center.x,
                collider.bounds.min.y,
                collider.bounds.center.z
            );
            // Set the VFX position to the calculated base position
            go.transform.position = position;

            // Wait for 1.5 seconds
            yield return new WaitForSeconds(1.5f);

            // Destroy the VFX object
            Destroy(go);
        }
    }
}