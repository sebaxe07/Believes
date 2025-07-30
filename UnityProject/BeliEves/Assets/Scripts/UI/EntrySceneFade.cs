using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI {
    public class EntrySceneFade : MonoBehaviour {
        [SerializeField] private string sceneName;
        [SerializeField] private float waitTime;
        public void Start() {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation!.allowSceneActivation = false;
            StartCoroutine(FadeCoroutine(asyncOperation));
        }

        private IEnumerator FadeCoroutine(AsyncOperation asyncOperation) {
            yield return new WaitForSeconds(waitTime);
            asyncOperation.allowSceneActivation = true;
        }
    }
}