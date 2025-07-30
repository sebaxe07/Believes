using System;
using Ami.BroAudio;
using Controller.Attack.AgileAttack;
using JetBrains.Annotations;
using ScriptableObjects.Attack;
using UnityEngine;

namespace Controller.Attack {
    public class GrapplingRope : MonoBehaviour {
        private AgileAttackSettings _agileAttackSettings;
        private Spring _spring;
        private LineRenderer _lr;
        private Vector3 _currentGrapplePosition;
        public GrapplingGun grapplingGun;

        private bool _isEnabled = false;
        private GameObject _target;
        private Action _move;
        private SoundID _hitSound;
        private bool _grapplingSoundsPlayed = false;

        private void Awake() {
            _agileAttackSettings = Resources.Load<AgileAttackSettings>("Settings/AttackSettings/AgileAttack");

            _lr = GetComponent<LineRenderer>();
            _spring = new Spring();
            _spring.SetTarget(0);

            _spring.SetDamper(_agileAttackSettings.damper);
            _spring.SetStrength(_agileAttackSettings.strength);

            ResetGrapple();
        }
        private void LateUpdate() {
            if (grapplingGun != null && grapplingGun.IsGrappling() && _isEnabled) DrawRope();
            else if (_move != null) ResetGrapple();
        }
        public void EnableGrapplingGun(GameObject target, [CanBeNull] Action move, SoundID hitSound) {
            ResetGrapple();
            _isEnabled = true;
            _target = target;
            _move = move;
            _hitSound = hitSound;
            _grapplingSoundsPlayed = false;
        }
        private void DrawRope() {
            if (_lr.positionCount == 0) {
                _spring.SetVelocity(_agileAttackSettings.velocity);
                _lr.positionCount = _agileAttackSettings.quality + 1;
            }
            _spring.Update(Time.deltaTime);

            var grapplePoint = _target.transform.position;
            var gunTipPosition = grapplingGun.gunTip.position + new Vector3(0,1.72f,0);
            var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

            _currentGrapplePosition = Vector3.Lerp(_currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

            for (var i = 0; i < _agileAttackSettings.quality + 1; i++) {
                var delta = i / (float)_agileAttackSettings.quality;
                var offset = up * (_agileAttackSettings.waveHeight * Mathf.Sin(delta * _agileAttackSettings.waveCount * Mathf.PI) * _spring.Value * _agileAttackSettings.affectCurve.Evaluate(delta));

                _lr.SetPosition(i, Vector3.Lerp(gunTipPosition, _currentGrapplePosition, delta) + offset);
            }

            if (Vector3.Distance(_currentGrapplePosition, grapplePoint) <= 0.3 && _move != null) {
                if (!_grapplingSoundsPlayed) {
                    BroAudio.Play(_hitSound, _currentGrapplePosition);
                    _grapplingSoundsPlayed = true;
                }
                _move();
            }

        }

        private void ResetGrapple() {
            _move = null;
            _currentGrapplePosition = grapplingGun.gunTip.position;
            _spring.Reset();
            if (_lr.positionCount > 0)
                _lr.positionCount = 0;
        }
    }
}