using System.Collections;
using UnityEngine;

namespace Laser {
    public class LaserOpenDoor : LaserSensorTrigger {
        private Coroutine coroutine;
        
        private IEnumerator ShakeAndDestroy() {
            Vector3 originalPosition = transform.position;
            float shakeDuration = 1f;
            float shakeMagnitude = 0.1f;
            float elapsed = 0.0f;

            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-shakeMagnitude, shakeMagnitude);
                float y = Random.Range(-shakeMagnitude, shakeMagnitude);
                transform.position = new Vector3(originalPosition.x + x, originalPosition.y, originalPosition.z + y);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPosition; // Reset position
            Destroy(gameObject); // Remove the GameObject
        }

        protected override void Activate() {
            if (coroutine != null) return;

            coroutine = StartCoroutine(ShakeAndDestroy());
        }
    }
}