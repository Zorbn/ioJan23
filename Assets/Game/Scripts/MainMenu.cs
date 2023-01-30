using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private string connectingUiPath;
        [SerializeField] private string desktopUiPath;
        [SerializeField] private string mainMenuUiPath;
        
        private NetworkManager _manager;
        private GameObject _connectingUi;
        private GameObject _desktopUi;
        private GameObject _mainMenuUi;
        private bool _onMenu;

        private void Awake()
        {
            _manager = GetComponent<NetworkManager>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelLoaded;
        }

        private void OnLevelLoaded(Scene scene, LoadSceneMode mode)
        {
            _onMenu = scene.path == _manager.offlineScene;

            if (!_onMenu) return;
            
            _connectingUi = GameObject.Find(connectingUiPath);
            _desktopUi = GameObject.Find(desktopUiPath);
            _mainMenuUi = GameObject.Find(mainMenuUiPath);
            
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                _desktopUi.SetActive(false);
            }
        }

        private void Update()
        {
            // if (SceneManager.GetActiveScene().path != _manager.offlineScene) return;
            if (!_onMenu) return;

            if (!NetworkClient.active)
            {
                _connectingUi.SetActive(false);
                _mainMenuUi.SetActive(true);
            }
            else
            {
                _connectingUi.SetActive(true);
                _mainMenuUi.SetActive(false);
            }
            
            // if (!NetworkClient.active)
            // {
            //     foreach (var networkControl in networkControls)
            //     {
            //         if (!networkControl.activeSelf)
            //         {
            //             networkControl.SetActive(true);
            //         }
            //     }
            //     
            //     connectingUi.SetActive(false);
            // }
            // else
            // {
            //     foreach (var networkControl in networkControls)
            //     {
            //         if (!networkControl.activeSelf)
            //         {
            //             networkControl.SetActive(false);
            //         }
            //     }
            //     
            //     connectingUi.SetActive(true);
            // }
        }

        public void UpdateIp(string address)
        {
            _manager.networkAddress = address;
        }

        public void StartHost()
        {
            _manager.StartHost();
        }

        public void StartClient()
        {
            _manager.StartClient();
        }
    }
}