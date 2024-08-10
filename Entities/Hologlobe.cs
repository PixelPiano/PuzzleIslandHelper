using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class Hologlobe : Entity
    {
        public VirtualRenderTarget Target;
        public Effect Shader;
        public float Alpha;
        public Color BaseColor;
        public Color GlitchColor;
        public Color BoxColor;
        public Color FrontColor;
        public Color BackColor;
        public bool Glitchy;
        public float Alpha2;
        public Hologlobe(Vector2 position, float width, float height, Color baseColor, Color glitchColor, Color boxColor, Color front, Color back, float alpha = 1) : base(position)
        {
            Depth = 2;
            Collider = new Hitbox(width, height);
            Add(new BeforeRenderHook(BeforeRender));
            Add(new Coroutine(Flicker()));
            Target = VirtualContent.CreateRenderTarget("Hologram", (int)width, (int)height);
            Shader = ShaderHelper.TryGetEffect("globeHologram");
            BaseColor = baseColor;
            GlitchColor = glitchColor;
            BoxColor = boxColor;
            FrontColor = front;
            BackColor = back;
            Alpha = alpha;
            projector = new Projector(new Vector2(-16, Height / 2 + 2), Width + 32, Height / 2 + 17);
            Add(projector);
            Tag |= Tags.TransitionUpdate;
        }
        public void FadeIn()
        {
            Add(new Coroutine(FadeInRoutine()));
        }
        public void FadeOut()
        {
            Add(new Coroutine(FadeOutRoutine()));
        }
        public IEnumerator FadeInRoutine()
        {
            Alpha2 = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Alpha2 = Calc.LerpClamp(0, 1, Ease.CubeIn(i));
                yield return null;
            }
            Alpha2 = 1;
        }
        public IEnumerator FadeOutRoutine()
        {
            Alpha2 = 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Alpha2 = Calc.LerpClamp(1, 0, Ease.CubeIn(i));
                yield return null;
            }
            Alpha2 = 0;
        }
        public class Projector : Component
        {
            public static Vector2[] Points = new Vector2[] { new(0, 0), new(0.30f, 1), new(0.5f, 0), new(0.70f, 1), new(1, 0) };
            public VertexPositionColor[] Vertices = new VertexPositionColor[Points.Length];
            public int[] indices = new int[] { 1, 0, 2, 2, 3, 1, 2, 4, 3 };
            public Vector2 Position;
            public float Width;
            public float Height;
            public float Alpha = 1;
            public Projector(Vector2 position, float width, float height) : base(true, true)
            {
                Position = position;
                Width = width;
                Height = height;

            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                for (int i = 0; i < Points.Length; i++)
                {
                    Vertices[i] = new VertexPositionColor(new Vector3(Entity.Position + Position + Points[i] * new Vector2(Width, Height), 0), i % 2 == 0 ? Color.Transparent : Color.Blue);
                }
            }
            public override void Update()
            {
                base.Update();
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Position = new Vector3(Entity.Position + Position + Points[i] * new Vector2(Width, Height), 0);
                    Vertices[i].Color = i % 2 == 0 ? Color.Transparent : Color.Blue * Alpha;
                }
            }
            public override void Render()
            {
                base.Render();
                if (Scene is not Level level) return;
                Draw.SpriteBatch.End();
                GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, 5, indices, 3);
                GameplayRenderer.Begin();

            }
        }
        public Projector projector;
        public override void Update()
        {
            projector.Alpha = Alpha * Alpha2;
            base.Update();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
            Shader?.Dispose();
            Shader = null;
        }
        private void BeforeRender()
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Black);
        }
        public override void Render()
        {
            if (Scene is not Level level || Alpha * Alpha2 <= 0) return;
            Shader.ApplyParameters(level, level.Camera.Matrix);
            Shader.Parameters["Radius"]?.SetValue(Width / 2f / 180f);
            Shader.Parameters["BaseColor"]?.SetValue(BaseColor.ToVector4());
            Shader.Parameters["GlitchColor"]?.SetValue(GlitchColor.ToVector4());
            Shader.Parameters["BoxColor"]?.SetValue(BoxColor.ToVector4());
            Shader.Parameters["FrontColor"]?.SetValue(FrontColor.ToVector4());
            Shader.Parameters["BackColor"]?.SetValue(BackColor.ToVector4());
            Shader.Parameters["Glitchy"]?.SetValue(Glitchy);
            Shader.Parameters["Alpha"]?.SetValue(Alpha * Alpha2);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
                DepthStencilState.None, RasterizerState.CullNone, Shader, level.Camera.Matrix);
            Draw.SpriteBatch.Draw(Target, Position, Color.White);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
            projector.Render();
        }
        public IEnumerator Flicker()
        {
            while (true)
            {
                yield return Calc.Random.Range(0.05f, 0.1f);
                float duration = Calc.Random.Range(0.05f, 0.12f);
                float to = Calc.Random.Range(0.6f, 0.75f);
                float from = Alpha;
                for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
                {
                    Alpha = Calc.LerpClamp(from, to, i) * Alpha2;
                    yield return null;
                }
            }
        }
    }
}