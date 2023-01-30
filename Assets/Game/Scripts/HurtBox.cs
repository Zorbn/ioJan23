using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    public class HurtBox : MonoBehaviour
    {
        [SerializeField] private NetworkIdentity identity;
        [SerializeField] private int damage = 1;

        private void OnTriggerStay2D(Collider2D col)
        {
            if (!identity.isServer) return;
            if (!col.CompareTag("Player")) return;
            var playerInteraction = col.GetComponent<PlayerInteraction>();
            if (playerInteraction is null) return;
            playerInteraction.TakeDamage(damage);
        }
    }
}