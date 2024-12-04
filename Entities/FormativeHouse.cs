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
        private List<VertexBreath> breaths = [];
        private class triData
        {
            public ColorShifter Shifter;
            public int BrightShift;
            public float ColorLerp;
            public float GetAdditionalBrightness(int index)
            {
                if (index == BrightShift)
                {
                    return 0.3f;
                }
                return 0;
            }
        }
        private List<triData> triList = new();
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
        public List<Color> Colors = new();
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            Mesh<VertexPositionColor> m = CreateTriWallMesh(Vector3.Zero, Width, Height, 16, 16, 0, Color.Lime, out TriWall wall);
            m.Bake();
            Vertices = m.Vertices;
            this.indices = m.Indices;
            Points = Vertices.Select(item => item.Position.XY()).ToArray();
            /*            int xOffset = 8;
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
                        }*/
            int c = 0;
            List<Color> colors = [Color.Green, Color.Lime, Color.DarkGreen, Color.Turquoise, Color.DarkOliveGreen, Color.Red];
            for (int i = 0; i < Points.Length; i += 3)
            {
                triData data = new();
                triList.Add(data);
                int index = c;
                CreateTween(index, true);
                data.BrightShift = Calc.Random.Range(0, 3);
                colors.Shuffle();
                Add(data.Shifter = new ColorShifter([.. colors])
                {
                    Rate = Calc.Random.Range(0.3f, 0.7f)
                });
                c++;
                for (int j = 0; j < 3; j++)
                {
                    VertexBreath breath = new VertexBreath(Calc.Random.Range(4, 8f), Calc.Random.Range(1f, 8));
                    breaths.Add(breath);
                    Add(breath);
                }
            }
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Color = colors.Random();
            }
            UpdateVertices();
        }
        public override void Update()
        {
            base.Update();
            UpdateVertices();
        }
        public float mult = 1;
        public void CreateTween(int index, bool randomize = false)
        {
            float from = triList[index].ColorLerp;
            float target = from < 0.5f ? Calc.Random.Range(0.5f, 1f) : Calc.Random.Range(0, 0.5f);
            Tween tween = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut,
                    t => { triList[index].ColorLerp = Calc.LerpClamp(from, target, t.Eased); },
                    t => { CreateTween(index); });
            if (randomize)
            {
                tween.Randomize();
            }
        }
        public void UpdateVertices()
        {
            for (int i = 0; i < Points.Length; i++)
            {
                triData data = triList[i / 3];
                Vertices[i].Position = new Vector3(Points[i] + new Vector2(BufferExtend, BufferExtend + breaths[i].Amount * mult), 0);
                ColorShifter s = data.Shifter;
                Vertices[i].Color = Color.Lerp(s[0], s[1], data.ColorLerp + data.GetAdditionalBrightness(i % 3));

            }
        }
        public void UpdateVerticePosition()
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 p = Points[i];
                Vertices[i].Position = new Vector3(p + new Vector2(BufferExtend, BufferExtend + breaths[i].Amount * mult), 0);

            }
        }
        public void UpdateVerticeColor()
        {
            /*            for (int i = 0; i < Points.Length; i++)
                        {
                            triData data = triList[i / 3];
                            ColorShifter s = data.Shifter;
                            Vertices[i].Color = Color.Lerp(s[0], s[1], data.ColorLerp + data.GetAdditionalBrightness(i % 3));
                        }*/
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Target, Position - Vector2.One * 16, Color.White);
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
                foreach (FormativeHouse h in list)
                {
                    h.Target.SetAsTarget(true);
                    GFX.DrawIndexedVertices(Matrix.Identity, h.Vertices, h.Vertices.Length, h.indices, h.indices.Length / 3, effect);
                }
                Draw.SpriteBatch.End();
            }
        }

    }
}