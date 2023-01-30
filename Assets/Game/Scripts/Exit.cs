using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    public class Exit : NetworkBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!isServer) return;
            if (!col.CompareTag("Player")) return;
            mapGenerator.Regenerate();
        }
    }
}