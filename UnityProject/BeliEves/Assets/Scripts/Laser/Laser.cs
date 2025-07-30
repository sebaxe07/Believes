using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Laser {
    public class Laser: MonoBehaviour, ILaserSensor{
        public bool _activated = false;
        private bool _codeActive = false;
        
        private LineRenderer _lineRenderer;
        [SerializeField] private LaserRenderSettings laserRenderSettings;
        [SerializeField] private float topOffset = 0.5f;
        [SerializeField] private float frontOffset = 0.5f;
        
        private Vector3 _sourcePosition;
        [SerializeField] private const float FarDistance = 1000f;
        private List<Vector3> _bouncePositions = new List<Vector3>();
        private readonly int _maxBounces = 100;
        
        private ILaserSensor _prevStruckLaserSensor = null;

        List<Laser> _strikingLasers;
        
        public void Awake() {
            _strikingLasers = new List<Laser>();
            _lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (_lineRenderer == null) {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            laserRenderSettings.Apply(_lineRenderer);
        }

        private void FixedUpdate() {
            if (!_activated) {
                _lineRenderer.positionCount = 0;
                if (_prevStruckLaserSensor == null) return;
                
                LaserSensor.HandleLaser(this, _prevStruckLaserSensor,null);
                _prevStruckLaserSensor = null;
                return;
            }

            _sourcePosition = transform.position + transform.forward * frontOffset + transform.up * topOffset;
            _bouncePositions = new List<Vector3>() {_sourcePosition};
            
            CastBeam(_sourcePosition, transform.forward);
            
            _lineRenderer.positionCount = _bouncePositions.Count;
            _lineRenderer.SetPositions(_bouncePositions.ToArray());
        }

        public void CastBeam(Vector3 origin, Vector3 direction) {
            if(_bouncePositions.Count > _maxBounces)return;
            
            var ray = new Ray(origin, direction);
            bool didHit = Physics.Raycast(ray, out RaycastHit hitInfo, FarDistance, ~0, QueryTriggerInteraction.Ignore);

            if (!didHit) {
                var endPoint = origin + direction * FarDistance;
                _bouncePositions.Add(endPoint);
                if (_prevStruckLaserSensor != null) {
                    LaserSensor.HandleLaser(this, _prevStruckLaserSensor, null);
                    _prevStruckLaserSensor = null;
                }
                return;
            }
            
            _bouncePositions.Add(hitInfo.point);
            
            var reflectiveObject = hitInfo.collider.GetComponent<ILaserReflective>();
            if (reflectiveObject != null) {
                reflectiveObject.Reflect(this,ray,hitInfo);
            }
            else {
                var currentLaserSensor = hitInfo.collider.GetComponent<ILaserSensor>();
                if (currentLaserSensor != _prevStruckLaserSensor) {
                    LaserSensor.HandleLaser(this,_prevStruckLaserSensor,currentLaserSensor);
                    _prevStruckLaserSensor = currentLaserSensor;
                }
            }
        }

        public void AddLaser(Laser laser) {
            if(this._activated|| this._codeActive) return;
            _strikingLasers.Add(laser);
            if (_strikingLasers.Count == 1) {
                this._activated = true;
                this._codeActive = true;
            }
        }

        public void RemoveLaser(Laser laser) {
            if(!this._activated || !this._codeActive) return;
            _strikingLasers.Remove(laser);
            if (_strikingLasers.Count == 0) {
                this._activated = false; 
                this._codeActive = false;
            }
        }
    }
}