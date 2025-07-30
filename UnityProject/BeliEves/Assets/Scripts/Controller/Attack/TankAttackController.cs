using UnityEngine;
using System;
using System.Threading;
using Events;
using Events.EventsLayout;
using ScriptableObjects.Attack;
using Utilities.Events.EventsLayout;
using System.Collections;
using ScriptableObjects.Movement;
using Pada1.BBCore.Actions;
using Ami.BroAudio;
using Utilities;

namespace Controller.Attack {
    public class TankAttackController : AttackController {

        //private TankAttackSettings _tankAttack = Resources.Load<TankAttackSettings>("Settings/AttackSettings/TankAttackSettings");
        private Rigidbody _rigidbody;
        private TankAttack _tankAttack = Resources.Load<TankAttack>("Settings/AttackSettings/TankAttack");
        private BasicEventChannel _lighAttackAnimationEventChannel;
        private BasicEventChannel _heavyAttackAnimationEventChannel;
        private BasicEventChannel _specialAttackAnimationEventChannel;
        private EventUseStamina _useStaminaEvent;
        private ParticleSystem _shockwaveAnimation;
        private ParticleSystem _punchLeftAnimation;
        private ParticleSystem _punchRightAnimation;
        private ParticleSystem _dashAnimation;
        private EventTakeDamage _takeDamage;
        private EventWithVector3 _addMovementEvent;
        private bool _isPlayer;
        private BasicEventChannel _jumpEventChannel;
        private float _lightAttackDamage;
        private float _heavyAttackForce;
        private float _heaveAttackDamage;

        private Color LightPrepareColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
        private Color HeavyPrepareColor = new Color(50f / 255f, 0, 0);
        private Coroutine PreparationCoroutine;
        private Coroutine CooldownCoroutine;
        private Coroutine LightDurationCoroutine;
        private Coroutine HeavyDurationCoroutine;
        private Coroutine SpecialDurationCoroutine;

        [SerializeField] private float specialAttackDistance = 12f;

        private ParticleSystem _punchAnimationL;
        private ParticleSystem _punchAnimationR;
        private GameObject _lightAttackColliderL;
        private GameObject _lightAttackColliderR;

        public TankAttackController(Func<Vector3> getPlayerDirection, bool subscribeToEvents = true)
    : base(getPlayerDirection, subscribeToEvents) {
            //Debug.Log("Tank");

            _isPlayer = subscribeToEvents;

            if (!_isPlayer) {
                _takeDamage = (EventTakeDamage)EventBroker.TryToAddEventChannel("takeDamageEvent", ScriptableObject.CreateInstance<EventTakeDamage>());
                _addMovementEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("playerMovementEventChannel", ScriptableObject.CreateInstance<EventWithVector3>());
                _lightAttackDamage = _tankAttack.lightAttackDamageEnemy;
                _heavyAttackForce = _tankAttack.heavyAttackForceEnemy;
                _heaveAttackDamage = _tankAttack.heavyAttackDamageEnemy;
            }
            else {
                _lightAttackDamage = _tankAttack.lightAttackDamage;
                _heavyAttackForce = _tankAttack.heavyAttackForce;
                _heaveAttackDamage = _tankAttack.heavyAttackDamage;
                _jumpEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("jumpEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
                _jumpEventChannel.Subscribe(() => JumpSound());
            }


            _useStaminaEvent = (EventUseStamina)EventBroker.TryToAddEventChannel("useStaminaEvent", ScriptableObject.CreateInstance<EventUseStamina>());

            _lighAttackAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("lightAttackAnimationEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            _heavyAttackAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("heavyAttackAnimationEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            _specialAttackAnimationEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("specialAttackAnimationEvent", ScriptableObject.CreateInstance<BasicEventChannel>());

            if (Player != null) {
                SearchPunchAnimation();
                SearchLightAttackColliders();
            }
        }

        private void SearchPunchAnimation() {
            ParticleSystem[] particleSystems = Player.GetComponentsInChildren<ParticleSystem>();
            // find the punch animation
            foreach (ParticleSystem particleSystem in particleSystems) {
                if (particleSystem.name == "PunchParticleL") {
                    _punchAnimationL = particleSystem;
                }
                else if (particleSystem.name == "PunchParticleR") {
                    _punchAnimationR = particleSystem;
                }
            }
        }

        private void SearchLightAttackColliders() {

            BoxCollider[] colliders = Player.GetComponentsInChildren<BoxCollider>(true);
            foreach (BoxCollider collider in colliders) {
                if (collider.name == "LightAttackColliderL") {
                    _lightAttackColliderL = collider.gameObject;
                }
                else if (collider.name == "LightAttackColliderR") {
                    _lightAttackColliderR = collider.gameObject;
                }
            }
        }
        protected override void LightAttackTrigger() {

            if (Player != null && (_lightAttackColliderL == null || _lightAttackColliderR == null)) {
                SearchLightAttackColliders();
            }


            if (Player.TryGetComponent<Npc.Enemy>(out var enemy)) {
                if (PreparationCoroutine != null) {
                    return;
                }
                PreparationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(LightAttackPreparation());
                return;
            }

            if (_parryEvent.eventBool) return;//avoid simultaneous attack and parry

            //Light Attack: IT throws a punch
            _useStaminaEvent.UpdateValue(Player, _tankAttack.lightAttackStaminaCost, (Exception ex) => {
                if (ex == null) {

                    int groundLayerMask = LayerMask.GetMask("Ground");
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, groundLayerMask)) {
                        _moveTowardsEventChannel.UpdateValue(Player.transform.position, 0f, false, null, hit.point - Player.transform.position,1.2f);
                    }

                    playerScript.isAttackActive = true;
                    playerScript.LightCooldown = true;

                    CoroutineRunner.Instance.RunCoroutine(_attackCoolDownCoroutine(_tankAttack.lightAttackCooldown, true, 0));

                    CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(_lightAttackColliderL, _tankAttack.lightAttackDuration, true));
                    CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(_lightAttackColliderR, _tankAttack.lightAttackDuration, true));

                    AttackEnum = AttackEnum.LightAttack;
                    _lighAttackAnimationEventChannel.RaiseEvent();
                }
            });


        }

        protected override void HeavyAttackTrigger() {
            CooldownCoroutine = null;
            HeavyDurationCoroutine = null;

            if (Player.TryGetComponent<Npc.Enemy>(out var enemy)) {
                if (PreparationCoroutine != null) {
                    return;
                }
                PreparationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(HeavyAttackPreparation());
                return;
            }

            if (_parryEvent.eventBool) return;//avoid simultaneous attack and parry

            //Heavy Attack: It stomps the ground
            _useStaminaEvent.UpdateValue(Player, _tankAttack.heavyAttackStaminaCost, (Exception ex) => {
                if (ex == null) {
                    AttackEnum = AttackEnum.HeavyAttack;
                    _heavyAttackAnimationEventChannel.RaiseEvent();
                    playerScript.isAttackActive = true;
                    playerScript.HeavyCooldown = true;
                    int groundLayerMask = LayerMask.GetMask("Ground"); // Assuming "Ground" is the name of the layer
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, groundLayerMask)) {
                        _moveTowardsEventChannel.UpdateValue(Player.transform.position, 0f, false, null, hit.point);
                    }


                }
            });



        }

        protected override void SpecialAttackTrigger() {

            if (Player.TryGetComponent<Npc.Enemy>(out var enemy)) {
                AttackEnum = AttackEnum.SpecialAttack;
                animator = Player.GetComponentInChildren<Animator>();
                animator.SetInteger("State", 4);
                _dashAnimation = Player.transform.Find("DashParticle").GetComponent<ParticleSystem>();
                _dashAnimation.Play();
                enemyScript.specialAttackColliderGameObject.SetActive(true);
                enemyScript.isAttackActive = true;
                enemyScript.SpecialCooldown = true;
                CoroutineRunner.Instance.RunCoroutine(_attackCoolDownCoroutine(_tankAttack.specialAttackCooldownEnemy, false, 2));
                CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(enemyScript.specialAttackColliderGameObject, _tankAttack.specialAttackDuration, false));
                BroAudio.Play(_tankAttack.specialAttackSound);

                Vector3 forwardDirection = enemyScript.transform.forward; // Get the forward direction of the enemy
                Vector3 newPosition = enemyScript.transform.position + forwardDirection * specialAttackDistance; // Calculate the new position

                CoroutineRunner.Instance.StartCoroutine(PushEnemy(enemyScript, forwardDirection, specialAttackDistance));


                return;
            }

            //Block the keyboard input
            if (_parryEvent.eventBool) return;//avoid simultaneous attack and parry
            _useStaminaEvent.UpdateValue(Player, _tankAttack.specialAttackStaminaCost, (Exception ex) => {
                if (ex == null) {
                    AttackEnum = AttackEnum.SpecialAttack;
                    _dashAnimation = Player.transform.Find("DashParticle").GetComponent<ParticleSystem>();
                    _dashAnimation.Play();
                    _specialAttackAnimationEventChannel.RaiseEvent();
                    playerScript.specialAttackColliderGameObject.SetActive(true);
                    playerScript.isAttackActive = true;
                    playerScript.SpecialCooldown = true;
                    CoroutineRunner.Instance.RunCoroutine(_attackCoolDownCoroutine(_tankAttack.specialAttackCooldown, true, 2));
                    CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(playerScript.specialAttackColliderGameObject, _tankAttack.specialAttackDuration, true));
                    BroAudio.Play(_tankAttack.specialAttackSound);

                    Vector3 forwardDirection = playerScript.transform.forward; // Get the forward direction of the player
                    Vector3 newPosition = playerScript.transform.position + forwardDirection * specialAttackDistance; // Calculate the new position
                    _moveTowardsEventChannel.UpdateValue(newPosition, _tankAttack.specialAttackDuration, false, null, coolDown: _tankAttack.specialAttackCooldown);

                }
            });


        }

        protected override void AttackHit(GameObject attacker, GameObject hitObj) {
            var direction = new Vector3(hitObj.transform.position.x - attacker.transform.position.x, 0, hitObj.transform.position.z - attacker.transform.position.z);

            // Call the appropriate function based on the attack type
            switch (AttackEnum) {
                case AttackEnum.LightAttack:
                    HandleLightAttackHit(hitObj);
                    break;
                case AttackEnum.HeavyAttack:
                    HandleHeavyAttackHit(attacker, hitObj);
                    break;
                case AttackEnum.SpecialAttack:
                    HandleSpecialAttackHit(hitObj);
                    break;
                default:
                    break; // No action for unrecognized attack types
            }
        }

        private bool isLeftAttack = true;
        private int _attackCount = 0;

        protected override void CreateLightAttack() {
            _attackCount++;
            if (isLeftAttack) {
                isLeftAttack = false;
                _lightAttackColliderR.SetActive(false);
                _lightAttackColliderL.SetActive(true);
            }
            else {
                isLeftAttack = true;
                _lightAttackColliderL.SetActive(false);
                _lightAttackColliderR.SetActive(true);
            }
            if (_attackCount >= 2) {
                _attackCount = 0;
                animator = Player.GetComponentInChildren<Animator>();
                animator.SetInteger("State", 0);
            }
        }

        protected override void CreateHeavyAttack() {

            _shockwaveAnimation = Player.transform.Find("ShockwaveParticle").GetComponent<ParticleSystem>();
            _shockwaveAnimation.Play();

            if (!_isPlayer) {
                enemyScript.heavyAttackColliderGameObject.SetActive(true);

                CooldownCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackCoolDownCoroutine(_tankAttack.heavyAttackCooldownEnemy, false, 1));
                HeavyDurationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackDurationCoroutine(enemyScript.heavyAttackColliderGameObject, _tankAttack.heavyAttackDuration, false));
                animator = Player.GetComponentInChildren<Animator>();
                animator.SetInteger("State", 0);
                return;
            }

            playerScript.heavyAttackColliderGameObject.SetActive(true);

            CoroutineRunner.Instance.RunCoroutine(_attackCoolDownCoroutine(_tankAttack.heavyAttackCooldown, true, 1));
            CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(playerScript.heavyAttackColliderGameObject, _tankAttack.heavyAttackDuration, true));
        }

        protected override void CreateSpecialAttack() {
            if (!_isPlayer) {
                animator = Player.GetComponentInChildren<Animator>();
                animator.SetInteger("State", 0);
                return;
            }
        }
        // New methods to handle different attack hits
        private void HandleLightAttackHit(GameObject hitObj) {
            // Implement logic for light attack hit
            if (hitObj.TryGetComponent<Npc.Enemy>(out var enemys)) {
                PunchAnimation(true, !isLeftAttack);
                enemys.TakeDamage(_lightAttackDamage / 2);
            }
            else if (hitObj.TryGetComponent<Npc.NPCEnemy>(out var npcEnemy)) {
                PunchAnimation(true, !isLeftAttack);
                npcEnemy.TakeDamage(_lightAttackDamage / 2);
            }
            else if (hitObj.TryGetComponent<Player.Player>(out var player)) { //TODO: Ignore hit if attacking
                PunchAnimation(false, !isLeftAttack);
                _takeDamage.UpdateValue(player.gameObject, _lightAttackDamage / 2);
            }
        }

        private void PunchAnimation(bool isPlayer, bool isLeft) {
            // Get the particle system for the punch animation
            if (_punchAnimationL == null && _punchAnimationR == null && Player != null) {
                SearchPunchAnimation();
            }
            else if (Player == null) {
                return;
            }

            if (isPlayer) {
                BroAudio.Play(_tankAttack.lightAttackSound, playerScript.transform);
            }
            else {
                BroAudio.Play(_tankAttack.lightAttackSound, enemyScript.transform);
            }
            if (_punchAnimationL == null && _punchAnimationR == null) return;
            if (isLeft) {
                _punchAnimationL.Play();
            }
            else {
                _punchAnimationR.Play();
            }
        }

        private void HandleHeavyAttackHit(GameObject attacker, GameObject hitObj) {
            // Implement logic for heavy attack hit

            if (hitObj.TryGetComponent<Rigidbody>(out var rb)) {

                Vector3 forceDirection = (hitObj.transform.position - attacker.transform.position).normalized;
                // Add an upwards force to the hitObj
                forceDirection.y = _tankAttack.heavyAttackYForce;

                // Check if the enemy has a NavMeshAgent and temporarily disable Y-axis position updating
                if (hitObj.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var navAgent)) {
                    navAgent.updatePosition = false;  // Temporarily allow Rigidbody to control position
                }

                // Apply the force to the Rigidbody
                if (!_isPlayer) {
                    // Add the velocity to the player that is a KinematicCharacterMotor so send the Vector3 to the MovementController
                    _addMovementEvent.UpdateValue(forceDirection * _heavyAttackForce);

                }
                else if (!_parryEvent.eventBool) { //if player isn't parry push him away
                    // Disable the kinematic rb
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.AddForce(forceDirection * _heavyAttackForce, ForceMode.Impulse);
                }

                if (hitObj.TryGetComponent<Npc.Enemy>(out var enemys)) {
                    enemys.TakeDamage(_heaveAttackDamage);
                }
                else if (hitObj.TryGetComponent<Npc.NPCEnemy>(out var npcEnemy)) {
                    npcEnemy.TakeDamage(_heaveAttackDamage);
                }
                else if (hitObj.TryGetComponent<Player.Player>(out var player)) {
                    _takeDamage.UpdateValue(player.gameObject, _heaveAttackDamage);
                }


                // Re-enable NavMeshAgent Y-axis control after a delay
                if (hitObj.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var navAgent2)) {
                    CoroutineRunner.Instance.RunCoroutine(RestoreNavAgentPosition(navAgent2, rb, 2f));
                }
            }
        }

        private void HandleSpecialAttackHit(GameObject hitObj) {
            // Implement logic for special attack hit

            if (hitObj.TryGetComponent<Npc.Enemy>(out var enemys)) {
                enemys.TakeDamage(_tankAttack.specialAttackDamage);
            }
            else if (hitObj.TryGetComponent<Npc.NPCEnemy>(out var npcEnemy)) {
                npcEnemy.TakeDamage(_tankAttack.specialAttackDamage);
            }
            else if (hitObj.TryGetComponent<Player.Player>(out var player)) {
                _takeDamage.UpdateValue(player.gameObject, _tankAttack.specialAttackDamageEnemy);
            }
        }


        private IEnumerator RestoreNavAgentPosition(UnityEngine.AI.NavMeshAgent agent, Rigidbody rb, float delay) {
            yield return new WaitForSeconds(delay);

            // Ensure the NavMeshAgent’s position syncs with Rigidbody's final position
            rb.isKinematic = true;
            rb.useGravity = false;
            agent.nextPosition = rb.position;
            agent.updatePosition = true;  // Re-enable NavMeshAgent control over position
        }

        private void JumpSound() {
            if (playerScript == null) return;
            // BroAudio.Play(_tankAttack.jumpSound);
        }


        private IEnumerator LightAttackPreparation() {
            yield return enemyScript.ChangeColorGradually(LightPrepareColor, 0.5f);

            AttackEnum = AttackEnum.LightAttack;

            enemyScript.isAttackActive = true;
            enemyScript.LightCooldown = true;

            animator = Player.GetComponentInChildren<Animator>();
            animator.SetInteger("State", 2);


            CoroutineRunner.Instance.RunCoroutine(_attackCoolDownCoroutine(_tankAttack.lightAttackCooldownEnemy, false, 0));

            CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(_lightAttackColliderL, _tankAttack.lightAttackDuration, false));
            CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(_lightAttackColliderR, _tankAttack.lightAttackDuration, false));


            enemyScript.ChangeColor(Color.black);
            PreparationCoroutine = null;
        }

        private IEnumerator HeavyAttackPreparation() {
            yield return enemyScript.ChangeColorGradually(HeavyPrepareColor, 1f);

            AttackEnum = AttackEnum.HeavyAttack;
            enemyScript.isAttackActive = true;
            enemyScript.HeavyCooldown = true;

            animator = Player.GetComponentInChildren<Animator>();
            animator.SetInteger("State", 3);

            enemyScript.ChangeColor(Color.black);
            PreparationCoroutine = null;
        }
        private IEnumerator PushEnemy(Npc.Enemy enemy, Vector3 direction, float distance) {

            UnityEngine.AI.NavMeshAgent navAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null) {
                float pushDuration = 0.5f; // Duration of the push
                float elapsedTime = 0f;
                Vector3 pushVelocity = direction.normalized * (distance / pushDuration);

                while (elapsedTime < pushDuration) {
                    navAgent.Move(pushVelocity * Time.deltaTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            /*             UnityEngine.AI.NavMeshAgent navAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (navAgent != null) {
                            navAgent.enabled = false; // Disable the NavMeshAgent
                        }

                        Rigidbody rb = enemy.GetComponent<Rigidbody>();
                        if (rb != null) {
                            rb.isKinematic = false; // Make the Rigidbody non-kinematic
                            rb.AddForce(direction * distance, ForceMode.Impulse); // Apply force to push the enemy
                        }

                        yield return new WaitForSeconds(0.5f); // Wait for a short duration

                        if (rb != null) {
                            rb.isKinematic = true; // Make the Rigidbody kinematic again
                        }

                        if (navAgent != null) {
                            navAgent.enabled = true; // Re-enable the NavMeshAgent
                        } */
        }

        protected override void CleanStates() {
            if (!_isPlayer) {
                if (CooldownCoroutine == null) {
                    enemyScript.isAttackActive = false;

                    if (HeavyDurationCoroutine == null) {
                        enemyScript.HeavyCooldown = false;
                    }
                }
                return;
            }

            if (CooldownCoroutine == null) {
                playerScript.isAttackActive = false;

                if (HeavyDurationCoroutine == null) {
                    playerScript.HeavyCooldown = false;
                }
            }
        }
    }
}