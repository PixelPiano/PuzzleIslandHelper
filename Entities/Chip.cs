using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    //[CustomEntity("PuzzleIslandHelper/WipEntity")]
    public class Chip : Entity
    {
        public struct VertexInfo
        {
            public float ZAmount;
            public Vector2 Offset;
            public Color GetColor(Color baseColor)
            {
                return ZAmount switch
                {
                    > 0 => Color.Lerp(baseColor, Color.White, ZAmount),
                    < 0 => Color.Lerp(baseColor, Color.Black, ZAmount),
                    _ => baseColor
                };
            }
        }
        public class BinaryChar
        {
            public Vector2 Offset;
            public float Scale;
            public Color Color;
            public float Alpha = 1;
            public VertexInfo[] Info = new VertexInfo[4];
            public bool Off;
            public BinaryChar(Vector2 offset, float scale, Color color, float alpha)
            {
                Offset = offset;
                Scale = scale;
                Color = color;
                Alpha = alpha;
            }
            public Color GetColor(int index)
            {
                if (Off) return Color.Transparent;
                return Info[index].GetColor(Color) * Alpha;
            }
            public static string GetBinaryChar(char c)
            {
                if (char.IsWhiteSpace(c)) return "/";
                return Lookup[c].Substring(3);
            }
            public static string[] GetBinaryArray(string word)
            {
                string output = "";
                foreach (char c in word)
                {
                    output += GetBinaryChar(c) + " ";
                }
                output.Trim();

                return output.Split(' ');
            }
            public static string GetBinaryWord(string word)
            {
                string output = "";
                foreach (char c in word)
                {
                    output += GetBinaryChar(c) + " ";
                }
                output.Trim();
                return output;
            }
            public static List<BinaryChar> ParseWord(string word, int size, Color color, float alpha)
            {
                if (string.IsNullOrEmpty(word)) return null;
                List<BinaryChar> bits = new();

                float x = 0;
                float xSpacing = 1;
                float ySpacing = 1;
                string[] array = GetBinaryArray(word);
                foreach (string s in array)
                {
                    if (!(string.IsNullOrEmpty(s) || s == "/"))
                    {
                        float y = 0;
                        for (int i = 0; i < s.Length; i++)
                        {
                            if (s[i] != '0')
                            {
                                bits.Add(new BinaryChar(new Vector2(x, y), size, color, alpha));
                            }
                            y += size + ySpacing;
                        }
                    }
                    x += size + xSpacing;
                }
                return bits;
            }
        }

        private static Vector2[] points = [new(0), new(1, 0), new(0, 1), new(1)];
        public static string[] Lookup = new string[256];

        public VertexPositionColor[] Vertices;
        public float Alpha = 1;
        public int[] Indices;
        public string Word;
        public int Primitives;
        public bool Baked;
        public List<BinaryChar> Bins = new();
        public Vector2 Speed;
        public Color Color;
        public float Scale = 1;
        public static Effect Shader;
        public Vector2 Scroll = Vector2.One;
        public Chip(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("stringValue"), Vector2.Zero, 1, 4, Color.Lime)
        {
        }

        public Chip(Vector2 position, string phrase, Vector2 speed, float alpha, int boxSize, Color color) : base(position)
        {
            Speed = speed;
            Word = phrase;
            Bins = BinaryChar.ParseWord(Word.ToLower(), boxSize, color, alpha);
            float right = int.MinValue, left = int.MaxValue, up = int.MaxValue, down = int.MinValue;
            foreach (BinaryChar c in Bins)
            {
                right = Math.Max(right, c.Offset.X);
                down = Math.Max(down, c.Offset.Y);
                left = Math.Min(left, c.Offset.X);
                up = Math.Min(up, c.Offset.Y);
            }
            Collider = new Hitbox(right - left + 4, down - up + 4, left, up);
            Bake();
            Tag |= Tags.TransitionUpdate;
        }
        public IEnumerator Flicker(BinaryChar bin, bool endState)
        {
            int loops = Calc.Random.Range(4, 8);
            int i = Calc.Random.Choose(0, 1);
            for (; i < loops; i++)
            {
                bin.Off = i % 2 == 0;
                float mult = (float)i / loops;
                yield return Calc.Random.Range(mult * 0.1f, mult * 0.4f);
            }
            bin.Off = !endState;
        }
        public bool OnScreen;
        public override void Update()
        {
            Position += Speed * Engine.DeltaTime;
            OnScreen = Collider.OnScreen(SceneAs<Level>());
            base.Update();
            UpdateVertices();
        }
        public void UpdateVertices()
        {
            if (!Baked || Scene is not Level level) return;
            Vector2 vector = level.Camera.Position.Floor();
            Vector2 position = Position + (Position - vector * Scroll).Floor();
            Vector2 center = Center;
            for (int i = 0; i < Bins.Count; i++)
            {
                BinaryChar info = Bins[i];
                int index = i * 4;
                for (int v = 0; v < 4; v++)
                {
                    int vind = index + v;
                    Vector2 p = Vertices[vind].Position.XY();

                    Vertices[vind].Position = new(Position + info.Offset + points[v] * info.Scale + info.Info[v].Offset, 0);
                    Vertices[vind].Color = info.GetColor(v) * Alpha;
                }
            }
        }
        private Tween tween;
        public void FlickerOn(Action onEnd = null)
        {
            List<Coroutine> routines = [];
            foreach (BinaryChar bin in Bins)
            {
                bin.Off = true;
                Coroutine r = new Coroutine(Flicker(bin, true));
                routines.Add(r);
                Add(r);
            }
            Add(new Coroutine(routine(routines, onEnd)));
        }
        private IEnumerator routine(List<Coroutine> routines, Action onEnd)
        {
            foreach (Coroutine r in routines)
            {
                while (!r.Finished) yield return null;
            }
            onEnd?.Invoke();
        }
        public void FlickerOff(Action onEnd = null)
        {
            List<Coroutine> routines = [];
            foreach (BinaryChar bin in Bins)
            {
                bin.Off = false;
                Coroutine r = new Coroutine(Flicker(bin, false));
                routines.Add(r);
                Add(r);
            }
            Add(new Coroutine(routine(routines, onEnd)));
        }
        public void FadeTo(float from, float to, float time, Ease.Easer ease, Action onEnd = null)
        {

            tween?.Stop();
            tween = Tween.Set(this, Tween.TweenMode.Oneshot, time, ease, t => Alpha = Calc.LerpClamp(from, to, t.Eased), delegate { onEnd?.Invoke(); });
        }
        public void Bake()
        {
            if (Bins.Count == 0) return;
            List<VertexPositionColor> vertices = new();
            List<int> indices = new();
            foreach (BinaryChar info in Bins)
            {
                int c = vertices.Count;
                indices.Add(c + 0);
                indices.Add(c + 1);
                indices.Add(c + 2);
                indices.Add(c + 2);
                indices.Add(c + 1);
                indices.Add(c + 3);
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(new(new(Position + info.Offset + points[i] * info.Scale, 0), info.Color));
                }
                Primitives += 2;
            }
            Vertices = vertices.ToArray();
            Indices = indices.ToArray();
            Baked = true;
        }
        public override void Render()
        {
            base.Render();
            if (!Baked || !OnScreen || Scene is not Level level) return;
            Draw.SpriteBatch.End();
            DrawVertices(level);
            GameplayRenderer.Begin();
        }
        public void DrawVertices(Level level)
        {
            Effect obj = (ShaderHelperIntegration.TryGetEffect("PuzzleIslandHelper/Shaders/chipShader") ?? GFX.FxPrimitive);
            BlendState blendState2 = BlendState.AlphaBlend;
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState2;
            obj.ApplyParameters(level, level.Camera.Matrix);
            foreach (EffectPass pass in obj.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Primitives);
            }
        }
        public static void DrawIndexedVertices<T>(Matrix matrix, T[] vertices, int vertexCount, int[] indices, int primitiveCount, Effect effect = null, BlendState blendState = null) where T : struct, IVertexType
        {
            Effect obj = ((effect != null) ? effect : GFX.FxPrimitive);
            BlendState blendState2 = ((blendState != null) ? blendState : BlendState.AlphaBlend);
            Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            matrix *= Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState2;
            obj.Parameters["World"].SetValue(matrix);
            foreach (EffectPass pass in obj.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexCount, indices, 0, primitiveCount);
            }
        }
        [OnInitialize]
        public static void Initialize()
        {
            //Thank you Rik on Stack Overflow
            //One of the not rude people there
            //https://stackoverflow.com/questions/26887113/get-8-digit-binary-from-char-in-c-sharp-win-form
            Lookup = new string[256];
            for (int i = 0; i < 256; i++)
            {
                Lookup[i] = Convert.ToString(i, 2).PadLeft(8, '0');
            }
        }
        [Command("get_binary", "get binary data from string")]
        public static void GetBin(string value)
        {
            if (Engine.Scene is not Level level) return;
            Engine.Commands.Log(BinaryChar.GetBinaryWord(value));
        }
    }
}
