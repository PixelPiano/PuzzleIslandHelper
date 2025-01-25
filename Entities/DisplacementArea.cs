using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/DisplacementArea")]
    public class DisplacementArea : Entity
    {
        public string Flag;
        public bool Inverted;
        public bool Fade;
        public float FadeTime;
        private float alpha;
        public float Opacity;
        public float XScroll;
        public float YScroll;
        public Color Color;
        public Sprite Map;
        private VirtualRenderTarget Target;
        public VertexBuffer Buffer;
        public Vector2 Scroll;
        private Vector2 scrollRate;
        public DisplacementArea(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = data.Int("depth");
            Collider = new Hitbox(data.Width, data.Height);
            Flag = data.Attr("flag");
            Inverted = data.Bool("inverted");
            Fade = data.Bool("fadeInOut");
            FadeTime = data.Float("fadeTime", 1);
            alpha = Opacity = data.Float("alpha", 1);
            Color = data.HexColor("color", Color.White);
            scrollRate = new Vector2(data.Float("scrollX"), data.Float("scrollY"));

            Add(Map = new Sprite(GFX.Game, data.Attr("path")));
            Map.AddLoop("idle","", data.Float("delay"));
            Map.Visible = false;
            Map.Play("idle");
            Target = VirtualContent.CreateRenderTarget("displacement+scroll", data.Width, data.Height);
            Add(new DisplacementRenderHook(RenderDisplacement));
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Update()
        {
            base.Update();
            bool flag = Flag.GetFlag(Inverted);
            float target = flag ? Opacity : 0;
            float rate = Fade ? Opacity : Engine.DeltaTime / FadeTime;
            alpha = Calc.Approach(alpha, target, rate);
            Scroll.X = (Scroll.X + scrollRate.X * Engine.DeltaTime) % 1;
            Scroll.Y = (Scroll.Y + scrollRate.Y * Engine.DeltaTime) % 1;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
        }
        public void BeforeRender()
        {
            Target.SetAsTarget(true);
            if (Scene is not Level level) return;
            Effect effect = ShaderFX.Scroll.ApplyParameters(level, Matrix.Identity);
            effect.Parameters["Scroll"]?.SetValue(Vector2.Zero);
            effect.Parameters["atlas_texture"].SetValue(Map.Texture.Texture.Texture_Safe);
            Draw.SpriteBatch.StandardBegin(Matrix.Identity, effect);
            Draw.Rect(Vector2.Zero, Width, Height, Color.White);
            Draw.SpriteBatch.End();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            RenderDisplacement();
        }
        public void RenderDisplacement()
        {
            if (alpha * Opacity > 0)
            {
                Draw.SpriteBatch.Draw(Target, Position, Color * alpha * Opacity);
            }
        }
    }
}