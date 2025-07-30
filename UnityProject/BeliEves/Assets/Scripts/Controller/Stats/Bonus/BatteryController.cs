using System;
using System.Collections;
using Ami.BroAudio;
using Events;
using Events.EventsLayout;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities;
using Utilities.Events.EventsLayout;
using Utilities.Pool;

namespace Controller.Stats.Bonus {
    public class BatteryController {
        private int _activeBatteryCount = 0;
        private readonly int _requiredBatteryCount = 5;
        private readonly float _bonusAmount;
        private GameObject _batteryText;
        private Slider _batterySlider;

        //events
        private readonly BasicEventChannel _batteryEventChannel;
        private EventWithFloat _currentBatteryCount;
        private EventWithFloat _loadBatteryCount;
        private EventWithFloat _staminaIncreaseEvent;
        private EventWithVector3 _placeBatteryEvent;

        //playerInput
        private readonly PlayerInput _playerInput;
        private SoundID _batteryActivateSound;
        private InputAction _bonusAction;
        
        private Coroutine _bonusCoroutine;
        public BatteryController(float bonusAmount, PlayerInput playerInput, GameObject ui, SoundID batteryActivateSound) {
            _bonusAmount = bonusAmount;
            _batteryActivateSound = batteryActivateSound;
            _batteryText = ui.transform.Find("BatteryText")?.gameObject;
            _batteryText.SetActive(false);
            _batterySlider = ui.transform.Find("BonusBar").GetComponent<Slider>();
            _batterySlider.value = 0;
            
            
            //setup events
            _batteryEventChannel = EventBroker.TryToAddEventChannel("BatteryEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
            _batteryEventChannel.Subscribe(BatteryCallBack);
            _currentBatteryCount = (EventWithFloat)EventBroker.TryToAddEventChannel("CurrentBatteryCount", ScriptableObject.CreateInstance<EventWithFloat>());
            _currentBatteryCount.Subscribe(new Action(()=>{SetupBattery(_currentBatteryCount.value);}));
            _currentBatteryCount = (EventWithFloat)EventBroker.TryToAddEventChannel("CurrentBatteryCount", ScriptableObject.CreateInstance<EventWithFloat>());
            _currentBatteryCount.Subscribe(new Action(()=>{SetupBattery(_currentBatteryCount.value);}));
            _staminaIncreaseEvent = (EventWithFloat)EventBroker.TryToAddEventChannel("staminaIncrease", ScriptableObject.CreateInstance<EventWithFloat>());
            _placeBatteryEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("placeBattery", ScriptableObject.CreateInstance<EventWithVector3>());
            _placeBatteryEvent.Subscribe(new Action(() => SpawnBattery(_placeBatteryEvent.vector)));

            //initializePlayerInput
            _playerInput = playerInput;
            SetupPlayerInput();
            
            BatteryPoolSetup();
        }
        private void SetupPlayerInput() {
            _bonusAction = _playerInput.actions["StatsBonus"];
        }

        private void SetupBattery(float batteryCount) {
            if(Mathf.Approximately(batteryCount, _activeBatteryCount))return;
            _activeBatteryCount = (int)batteryCount;
            UpdateUI(_activeBatteryCount);
            
            if (_activeBatteryCount == _requiredBatteryCount) {
                var control = InputSystem.FindControl(_bonusAction.bindings[0].effectivePath);
                _batteryText.GetComponent<TextMeshProUGUI>().text = "press [" + control.name.ToUpper() + "] to use the battery";
                _batteryText.SetActive(true);
            }
        }

        private void BatteryPoolSetup() {
            PoolManager.CreateNewPool("BatteryPool", Resources.Load<GameObject>("Battery/Battery"), null, 25, 10);
        }

        private void SpawnBattery(Vector3 position) {
            PoolManager.GetPoolObject<Npc.Npc>("BatteryPool", position);
        }
        
        public void BatteryUpdate() {
            if (_bonusAction.WasPressedThisFrame()) ApplyBonus();
        }

        private void BatteryCallBack() {
            if (_activeBatteryCount >= _requiredBatteryCount) return;
            _activeBatteryCount += 1;
            _currentBatteryCount.UpdateValue(_activeBatteryCount);
            
            UpdateUI(_activeBatteryCount);

            if (_activeBatteryCount == _requiredBatteryCount) {
                var control = InputSystem.FindControl(_bonusAction.bindings[0].effectivePath);
                _batteryText.GetComponent<TextMeshProUGUI>().text = "press [" + control.name.ToUpper() + "] to use the battery";
                _batteryText.SetActive(true);
            }
        }

        private void ApplyBonus() {
            if (_activeBatteryCount >= _requiredBatteryCount && _bonusCoroutine == null) {
                _bonusCoroutine = CoroutineRunner.Instance.StartCoroutine(PlayAudioAndApplyBonus());
            }
        }

        private IEnumerator PlayAudioAndApplyBonus() {
            BroAudio.Play(_batteryActivateSound);

            // Wait for the audio to finish playing
            //yield return new WaitForSeconds(2f);
            _activeBatteryCount = 0;
            UpdateUI(_activeBatteryCount);
            _currentBatteryCount.UpdateValue(_activeBatteryCount);

            _staminaIncreaseEvent.UpdateValue(_bonusAmount);
            _bonusCoroutine = null;
            _batteryText.SetActive(false);
            yield return null;
        }

        private void UpdateUI(int batteryCount) {
            _batterySlider.value = batteryCount;
        }
    }
}