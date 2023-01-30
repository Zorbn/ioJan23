using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private string connectingUiPath;
        [SerializeField] private string desktopUiPath;
        [SerializeField] private string mainMenuUiPath;
        [SerializeField] private string ipInputPath;
        
        private NetworkManager _manager;
        private GameObject _connectingUi;
        private GameObject _desktopUi;
        private GameObject _mainMenuUi;
        private TMP_InputField _ipInput;
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
            _ipInput = GameObject.Find(ipInputPath).GetComponent<TMP_InputField>();
            
            UpdateIp(_ipInput.text);
            
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                _desktopUi.SetActive(false);
            }
        }

        private void Update()
        {
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