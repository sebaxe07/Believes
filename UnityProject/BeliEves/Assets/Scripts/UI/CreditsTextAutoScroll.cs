using System.Collections;
using MenuManagement;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Pool;

namespace UI {
    public class CreditsTextAutoScroll : MonoBehaviour {
        [SerializeField] private float speed = 100f;
        [SerializeField] private GameObject textBox;
        [SerializeField] private float beginPose;
        [SerializeField] private float boundaryTextEnd;
        
        [SerializeField] private string menuSceneName = "MainMenu";
        private RectTransform _rectTransform;
        private AsyncOperation _asyncOperation;

        void Start() {
            _asyncOperation = SceneManager.LoadSceneAsync(menuSceneName);
            _asyncOperation!.allowSceneActivation = false;
            
            _rectTransform = textBox.GetComponent<RectTransform>();
            StartCoroutine(AutoScroll());
        }

        private IEnumerator AutoScroll() {
            while (_rectTransform.localPosition.y < boundaryTextEnd) {
                _rectTransform.Translate(Vector3.up * speed * Time.deltaTime);
                yield return null;
            }
            MenuReturn();
        }

        private void MenuReturn() {
            PoolManager.ClearAllPools();
            _asyncOperation.allowSceneActivation = true; 
            MainMenu.Open();
        }
    }
}