using System;
using System.Collections;
using Events;
using Events.EventsLayout;
using Unity.VisualScripting;
using UnityEngine;
using Utilities.Events.EventsLayout;
using ScriptableObjects.Attack;
using Player;
using Npc;
using ScriptableObjects.Movement;
using UnityEngine.AI;
using Ami.BroAudio;

namespace Controller.Attack {
    public class EveAttackController : AttackController {
        private EveAttack _eveAttack = Resources.Load<EveAttack>("Settings/AttackSettings/EveAttack");

        private readonly EventBodySwitch _bodySwitchChannel;

        private readonly KinematicMovementSettings _kinematicMovementSettings;

        private Vector3 _oldGravity = Vector3.zero;
        private readonly Action _startEveFlightAction;
        private readonly BasicEventChannel _stopEveFlightActionEventChannel;

        private readonly EventUseStamina _useStaminaEvent;

        private readonly BasicEventChannel _jumpEventChannel;

        public EveAttackController(Func<Vector3> getPlayerDirection, Action startEveFlightAction) : base(getPlayerDirection) {
            _bodySwitchChannel = (EventBodySwitch)EventBroker.TryToAddEventChannel("bodySwitch", ScriptableObject.CreateInstance<EventBodySwitch>());
            _kinematicMovementSettings = Resources.Load<KinematicMovementSettings>("Settings/MovementSettings/EveKinematicMovementSettings");

            this._startEveFlightAction = startEveFlightAction;

            _stopEveFlightActionEventChannel = ScriptableObject.CreateInstance<BasicEventChannel>();
            _stopEveFlightActionEventChannel = EventBroker.TryToAddEventChannel("heavyAttackStop", _stopEveFlightActionEventChannel);
            _stopEveFlightActionEventChannel.Subscribe(HeavyAttackStop);

            _jumpEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("jumpEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
            _jumpEventChannel.Subscribe(() => JumpSound());

            _useStaminaEvent = (EventUseStamina)EventBroker.TryToAddEventChannel("useStaminaEvent", ScriptableObject.CreateInstance<EventUseStamina>());

            _kinematicMovementSettings.Gravity = new Vector3(0, -30, 0);//avoid bugs
        }

        protected override void CreateLightAttack() {
            throw new NotImplementedException();
        }

        protected override void CreateHeavyAttack() {
            throw new NotImplementedException();
        }

        protected override void CreateSpecialAttack() {
            throw new NotImplementedException();
        }
        protected override void LightAttackTrigger() {
            _useStaminaEvent.UpdateValue(Player, _eveAttack.lightAttackStaminaCost, (Exception ex) => {
                if (ex != null) return;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, _eveAttack.dashLayerMask)) {
                    Vector3 hitPoint = hit.point; // Punto colpito sul suolo
                    Vector3 playerPosition = Player.transform.position; // Posizione del giocatore

                    float distance = Vector3.Distance(new Vector3(hitPoint.x, 0, hitPoint.z),
                        new Vector3(playerPosition.x, 0, playerPosition.z));
                    if (distance > _eveAttack.dashDistance) return;

                    AttackEnum = AttackEnum.LightAttack;
                    playerScript.lightAttackColliderGameObject.SetActive(true);
                    BroAudio.Play(_eveAttack.lightAttackSound);
                    var destination = new Vector3(hit.point.x, playerScript.transform.position.y, hit.point.z);
                    _moveTowardsEventChannel.UpdateValue(destination, _eveAttack.dashForceMagnitude, false, new Action(
                        () => {
                            if (playerScript != null) playerScript.lightAttackColliderGameObject.SetActive(false);
                        }));
                    //_attackCoolDownCoroutine(playerScript.lightAttackColliderGameObject, _eveAttack.lightAttackCoolDownTime);

                }
                else {
                    var stun = Player.transform.Find("Stun").gameObject;
                    stun.SetActive(true);
                }
            });
        }

        protected override void HeavyAttackTrigger() {
            if (_oldGravity != Vector3.zero) {
                HeavyAttackStop();
                return;
            }

            _useStaminaEvent.UpdateValue(Player, _eveAttack.heavyAttackStaminaCost / 2, (Exception ex) => {
                if (ex != null) {
                    BroAudio.Stop(_eveAttack.Flying);
                    BroAudio.Play(_eveAttack.Down);
                    return;
                }
                BroAudio.Play(_eveAttack.Up);
                AttackEnum = AttackEnum.HeavyAttack;
                _oldGravity = _kinematicMovementSettings.Gravity;
                _kinematicMovementSettings.Gravity = Vector3.zero;
                _startEveFlightAction();
                BroAudio.Play(_eveAttack.Flying);
                _useStaminaEvent.UpdateValue(Player, _eveAttack.heavyAttackStaminaCost / 2, (Exception ex) => {
                    if (ex != null) {
                        BroAudio.Stop(_eveAttack.Flying);
                        _kinematicMovementSettings.Gravity = _oldGravity;
                        _oldGravity = Vector3.zero;
                        BroAudio.Play(_eveAttack.Down);
                    }
                }, true, _eveAttack.specialAttackRate);
            });
        }

        protected override void SpecialAttackTrigger() {//TODO special attack for Eve
        }

        protected override void AttackHit(GameObject player, GameObject hitObj) {
            if (AttackEnum == AttackEnum.LightAttack && !hitObj.TryGetComponent<Player.Player>(out var controlledNpc) && !hitObj.TryGetComponent<Npc.NPCEnemy>(out var NPCEnemy)) {
                //UnityEngine.Object.Destroy(controlledNpc);

                // Check if the hit object is a robot enemy that has been defeated
                if (hitObj.TryGetComponent<Npc.Enemy>(out var npc)) {
                    if (!npc._isDefeated) {
                        return;
                    }
                }

                UnityEngine.Object.Destroy(player);  //TODO understand if destroying and reallocating Eve cause a high impact on performances

                //hitObj.AddComponent<Player.Player>();


                // Set the tag of the hit object to Player
                hitObj.tag = "Player";

                Transform vfx = hitObj.transform.Find("SelectedArea");
                if (vfx != null) vfx.gameObject.SetActive(false);

                var robotType = BodyType.Robot;
                if (hitObj.TryGetComponent<AgileRobot>(out var agileRobot)) {
                    robotType = BodyType.Agile;
                    UnityEngine.Object.Destroy(agileRobot);
                    hitObj.AddComponent<RobotAttackInput>();
                }
                else if (hitObj.TryGetComponent<TankRobot>(out var tankRobot)) {
                    robotType = BodyType.Tank;
                    UnityEngine.Object.Destroy(tankRobot);
                    hitObj.AddComponent<RobotAttackInput>();
                }
                else if (hitObj.TryGetComponent<SupportRobot>(out var supportRobot)) {
                    robotType = BodyType.Support;
                    UnityEngine.Object.Destroy(supportRobot);
                    hitObj.AddComponent<SupportHealthInput>();
                }

                if (hitObj.TryGetComponent<NavMeshAgent>(out var agentMesh)) UnityEngine.Object.Destroy(agentMesh);

                _bodySwitchChannel.UpdateValue(robotType);
            }
        }


        private void HeavyAttackStop() {
            if (_oldGravity == Vector3.zero) return;
            BroAudio.Stop(_eveAttack.Flying);
            _kinematicMovementSettings.Gravity = _oldGravity;
            BroAudio.Play(_eveAttack.Down);
            _oldGravity = Vector3.zero;
            _useStaminaEvent.UpdateValue(Player, _eveAttack.heavyAttackStaminaCost / 2, (Exception ex) => { }, true, _eveAttack.specialAttackRate);
        }
        // Cooldown coroutine
        private IEnumerator _attackCoolDownCoroutine(GameObject gameObject, float seconds) {
            yield return new WaitForSeconds(seconds);
            playerScript.isAttackActive = false;
            gameObject.SetActive(false);
        }

        private void JumpSound() {
            if (playerScript == null) return;
            BroAudio.Play(_eveAttack.Up);
        }

        protected override void CleanStates() {
        }
    }
}