using System;
using System.Collections.Generic;
using UnityEngine;

namespace Laser {
    public class LaserSensor: MonoBehaviour, ILaserSensor {
        [SerializeField] private GameObject feedbackObject;
        
        private List<Laser> _strikingLasers;
        public LaserSensorTrigger laserSensorTrigger;
        
        private void Awake() {
            if(feedbackObject!=null)feedbackObject.SetActive(false);
            _strikingLasers = new List<Laser>();
        }

        public static void HandleLaser(Laser laser, ILaserSensor prev, ILaserSensor current) {
            if(prev == current) return;
            
            if (prev != null) prev.RemoveLaser(laser);
            if (current != null) current.AddLaser(laser);
        }

        public void AddLaser(Laser laser) {
            _strikingLasers.Add(laser);
            if (_strikingLasers.Count == 1) {
                if(feedbackObject!=null)feedbackObject.SetActive(true);
                if(laserSensorTrigger!=null) laserSensorTrigger.SetSensorsActive(this);
            }
        }

        public void RemoveLaser(Laser laser) {
            _strikingLasers.Remove(laser);
            if (_strikingLasers.Count == 0) {
                if(feedbackObject!=null)feedbackObject.SetActive(false);
                if(laserSensorTrigger!=null) laserSensorTrigger.SetSensorInactive(this);
            }
        }
    }
    
    public interface ILaserSensor {
        public abstract void AddLaser(Laser laser);
        public abstract void RemoveLaser(Laser laser);
    }
}