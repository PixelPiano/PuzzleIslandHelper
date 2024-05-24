using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class TiledImage : Image
    {
        private bool up, down, left, right;
        private int mult;
        public TiledImage(MTexture texture, bool up, bool down, bool left, bool right, int mult) : base(texture, true)
        {
            this.up = up;
            this.down = down;
            this.left = left;
            this.right = right;
            this.mult = mult;
        }
        public override void Render()
        {
            base.Render();
            if (Texture != null)
            {
                Draw(Vector2.Zero);
                if (up)
                {
                    for (int i = 1; i < mult; i++)
                    {
                        Draw(-Vector2.UnitY * Texture.Height);
                    }
                }
                if (down)
                {
                    for (int i = 1; i < mult; i++)
                    {
                        Draw(Vector2.UnitY * Texture.Height);
                    }
                }
                if (left)
                {
                    for (int i = 1; i < mult; i++)
                    {
                        Draw(-Vector2.UnitX * Texture.Width);
                    }
                }
                if (right)
                {
                    for (int i = 1; i < mult; i++)
                    {
                        Draw(Vector2.UnitX * Texture.Width);
                    }
                }
            }
        }
        public void Draw(Vector2 offset)
        {
            Texture.Draw(RenderPosition + offset, Origin, Color, Scale, Rotation, Effects);
        }
    }
}