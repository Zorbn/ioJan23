using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class Hud : MonoBehaviour
    {
        [SerializeField] private Image heartImage;

        private void Start()
        {
            UpdateHearts(PlayerInteraction.StartingHealth);
        }

        public void UpdateHearts(int count)
        {
            heartImage.rectTransform.sizeDelta = new Vector2(count * 100f, 100f);
        }
    }
}