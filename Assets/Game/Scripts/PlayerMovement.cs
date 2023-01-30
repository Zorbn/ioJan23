using System;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovement : NetworkBehaviour
    {
        private const float Speed = 5f;
        private const float JumpForce = 8f;
        private const float MinYVelocity = -24f;
        private const float AnimationTime = 0.2f;

        private static readonly Vector2 CameraSpeed = new(10f, 5f);
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private Sprite jumpSprite;
        [SerializeField] private Sprite standSprite;
        
        private Rigidbody2D _rb;
        private PlayerInput _playerInput;
        private Transform _cameraTransform;
        
        private int _mapLayerMask;
        
        private Vector2 _direction;
        private bool _tryJump;
        private bool _grounded;
        private float _xVelocity;
        private bool _jumpHeld;

        private MapGenerator _mapGenerator;
        private NetworkManager _networkManager;
        
        private float _animationTimer;
        private int _animationFrame;
        
        public override void OnStartLocalPlayer()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerInput.enabled = true;
        }

        public void Start()
        {
            _networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            _mapGenerator = GameObject.Find("Grid/Tilemap").GetComponent<MapGenerator>();
            _mapGenerator.RegenerateEvent += OnRegenerate;
            _mapLayerMask = LayerMask.GetMask("Map");
            _rb = GetComponent<Rigidbody2D>();
            var cam = Camera.main;
            if (cam is null) throw new NullReferenceException("Camera.main is null");
            _cameraTransform = cam.transform;
        }

        private void OnDestroy()
        {
            _mapGenerator.RegenerateEvent -= OnRegenerate;
        }

        private void OnRegenerate()
        {
            if (!isLocalPlayer) return;
            if (transform.position.x > 0f) ReturnToLobby();
        }

        public void Move(InputAction.CallbackContext context)
        {
            _direction = context.action.ReadValue<Vector2>();
        }

        public void Jump(InputAction.CallbackContext context)
        {
            _tryJump = context.ReadValueAsButton();
        }

        public void Restart(InputAction.CallbackContext context)
        {
            if (!context.ReadValueAsButton()) return;
            ReturnToLobby();
        }

        private void Update()
        {
            UpdateAnimation();
            
            if (!isLocalPlayer) return;

            var newCameraPosition = transform.position;
            var currentCameraPosition = _cameraTransform.position;
            newCameraPosition.x = currentCameraPosition.x + (newCameraPosition.x - currentCameraPosition.x) * CameraSpeed.x * Time.deltaTime;
            newCameraPosition.y = currentCameraPosition.y + (newCameraPosition.y - currentCameraPosition.y) * CameraSpeed.y * Time.deltaTime;
            newCameraPosition.z = currentCameraPosition.z;
            _cameraTransform.position = newCameraPosition;
        }

        private void UpdateAnimation()
        {
            var moving = MathF.Abs(_xVelocity) > 0.1f;

            if (moving)
            {
                var direction = _xVelocity > 0f ? 1f : -1f;
                transform.localScale = new Vector2(direction, 1f);
            }
            
            if (!_grounded)
            {
                spriteRenderer.sprite = jumpSprite;
                return;
            }

            if (!moving)
            {
                spriteRenderer.sprite = standSprite;
                return;
            }

            _animationTimer -= Time.deltaTime;

            while (_animationTimer < 0f)
            {
                _animationTimer += AnimationTime;
                _animationFrame = (_animationFrame + 1) % 2;
                spriteRenderer.sprite = walkFrames[_animationFrame];
            }
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;
            
            _grounded = Physics2D.OverlapBox(transform.position + new Vector3(0f, -0.4f), new Vector2(0.7f, 0.1f), 0f,
                _mapLayerMask);
            _rb.velocity = new Vector2(Math.Sign(_direction.x) * Speed, _rb.velocity.y);
            
            if (_grounded && _tryJump)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, JumpForce);
                _jumpHeld = true;
                _rb.gravityScale = 0.5f;
            }
            else if (_jumpHeld && (_grounded || !_tryJump || _rb.velocity.y < 0f))
            {
                _jumpHeld = false;
                _rb.gravityScale = 1.0f;
            }

            if (_rb.velocity.y < MinYVelocity)
            {
                var newVelocity = _rb.velocity;
                newVelocity.y = MinYVelocity;
                _rb.velocity = newVelocity;
            }

            _xVelocity = _rb.velocity.x;
            
            CmdUpdateGrounded(_grounded);
            CmdUpdateXVelocity(_xVelocity);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!isLocalPlayer) return;

            if (col.CompareTag("Start"))
            {
                StartLevel();
            }
        }

        private void StartLevel()
        {
            transform.position = _mapGenerator.SpawnPosition;
        }
        
        public void ReturnToLobby()
        {
            if (!isLocalPlayer)
                throw new AccessViolationException($"{nameof(ReturnToLobby)} can only be called on the local player!");
            transform.position = _networkManager.GetStartPosition().position;
        }
        
        [Command]
        private void CmdUpdateGrounded(bool grounded)
        {
            RpcUpdateGrounded(grounded);
            if (isLocalPlayer) return;
            _grounded = grounded;
        }
        
        [ClientRpc]
        private void RpcUpdateGrounded(bool grounded)
        {
            if (isLocalPlayer) return;
            _grounded = grounded;
        }

        [Command]
        private void CmdUpdateXVelocity(float velocity)
        {
            RpcUpdateXVelocity(velocity);
            if (isLocalPlayer) return;
            _xVelocity = velocity;
        }
        
        [ClientRpc]
        private void RpcUpdateXVelocity(float velocity)
        {
            if (isLocalPlayer) return;
            _xVelocity = velocity;
        }
    }
}