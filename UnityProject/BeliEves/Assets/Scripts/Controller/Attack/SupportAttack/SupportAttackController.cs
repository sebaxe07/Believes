using UnityEngine;
using System;
using ScriptableObjects.Attack;
using Utilities.Events.EventsLayout;
using Events;
using System.Collections;
using UnityEngine.AI;
using Ami.BroAudio;
using Events.EventsLayout;
using Utilities;

namespace Controller.Attack {
    public class SupportAttackController : AttackController {


        private SupportAttack _supportAttack = Resources.Load<SupportAttack>("Settings/AttackSettings/SupportAttack");
        private EventHeal _healEvent;
        private EventUseStamina _useStaminaEvent;
        private ParticleSystem _healAnimation;
        private ParticleSystem _shockwaveAnimation;
        private ParticleSystem _punchAnimation;
        private EventTakeDamage _takeDamage;
        private bool _isPlayer = false;
        private bool _isHealing = false;

        private float lightAttackDamage;
        private float heavyAttackDamage;
        private float heavyAttackStun;

        private Color LightPrepareColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
        private Color HeavyPrepareColor = new Color(50f / 255f, 0, 0);
        private Coroutine PreparationCoroutine;
        private Coroutine CooldownCoroutine;
        private Coroutine LightDurationCoroutine;
        private Coroutine HeavyDurationCoroutine;
        private Coroutine SpecialDurationCoroutine;
        private BasicEventChannel _jumpEventChannel;

        private BasicEventChannel _lighAttackAnimationEventChannel;
        private BasicEventChannel _stopHealingEvent;
        private BasicEventChannel _heavyAttackAnimationEventChannel;
        private BasicEventChannel _specialAttackAnimationEventChannel;

        private GameObject _lightAttackColliderR;


        public SupportAttackController(Func<Vector3> getPlayerDirection, bool subscribeToEvents = true)
    : base(getPlayerDirection, subscribeToEvents) {
            _isPlayer = subscribeToEvents;

            if (!_isPlayer) {
                _takeDamage = (EventTakeDamage)EventBroker.TryToAddEventChannel("takeDamageEvent", ScriptableObject.CreateInstance<EventTakeDamage>());
                lightAttackDamage = _supportAttack.lightAttackDamageEnemy;
                heavyAttackStun = _supportAttack.heavyAttackStunEnemy;
                heavyAttackDamage = _supportAttack.heavyAttackDamageEnemy;
            }
            else {
                lightAttackDamage = _supportAttack.lightAttackDamage;
                heavyAttackDamage = _supportAttack.heavyAttackDamage;
                heavyAttackStun = _supportAttack.heavyAttackStunDuration;
                _jumpEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("jumpEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
                _jumpEventChannel.Subscribe(() => JumpSound());

                _stopHealingEvent = EventBroker.TryToAddEventChannel("stopHealingEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
                _stopHealingEvent.Subscribe(LongPressHealingStop);

                _parryEvent.Subscribe(new Action(() => {
                    if (_parryEvent.eventBool && _isHealing) StopHealingAnimation();
                }));

            }
            _healEvent = (EventHeal)EventBroker.TryToAddEventChannel("healEvent", ScriptableObject.CreateInstance<EventHeal>());
            _useStaminaEvent = (EventUseStamina)EventBroker.TryToAddEventChannel("useStaminaEvent", ScriptableObject.CreateInstance<EventUseStamina>());

            _lighAttackAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("lightAttackAnimationEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            _heavyAttackAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("heavyAttackAnimationEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            _specialAttackAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("specialAttackAnimationEvent", ScriptableObject.CreateInstance<BasicEventChannel>());

            if (Player != null) {
                SearchPunchAnimation();
            }
        }

        private void SearchPunchAnimation() {
            ParticleSystem[] particleSystems = Player.GetComponentsInChildren<ParticleSystem>();
            // find the punch animation
            foreach (ParticleSystem particleSystem in particleSystems) {
                if (particleSystem.name == "PunchParticle") {
                    _punchAnimation = particleSystem;
                }
            }
        }
        private void SearchLightAttackColliders() {

            BoxCollider[] colliders = Player.GetComponentsInChildren<BoxCollider>(true);
            foreach (BoxCollider collider in colliders) {
                if (collider.name == "LightAttackColliderR") {
                    _lightAttackColliderR = collider.gameObject;
                }
            }
        }
        protected override void LightAttackTrigger() {
            // Basic punch
            if (Player != null && _lightAttackColliderR == null) {
                SearchLightAttackColliders();
            }

            CooldownCoroutine = null;
            LightDurationCoroutine = null;

            // Check if the player is an enemy and if so, ignore the use of stamina
            if (Player.TryGetComponent<Npc.Enemy>(out var enemy)) {
                if (PreparationCoroutine != null) {
                    return;
                }
                PreparationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(LightAttackPreparation());
                return;
            }

            if (_parryEvent.eventBool) return;//avoid simultaneous attack and parry
            if (_isHealing) {
                StopHealingAnimation();
            }
            _useStaminaEvent.UpdateValue(Player, _supportAttack.lightAttackStaminaCost, (Exception ex) => {
                if (ex == null) {
                    AttackEnum = AttackEnum.LightAttack;
                    //                    Debug.LogError("Light Attack");
                    int groundLayerMask = LayerMask.GetMask("Ground");
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, groundLayerMask)) {
                        _moveTowardsEventChannel.UpdateValue(Player.transform.position, 0f, false, null, hit.point - Player.transform.position, 1f);
                    }
                    _lighAttackAnimationEventChannel.RaiseEvent();
                    playerScript.isAttackActive = true;
                    playerScript.LightCooldown = true;
                }
            });
        }

        protected override void CreateLightAttack() {

            if (!_isPlayer) {
                _lightAttackColliderR.SetActive(true);

                CooldownCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackCoolDownCoroutine(_supportAttack.lightAttackCooldownEnemy, false, 0));
                LightDurationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackDurationCoroutine(_lightAttackColliderR, _supportAttack.lightAttackDuration, false));
                animator = Player.GetComponentInChildren<Animator>();
                animator.SetInteger("State", 0);
                return;
            }

            _lightAttackColliderR.SetActive(true);


            CooldownCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackCoolDownCoroutine(_supportAttack.lightAttackCooldown, true, 0));
            LightDurationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackDurationCoroutine(_lightAttackColliderR, _supportAttack.lightAttackDuration, true));
        }

        protected override void CreateHeavyAttack() {
            ShockwaveAnimation();
            if (!_isPlayer) {
                enemyScript.heavyAttackColliderGameObject.SetActive(true);

                CooldownCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackCoolDownCoroutine(_supportAttack.heavyAttackCooldownEnemy, false, 1));
                HeavyDurationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackDurationCoroutine(enemyScript.heavyAttackColliderGameObject, _supportAttack.heavyAttackDuration, false));
                animator = Player.GetComponentInChildren<Animator>();
                animator.SetInteger("State", 0);
                return;
            }

            playerScript.heavyAttackColliderGameObject.SetActive(true);


            CooldownCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackCoolDownCoroutine(_supportAttack.heavyAttackCooldown, true, 1));
            HeavyDurationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackDurationCoroutine(playerScript.heavyAttackColliderGameObject, _supportAttack.heavyAttackDuration, true));
        }

        protected override void CreateSpecialAttack() {
            // Not used
        }


        protected override void HeavyAttackTrigger() {
            // Shockwave
            CooldownCoroutine = null;
            HeavyDurationCoroutine = null;

            // Check if the player is an enemy and if so, ignore the use of stamina
            if (Player.TryGetComponent<Npc.Enemy>(out var enemy)) {
                if (PreparationCoroutine != null) {
                    return;
                }
                PreparationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(HeavyAttackPreparation());
                return;
            }

            if (_parryEvent.eventBool) return;//avoid simultaneous attack and parry
            if (_isHealing) {
                StopHealingAnimation();
            }

            _useStaminaEvent.UpdateValue(Player, _supportAttack.heavyAttackStaminaCost, (Exception ex) => {
                if (ex == null) {
                    AttackEnum = AttackEnum.HeavyAttack;
                    int groundLayerMask = LayerMask.GetMask("Ground");
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, groundLayerMask)) {
                        _moveTowardsEventChannel.UpdateValue(Player.transform.position, 0f, false, null, hit.point - Player.transform.position, 1f);
                    }
                    // BroAudio.play(_supportAttack.heavyAttackSound, playerScript.transform.position);
                    _heavyAttackAnimationEventChannel.RaiseEvent();
                    playerScript.isAttackActive = true;
                    playerScript.HeavyCooldown = true;

                }
            });
        }

        private float _NavSpeed;
        protected override void SpecialAttackTrigger() {

            if (_isHealing) {
                _isHealing = false;
                StopHealingAnimation();
                return;
            }

            if (!_isPlayer) {
                // Find the NavMeshAgent and stop it from moving
                if (enemyScript.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var navAgent)) {
                    _NavSpeed = navAgent.speed;
                    navAgent.speed = 0;
                    navAgent.updatePosition = false;
                }

                enemyScript.SpecialCooldown = true;
                AttackEnum = AttackEnum.SpecialAttack;
                _healAnimation = Player.transform.Find("HealingAura").GetComponent<ParticleSystem>();
                _isHealing = true;
                enemyScript.specialAttackColliderGameObject.SetActive(true);
                BroAudio.Play(_supportAttack.specialAttackSound);
                _healAnimation.Play();
                // Start a coroutine to stop the healing after a certain amount of time
                CoroutineRunner.Instance.RunCoroutine(HealEnemyTimer(5));

                return;
            }

            if (_parryEvent.eventBool) return;//avoid simultaneous attack and parry

            // Heal
            AttackEnum = AttackEnum.SpecialAttack;
            _healAnimation = Player.transform.Find("HealingAura").GetComponent<ParticleSystem>();

            _useStaminaEvent.UpdateValue(Player, _supportAttack.specialAttackStaminaCost / 2, (Exception ex) => {
                if (ex != null) {
                    StopHealingAnimation();
                    return;
                }

                _isHealing = true;
                playerScript.specialAttackColliderGameObject.SetActive(true);
                BroAudio.Play(_supportAttack.specialAttackSound);
                _healAnimation.Play();
                _healEvent.UpdateValue(Player, _supportAttack.specialAttackHealRate, _supportAttack.specialAttackRate);
                _useStaminaEvent.UpdateValue(Player, _supportAttack.specialAttackStaminaCost / 2, (Exception ex) => {
                    if (ex != null) {
                        StopHealingAnimation();
                        return;
                    }
                }, true, _supportAttack.specialAttackRate);
            });
        }

        private void LongPressHealingStop() {
            if (_isHealing) {
                StopHealingAnimation();
            }
        }

        private void StopHealingAnimation() {

            _healAnimation = Player.transform.Find("HealingAura").GetComponent<ParticleSystem>();

            if (!_isPlayer) {
                if (enemyScript.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var navAgent)) {
                    navAgent.speed = _NavSpeed;
                    navAgent.nextPosition = enemyScript.transform.position;
                    navAgent.updatePosition = true;
                }
                enemyScript.SpecialCooldown = false;
                enemyScript.specialAttackColliderGameObject.SetActive(false);
                BroAudio.Stop(_supportAttack.specialAttackSound, 2f);
                _healAnimation.Stop();
                _isHealing = false;
                return;
            }


            playerScript.specialAttackColliderGameObject.SetActive(false);
            playerScript.SpecialCooldown = true;
            CoroutineRunner.Instance.RunCoroutine(_attackCoolDownCoroutine(_supportAttack.specialAttackCooldown, true, 2));
            BroAudio.Stop(_supportAttack.specialAttackSound, 2f);
            _healAnimation.Stop();
            _isHealing = false;

            _useStaminaEvent.UpdateValue(Player, _supportAttack.specialAttackStaminaCost / 2, (Exception ex) => { }, true, _supportAttack.specialAttackRate);
        }

        private void PunchAnimation(bool isPlayer) {
            // Get the particle system for the punch animation
            if (_punchAnimation == null && Player != null) {
                SearchPunchAnimation();
            }
            else if (Player == null) {
                return;
            }

            if (isPlayer) {
                BroAudio.Play(_supportAttack.lightAttackSound, playerScript.transform);
            }
            else {
                BroAudio.Play(_supportAttack.lightAttackSound, enemyScript.transform);
            }
            if (_punchAnimation == null) return;
            _punchAnimation.Play();
        }

        private void ShockwaveAnimation() {
            // Get the particle system for the shockwave animation
            _shockwaveAnimation = Player.transform.Find("ShockwaveParticle").GetComponent<ParticleSystem>();
            _shockwaveAnimation.Play();
        }

        protected override void AttackHit(GameObject attacker, GameObject hitObj) {
            if (AttackEnum == AttackEnum.LightAttack) {
                if (hitObj.TryGetComponent<Npc.Enemy>(out var enemy)) {
                    PunchAnimation(true);
                    enemy.TakeDamage(lightAttackDamage);
                }
                else if (hitObj.TryGetComponent<Npc.NPCEnemy>(out var npcEnemy)) {
                    PunchAnimation(true);
                    npcEnemy.TakeDamage(lightAttackDamage);
                }
                else if (hitObj.TryGetComponent<Player.Player>(out var player)) {
                    PunchAnimation(false);
                    _takeDamage.UpdateValue(player.gameObject, lightAttackDamage);
                }

            }
            else if (AttackEnum == AttackEnum.HeavyAttack) {
                // Create a shockwave

                // Stun the enemy   
                if (hitObj.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var navAgent)) {
                    // stop the navAgent from moving for the heavyAttackStun time and then resume
                    var Speed = navAgent.speed;
                    navAgent.speed = 0;
                    CoroutineRunner.Instance.RunCoroutine(ResumeNavAgent(navAgent, heavyAttackStun, Speed));
                    if (navAgent.gameObject.TryGetComponent<Npc.Enemy>(out var enemy)) {
                        enemy.TakeDamage(heavyAttackDamage);
                    }
                    else {
                        if (navAgent.gameObject.TryGetComponent<Npc.NPCEnemy>(out var npcEnemy)) {
                            npcEnemy.TakeDamage(heavyAttackDamage);
                        }
                    }
                    return;
                }

                // stun the player
                if (hitObj.TryGetComponent<Player.Player>(out var player)) {
                    if (!_parryEvent.eventBool) player.StunPlayer(heavyAttackStun); //if player isn't parry lock him\
                    _takeDamage.UpdateValue(player.gameObject, heavyAttackDamage);
                    return;
                }


            }
        }

        private IEnumerator ResumeNavAgent(UnityEngine.AI.NavMeshAgent agent, float delay, float speed) {
            yield return new WaitForSeconds(delay);
            agent.speed = speed;
        }


        private void JumpSound() {
            if (playerScript == null) return;
            // BroAudio.play(_supportAttack.jumpSound);
        }


        private IEnumerator LightAttackPreparation() {
            yield return enemyScript.ChangeColorGradually(LightPrepareColor, 0.5f);
            AttackEnum = AttackEnum.LightAttack;
            // PunchAnimation();
            // BroAudio.play(_supportAttack.lightAttackSound, enemyScript.transform);
            animator = Player.GetComponentInChildren<Animator>();
            animator.SetInteger("State", 2);
            enemyScript.isAttackActive = true;
            enemyScript.LightCooldown = true;


            enemyScript.ChangeColor(Color.black);
            PreparationCoroutine = null;
        }

        private IEnumerator HeavyAttackPreparation() {
            yield return enemyScript.ChangeColorGradually(HeavyPrepareColor, 1f);

            AttackEnum = AttackEnum.HeavyAttack;
            // BroAudio.play(_supportAttack.heavyAttackSound, enemyScript.transform.position);
            animator = Player.GetComponentInChildren<Animator>();
            animator.SetInteger("State", 3);
            enemyScript.isAttackActive = true;
            enemyScript.HeavyCooldown = true;

            enemyScript.ChangeColor(Color.black);
            PreparationCoroutine = null;
        }


        protected override void CleanStates() {
            if (!_isPlayer) {
                if (CooldownCoroutine == null) {
                    enemyScript.isAttackActive = false;
                    if (LightDurationCoroutine == null) {
                        enemyScript.LightCooldown = false;
                    }
                    if (HeavyDurationCoroutine == null) {
                        enemyScript.HeavyCooldown = false;
                    }
                }
                return;
            }

            if (CooldownCoroutine == null) {
                playerScript.isAttackActive = false;
                if (LightDurationCoroutine == null) {
                    playerScript.LightCooldown = false;
                }
                if (HeavyDurationCoroutine == null) {
                    playerScript.HeavyCooldown = false;
                }
            }
        }
        private IEnumerator HealEnemyTimer(float time) {
            yield return new WaitForSeconds(time);
            StopHealingAnimation();
        }
    }
}