using UnityEngine;

namespace Pool {
    public class DontDestroyOnLoad : MonoBehaviour {
        void Awake(){
            DontDestroyOnLoad(this.gameObject);
        }
    }
}