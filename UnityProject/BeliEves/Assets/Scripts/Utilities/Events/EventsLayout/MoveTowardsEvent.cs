using System;
using Events.EventsLayout;
using JetBrains.Annotations;
using UnityEngine;

namespace Utilities.Events.EventsLayout {
    public class MoveTowardsEvent: BasicEventChannel {
        public Vector3 goalPosition;
        public float time;
        public Action onComplete;
        public bool requestJump;

        public Vector3 goalDirection;

        public float coolDown;

        public void UpdateValue(Vector3 goalPosition, float time, bool requestJump, [CanBeNull] Action onComplete = null, Vector3 goalDirection = default, float coolDown = 0f) {
            this.goalPosition = goalPosition;
            this.time = time;
            this.onComplete = onComplete;
            this.requestJump = requestJump;
            this.goalDirection = goalDirection;
            this.coolDown = coolDown;
            RaiseEvent();
        }
    }
}