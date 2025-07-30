using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Laser {
    public abstract class LaserSensorTrigger: MonoBehaviour {
        [SerializeField] private List<LaserSensor> sensors;
        private readonly List<LaserSensor> _activeSensors = new List<LaserSensor>();

        public void Start() {
            foreach (var sensor in sensors) {
                sensor.laserSensorTrigger = this;
            }
        }

        public void AddSensor(LaserSensor sensor) {
            sensors.Add(sensor);
        }

        public void SetSensorsActive(LaserSensor sensor) {
            if(!sensors.Contains(sensor)) return;
            
            _activeSensors.Add(sensor);
            if(_activeSensors.Count==sensors.Count) Activate();
        }

        public void SetSensorInactive(LaserSensor sensor) {
            if(!sensors.Contains(sensor) || !_activeSensors.Contains(sensor)) return;
            
            _activeSensors.Remove(sensor);
        }

        protected abstract void Activate();
    }
}