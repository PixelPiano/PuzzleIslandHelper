using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class FadePoint : Entity
    {
        public static FadePoint Create(Scene scene, Vector2 position, int size, Color colorA, Color colorB, float duration, bool shrinks)
        {
            FadePoint fadePoint = new FadePoint(position, size, colorA, colorB, duration, shrinks);
            scene.Add(fadePoint);
            return fadePoint;
        }
        public int MaxSize;
        public float Size;
        public Color ColorA;
        public Color ColorB;
        private float alpha = 1;
        public FadePoint(Vector2 position, int size, Color colorA, Color colorB, float duration, bool shrinks = false, float alpha = 1) : base(position)
        {
            MaxSize = size;
            ColorA = colorA;
            ColorB = colorB;
            Depth = int.MinValue;
            Tween.Set(this, Tween.TweenMode.Oneshot, duration, Ease.SineIn, t => { if (shrinks) Size = Calc.LerpClamp(MaxSize, 0, t.Eased); else this.alpha = alpha * (1 - t.Eased); }, t => RemoveSelf());

        }
        public override void Render()
        {
            base.Render();
            Vector2 offset = Vector2.One * (Size / 2);
            if (Size - 1 > 0)
            {
                Draw.Rect(Position - offset + Vector2.One, Size - 1, Size - 1, ColorB);
                Draw.HollowRect(Position - offset, Size, Size, ColorA);
            }
            else
            {
                Draw.Rect(Position - offset, Size, Size, ColorA);
            }
        }
    }
}