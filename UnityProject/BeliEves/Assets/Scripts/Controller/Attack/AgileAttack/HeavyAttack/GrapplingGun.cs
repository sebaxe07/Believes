﻿using ScriptableObjects.Attack;
 using Unity.Mathematics;
 using UnityEngine;

namespace Controller.Attack.AgileAttack {
    public class GrapplingGun : MonoBehaviour {
        private GameObject _player;
        private AgileAttackSettings _agileAttackSettings;
        private SpringJoint _joint;
        private Vector3 _grapplePoint;
        public Transform gunTip;

        private void Awake() {
            _agileAttackSettings = Resources.Load<AgileAttackSettings>("Settings/AttackSettings/AgileAttack");
        }
        public bool UseGrapplingGun(GameObject player) {
            gunTip = player.transform;
           //gunTip.position = player.transform.position + new Vector3(0, 1.72f, 0);
            _player = player;
            
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, math.INFINITY, _agileAttackSettings.whatIsGrappable)) {
                float distanceFromPlayerToHit = Vector3.Distance(player.transform.position, hit.point);

                if (distanceFromPlayerToHit > _agileAttackSettings.grapplingMaxDistance) {
                    return false;
                }
                
                _grapplePoint = hit.point;
                _joint = _player.gameObject.AddComponent<SpringJoint>();
                _joint.autoConfigureConnectedAnchor = false;
                _joint.connectedAnchor = _grapplePoint;
                
                var distanceFromPoint = Vector3.Distance(_player.transform.position, _grapplePoint);
                
                _joint.maxDistance = distanceFromPoint * 0.8f;
                _joint.minDistance = distanceFromPoint * 0.25f;
                _joint.spring = 4.5f;
                _joint.damper = 7f;
                _joint.massScale = 4.5f;
            }
            return true;
        }

        
        public GameObject GetTarget(GameObject player) {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, math.INFINITY, _agileAttackSettings.whatIsGrappable)) {
                float distance = Vector3.Distance(player.transform.position, hit.point);
                if (distance <=  _agileAttackSettings.grapplingMaxDistance) {
                    return hit.collider.gameObject;
                }
            }
            return null;
        }
        public void StopGrapple() {
            Destroy(_joint);
            _joint = null;
        }
        public bool IsGrappling() {
            return _joint != null;
        }
        public Vector3 GetGrapplePoint() {
            return _grapplePoint;
        }
    }
}