using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Npc {
    public class NPCEnemy : MonoBehaviour {

        public bool Turret = false;
        public Slider influenceBar;
        private Image _fillImage;
        private Camera _camera;

        [SerializeField] private float _influence = 100;

        [SerializeField] private GameObject _dieEffect;
        [SerializeField] private GameObject ExplosionArea;
        // Influence bar color
        [SerializeField] public Color _influenceColor = Color.magenta;
        [SerializeField] public Color _defeatedColor = Color.red;

        private bool _isAttackActive = false;



        void Start() {
            if (!Turret) {
                influenceBar.maxValue = _influence;
                influenceBar.value = _influence;
                _camera = Camera.main;
                _fillImage = influenceBar.fillRect.GetComponent<Image>();
                _fillImage.color = _influenceColor;
                ExplosionArea.SetActive(false);
            }
        }

        private void OnEnable() {
            _camera = Camera.main;
        }

        void Update() {
            if (influenceBar != null) {
                influenceBar.transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
            }
        }

        public void PlayerHit(Player.Player player) {
            //Debug.Log("Player hit");
        }


        public void Explode() {
            if (!_isAttackActive) {
                _isAttackActive = true;
                if (Turret) {
                    Instantiate(_dieEffect, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
                    // set inactive the Head that is a child of the turret
                    transform.Find("Head").gameObject.SetActive(false);
                    return;
                }
                StartCoroutine(ChargeAndExplode());
            }
        }

        private IEnumerator ChargeAndExplode() {
            //Debug.Log("Charging for explosion...");
            _fillImage.color = Color.red;

            // Find the Renderer component of the spider GameObject
            Renderer spiderRenderer = transform.Find("Spider").GetComponent<Renderer>();
            if (spiderRenderer != null) {
                // Store the original color of the material
                Color originalColor = spiderRenderer.material.color;
                Color targetColor = Color.red;
                float duration = 2.0f;
                float elapsedTime = 0f;

                // Gradually change the color from the original to red
                while (elapsedTime < duration) {
                    spiderRenderer.material.color = Color.Lerp(originalColor, targetColor, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Ensure the final color is set to red
                spiderRenderer.material.color = targetColor;
            }


            // Activate the explosion area and instantiate the explosion effect
            ExplosionArea.SetActive(true);
            Instantiate(_dieEffect, transform.position + new Vector3(0, 1, 0), Quaternion.identity);

            // Wait for the explosion to finish
            yield return new WaitForSeconds(0.2f);

            // Destroy the game object
            gameObject.SetActive(false);
            // return the influence bar to its original color and value after the explosion
            _fillImage.color = _influenceColor;
            _influence = 100;
            influenceBar.value = _influence;
            _isAttackActive = false;
            // Return the material color to its original color
            if (spiderRenderer != null) {
                spiderRenderer.material.color = Color.white;
            }
            ExplosionArea.SetActive(false);

        }
        public void TakeDamage(float damage) {
            if (Turret) {
                Explode();
                return;
            }
            _influence -= damage;
            influenceBar.value = _influence;
            if (_influence <= 0) {
                Explode();
            }
        }
    }
}
