using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private VirtualRenderTarget Target = VirtualContent.CreateRenderTarget("WindowDecal", 320, 180);
        public string CustomTag;
        public WindowDecal(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 2;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/Window/");
            Sprite.AddLoop("idle", "pane", 1f);
            Add(Sprite);
            isFG = data.Bool("fg");
            Depth = isFG? -10501:9001;
            Opacity = data.Float("opacity",0.8f);
            if (isFG)
            {
                Opacity += 0.2f;
            }
            Sprite.Play("idle");
            Sprite.Visible = false;
            Collider = new Hitbox(data.Width, data.Height);
            Scale = new Vector2(Width / Sprite.Width, Height / Sprite.Height);
            Sprite.Scale = Scale;
            Color = data.HexColor("color");
            Add(new BeforeRenderHook(BeforeRender));
            CustomTag = data.Attr("customTag");
        }
        private void BeforeRender()
        {
            Target.DrawThenMask(Sprite, (Action)DrawWindow,level.Camera.Matrix);
        }
        private void DrawWindow()
        {
            Sprite.Render();
            //DrawLines
            int offset = 8;
            float width = (Sprite.Width * Sprite.Scale.X) + (offset * 2);
            float height = (Sprite.Height * Sprite.Scale.Y) + (offset * 2);
            int thickness = 8 + (int)width / 64;
            int smallThickness = 2;
            int lineGroups = (int)width / 32;
            Rectangle bounds = new Rectangle((int)Position.X - offset, (int)Position.Y - offset, (int)width, (int)height);
            Draw.HollowRect(bounds, Color.Blue);
            Vector2[] Starting = new Vector2[lineGroups];
            for (int i = 0; i < lineGroups; i++)
            {
                Starting[i] = new Vector2(bounds.X + offset + (bounds.Width / (i + 1)), bounds.Y);
            }

            for (int i = 0; i < lineGroups; i++)
            {

                float angle = 135f.ToRad();
                float mult = Math.Min(1, Math.Max(0.3f, (width - (Starting[i].X - Position.X)) / width));
                Vector2 smallLineOffset = Vector2.UnitX * ((thickness * mult) + (smallThickness * mult) + 1);
                Draw.LineAngle(Starting[i], angle, height * 1.5f, Color.White, thickness * mult);
                Draw.LineAngle(Starting[i] - smallLineOffset, angle, height * 1.5f, Color.White, Math.Max(1, smallThickness * mult));
            }
        }
        public override void Render()
        {
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color * Opacity);
            base.Render();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }
    }
}