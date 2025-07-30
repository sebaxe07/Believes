using System;
using Ami.BroAudio;
using BBUnity.Actions;
using Controller.Attack.AgileAttack.GrapplingIdentifiers;
using Controller.Attack.AgileAttack.SpecialAttack;
using Events;
using Events.EventsLayout;
using ScriptableObjects.Attack;
using UnityEngine;
using Utilities;
using Utilities.Events.EventsLayout;
using Object = UnityEngine.Object;
using System.Collections;
using Npc;
using ScriptableObjects.Movement;

namespace Controller.Attack.AgileAttack {
    public class AgileAttackController : AttackController {
        private readonly AgileAttackSettings _agileAttackSettings;
        private GrapplingGun _grapplingGun;
        private GrapplingRope _grapplingRope;
        private ParticleSystem _punchAnimationL;
        private ParticleSystem _punchAnimationR;
        
        private readonly KinematicMovementSettings _kinematicMovementSettings;
        private float _oldDrag = 0.0f;

        private bool _isGrapplingDraggable = false;
        private GameObject _lastGrapplingRopeObject = null;

        private readonly EventUseStamina _useStaminaEvent;

        private bool _isPlayer;
        private EventTakeDamage _takeDamage;
        private EventWithVector3 _addMovementEvent;

        private GameObject grapplingPrefab;
        private bool _grapplingSoundsPlayed = false;

        private BasicEventChannel _jumpEventChannel;

        private float _lightAttackDamage;
        private float _heavyAttackDamage;

        private Color LightPrepareColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
        private Color HeavyPrepareColor = new Color(50f / 255f, 0, 0);
        private Coroutine PreparationCoroutine;
        private Coroutine CooldownCoroutine;
        private Coroutine LightDurationCoroutine;
        private Coroutine HeavyDurationCoroutine;
        private Coroutine SpecialDurationCoroutine;
        private BasicEventChannel _lighAttackAnimationEventChannel;
        private BasicEventChannel _heavyAttackAnimationEventChannel;
        private BasicEventChannel _specialAttackAnimationEventChannel;

        private GameObject _lightAttackColliderL;
        private GameObject _lightAttackColliderR;
        private GameObject target;

        public AgileAttackController(Func<Vector3> getPlayerDirection, bool subscribeToEvents = true)
: base(getPlayerDirection, subscribeToEvents) {
            _agileAttackSettings = Resources.Load<AgileAttackSettings>("Settings/AttackSettings/AgileAttack");


            _isPlayer = subscribeToEvents;

            if (!_isPlayer) {
                _takeDamage = (EventTakeDamage)EventBroker.TryToAddEventChannel("takeDamageEvent", ScriptableObject.CreateInstance<EventTakeDamage>());
                _addMovementEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("playerMovementEventChannel", ScriptableObject.CreateInstance<EventWithVector3>());
                _lightAttackDamage = _agileAttackSettings.punchDamageAmountEnemy;
                _heavyAttackDamage = _agileAttackSettings.grapplingShootingDamage;
            }
            else {
                _jumpEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("jumpEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
                _jumpEventChannel.Subscribe(() => JumpSound());
                // Set player damage values
                _lightAttackDamage = _agileAttackSettings.punchDamageAmount;
                _heavyAttackDamage = _agileAttackSettings.grapplingDamageAmount;
            }


            grapplingPrefab = Resources.Load<GameObject>("Prefabs/Robots/GrapplingShoot Variant");

            _useStaminaEvent = (EventUseStamina)EventBroker.TryToAddEventChannel("useStaminaEvent", ScriptableObject.CreateInstance<EventUseStamina>());

            _kinematicMovementSettings = Resources.Load<KinematicMovementSettings>("Settings/MovementSettings/AgileKinematicMovementSettings");
            _kinematicMovementSettings.Drag = 1.5f; //avoid bugs
            _oldDrag = _kinematicMovementSettings.Drag;
            
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

        protected override void LightAttackTrigger() { //punch

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

            _useStaminaEvent.UpdateValue(Player, _agileAttackSettings.lightAttackStaminaCost, (Exception ex) => {
                if (ex != null) return;

                int groundLayerMask = LayerMask.GetMask("Ground");
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, groundLayerMask)) {
                    _moveTowardsEventChannel.UpdateValue(Player.transform.position, 0f, false, null, hit.point - Player.transform.position,1.8f);
                }

                playerScript.isAttackActive = true;
                playerScript.LightCooldown = true;
                CoroutineRunner.Instance.RunCoroutine(_attackCoolDownCoroutine(_agileAttackSettings.lightAttackCooldown, true, 0));

                CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(_lightAttackColliderL, _agileAttackSettings.lightAttackDuration, true));
                CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(_lightAttackColliderR, _agileAttackSettings.lightAttackDuration, true));

                AttackEnum = AttackEnum.LightAttack;
                _lighAttackAnimationEventChannel.RaiseEvent();

            });
        }

        protected override void HeavyAttackTrigger() {
            if (Player.TryGetComponent<Npc.Enemy>(out var enemy)) {
                if (PreparationCoroutine != null) {
                    return;
                }
                PreparationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(HeavyAttackPreparation());
                return;
            }

            if (_parryEvent.eventBool) return;//avoid simultaneous attack and parry
            if (_grapplingGun == null) {
                _grapplingGun = Player.GetComponent<GrapplingGun>();
                _grapplingRope = Player.GetComponent<GrapplingRope>();
            }
            
            target = _grapplingGun.GetTarget(Player);
            if (target == null) return;
            _useStaminaEvent.UpdateValue(Player, _agileAttackSettings.heavyAttackStaminaCost, (Exception ex) => {
                if (ex != null) return;

                AttackEnum = AttackEnum.HeavyAttack;
                _heavyAttackAnimationEventChannel.RaiseEvent();

                int groundLayerMask = LayerMask.GetMask("Ground");
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, groundLayerMask)) {
                    _moveTowardsEventChannel.UpdateValue(Player.transform.position, 0f, false, null, hit.point - Player.transform.position);
                }


            });
        }


        protected override void SpecialAttackTrigger() {
            CooldownCoroutine = null;
            SpecialDurationCoroutine = null;

            if (_parryEvent.eventBool) return;//avoid simultaneous attack and parry

            _useStaminaEvent.UpdateValue(Player, _agileAttackSettings.specialAttackStaminaCost, (Exception ex) => {
                if (ex != null) return;

                AttackEnum = AttackEnum.SpecialAttack;
                playerScript.isAttackActive = true;
                playerScript.SpecialCooldown = true;
                _specialAttackAnimationEventChannel.RaiseEvent();

            });
        }

        protected override void AttackHit(GameObject attacker, GameObject hitObj) {
            switch (AttackEnum) {
                case AttackEnum.LightAttack:
                    _punchHit(hitObj);
                    break;
                case AttackEnum.HeavyAttack:
                    _grapplingHit(hitObj);
                    break;
                case AttackEnum.SpecialAttack:
                    _specialHit(attacker, hitObj);
                    break;
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

            if (_attackCount >= 4) {
                _attackCount = 0;
                animator = Player.GetComponentInChildren<Animator>();
                animator.SetInteger("State", 0);
            }

        }

        protected override void CreateHeavyAttack() {
            if (!_isPlayer) {
                enemyScript.heavyAttackColliderGameObject.SetActive(true);

                CoroutineRunner.Instance.RunCoroutine(_attackCoolDownCoroutine(_agileAttackSettings.heavyAttackCooldownEnemy, false, 1));
                CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(enemyScript.heavyAttackColliderGameObject, _agileAttackSettings.grapplingShootingCooldown, false));
                if (grapplingPrefab != null) {
                    GameObject projectile = _projectileSpawner.SpawnProjectile(grapplingPrefab, enemyScript.heavyAttackColliderGameObject.transform.position, Player.transform.rotation * Quaternion.Euler(0, 0, 0));
                    projectile.GetComponent<Bullet>().SetDamage(_heavyAttackDamage);
                    // Get the Rigidbody of the projectile and apply a forward force
                    Rigidbody rb = projectile.GetComponent<Rigidbody>();
                    if (rb != null) {
                        rb.velocity = Player.transform.forward * _agileAttackSettings.grapplingShootingSpeed;
                    }
                }
                else {
                    Debug.LogError("grapplingPrefab not found in Resources folder.");
                }
                animator = Player.GetComponentInChildren<Animator>();
                animator.SetInteger("State", 0);
                return;
            }
            
            if (!_grapplingGun.IsGrappling()) {
                _grapplingStart(target);
                playerScript.HeavyCooldown = true;
                CoroutineRunner.Instance.RunCoroutine(_attackCoolDownCoroutine(_agileAttackSettings.heavyAttackCooldown, true, 1));
            }
            else if (_isGrapplingDraggable) _grapplingSecondAction(target);

            target = null;
        }

        protected override void CreateSpecialAttack() {
            playerScript.specialAttackColliderGameObject.SetActive(true);


            CooldownCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackCoolDownCoroutine(_agileAttackSettings.specialAttackCooldown, true, 2));
            SpecialDurationCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(_attackDurationCoroutine(playerScript.specialAttackColliderGameObject, _agileAttackSettings.specialAttackDuration, true));

            var child = Player.transform.Find("SpinningGrapplingAttackPoint");

            GameObject grapplingPrefab = Resources.Load<GameObject>("Prefabs/Robots/Grappling");
            GameObject grappling = Object.Instantiate(grapplingPrefab);
            grappling.transform.position = child.transform.position + (Player.transform.forward * _agileAttackSettings.grapplingSpinRadius);
            grappling.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            grappling.transform.localRotation = Player.transform.rotation;
            grappling.transform.SetParent(child);

            RotatingGrappling rg = grappling.AddComponent<RotatingGrappling>();
            BroAudio.Play(_agileAttackSettings.specialAttackSound);
            rg.StartSpin(_agileAttackSettings.grapplingSpinSpeed, _agileAttackSettings.grapplingSpinRotationTime, _agileAttackSettings.specialAttackSound);
        }

        private void _grapplingStart(GameObject target) {
            Action action = null;
            GameObject draggablePlatform = null;

            BroAudio.Play(_agileAttackSettings.grapplingReelSound);
            _grapplingSoundsPlayed = false;

            if (target.TryGetComponent<Draggable>(out Draggable draggable)) {
                _isGrapplingDraggable = true;
                _lastGrapplingRopeObject = target;
                action = new Action(() => {
                    draggable.Dragged(
                    Player.transform.position,
                    _agileAttackSettings.draggableSafeOffset,
                    _agileAttackSettings.draggableSpeed,
                    _agileAttackSettings.draggableMaxDistance,
                    new Action(() => _grapplingGun.StopGrapple())
                );
                    BroAudio.Stop(_agileAttackSettings.grapplingReelSound);
                });
            }
            else if (target.TryGetComponent<GrapplingAnchorPoint>(out GrapplingAnchorPoint grapplingAnchorPoint)) {
                _kinematicMovementSettings.Drag = 0.1f;
                draggablePlatform = IsOnDraggablePlatform(Player.GetComponent<Collider>());
                if (draggablePlatform != null && !grapplingAnchorPoint.Grappling()) JointObjects(Player, draggablePlatform);

                action = new Action(() => {
                    grapplingAnchorPoint.DoAnchorPoint(
                    _agileAttackSettings.anchorTime,
                    _agileAttackSettings.anchorMaxDistance,
                    new Action(() => {
                        _grapplingGun.StopGrapple();
                        _kinematicMovementSettings.Drag = _oldDrag;
                    }),
                    Player.GetComponent<Rigidbody>(),
                    draggablePlatform
                    );
                    if (!_grapplingSoundsPlayed) {
                        BroAudio.Stop(_agileAttackSettings.grapplingReelSound);
                        _grapplingSoundsPlayed = true;
                    }
                });
            } else if (target.TryGetComponent<RotateGrapplingObject>(out RotateGrapplingObject rotateGrapplingObject)) {
                action = new Action(() => {
                    rotateGrapplingObject.Rotate(
                        new Action(() => {
                            _grapplingGun.StopGrapple();
                            _kinematicMovementSettings.Drag = _oldDrag;
                        }));
                        BroAudio.Stop(_agileAttackSettings.grapplingReelSound);
                });
            } else if (target.TryGetComponent<NPCEnemy>(out var npcEnemy)) {
                action = new Action(() => {
                    npcEnemy.TakeDamage(_agileAttackSettings.grapplingDamageAmount);
                    _grapplingGun.StopGrapple();
                    _kinematicMovementSettings.Drag = _oldDrag;
                    BroAudio.Stop(_agileAttackSettings.grapplingReelSound);
                });
            }
            
            _grapplingRope.EnableGrapplingGun(target, action, _agileAttackSettings.grapplingHitSound);
            if (!_grapplingGun.UseGrapplingGun(Player)) {
                if (draggablePlatform != null) draggablePlatform.transform.parent = null;
                BroAudio.Stop(_agileAttackSettings.grapplingReelSound);
            }
        }

        private void JointObjects(GameObject objectA, GameObject objectB) {
            objectB.transform.parent = objectA.transform;
        }

        private GameObject IsOnDraggablePlatform(Collider targetCollider) {
            float rayLength = 2f;

            // Determine the bottom center of the target collider
            Vector3 rayOrigin = targetCollider.bounds.center - new Vector3(0, targetCollider.bounds.extents.y, 0);

            // Cast a ray downward to check for a draggable platform
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayLength, _agileAttackSettings.whatIsGrappable)) {
                if (hit.collider.TryGetComponent<Draggable>(out Draggable draggable)) {
                    return draggable.gameObject;
                }
            }

            return null;
        }

        private void _grapplingSecondAction(GameObject target) {
            if (!_lastGrapplingRopeObject.TryGetComponent<Draggable>(out Draggable draggable) ||
                !target.TryGetComponent<GrapplingAnchorPoint>(out GrapplingAnchorPoint anchorPoint)) return;
            draggable.RestartDragging(
                target.transform.position,
                _agileAttackSettings.draggableSafeOffset,
                _agileAttackSettings.draggableSpeed,
                _agileAttackSettings.draggableMaxDistance,
                new Action(() => _grapplingGun.StopGrapple())
            );
            _grapplingGun.UseGrapplingGun(target);
            _lastGrapplingRopeObject = null;
        }

        private void _punchHit(GameObject hitObj) {
            if (hitObj.TryGetComponent<Npc.Enemy>(out var enemys)) {
                PunchAnimation(true, !isLeftAttack);
                enemys.TakeDamage(_lightAttackDamage / 4);
            }
            else if (hitObj.TryGetComponent<Npc.NPCEnemy>(out var npcEnemy)) {
                PunchAnimation(true, !isLeftAttack);
                npcEnemy.TakeDamage(_lightAttackDamage / 4);
            }
            else if (hitObj.TryGetComponent<Player.Player>(out var player)) {
                PunchAnimation(false, !isLeftAttack);
                _takeDamage.UpdateValue(player.gameObject, _lightAttackDamage / 4);
            }
        }

        private void _grapplingHit(GameObject hitObj) {
            if (hitObj.TryGetComponent<Npc.Enemy>(out var enemy)) {
                enemy.TakeDamage(_heavyAttackDamage);
            }
        }

        private void _specialHit(GameObject player, GameObject hitObj) { 
            if (hitObj.TryGetComponent<Npc.Enemy>(out var enemy)) {
                enemy.TakeDamage(_agileAttackSettings.grapplingSpinDamageAmount);
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
                BroAudio.Play(_agileAttackSettings.lightAttackSound, playerScript.transform);
            }
            else {
                BroAudio.Play(_agileAttackSettings.lightAttackSound, enemyScript.transform);
            }
            if (_punchAnimationL == null && _punchAnimationR == null) return;
            if (isLeft) {
                _punchAnimationL.Play();
            }
            else {
                _punchAnimationR.Play();
            }
        }

        private void JumpSound() {
            if (playerScript == null) return;
            // BroAudio.Play(_agileAttackSettings.jumpSound);
        }


        private IEnumerator LightAttackPreparation() {
            yield return enemyScript.ChangeColorGradually(LightPrepareColor, 0.5f);

            AttackEnum = AttackEnum.LightAttack;

            enemyScript.isAttackActive = true;
            enemyScript.LightCooldown = true;

            animator = Player.GetComponentInChildren<Animator>();
            animator.SetInteger("State", 2);

            CoroutineRunner.Instance.RunCoroutine(_attackCoolDownCoroutine(_agileAttackSettings.lightAttackCooldownEnemy, false, 0));


            CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(_lightAttackColliderL, _agileAttackSettings.lightAttackDuration, false));
            CoroutineRunner.Instance.RunCoroutine(_attackDurationCoroutine(_lightAttackColliderR, _agileAttackSettings.lightAttackDuration, false));


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

        protected override void CleanStates() {
            if (!_isPlayer) {
                return;
            }

            if (CooldownCoroutine == null) {
                playerScript.isAttackActive = false;
                if (SpecialDurationCoroutine == null) {
                    playerScript.SpecialCooldown = false;
                }
            }
        }
    }
}