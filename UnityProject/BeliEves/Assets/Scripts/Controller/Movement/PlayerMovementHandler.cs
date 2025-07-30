using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BBUnity.Actions;
using JetBrains.Annotations;
using KinematicCharacterController;
using ScriptableObjects.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Events.EventsLayout;
using Events;
using Events.EventsLayout;

namespace Controller.Movement {
    public class PlayerMovementHandler : MonoBehaviour, ICharacterController {
        public KinematicCharacterMotor _motor;
        private KinematicMovementSettings _kinematicMovementSettings;

        private Vector3 _lookInputVector;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private bool _jumpRequested = false;
        private Vector3 _moveInputVector;
        private Vector3 _internalVelocityAdd = Vector3.zero;
        private bool _jumpedThisFrame = false;
        private bool _jumpConsumed = false;
        private float _timeSinceLastAbleToJump = 0f;
        private List<Collider> _ignoredColliders = new List<Collider>();
        private bool _runRequested = false;

        private EventWithFloat _walkingAnimationEventChannel;
        private BasicEventChannel _jumpEventChannel;
        private TeleportEvent _teleportEvent;

        //second jump
        private bool _secondJumpEnable = false;
        private bool _secondJumpConsumed = false;
        private bool _secondJumpRequested = false;

        private Vector3 _lookingDirection;

        private static bool _isInputBlocked = false;

        private Coroutine _moveCoroutine;


        private void Awake() {
            _kinematicMovementSettings = Resources.Load<KinematicMovementSettings>("Settings/MovementSettings/EveKinematicMovementSettings");

            _motor.CharacterController = this;
            _lookingDirection =  FindObjectOfType<Player.Player>().gameObject.transform.forward;

            _motor.MaxStepHeight = _kinematicMovementSettings.MaxStepHeight;
            _motor.MinRequiredStepDepth = _kinematicMovementSettings.MinRequireStepDepth;

            PrepareCollisionAvoidance(_kinematicMovementSettings.IgnoredColliders);

            _walkingAnimationEventChannel = ScriptableObject.CreateInstance<EventWithFloat>();
            _walkingAnimationEventChannel = (EventWithFloat)EventBroker.TryToAddEventChannel("walkingAnimationEvent", _walkingAnimationEventChannel);

            _jumpEventChannel = ScriptableObject.CreateInstance<BasicEventChannel>();
            _jumpEventChannel = (BasicEventChannel)EventBroker.TryToAddEventChannel("jumpEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());

            _teleportEvent = (TeleportEvent)EventBroker.TryToAddEventChannel("teleportEvent", ScriptableObject.CreateInstance<TeleportEvent>());
            _teleportEvent.Subscribe(new Action(() => {
                Teleport(_teleportEvent.destination, _teleportEvent.destinationRotation);
            }));
        }
        public void ChangeMotor(KinematicCharacterMotor motor, Vector3 forward) {
            if (_moveCoroutine != null) {
                StopCoroutine(_moveCoroutine);
                DisableMovement(false);
            }
            _moveInputVector = Vector3.zero;

            _motor = motor;
            _motor.CharacterController = this;

            _motor.MaxStepHeight = _kinematicMovementSettings.MaxStepHeight;
            _motor.MinRequiredStepDepth = _kinematicMovementSettings.MinRequireStepDepth;

            PrepareCollisionAvoidance(_kinematicMovementSettings.IgnoredColliders);

            _motor.enabled = true;
        }

        public void ToggleSecondJump(bool enable) {
            _secondJumpEnable = enable;
        }
        private void PrepareCollisionAvoidance(List<GameObject> objects) {
            _ignoredColliders.Clear();
            if (objects.Count == 0) return;

            var tagList = new List<string>();
            foreach (var component in objects) {
                tagList.Add(component.tag);
            }

            if (tagList.Count == 0) return;

            foreach (var tag in tagList) {
                var collidersWithTag = GameObject.FindGameObjectsWithTag(tag);
                foreach (var obj in collidersWithTag) {
                    var collider = obj.GetComponent<Collider>();
                    if (collider != null) {
                        _ignoredColliders.Add(collider);
                    }
                }
            }
        }

        public void ChangeSettings(KinematicMovementSettings kinematicMovementSettings) {
            _kinematicMovementSettings = kinematicMovementSettings;
            _motor.MaxStepHeight = _kinematicMovementSettings.MaxStepHeight;
        }
        public void DisableMovement(bool b) {
            if (b) {
                _runRequested = false;
                _moveInputVector = Vector3.zero;
            }
            _isInputBlocked = b;
        }

        public void SetInputs(ref PlayerCharacterInputs inputs, Vector3 lookingDirection) {
            if (_isInputBlocked) return;
            SetInputs(ref inputs);

            //rotation input
            _lookingDirection.x = lookingDirection.x;
            _lookingDirection.z = lookingDirection.z;
        }
        public void SetInputs(ref PlayerCharacterInputs inputs) {
            if (_isInputBlocked) return;

            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);


            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, _motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f) {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, _motor.CharacterUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, _motor.CharacterUp);


            _moveInputVector = cameraPlanarRotation * moveInputVector;
            _lookInputVector = Vector3.zero;

            // Jumping input
            if (inputs.JumpDown) {
                _timeSinceJumpRequested = 0f;
                if (_jumpConsumed != true) _jumpRequested = true;
                else if ((_secondJumpEnable && !_motor.GroundingStatus.IsStableOnGround) || inputs.ForceSecondJump) {
                    _secondJumpRequested = !_secondJumpRequested;
                    inputs.ForceSecondJump = false;
                }
            }


            int mult = 1;
            // Running input
            if (inputs.Running) {
                _runRequested = true;
                mult = 2;
            }
            else _runRequested = false;
            _walkingAnimationEventChannel.UpdateValue(moveInputVector.magnitude * mult);

        }


        private void Teleport(Vector3 destination, Quaternion rotation) {
            _motor.SetPositionAndRotation(destination, rotation, true);
        }
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
            if (_lookingDirection != Vector3.zero) currentRotation = Quaternion.LookRotation(_lookingDirection);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            if (_motor.GroundingStatus.IsStableOnGround) {
                float currentVelocityMagnitude = currentVelocity.magnitude;
                if (_runRequested && _moveInputVector.sqrMagnitude > 0f) {
                    currentVelocityMagnitude += _kinematicMovementSettings.RunSpeedUpFactor;
                }
                Vector3 effectiveGroundNormal = _motor.GroundingStatus.GroundNormal;
                // Reorient velocity on slope
                currentVelocity = _motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(_moveInputVector, _motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                Vector3 targetMovementVelocity = reorientedInput * _kinematicMovementSettings.MaxStableMoveSpeed;

                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-_kinematicMovementSettings.StableMovementSharpness * deltaTime));
            }
            else {
                // Add move input
                if (_moveInputVector.sqrMagnitude > 0f) {
                    Vector3 addedVelocity = _moveInputVector * _kinematicMovementSettings.AirAccelerationSpeed * deltaTime;
                    Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, _motor.CharacterUp);

                    // Limit air velocity from inputs
                    if (currentVelocityOnInputsPlane.magnitude < _kinematicMovementSettings.MaxAirMoveSpeed) {
                        // clamp addedVel to make total vel not exceed max vel on inputs plane
                        Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, _kinematicMovementSettings.MaxAirMoveSpeed);
                        addedVelocity = newTotal - currentVelocityOnInputsPlane;
                    }
                    else {
                        // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                        if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f) {
                            addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                        }
                    }

                    // Prevent air-climbing sloped walls
                    if (_motor.GroundingStatus.FoundAnyGround) {
                        if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f) {
                            Vector3 perPenticularObstructionNormal = Vector3.Cross(Vector3.Cross(_motor.CharacterUp, _motor.GroundingStatus.GroundNormal), _motor.CharacterUp).normalized;
                            addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perPenticularObstructionNormal);
                        }
                    }

                    // Apply added velocity
                    currentVelocity += addedVelocity;
                }

                //_walkingAnimationEventChannel.UpdateValue(currentVelocity.magnitude);

                // Gravity
                currentVelocity += _kinematicMovementSettings.Gravity * deltaTime;

                // Drag
                currentVelocity *= (1f / (1f + (_kinematicMovementSettings.Drag * deltaTime)));
            }

            // Handle second jumping
            if (_secondJumpRequested && !_secondJumpConsumed && !_motor.GroundingStatus.IsStableOnGround) {
                var jumpDirection1 = _motor.CharacterUp;
                _motor.ForceUnground();
                currentVelocity += (jumpDirection1 * _kinematicMovementSettings.JumpUpSpeed) - Vector3.Project(currentVelocity, _motor.CharacterUp);
                currentVelocity += (_moveInputVector * _kinematicMovementSettings.JumpScalableForwardSpeed);

                _secondJumpRequested = false;
                _secondJumpConsumed = true;
                _jumpedThisFrame = true;

            }

            // Handle jumping
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;
            if (_jumpRequested) {
                // See if we actually are allowed to jump
                if (!_jumpConsumed && (_timeSinceLastAbleToJump <= _kinematicMovementSettings.JumpPostGroundingGraceTime)) {
                    _jumpEventChannel.RaiseEvent();
                    Vector3 jumpDirection = _motor.CharacterUp;
                    if (_motor.GroundingStatus.FoundAnyGround && !_motor.GroundingStatus.IsStableOnGround) {
                        jumpDirection = _motor.GroundingStatus.GroundNormal;
                    }

                    // Makes the character skip ground probing/snapping on its next update. 
                    // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                    _motor.ForceUnground();

                    // Add to the return velocity and reset jump state|
                    currentVelocity += (jumpDirection * _kinematicMovementSettings.JumpUpSpeed) - Vector3.Project(currentVelocity, _motor.CharacterUp);
                    currentVelocity += (_moveInputVector * _kinematicMovementSettings.JumpScalableForwardSpeed);
                    _jumpRequested = false;
                    _jumpConsumed = true;
                    _jumpedThisFrame = true;
                }

            }

            // Take into account additive velocity
            if (_internalVelocityAdd.sqrMagnitude > 0f) {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }


        }

        public void BeforeCharacterUpdate(float deltaTime) { } //not necessary for our use cases

        public void PostGroundingUpdate(float deltaTime) { } //not necessary for our use cases

        public void AfterCharacterUpdate(float deltaTime) {
            // Handle jumping pre-ground grace period
            if (_jumpRequested && _timeSinceJumpRequested > _kinematicMovementSettings.JumpPreGroundingGraceTime) {
                _jumpRequested = false;
            }
            if (_motor.GroundingStatus.IsStableOnGround) {
                // If we're on a ground surface, reset jumping values
                if (!_jumpedThisFrame) {
                    _jumpConsumed = false;
                    _secondJumpConsumed = false;
                }
                _timeSinceLastAbleToJump = 0f;
            }
            else _timeSinceLastAbleToJump += deltaTime;
        }

        public bool IsColliderValidForCollisions(Collider coll) {
            if (_ignoredColliders.Count == 0) return true;
            if (_ignoredColliders.Contains(coll)) return false;
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { } //not necessary for our use cases

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport) { } //not necessary for our use cases

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }  //not necessary for our use cases

        public void OnDiscreteCollisionDetected(Collider hitCollider) { }  //not necessary for our use cases

        public void AddVelocity(Vector3 velocity) {
            _internalVelocityAdd += velocity;
        }

        public void MoveTowards(Vector3 goalPosition, float time, bool requestJump, [CanBeNull] Action onComplete = null, Vector3 goalDirection = default, float coolDown = 0f) {
            DisableMovement(true);
            _moveCoroutine = StartCoroutine(MoveCoroutine(goalPosition, time, requestJump, onComplete, goalDirection, coolDown));
        }

        private IEnumerator MoveCoroutine(Vector3 goalPosition, float time, bool requestJump, [CanBeNull] Action onComplete, Vector3 goalDirection, float coolDown) {
            var tmpSpeed = Vector3.Distance(_motor.gameObject.transform.position, goalPosition) / time;
            _jumpRequested = requestJump;
            float distance = Vector3.Distance(_motor.gameObject.transform.position, goalPosition);

            if (goalDirection != Vector3.zero) {
                //var newLookingRotation = Quaternion.LookRotation(goalDirection).eulerAngles;
                _lookingDirection.x = goalDirection.x;
                _lookingDirection.z = goalDirection.z;
            }

            if (distance != 0) {
                while (true) {
                    distance = Vector3.Distance(_motor.gameObject.transform.position, goalPosition);
                    if (distance <= 0.5f) break; //reach destination
                    _moveInputVector = (goalPosition - _motor.gameObject.transform.position).normalized * tmpSpeed;
                    yield return new WaitForSeconds(0.03f);
                    if (distance <= Vector3.Distance(_motor.gameObject.transform.position, goalPosition)) break; //no improvement
                }
            }
            _moveInputVector = Vector3.zero;
            yield return new WaitForSeconds(coolDown); // wait for n seconds

            if (onComplete != null) onComplete();
            DisableMovement(false);
        }

    }
}