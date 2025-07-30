using System.Collections;
using System.Collections.Generic;
using PlayerControll.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Events.EventsLayout;
using Events.EventsLayout;
using Events;
using System;
using Ami.BroAudio;
using UnityEngine.Serialization;

namespace Controller.Stats {

    public class StatsController : MonoBehaviour {
        [SerializeField] private float startingStamina = 100f;
        [SerializeField] private List<Image> uiLowHealthImages = new List<Image>();
        private StatsSettings _statsSettings;
        private bool _isHealthLow = false;
        
        private float _maxHealth = 100f;
        private float _maxStamina = 100f;
        private float _currentHealth;
        private float _currentStamina;
        private int _staminaRecoveryRate;
        private float _staminaRecoveryDelay;
        private Coroutine _staminaRecoveryCoroutine;
        private Coroutine _staminaConsumerCoroutine;
        private Coroutine _healCoroutine;
        private GameObject _player;
        private BodyType _currentBodyType;

        public Slider healthSlider;
        public TextMeshProUGUI healthText;
        public Slider staminaSlider;
        public TextMeshProUGUI staminaText;
        public SoundID parrySound = default;
        private EventTakeDamage _takeDamageEvent;
        private EventWithBool _parryEvent;
        private EventUseStamina _useStaminaEvent;
        private EventHeal _healEvent;
        private EventGetStats _getStatsEvent;
        private EventWithFloat _staminaIncreaseEvent;
        private EventGetStats _setStatsEvent;
        private EventWithVector3 _deathEvent;

        private bool _loadBool = false;

        /// 
        /// General Management
        ///
        void Awake() {
            // Load default stats settings
            _statsSettings = Resources.Load<StatsSettings>("Settings/StatsSettings/EveStatsSettings");
            _maxHealth = _statsSettings.Health;
            _currentHealth = _maxHealth;
            _currentStamina = startingStamina;
            _staminaRecoveryRate = _statsSettings.StaminaRecoveryRate;
            _staminaRecoveryDelay = _statsSettings.StaminaRecoveryDelay;


            // Subscribe to events
            _setStatsEvent = (EventGetStats)EventBroker.TryToAddEventChannel("setStatsEvent", ScriptableObject.CreateInstance<EventGetStats>());
            _setStatsEvent.Subscribe(new Action(() => SetStats(_setStatsEvent.currentHealth, _setStatsEvent.currentStamina, _setStatsEvent.b)));
            _getStatsEvent = (EventGetStats)EventBroker.TryToAddEventChannel("getStatsEvent", ScriptableObject.CreateInstance<EventGetStats>());
        }
        void Start() {
            sentSignal = false;
            // Subscribe to events
            _takeDamageEvent = (EventTakeDamage)EventBroker.TryToAddEventChannel("takeDamageEvent", ScriptableObject.CreateInstance<EventTakeDamage>());
            _takeDamageEvent.Subscribe(new Action(() => TakeDamage(_takeDamageEvent.damageAmount)));
            _parryEvent = (EventWithBool)EventBroker.TryToAddEventChannel("ParryController", ScriptableObject.CreateInstance<EventWithBool>());
            _parryEvent.Subscribe(new Action(() => {
                if (_parryEvent.eventBool) StopHealing();
            }));
            _staminaIncreaseEvent = (EventWithFloat)EventBroker.TryToAddEventChannel("staminaIncrease", ScriptableObject.CreateInstance<EventWithFloat>());
            _staminaIncreaseEvent.Subscribe(new Action(() => {
                var oldMaxStamina = _maxStamina;
                _maxStamina += _staminaIncreaseEvent.value;
                _currentStamina = _maxStamina;
                ChangeStaminaBarColor(new Color(1f, 1f, 0f));
                UpdateStatsUI();
                StartCoroutine(StaminaMaxReset(oldMaxStamina));
            }));

            _useStaminaEvent = (EventUseStamina)EventBroker.TryToAddEventChannel("useStaminaEvent", ScriptableObject.CreateInstance<EventUseStamina>());
            _useStaminaEvent.Subscribe(new Action(() => {
                try {
                    // Check if is a toggle consumption
                    if (_useStaminaEvent.ToggleConsumption) {
                        ToggleConsumption(_useStaminaEvent.staminaAmount, _useStaminaEvent.rateConsumption);
                        return;
                    }
                    UseStamina(_useStaminaEvent.staminaAmount);
                    _useStaminaEvent.OnStaminaUsed?.Invoke(null); // No exception, invoke with null
                }
                catch (NotEnoughStaminaException ex) {
                    Debug.LogError(ex.Message);
                    _useStaminaEvent.OnStaminaUsed?.Invoke(ex); // Pass the exception to the callback
                }
            }));

            _healEvent = (EventHeal)EventBroker.TryToAddEventChannel("healEvent", ScriptableObject.CreateInstance<EventHeal>());
            _healEvent.Subscribe(new Action(() => ToggleHeal(_healEvent.healRate, _healEvent.healDuration)));

            _deathEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("deathEvent", ScriptableObject.CreateInstance<EventWithVector3>());
            // Initialize the health UI
            InitializeHealthUI();
        }



        private void ChangeStaminaBarColor(Color color) {
            var fillI = staminaSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();
            fillI.color = color;
        }
        private IEnumerator StaminaMaxReset(float oldMaxStamina) {
            yield return new WaitUntil(() => _currentStamina < oldMaxStamina);
            ChangeStaminaBarColor(new Color(1f, 1f, 1f));
            _maxStamina = oldMaxStamina;
        }

        public void SetGameObject(BodyType bodyType, GameObject player) {
            _player = player;
            //avoid bugs
            _currentBodyType = bodyType;
            StopHealing();
            StopConsumingStamina();

            //Debug.LogWarning("StatsController SetGameObject with bodyType: " + bodyType);
            switch (bodyType) {
                case BodyType.Eve:
                    _statsSettings = Resources.Load<StatsSettings>("Settings/StatsSettings/EveStatsSettings");
                    break;
                case BodyType.Robot:
                    _statsSettings = Resources.Load<StatsSettings>("Settings/StatsSettings/RobotStatsSettings");
                    break;
                case BodyType.Tank:
                    _statsSettings = Resources.Load<StatsSettings>("Settings/StatsSettings/TankStatsSettings");
                    break;
                case BodyType.Agile:
                    _statsSettings = Resources.Load<StatsSettings>("Settings/StatsSettings/AgileStatsSettings");
                    break;
                case BodyType.Support:
                    _statsSettings = Resources.Load<StatsSettings>("Settings/StatsSettings/SupportStatsSettings");
                    break;
                default:
                    _statsSettings = Resources.Load<StatsSettings>("Settings/StatsSettings/EveStatsSettings");
                    break;
            }

            _maxHealth = _statsSettings.Health;
            _maxStamina = _statsSettings.Stamina;
            _staminaRecoveryDelay = _statsSettings.StaminaRecoveryDelay;
            _staminaRecoveryRate = _statsSettings.StaminaRecoveryRate;
            
            if (!_loadBool) {
                _currentHealth = _maxHealth;
                _currentStamina = startingStamina;
            }else {
                _loadBool = false;
            }

            if (_currentStamina > _maxStamina) {
                _currentStamina = _maxStamina;
            }
            else if (_currentStamina < _maxStamina) {
                if (_staminaRecoveryCoroutine != null) {
                    StopCoroutine(_staminaRecoveryCoroutine);
                }
                _staminaRecoveryCoroutine = StartCoroutine(StaminaRecoveryCooldown());
            }
            
            InitializeHealthUI();
        }


        private void SetStats(float health, float stamina, bool loadBool = false) {
            _loadBool = loadBool;
            _currentHealth = health;
            _currentStamina = stamina;
            UpdateStatsUI();

            if (_staminaRecoveryCoroutine == null && _currentStamina < _maxStamina) _staminaRecoveryCoroutine = StartCoroutine(StaminaRecoveryCooldown());
            
            if (_currentHealth <= _maxHealth/2.5) UILowHealth();
            else if (_isHealthLow) UILowHealthReset();
        }

        ///
        /// UI management
        ///
        private void InitializeHealthUI() {
            if (healthSlider != null) {
                healthSlider.maxValue = _maxHealth;
                healthSlider.value = _currentHealth;
            }

            if (healthText != null) {
                healthText.text = $"{_currentHealth} / {_maxHealth}";
            }
            if (staminaSlider != null) {
                staminaSlider.maxValue = _maxStamina;
                staminaSlider.value = _currentStamina;
            }

            if (staminaText != null) {
                staminaText.text = $"{_currentStamina} / {_maxStamina}";
            }
            
            if (_currentHealth <= _maxHealth/2.5) UILowHealth();
            else if (_isHealthLow) UILowHealthReset();
            
            // Set the current stats to the event
            _getStatsEvent.UpdateValue(gameObject, _currentHealth, _currentStamina);
        }

        private void UpdateStatsUI() {
            if (healthSlider != null) {
                healthSlider.value = _currentHealth;
            }

            if (healthText != null) {
                healthText.text = $"{_currentHealth} / {_maxHealth}";
            }
            if (staminaSlider != null) {
                staminaSlider.value = _currentStamina;
            }
            if (staminaText != null) {
                staminaText.text = $"{_currentStamina} / {_maxStamina}";
            }

            // Set the current stats to the event
            _getStatsEvent.UpdateValue(gameObject, _currentHealth, _currentStamina);
        }



        ///
        /// Health management
        ///
        private bool sentSignal = false;
        public void TakeDamage(float damage) {
            if (_parryEvent.eventBool) {
                //if perry is active do not take damage
                BroAudio.Play(parrySound);
                return;
            }

            StopHealing();
            StopConsumingStamina(true, (ex) => {
                if (ex != null) {
                    Debug.LogError(ex.Message);
                    _useStaminaEvent.OnStaminaUsed?.Invoke(ex); // Pass the exception to the callback
                }
            });
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            BroAudio.Play(_statsSettings.HurtSound);

            if (_currentHealth <= 0 && !sentSignal) {
                sentSignal = true;
                if (_currentBodyType != BodyType.Eve) {
                    _deathEvent.UpdateValue(_player.gameObject.transform.position);
                } else {
                    _deathEvent.UpdateValue(Vector3.zero);
                }
                BroAudio.Play(_statsSettings.DeathSound);
                //_currentHealth = _maxHealth;        //reset Health
            }
            UpdateStatsUI();

            if (_currentHealth <= _maxHealth/2.5) UILowHealth();
            else if (_isHealthLow) UILowHealthReset();
        }

        private void UILowHealth() {
            _isHealthLow = true;
            var color = new Color(1f, 1f, 0f);
            foreach (var image in uiLowHealthImages) {
                image.color = color;
            }
        }

        private void UILowHealthReset() {
            _isHealthLow = false;
            var color = new Color(1f, 1f, 1f);
            foreach (var image in uiLowHealthImages) {
                image.color = color;
            }
        }
        
        public void ToggleHeal(float healAmount, float healDuration = 1f) {
            if (_healCoroutine == null) {
                StartHealing(healAmount, healDuration);
            }
            else {
                StopHealing();
            }
        }

        public void StartHealing(float healAmount, float healDuration) {
            if (_healCoroutine != null) {
                StopCoroutine(_healCoroutine);
            }
            _healCoroutine = StartCoroutine(HealOverTime(healAmount, healDuration));
        }

        public void StopHealing() {
            if (_healCoroutine != null) {
                StopCoroutine(_healCoroutine);
                _healCoroutine = null;
            }
        }

        private IEnumerator HealOverTime(float healRate, float healDuration) {
            while (_currentHealth < _maxHealth) {
                _currentHealth += healRate;

                _currentHealth = Mathf.Max(_currentHealth, 0);

                UpdateStatsUI();

                if (_currentHealth <= _maxHealth/2.5) UILowHealth();
                else if (_isHealthLow) UILowHealthReset();
                
                yield return new WaitForSeconds(healDuration);
            }
            // If the player runs out of stamina, stop this coroutine and start the stamina recovery coroutine
            _healCoroutine = null;
        }

        public float GetCurrentHealth() {
            return _currentHealth;
        }
        ///
        /// Stamina management
        ///
        public void UseStamina(float amount) {
            StopHealing();
            StopConsumingStamina();
            if (_currentStamina >= amount) {
                // Stop any ongoing stamina recovery coroutine
                if (_staminaRecoveryCoroutine != null) {
                    StopCoroutine(_staminaRecoveryCoroutine);
                }
                _currentStamina -= amount;
                _currentStamina = Mathf.Clamp(_currentStamina, 0, _maxStamina);
                UpdateStatsUI();

                // Start the cooldown timer for stamina recovery
                _staminaRecoveryCoroutine = StartCoroutine(StaminaRecoveryCooldown());
            }
            else {
                throw new NotEnoughStaminaException("Not enough stamina to perform the action.");
            }
        }
        private IEnumerator StaminaRecoveryCooldown() {
            // Wait for the cooldown period
            yield return new WaitForSeconds(_staminaRecoveryDelay);
            float accumulatedRecovery = 0f;
            // Start recovering stamina
            while (_currentStamina < _maxStamina) {
                accumulatedRecovery += _staminaRecoveryRate * Time.deltaTime;
                int recoveryAmount = Mathf.FloorToInt(accumulatedRecovery);


                if (recoveryAmount >= _staminaRecoveryRate) {
                    // Make sure the recovery amount is not greater than the recovery rate
                    recoveryAmount = _staminaRecoveryRate;
                    _currentStamina += recoveryAmount;
                    _currentStamina = Mathf.Clamp(_currentStamina, 0, _maxStamina);
                    accumulatedRecovery -= recoveryAmount;
                    UpdateStatsUI();
                }

                yield return null;
            }
            _staminaRecoveryCoroutine = null;
        }

        public void ToggleConsumption(float staminaAmount, float rateConsumption = 1f) {
            if (_staminaConsumerCoroutine == null) {
                StartConsumingStamina(staminaAmount, rateConsumption);
            }
            else {
                StopConsumingStamina(false); // Do not send exception when toggling
            }
        }

        public void StartConsumingStamina(float staminaAmount, float rateConsumption = 1f) {
            if (_staminaRecoveryCoroutine != null) {
                StopCoroutine(_staminaRecoveryCoroutine);
            }
            if (_staminaConsumerCoroutine != null) {
                StopCoroutine(_staminaConsumerCoroutine);
            }
            _staminaConsumerCoroutine = StartCoroutine(ConsumeStamina(staminaAmount, rateConsumption, (ex) => {
                if (ex != null) {
                    _useStaminaEvent.OnStaminaUsed?.Invoke(ex); // Pass the exception to the callback
                }
            }));
        }

        public void StopConsumingStamina(bool sendException = false, Action<Exception> onStaminaDepleted = null) {
            if (_staminaConsumerCoroutine != null) {
                StopCoroutine(_staminaConsumerCoroutine);
                _staminaConsumerCoroutine = null;
                if (_staminaRecoveryCoroutine != null) {
                    StopCoroutine(_staminaRecoveryCoroutine);
                }
                _staminaRecoveryCoroutine = StartCoroutine(StaminaRecoveryCooldown());

                if (sendException) {
                    onStaminaDepleted?.Invoke(new NotEnoughStaminaException("Stamina consumption stopped."));
                }
            }
        }

        private IEnumerator ConsumeStamina(float staminaAmount, float rateConsumption, Action<Exception> onStaminaDepleted) {
            while (_currentStamina > 0) {
                _currentStamina -= staminaAmount;
                _currentStamina = Mathf.Max(_currentStamina, 0);

                UpdateStatsUI();

                yield return new WaitForSeconds(rateConsumption);
            }

            // If the player runs out of stamina, stop this coroutine and start the stamina recovery coroutine
            _staminaConsumerCoroutine = null;
            if (_staminaRecoveryCoroutine != null) {
                StopCoroutine(_staminaRecoveryCoroutine);
            }
            _staminaRecoveryCoroutine = StartCoroutine(StaminaRecoveryCooldown());

            // if is healing stop the healing coroutine
            if (_healCoroutine != null) {
                StopCoroutine(_healCoroutine);
                _healCoroutine = null;
            }

            // Invoke the callback with the exception
            onStaminaDepleted?.Invoke(new NotEnoughStaminaException("Stamina depleted."));
        }

    }
    public class NotEnoughStaminaException : Exception {
        public NotEnoughStaminaException(string message) : base(message) { }
    }
}
