using UnityEngine;

namespace Game.Scripts
{
    public static class Enemy
    {
        public static int UpdateAnimation(SpriteRenderer spriteRenderer, int animationFrame, Sprite[] frames)
        {
            animationFrame = (animationFrame + 1) % frames.Length;
            spriteRenderer.sprite = frames[animationFrame];

            return animationFrame;
        }
    }
}