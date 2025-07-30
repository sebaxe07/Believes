using System.Collections;
using Ami.BroAudio;
using UnityEngine;

namespace Controller.Attack.AgileAttack.SpecialAttack {
    public class RotatingGrappling : MonoBehaviour {
        private Transform _pivotObject;
        private Coroutine _rotatingGrapplingCoroutine;

        private LineRenderer _lineRenderer;
        private void Awake() {
            _pivotObject = transform.parent;
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = 0.1f;
            _lineRenderer.endWidth = 0.1f;
        }
        public void StartSpin(float rotationSpeed, int rotationTime, SoundID audio) {
            if (_lineRenderer == null) return;
            if (_lineRenderer.positionCount == 0) _lineRenderer.positionCount = 2;

            _rotatingGrapplingCoroutine = StartCoroutine(Spin(rotationSpeed, rotationTime, audio));
        }
        private IEnumerator Spin(float rotationSpeed, int rotationTime, SoundID audio) {
            for (int i = 0; i < rotationTime; i++) {
                transform.RotateAround(_pivotObject.transform.position, new Vector3(0, 1, 0), rotationSpeed * Time.deltaTime);
                // Update the LineRenderer's positions as the object moves
                _lineRenderer.SetPosition(0, transform.position);
                _lineRenderer.SetPosition(1, _pivotObject.position);

                yield return null;
            }
            BroAudio.Stop(audio);
            Destroy(this.gameObject);
        }
    }
}