using Celeste.Mod.CommunalHelper;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class WindowImage : WindowComponent
    {
        public MTexture Texture;
        public float Alpha = 1;
        public virtual float Width => Texture.Width;
        public virtual float Height => Texture.Height;
        public Vector2 ImageOffset;

        public Vector2 Origin;

        public Vector2 Scale = Vector2.One;

        public float Rotation;

        public Color Color = Color.White;
        public SpriteEffects Effects;

        public bool Outline;

        public Color OutlineColor = Color.Black;
        public float X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position.X = value;
            }
        }

        public float Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position.Y = value;
            }
        }

        public bool FlipX
        {
            get
            {
                return (Effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
            }
            set
            {
                Effects = (value ? (Effects | SpriteEffects.FlipHorizontally) : (Effects & ~SpriteEffects.FlipHorizontally));
            }
        }

        public bool FlipY
        {
            get
            {
                return (Effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            }
            set
            {
                Effects = (value ? (Effects | SpriteEffects.FlipVertically) : (Effects & ~SpriteEffects.FlipVertically));
            }
        }
        public WindowImage(BetterWindow window, MTexture texture)
            : base(window)
        {
            Texture = texture;
        }
        public WindowImage(BetterWindow window, MTexture texture, bool active)
            : base(window, active)
        {
            Texture = texture;
        }

        public override void Render()
        {
            if (Outline)
            {
                DrawOutline(OutlineColor);
            }
            DrawTexture(Color);
        }
        public void DrawTexture(Color color)
        {
            if (Texture != null)
            {
                Texture.Draw(RenderPosition + ImageOffset, Origin, color * Alpha, Scale, Rotation, Effects);
            }
        }
        public void DrawOutline(int offset = 1)
        {
            DrawOutline(Color.Black, offset);
        }

        public void DrawOutline(Color color, int offset = 1)
        {
            Vector2 position = Position;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Position = position + new Vector2(i * offset, j * offset);
                        DrawTexture(color);
                    }
                }
            }

            Position = position;
        }

        public void DrawSimpleOutline()
        {
            Vector2 position = Position;
            Color color = Color.Black;
            Position = position + new Vector2(-1f, 0f);
            DrawTexture(color);
            Position = position + new Vector2(0f, -1f);
            DrawTexture(color);
            Position = position + new Vector2(1f, 0f);
            DrawTexture(color);
            Position = position + new Vector2(0f, 1f);
            DrawTexture(color);
            Position = position;
        }

        public WindowImage SetOrigin(float x, float y)
        {
            Origin.X = x;
            Origin.Y = y;
            return this;
        }

        public WindowImage CenterOrigin()
        {
            Origin.X = Width / 2f;
            Origin.Y = Height / 2f;
            return this;
        }

        public WindowImage JustifyOrigin(Vector2 at)
        {
            Origin.X = Width * at.X;
            Origin.Y = Height * at.Y;
            return this;
        }

        public WindowImage JustifyOrigin(float x, float y)
        {
            Origin.X = Width * x;
            Origin.Y = Height * y;
            return this;
        }

        public WindowImage SetColor(Color color)
        {
            Color = color;
            return this;
        }
        public override void OnOpened(Scene scene)
        {
        }
        public override void OnClosed(Scene scene)
        {
        }
    }
}