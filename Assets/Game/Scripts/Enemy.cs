using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : NetworkBehaviour
    {
        private const float AnimationTime = 0.2f;
        
        [SerializeField] private float speed = 2f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] walkFrames;
        
        private Rigidbody2D _rb;
        private int _mapLayerMask;
        private bool _grounded;
        private float _direction = 1f;
        
        private float _animationTimer;
        private int _animationFrame;
        
        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _mapLayerMask = LayerMask.GetMask("Map");
        }
        
        private void Update()
        {
            _animationTimer -= Time.deltaTime;
            transform.localScale = new Vector2(_direction, 1f);

            while (_animationTimer < 0f)
            {
                _animationTimer += AnimationTime;
                _animationFrame = (_animationFrame + 1) % 2;
                spriteRenderer.sprite = walkFrames[_animationFrame];
            }
        }

        private void FixedUpdate()
        {
            if (!isServer) return;

            var groundedHitBox = new Vector2(0.7f, 0.1f);
            var obstructedHitBox = new Vector2(0.7f, 0.7f);
            var position = transform.position;
            _grounded = Physics2D.OverlapBox(position + new Vector3(0f, -0.4f), groundedHitBox, 0f,
                _mapLayerMask);
            bool projectionGrounded = Physics2D.OverlapBox(position + new Vector3(_direction * 0.8f, -0.4f), groundedHitBox, 0f,
                _mapLayerMask);
            bool projectionObstructed = Physics2D.OverlapBox(position + new Vector3(_direction * 0.2f, 0f), obstructedHitBox, 0f,
                _mapLayerMask);
            
            if (_grounded && (!projectionGrounded || projectionObstructed))
            {
                _direction *= -1f;
            }

            _rb.velocity = new Vector2(_direction * speed, _rb.velocity.y);
            
            RpcUpdateDirection(_direction);
        }

        [ClientRpc]
        private void RpcUpdateDirection(float direction)
        {
            if (isServer) return;
            _direction = direction;
        }
    }
}