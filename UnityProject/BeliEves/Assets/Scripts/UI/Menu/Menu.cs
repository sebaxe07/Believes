using UnityEngine;

namespace UI.Menu {
    [RequireComponent(typeof(Canvas))]
    public abstract class Menu : MonoBehaviour {
        [SerializeField] public GameObject firstSelected;
        public virtual void OnBackPressed()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.CloseMenu();
            }
        }
        public virtual void SecurityAction(){}
    }

    public abstract class Menu<T> : Menu where T : Menu<T> {
        private static T _instance;
        public static string friendlyName;

        public static T Instance {
            get { return _instance; }
        }

        

        protected virtual void Awake()
        {
            if (_instance != null) {
                Destroy(gameObject);
            }
            else {
                _instance = (T) this;
            }
        }

        protected void OnDestroy()
        {
            _instance = null;
        }

        public static void Open() {
            if (MenuManager.Instance != null && Instance != null)
            {
                MenuManager.Instance.OpenMenu(Instance);
            }
        }

    }

}
