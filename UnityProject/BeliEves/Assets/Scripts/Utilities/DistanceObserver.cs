using System;
using UnityEngine;

namespace Utilities {
    public class DistanceObserver : MonoBehaviour{
        public GameObject objectA; // Assign in Inspector
        public GameObject objectB; // Assign in Inspector
        public float range = 90f; // Set range in Inspector
        public Action Callback = null;
        
        void Update() {
            if (objectA != null && objectB != null) {
                float sqrDistance = (objectA.transform.position - objectB.transform.position).sqrMagnitude;
                if (sqrDistance >= range * range && Callback!=null) {
                    Callback();
                }
            }
        }
    }
}