using System;
using System.Collections;
using Events;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Events.EventsLayout;
using Controller.Attack;
using Controller.Attack.AgileAttack;
using Controller;
using UnityEngine.AI;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using static Utilities.Pool.PoolManager;

namespace Npc {
    public class Enemy : MonoBehaviour {

        public Slider influenceBar;
        private Image _fillImage;
        private Camera _camera;

        [SerializeField] private float _influence = 100;
        [SerializeField] public bool _isDefeated = false;
        [SerializeField] private float TakeoverTime = 10;

        // Influence bar color
        [SerializeField] public Color _influenceColor = Color.magenta;
        [SerializeField] public Color _defeatedColor = Color.red;

        [SerializeField] public List<SkinnedMeshRenderer> body = new List<SkinnedMeshRenderer>();
        [SerializeField] public Material BaseMaterial = null;

        private Coroutine claimableCoroutine;
        private Coroutine checkPlayerCoroutine;
        private float _maxInfluence;

        public GameObject lightAttackColliderGameObject;
        public GameObject heavyAttackColliderGameObject;
        public GameObject specialAttackColliderGameObject;
        public bool isAttackActive = false;
        public bool LightCooldown = false;
        public bool HeavyCooldown = false;
        public bool SpecialCooldown = false;

        public bool Yolo = false;

        private AttackController _attackController;
        private String type;
        public bool HasSpecial = false;
        private Animator _animator;
        private NavMeshAgent _navAgent;

        [Header("Screen")]
        [SerializeField] private GameObject screen;
        [SerializeField] private Material nextScreenMaterial;
        [SerializeField] private Material defaultScreenMaterial;

        void Start() {
            _maxInfluence = _influence;
            StartEnemy();
        }
        

        public void StartEnemy() {
            ResetScreenMaterial();
            _animator = GetComponentInChildren<Animator>();
            _navAgent = GetComponent<NavMeshAgent>();
            influenceBar.maxValue = _influence;
            influenceBar.value = _influence;
            _camera = Camera.main;
            _fillImage = influenceBar.fillRect.GetComponent<Image>();
            _fillImage.color = _influenceColor;
            Yolo = false;

            specialAttackColliderGameObject = this.transform.Find("SpecialAttackCollider").gameObject;

            if (_isDefeated) {
                ChangeScreenMaterial();
                // Set the layer of the enemy to the NPC layer
                gameObject.layer = 3;

                // Set the tag of the SpecialAttackCollider to Heal
                specialAttackColliderGameObject.tag = "Heal";

                Destroy(influenceBar.gameObject);
                // Find and destroy the Behaviour executor script searching by name
                Destroy(gameObject.GetComponent("BehaviorExecutor"));
                // Destroy the navMeshAgent
                Destroy(gameObject.GetComponent<NavMeshAgent>());
                Destroy(this);
            }

            lightAttackColliderGameObject = this.transform.Find("LightAttackCollider").gameObject;
            lightAttackColliderGameObject.SetActive(false);
            heavyAttackColliderGameObject = this.transform.Find("HeavyAttackCollider").gameObject;
            heavyAttackColliderGameObject.SetActive(false);
            specialAttackColliderGameObject.SetActive(false);

            // Check the type of enemy and set the appropriate controller
            if (GetComponent<SupportRobot>()) {
                //Debug.Log("Support robot enemy detected");
                _attackController = new SupportAttackController(() => transform.forward, false);
                type = "SupportRobot";
            }
            else if (GetComponent<AgileRobot>()) {
                _attackController = new AgileAttackController(() => transform.forward, false);
                _attackController.SetProjectileSpawner(GetComponent<ProjectileSpawner>());
                type = "AgileRobot";
            }
            else if (GetComponent<TankRobot>()) {
                _attackController = new TankAttackController(() => transform.forward, false);
                type = "TankRobot";
                HasSpecial = true;
            }
            // Set the "player" for the controller as the enemy
            _attackController.SetGameObject(gameObject);


            // Find all of the body parts of the enemy that has the tag "Body" and save the Material in a list to change the color of the enemy later
            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            // Filter the body parts that have the tag "Body"
            foreach (SkinnedMeshRenderer renderer in renderers) {
                if (renderer.CompareTag("Body")) {
                    // Add the body part to the list if not already added
                    if (body.Count == 0) {
                        body.Add(renderer);
                        BaseMaterial = renderer.material;
                        continue;
                    }

                    if (!body.Contains(renderer)) {
                        body.Add(renderer);
                        renderer.material = BaseMaterial;
                    }
                }
            }

        }

        void Update() {
            if (influenceBar != null) {
                influenceBar.transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
            }

            // Check the velocity of the NavMeshAgent to determine if the enemy is moving
            if (_navAgent != null && _animator != null) {
                if (_navAgent.velocity.magnitude > 0.1f) {
                    _animator.SetFloat("Speed", 1);
                }
                else {
                    _animator.SetFloat("Speed", 0);
                }
            }
        }

        public void PlayerHit(Player.Player player) {
            //Debug.Log("Attack controller " + _attackController);
            _attackController.EnemyAttackHit(this.gameObject, player.gameObject);
        }

        public void SendLightAttack() {
            if (!isAttackActive && !LightCooldown) {
                _attackController.EnemyLightAttack();
            }
        }

        public void SendHeavyAttack() {
            if (!isAttackActive && !HeavyCooldown) {
                _attackController.EnemyHeavyAttack();
            }
        }

        public void SendSpecialAttack() {
            if (!isAttackActive && !SpecialCooldown) {
                HasSpecial = false;
                _attackController.EnemySpecialAttack();
            }
        }


        public void CreateEnemyLightAttack() {
            _attackController.EnemyCreateLightAttack();
        }

        public void CreateEnemyHeavyAttack() {
            _attackController.EnemyCreateHeavyAttack();
        }

        public void CreateEnemySpecialAttack() {
            _attackController.EnemyCreateSpecialAttack();
        }

        public void CleanStates() {
            _attackController.EnemyCleanStates();
        }
        public void TakeDamage(float damage) {
            _influence -= damage;
            influenceBar.value = _influence;
            if (_influence <= 0) {
                //_influence = 100;
                SetClaimable();
            }
        }

        public void SetClaimable() {

            var animator = GetComponentInChildren<Animator>();
            animator.SetInteger("State", 0);

            _isDefeated = true;
            _fillImage.color = _defeatedColor;

            // Set the layer of the enemy to the NPC layer
            gameObject.layer = 3;
            specialAttackColliderGameObject.tag = "Heal";

            // Set inactive the Behaviour executor script
            BehaviorExecutor behaviorExecutor = gameObject.GetComponent<BehaviorExecutor>();
            behaviorExecutor.enabled = false;
            gameObject.GetComponent<NavMeshAgent>().enabled = false;
            // Add a timer to claim the enemy
            claimableCoroutine = StartCoroutine(Claimable());

            // Start checking for the Player component
            checkPlayerCoroutine = StartCoroutine(CheckForPlayerComponent());
        }


        private IEnumerator Claimable() {
            ChangeScreenMaterial();
            // Start filling the influence bar during the takeover time
            float time = 0;
            while (time < TakeoverTime) {
                time += Time.deltaTime;
                influenceBar.value = Mathf.Lerp(0, influenceBar.maxValue, time / TakeoverTime);
                yield return null;
            }

            // Last check to see if the player claimed the enemy
            if (GetComponent<Player.Player>() == null) {
                //spawn bonus battery
                var animator = GetComponentInChildren<Animator>();
                animator.SetInteger("State", 6);
                yield return new WaitForSeconds(2.5f);
                var placeBatteryEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("placeBattery", ScriptableObject.CreateInstance<EventWithVector3>());
                placeBatteryEvent.UpdateValue(this.transform.position);

                // The player did not claim the enemy
                gameObject.SetActive(false);

            }
            else {
                // The player claimed the enemy
                DestroyThis();
            }
        }
        private IEnumerator CheckForPlayerComponent() {
            while (true) {
                if (GetComponent<Player.Player>() != null) {
                    // Player component detected, stop the claimable coroutine
                    if (claimableCoroutine != null) {
                        StopCoroutine(claimableCoroutine);
                    }
                    DestroyThis();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
            }
        }

        private void ChangeScreenMaterial() {
            if (screen == null || nextScreenMaterial == null) return;

            Renderer screenRenderer = screen.GetComponent<Renderer>();
            Material[] materials = screenRenderer.materials;

            // Create a new materials array with space for one new material
            Material[] newMaterials = new Material[materials.Length];
            for (int i = 0; i < materials.Length - 1; i++) {
                newMaterials[i] = materials[i];
            }

            // Add the nextScreenMaterial as the last material
            newMaterials[^1] = nextScreenMaterial;

            // Assign the new materials array to the Renderer
            screenRenderer.materials = newMaterials;
        }

        private void ResetScreenMaterial() {
            if (screen == null || defaultScreenMaterial == null) return;
            Renderer screenRenderer = screen.GetComponent<Renderer>();
            Material[] materials = screenRenderer.materials;

            // Create a new materials array with space for one new material
            Material[] newMaterials = new Material[materials.Length];
            for (int i = 0; i < materials.Length - 1; i++) {
                newMaterials[i] = materials[i];
            }

            // Add the nextScreenMaterial as the last material
            newMaterials[^1] = defaultScreenMaterial;

            // Assign the new materials array to the Renderer
            screenRenderer.materials = newMaterials;
        }

        private void OnEnable() {
            if (_influence <= 0) {
                ResetEnemy();
            }
            StartEnemy();
        }

        public void ResetEnemy() {
            var behaviorExecutor = gameObject.GetComponent("BehaviorExecutor") as MonoBehaviour;
            behaviorExecutor!.enabled = true;
            var agent = gameObject.GetComponent<NavMeshAgent>();
            agent.gameObject.SetActive(true);
            influenceBar.gameObject.SetActive(true);
            
            _influence = _maxInfluence;
            _navAgent.enabled = true;
            _isDefeated = false;
            influenceBar.value = _influence;
            _fillImage.color = _influenceColor;
            gameObject.layer = 8;
            if (claimableCoroutine != null) {
                StopCoroutine(claimableCoroutine);
            }
            if (checkPlayerCoroutine != null) {
                StopCoroutine(checkPlayerCoroutine);
            }
            ResetScreenMaterial();
        }
        private void DestroyThis() {
            influenceBar.gameObject.SetActive(false);
            StartCoroutine(DestroyObject());
        }

        private IEnumerator DestroyObject() {
            yield return new WaitForSeconds(2.5f);
            RemovePoolObject("SpawnerNPC" + type, gameObject);
            Destroy(this);
        }


        public float GetInfluence() {
            // Normalize the influence value between 0 and 1 based on the max influence
            return _influence / _maxInfluence;
        }

        public void ChangeColor(Color color) {
            // Change the Emission color of the enemy 
            BaseMaterial.SetColor("_EmissionColor", color);
        }

        // Coroutine to change the color of the enemy gradually
        public IEnumerator ChangeColorGradually(Color color, float duration) {
            float time = 0;
            Color startColor = BaseMaterial.GetColor("_EmissionColor");
            while (time < duration) {
                time += Time.deltaTime;
                BaseMaterial.SetColor("_EmissionColor", Color.Lerp(startColor, color, time / duration));
                yield return null;
            }
        }


        public void TriggerHeal() {
            _attackController.EnemySpecialAttack();
        }

        public Coroutine HealCoroutine;
        public void OnTriggerEnter(Collider other) {
            if (other.CompareTag("HealEnemy")) {
                HealCoroutine = StartCoroutine(HealOverTime(5));
            }
        }

        public void OnTriggerExit(Collider other) {
            if (other.CompareTag("HealEnemy")) {
                if (HealCoroutine != null) {
                    StopCoroutine(HealCoroutine);
                }
            }
        }

        private IEnumerator HealOverTime(float time) {
            // Start healing the enemy with a rate calculated by the time and half of the max influence
            float healRate = _maxInfluence / 2 / time;
            while (_influence < _maxInfluence / 2) {
                _influence += healRate * Time.deltaTime;
                influenceBar.value = _influence;
                yield return null;
            }

            // Stop the healing coroutine
            HealCoroutine = null;
        }

    }
}