using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/WindowDecal")]
    [Tracked]
    public class WindowDecal : Entity
    {
        public Sprite Sprite;
        public Vector2 Scale;
        private Level level;
        private float Opacity;
        private bool isFG;
        public Color Color;
        private bool rendered;
        private VirtualRenderTarget Target;
        public string CustomTag;
        private bool onScreen;
        public WindowDecal(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/Window/");
            Sprite.AddLoop("idle", "pane", 1f);
            Add(Sprite);
            isFG = data.Bool("fg");
            Depth = isFG ? -10501 : 10001;
            Opacity = data.Float("opacity", 0.8f);
            if (isFG)
            {
                Opacity += 0.2f;
            }
            Sprite.Play("idle");
            Sprite.Visible = false;
            Collider = new Hitbox(data.Width, data.Height);
            Target = VirtualContent.CreateRenderTarget("WindowDecal", data.Width, data.Height);
            Scale = new Vector2(Width / Sprite.Width, Height / Sprite.Height);
            Sprite.Scale = Scale;
            Color = data.HexColor("color");
            Add(new BeforeRenderHook(BeforeRender));
            CustomTag = data.Attr("customTag");
            Tag |= Tags.TransitionUpdate;
        }
        private void BeforeRender()
        {
            if (onScreen)
            {
                Target.SetAsTarget(true);
                Draw.SpriteBatch.Begin();
                //DrawLines
                int offset = 8;
                float width = (Sprite.Width * Sprite.Scale.X) + (offset * 2);
                float height = (Sprite.Height * Sprite.Scale.Y) + (offset * 2);
                int thickness = 8 + (int)width / 64;
                int smallThickness = 2;
                int lineGroups = (int)width / 32;
                Rectangle bounds = new Rectangle(-offset, -offset, (int)width, (int)height);
                Sprite.RenderAt(Vector2.Zero);
                Draw.HollowRect(bounds, Color.Blue);
                for (int i = 0; i < lineGroups; i++)
                {
                    Vector2 p = new Vector2(bounds.X + offset + bounds.Width / (i + 1), bounds.Y);
                    float angle = 135f.ToRad();
                    float mult = Math.Min(1, Math.Max(0.3f, (width - p.X) / width));
                    Vector2 smallLineOffset = Vector2.UnitX * (thickness * mult + smallThickness * mult + 1);
                    Draw.LineAngle(p, angle, height * 1.5f, Color.White, thickness * mult);
                    Draw.LineAngle(p - smallLineOffset, angle, height * 1.5f, Color.White, Math.Max(1, smallThickness * mult));
                }
                Draw.SpriteBatch.End();
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
        }
        public override void Render()
        {
            if (onScreen)
            {
                Draw.SpriteBatch.Draw(Target, Position, Color * Opacity);
            }
            base.Render();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }
        public override void Update()
        {
            base.Update();
            onScreen = CullHelper.IsRectangleVisible(X, Y, Width, Height);
        }
    }
}