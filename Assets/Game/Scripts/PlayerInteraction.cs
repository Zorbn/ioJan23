using System;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInteraction : NetworkBehaviour
    {
        public const int StartingHealth = 3;
        private const float InvincibilityDuration = 0.5f;

        private NetworkManager _networkManager;
        private MapGenerator _mapGenerator;

        private PlayerMovement _playerMovement;
        private Hud _hud;
        
        private int _health = StartingHealth;
        private float _invincibilityTimer;

        private void Start()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _hud = GameObject.Find("Canvas").GetComponent<Hud>();
            _networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            _mapGenerator = GameObject.Find("Grid/Tilemap").GetComponent<MapGenerator>();
            _mapGenerator.RegenerateEvent += OnRegenerate;
            CmdRequestHealthUpdate();
        }
        
        private void OnDestroy()
        {
            _mapGenerator.RegenerateEvent -= OnRegenerate;
        }

        private void Update()
        {
            if (!isServer) return;
            _invincibilityTimer -= Time.deltaTime;
        }
        
        private void OnRegenerate()
        {
            if (!isServer) return;
            if (transform.position.x > 0f) TakeDamage(_health);
        }

        public void ExitServer(InputAction.CallbackContext context)
        {
            if (!context.ReadValueAsButton()) return;
            
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                _networkManager.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                _networkManager.StopClient();
            }
            else if (NetworkServer.active)
            {
                _networkManager.StopServer();
            }
        }
        
        public void Restart(InputAction.CallbackContext context)
        {
            if (!context.ReadValueAsButton()) return;
            CmdSuicide();
        }

        private void UpdateHealth(int amount)
        {
            _health = amount;
            if (!isLocalPlayer) return;
            _hud.UpdateHearts(_health);
        }

        public void TakeDamage(int damage)
        {
            if (!isServer) throw new AccessViolationException($"{nameof(TakeDamage)} cannot be called on a client!");
            if (_invincibilityTimer > 0f) return;
            
            _invincibilityTimer = InvincibilityDuration;
            UpdateHealth(_health - damage);
            RpcTakeDamage(damage);

            if (_health > 0) return;
            
            UpdateHealth(StartingHealth);
            RpcRespawn();
            TargetRespawn(netIdentity.connectionToClient);
        }

        [Command]
        private void CmdSuicide()
        {
            TakeDamage(_health);
        }

        [ClientRpc]
        private void RpcTakeDamage(int damage)
        {
            if (isServer) return;
            UpdateHealth(_health - damage);
        }
        
        [ClientRpc]
        private void RpcRespawn()
        {
            if (isServer) return;
            UpdateHealth(StartingHealth);
        }

        [TargetRpc]
        private void TargetRespawn(NetworkConnection target)
        {
            _playerMovement.ReturnToLobby();
        }

        [Command(requiresAuthority = false)]
        private void CmdRequestHealthUpdate(NetworkConnectionToClient sender = null)
        {
            TargetReceiveHealthUpdate(sender, _health);
        }

        [TargetRpc]
        private void TargetReceiveHealthUpdate(NetworkConnection target, int amount)
        {
            UpdateHealth(amount);
        }
    }
}