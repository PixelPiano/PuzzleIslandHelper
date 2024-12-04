using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.WIP.ShiftArea;
using static MonoMod.InlineRT.MonoModRule;
using static PianoUtils;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FormativeHouse")]
    [Tracked]
    public class FormativeHouse : Entity
    {
        public const int BufferExtend = 16;
        public VirtualRenderTarget Target;
        public VertexPositionColor[] Vertices;
        public Vector2[] Points;
        public int[] indices;
        private Dictionary<Vector2, VertexBreath> breaths = [];
        private List<float> triColorLerps = [];
        private List<ColorShifter> Shifters = [];
        private List<float> randomBrightness = [];
        public FormativeHouse(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Target = VirtualContent.CreateRenderTarget("formative-house-target", data.Width + BufferExtend * 2, data.Height + BufferExtend * 2);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target.Dispose();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 breath = -Vector2.UnitY * breaths[Points[i]].Amount;
                Vector2 point = Position + Points[i];
                Draw.Line(point, point + breath, Color.White);
                Draw.Point(point, Color.Cyan);
                Draw.Point(point + breath, Color.Red);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            List<Vector2> points = new();
            List<int> indices = new();
            int xOffset = 8;
            for (int y = 0; y < Height; y += 16)
            {
                for (int x = xOffset; x < Width; x += 16)
                {
                    int count = indices.Count;
                    points.Add(new(x, y));
                    points.Add(new(x + 16, y));
                    points.Add(new(x - 8, y + 16));
                    points.Add(new(x - 8, y + 16));
                    points.Add(new(x + 16, y));
                    points.Add(new(x + 8, y + 16));
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                }
                xOffset = xOffset == 0 ? 8 : 0;
            }
            int c = 0;
            for (int i = 0; i < points.Count; i += 3)
            {
                List<Color> colors = [Color.Green, Color.Lime, Color.DarkGreen, Color.Turquoise, Color.DarkOliveGreen, Color.Red];
                triColorLerps.Add(0);
                int index = c;
                CreateTween(index, true);
                int r = Calc.Random.Range(0, 3);
                for (int j = 0; j < 3; j++)
                {
                    randomBrightness.Add(j == r ? 0.3f : 0);
                }
                c++;
                colors.Shuffle();
                ColorShifter s = new ColorShifter([.. colors]);
                Shifters.Add(s);
                Add(s);
                s.Rate = Calc.Random.Range(0.3f, 0.7f);
            }
            Points = points.ToArray();
            Vertices = points.Select(item => new VertexPositionColor(new Vector3(item, 0), Color.White)).ToArray();
            this.indices = indices.ToArray();
            foreach (var v in Points)
            {
                if (!breaths.ContainsKey(v))
                {
                    VertexBreath breath = new VertexBreath(Calc.Random.Range(4, 8f), Calc.Random.Range(1f, 8));
                    breaths.Add(v, breath);
                    Add(breath);
                }
            }
        }
        public void CreateTween(int index, bool randomize = false)
        {
            float from = triColorLerps[index];
            float target = from < 0.5f ? Calc.Random.Range(0.5f, 1f) : Calc.Random.Range(0, 0.5f);
            Tween tween = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut,
                    t => { triColorLerps[index] = Calc.LerpClamp(from, target, t.Eased); },
                    t => { CreateTween(index); });
            if (randomize)
            {
                tween.Randomize();
            }
        }
        private bool once = false;
        public void UpdateVertices()
        {
            if (once)
            {
                for (int i = 0; i < Points.Length; i++)
                {
                }
            }
            else
            {
                for (int i = 0; i < Points.Length; i++)
                {
                    Vector2 p = Points[i];
                    ColorShifter s = Shifters[i / 3];
                    Vertices[i].Position = new Vector3(p + new Vector2(BufferExtend, BufferExtend + breaths[p].Amount), 0);
                    Vertices[i].Color = Color.Lerp(s[0], s[1], triColorLerps[i / 3] + randomBrightness[i]);
                }
            }
            once = true;
        }
        public override void Update()
        {
            base.Update();
            UpdateVertices();
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Target, Position - Vector2.One * 16, Color.White);
            /*            Draw.SpriteBatch.End();
                        GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, Vertices.Length, indices, Vertices.Length / 3);
                        GameplayRenderer.Begin();*/
        }
    }
    [ConstantEntity("PuzzleIslandHelper/FormativeHouseRenderer")]
    internal class FormativeHouseRenderHelper : Entity
    {
        public FormativeHouseRenderHelper() : base()
        {
            Tag |= Tags.Global;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public void BeforeRender()
        {
            if (Scene is not Level level) return;
            List<Entity> list = level.Tracker.GetEntities<FormativeHouse>();
            if (list.Count > 0)
            {
                Draw.SpriteBatch.StandardBegin(Matrix.Identity);
                Effect effect = ShaderHelperIntegration.GetEffect("PuzzleIslandHelper/Shaders/formativeHouseShader");
                EffectParameterCollection parameters = effect.Parameters;
                Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
                Matrix matrix = Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
                matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
                matrix *= Matrix.CreateRotationX((float)Math.PI / 3f);
                parameters["World"]?.SetValue(matrix);
                parameters["Time"]?.SetValue(level.TimeActive);
                /*
                                Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                                Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                                EffectTechnique effectTechnique = effect.Techniques[0];*/
                foreach (FormativeHouse h in list)
                {
                    h.Target.SetAsTarget(true);
                    GFX.DrawIndexedVertices(Matrix.Identity, h.Vertices, h.Vertices.Length, h.indices, h.indices.Length / 3, effect);
                    /*                    foreach (EffectPass pass in effectTechnique.Passes)
                                        {
                                            pass.Apply();
                                            GFX.DrawIndexedVertices(Matrix.Identity, h.Vertices, h.Vertices.Length, h.indices, h.indices.Length / 3);
                                        }*/
                }
                /*                Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                                Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;*/
                Draw.SpriteBatch.End();
            }
        }

    }
}