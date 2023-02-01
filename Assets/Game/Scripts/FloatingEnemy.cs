using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FloatingEnemy : NetworkBehaviour
    {
        private const float AnimationTime = 0.2f;
        
        [SerializeField] private float speed = 2f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] floatFrames;
        
        private Rigidbody2D _rb;
        private int _mapLayerMask;
        private float _direction = 1f;
        
        private float _animationTimer;
        private int _animationFrame;
        
        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _mapLayerMask = LayerMask.GetMask("Map");
            transform.localScale = new Vector2(Random.Range(0, 2) * 2 - 1, 1f);
        }
        
        private void Update()
        {
            _animationTimer -= Time.deltaTime;

            while (_animationTimer < 0f)
            {
                _animationTimer += AnimationTime;
                _animationFrame = Enemy.UpdateAnimation(spriteRenderer, _animationFrame, floatFrames);
            }
        }

        private void FixedUpdate()
        {
            if (!isServer) return;

            var obstructedHitBox = new Vector2(0.7f, 0.7f);
            var position = transform.position;
            bool projectionObstructed = Physics2D.OverlapBox(position + new Vector3(0f, _direction * 0.2f), obstructedHitBox, 0f,
                _mapLayerMask);
            
            if (projectionObstructed)
            {
                _direction *= -1f;
            }

            _rb.velocity = new Vector2(_rb.velocity.x, _direction * speed);
        }
    }
}