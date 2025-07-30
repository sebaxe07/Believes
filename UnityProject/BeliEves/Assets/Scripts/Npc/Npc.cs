using System;
using Ami.Extension;
using Events;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Events.EventsLayout;
using Object = UnityEngine.Object;

namespace Npc {
    public abstract class Npc : MonoBehaviour {

        [SerializeField] NavMeshAgent _navMeshAgent;
        [SerializeField] private float _speed = 3.5f;
        [SerializeField] private Transform[] _waypoints;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rigidbody;

        private int _currentWaypointIndex = 0;

        private void Start() {
            _animator = GetComponent<Animator>();
            /*_navMeshAgent.speed = _speed;
            _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);*/
        }

        private void Update() {
            if (_waypoints.Length == 0) return;
            if (_navMeshAgent == null) {
                _navMeshAgent = GetComponent<NavMeshAgent>();
                if (_navMeshAgent == null) {
                    _navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
                }
            };
            float actualSpeed = _navMeshAgent.velocity.magnitude.ClampNormalize();
            _animator.SetFloat("Speed", actualSpeed);
            if (_navMeshAgent.isOnNavMesh == true) {
                if (_navMeshAgent.remainingDistance < 1f) {
                    _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
                    _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);
                }
            }
        }

        public void SetWaypoints(Transform[] waypoints) {
            _waypoints = waypoints;
            _navMeshAgent.speed = _speed;
            _navMeshAgent.ResetPath();
            _currentWaypointIndex = 0;
            _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);
        }
    }
}