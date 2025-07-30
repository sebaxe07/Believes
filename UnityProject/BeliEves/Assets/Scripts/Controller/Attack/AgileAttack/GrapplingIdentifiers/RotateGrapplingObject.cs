using System;
using Events;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Events.EventsLayout;

namespace Controller.Attack.AgileAttack.GrapplingIdentifiers {
    public class RotateGrapplingObject : MonoBehaviour {

        public void Rotate(Action endAction) {
            transform.Rotate(0f, 22.5f, 0f);
            endAction();
        }

    }
}