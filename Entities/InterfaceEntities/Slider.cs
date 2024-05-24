using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public class Slider : WindowImage
    {
        public bool Disabled;
        public Collider Collider;
        public bool BoxIn;
        public Color BoxColor;
        public Vector2 HalfArea
        {
            get
            {
                return new Vector2(Width / 2, Height / 2);
            }
        }
        public bool Held;
        public float LineWidth;
        public float Value
        {
            get
            {
                return Calc.LerpClamp(from, to, progress);
            }
            set
            {
                progress = value / LineWidth;
            }
        }
        private float progress;
        private float from;
        private float to;
        private Vector2 yOffset;
        public Slider(BetterWindow window, float width, float from, float to, float startAt = 0) : base(window, GFX.Game["objects/PuzzleIslandHelper/interface/slider/handle"])
        {
            Collider = new Hitbox(width, Texture.Height);
            LineWidth = width;
            yOffset = Vector2.UnitY * Collider.Height / 2;
            Outline = true;
            this.from = from;
            this.to = to;
            Value = startAt;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Visible)
            {
                Color color = Color.Lerp(Color.Aqua, Color.Gray, Disabled ? 0.3f : 0);
                if (Window.Drawing && Entity.Visible)
                {
                    Draw.HollowRect(Collider, color);
                }
            }
        }
        public override void Render()
        {
            if (BoxIn)
            {
                Draw.Rect(Collider.Position - Vector2.One * 2, Collider.Width + 4, Collider.Height + 4, BoxColor);
                if (Outline)
                {
                    Draw.HollowRect(Collider.Position - Vector2.One * 3, Collider.Width + 6, Collider.Height + 6, Color.Black);
                }
            }
            Draw.Line(RenderPosition + yOffset, RenderPosition + yOffset + Vector2.UnitX * LineWidth, Color.Gray);
            base.Render();
        }

        public override void Update()
        {
            base.Update();

            Color = Color.Lerp(Color.White, Disabled ? Color.LightGray : Color.White, 0.5f);
            if (Held)
            {
                Color = Color.Lerp(Color, Color.Black, 0.2f);
            }
            Collider.Position = RenderPosition;
            if (Disabled)
            {
                Held = false;
                return;
            }
            if (Held)
            {
                float m = Interface.MouseWorldPosition.X;
                progress = Calc.Clamp((m - RenderPosition.X) / LineWidth, 0, 1);
            }
            ImageOffset.X = Calc.LerpClamp(0, LineWidth, progress);
            if (Interface.LeftPressed && Interface.MouseOver(Collider))
            {
                Press();
            }
            else
            {
                Unpress();
            }
        }
        public void Press()
        {
            Held = true;
        }
        public void Unpress()
        {
            Held = false;
        }
    }
}