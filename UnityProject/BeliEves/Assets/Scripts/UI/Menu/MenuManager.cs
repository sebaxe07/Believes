using System.Collections.Generic;
using MenuManagement;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace UI.Menu {
    public class MenuManager : Singleton<MenuManager> {
        [Header("Menu Prefabs")] 
        [SerializeField] private Menu mainMenu;
        [SerializeField] private List<Menu> primaryMenus = new List<Menu>();
        
        [FormerlySerializedAs("_menuParent")]
        [Header("Where Menus is placed in the scene")]
        [SerializeField] private Transform menuParent;
        

        private readonly Stack<Menu> _menuStack = new Stack<Menu>();
        private static bool _ignoreEscape;
        private Menu _mainMenu;

        private static MenuManager _instance;
        public static MenuManager Instance {
            get { return _instance; }
        }
        
        
        private void Awake() {
            if (_instance != null){
                Destroy(gameObject);
            }else {
                _instance = this;
                InitializeMenu();
                DontDestroyOnLoad(menuParent);
            }

        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape) && !_ignoreEscape && SceneManager.GetActiveScene().name != "CreditScene") {
                if (_menuStack.Count > 0 && _menuStack.Peek()!=_mainMenu) {
                    CloseMenu();
                    EventSystem.current.SetSelectedGameObject(null);
                } else if(_menuStack.Count==0){
                    PauseMenu.Open();
                }
            } else if (Input.GetKeyDown(KeyCode.Escape) && _ignoreEscape) {
                _ignoreEscape = false;
            }
        }

        public void IgnoreEscapeOne() {
            _ignoreEscape = true;
        }

        public void OpenMenu(Menu menuInstance) {
            if (menuInstance == null)return;

            if (_menuStack.Count > 0) {
                foreach (Menu menu in _menuStack) {
                    menu.gameObject.SetActive(false);
                }
            }

            EventSystem.current.SetSelectedGameObject(menuInstance.firstSelected);
            menuInstance.gameObject.SetActive(true);
            _menuStack.Push(menuInstance);
        }

        public void CloseMenu() {
            if (_menuStack.Count == 0) return;
            Menu topMenu = _menuStack.Pop();
            topMenu.gameObject.SetActive(false);
            topMenu.SecurityAction();
            if (_menuStack.Count > 0) {
                Menu nextMenu = _menuStack.Peek();
                nextMenu.gameObject.SetActive(true);
            }
        }

        
        private void InitializeMenu() {
            if (_mainMenu == null) {
                _mainMenu = Instantiate(mainMenu, menuParent);
                _mainMenu.gameObject.SetActive(false);
                OpenMenu(_mainMenu);
            }

            foreach (var menu in primaryMenus) {
                var menuInstance = Instantiate(menu,menuParent);
                menuInstance.gameObject.SetActive(false);
            }
        }
    }
}