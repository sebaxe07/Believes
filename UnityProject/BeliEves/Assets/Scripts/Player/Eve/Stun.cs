using System;
using System.Collections;
using UnityEngine;

namespace Player {
    public class Stun : MonoBehaviour {
        [SerializeField] private float stunDuration =1f;
        private void OnEnable() {
            StartCoroutine(_stun());
        }
        private IEnumerator _stun() {
            yield return new WaitForSeconds(stunDuration);
            this.gameObject.SetActive(false);
        }
    }
}