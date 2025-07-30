using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utilities {
    public class ObjLink: MonoBehaviour {
        [SerializeField] private Rigidbody other;
        
        private Rigidbody _rigidbody;

        void Start() {
            _rigidbody = this.gameObject.GetComponent<Rigidbody>();
            
            if(_rigidbody != null && other != null)  ConnectRigidbody();
            
        }
        
        private void ConnectRigidbody() {
            FixedJoint joint = other.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = _rigidbody;
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = other.transform.InverseTransformPoint(other.transform.position);
            joint.connectedAnchor = _rigidbody.transform.InverseTransformPoint(other.transform.position);
            
            joint.breakForce = float.PositiveInfinity;  // Prevents breaking under force
            joint.breakTorque = float.PositiveInfinity; // Prevents breaking under torque
            joint.massScale = 1f; 
            joint.connectedMassScale = 1f;
        }

    }

    
}