using System.Collections;
using TMPro;
using UnityEngine;

namespace UI.Transition {
    public class TransitionFader : ScreenFader {
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float delay = 0.3f;
        [SerializeField] private TMP_Text transitionText;

        public float Delay {
            get { return delay; }
        }
    
        private void Awake() {
            lifetime = Mathf.Clamp(lifetime, FadeOnDuration + FadeOffDuration + delay, 10f);
        }

        public void SetText(string text="") {
            if (transitionText != null) {
                transitionText.text = text;
            }
        }
    
        private IEnumerator PlayRoutine() {   
            SetAlpha(clearAlpha);
            yield return new WaitForSeconds(delay);

            FadeOn();

            float onTime = lifetime - (FadeOffDuration + delay);
            yield return new WaitForSeconds(onTime);

            FadeOff();
            Destroy(gameObject, FadeOffDuration);
        }

        public void Play()
        {
            StartCoroutine(PlayRoutine());
        }

        public static void PlayTransition(TransitionFader transitionPrefab, string text = "READY")
        {
            if (transitionPrefab != null)
            {
                TransitionFader instance = Instantiate(transitionPrefab, Vector3.zero, Quaternion.identity);
                DontDestroyOnLoad(instance);
                instance.SetText(text);
                instance.Play();
            }
        }
    }
}
