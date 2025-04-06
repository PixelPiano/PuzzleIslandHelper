using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
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
        public static readonly Vector2[] Points = [new(0), new(1, 0), new(0, 1), new(1)];

        public readonly int BoxSize;
        public VertexPositionColor[] Vertices;
        public List<ChipBox> Bins = [];
        public int[] Indices;
        public int Primitives;
        public float Alpha
        {
            get { return alpha * AlphaMult; }
            set { alpha = value; }
        }
        private float alpha = 1;
        public float AlphaMult = 1;
        public float Scale = 1;

        public bool UseShader = true;
        public bool Baked;
        public bool OnScreen;
        public Vector2 Speed;
        public Vector2 Scroll = Vector2.One;
        public Color Color = Color.White;


        public Chip(EntityData data, Vector2 offset) : this(data.Position + offset, Vector2.Zero, 1, 4, Color.Lime)
        {
        }
        public Chip(Vector2 position, Vector2 speed, float alpha, int boxSize, Color color) : base(position)
        {
            BoxSize = boxSize;
            Speed = speed;
            Tag |= Tags.TransitionUpdate;
        }
        public static void GenerateGrid(Vector2 offset, int cols, int rows, int boxSize, Color color, out ChipBox[,] grid, float alpha = 1, Func<int, int, bool> chanceFunction = null)
        {
            grid = new ChipBox[cols, rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (chanceFunction == null || chanceFunction(j, i))
                    {
                        Vector2 pos = new(j * (1 + boxSize), i * (1 + boxSize));
                        ChipBox box = new(pos + offset, boxSize, color, alpha);
                        grid[j, i] = box;
                    }
                    else
                    {
                        grid[j,i] = null;
                    }
                }
            }
        }
        public override void Update()
        {
            Position += Speed * Engine.DeltaTime;
            OnScreen = Collider != null && Collider.OnScreen(SceneAs<Level>());
            base.Update();
            if (Baked)
            {
                UpdateVertices();
            }
        }
        public void CreateCollider()
        {
            float right = int.MinValue, left = int.MaxValue, up = int.MaxValue, down = int.MinValue;
            foreach (ChipBox c in Bins)
            {
                right = Math.Max(right, c.Position.X);
                down = Math.Max(down, c.Position.Y);
                left = Math.Min(left, c.Position.X);
                up = Math.Min(up, c.Position.Y);
            }
            Collider = new Hitbox(right - left + BoxSize, down - up + BoxSize, left, up);
        }
        public IEnumerator Flicker(ChipBox bin, bool endState)
        {
            int loops = Calc.Random.Range(4, 8);
            int i = Calc.Random.Choose(0, 1);
            for (; i < loops; i++)
            {
                bin.Visible = i % 2 == 0;
                float mult = (float)i / loops;
                yield return Calc.Random.Range(mult * 0.1f, mult * 0.4f);
            }
            bin.Visible = endState;
        }
        public void UpdateVertices()
        {
            for (int i = 0; i < Bins.Count; i++)
            {
                ChipBox box = Bins[i];
                int index = i * 4;
                for (int v = 0; v < 4; v++)
                {
                    int vind = index + v;
                    Vertices[vind].Position = new(Position + box.Position + Points[v] * box.Scale + box.Info[v].Offset, 0);
                    Vertices[vind].Color = box.GetColor(v) * Alpha;
                }
            }
        }
        public void FlickerOn(Action onEnd = null)
        {
            List<Coroutine> routines = [];
            foreach (ChipBox bin in Bins)
            {
                bin.Visible = false;
                Coroutine r = new Coroutine(Flicker(bin, true));
                routines.Add(r);
                Add(r);
            }
            Add(new Coroutine(routine(onEnd, [..routines])));
        }
        public void FlickerOff(Action onEnd = null)
        {
            List<Coroutine> routines = [];
            foreach (ChipBox bin in Bins)
            {
                bin.Visible = true;
                Coroutine r = new Coroutine(Flicker(bin, false));
                routines.Add(r);
                Add(r);
            }
            Add(new Coroutine(routine(onEnd, [..routines])));
        }
        private IEnumerator routine(Action onEnd = null, params Coroutine[] routines)
        {
            foreach (Coroutine r in routines)
            {
                while (!r.Finished) yield return null;
            }
            onEnd?.Invoke();
        }
        public void Bake()
        {
            if (Bins == null || Bins.Count == 0 || Baked) return;
            List<VertexPositionColor> vertices = [];
            List<int> indices = [];
            foreach (ChipBox info in Bins)
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
                    vertices.Add(new(new(Position + info.Position + Points[i] * info.Scale, 0), info.Color));
                }
                Primitives += 2;
            }
            Vertices = [.. vertices];
            Indices = [.. indices];
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
            Effect obj = UseShader ? ShaderHelperIntegration.TryGetEffect("PuzzleIslandHelper/Shaders/chipShader") ?? GFX.FxPrimitive : GFX.FxPrimitive;       
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
            Effect obj = effect ?? GFX.FxPrimitive;
            BlendState blendState2 = ((blendState != null) ? blendState : BlendState.AlphaBlend);
            Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            matrix *= Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState2;
            obj.Parameters["World"]?.SetValue(matrix);
            foreach (EffectPass pass in obj.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexCount, indices, 0, primitiveCount);
            }
        }

        [Tracked]
        public class ChipBox : Component
        {
            public VertexInfo[] Info = new VertexInfo[4];
            public float Alpha
            { 
                get { return (alpha + flickerOffset) * AlphaMult; }
                set { alpha = value; }
            }
            public float AlphaMult = 1;
            private float alpha = 1;
            public float Scale;
            private float flickerOffset;
            private bool cancelRoutine;
            public Vector2 Position;
            public Color Color;
            public ChipBox(Vector2 offset, float scale, Color color, float alpha) : base(true, true)
            {
                Position = offset;
                Scale = scale;
                Color = color;
                Alpha = alpha;
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                if (Entity != null)
                {
                    Draw.Point(Entity.Position + Position, Color.Orange);
                }
            }
            public Color GetColor(int index) =>  Visible ? Info[index].GetColor(Color) * Alpha : Color.Transparent;
            public void FadeToColor(Color color, float lerp, float time, float delay = 0)
            {
                Entity?.Add(new Coroutine(FadeToColorRoutine(color, lerp, time, delay)));
            }
            public void CancelEndlessFlicker(float? alpha = null)
            {
                if (alpha.HasValue)
                {
                    Alpha = alpha.Value;
                }
                cancelRoutine = true;
            }
            public IEnumerator EndlessFlicker(float minOffset, float maxOffset, float interval, bool first = true)
            {
                if (first)
                {
                    cancelRoutine = false;
                    yield return Calc.Random.Range(0, interval);
                }
                flickerOffset = Calc.Random.Range(minOffset, maxOffset);
                for (float i = 0; i < interval && !cancelRoutine; i += Engine.DeltaTime)
                {
                    yield return null;
                }
                if (cancelRoutine)
                {
                    cancelRoutine = false;
                    yield break;
                }
                yield return EndlessFlicker(minOffset, maxOffset, interval, false);
            }
            public IEnumerator FadeToColorRoutine(Color color, float lerp, float time, float delay = 0)
            {
                yield return delay;
                Color from = Color;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    Color = Color.Lerp(from, color, Ease.SineOut(i * lerp));
                    yield return null;
                }
            }

        }
    }
    [TrackedAs(typeof(Chip))]
    public class ChipPhrase : Chip
    {
        public static string[] Lookup = new string[256];
        public void ParsePhrase(string phrase, int size, Color color, float alpha, out List<ChipBox> bins, out List<Word> words)
        {
            bins = [];
            words = [];
            if (string.IsNullOrEmpty(phrase)) return;
            int xSpacing = 1;
            int ySpacing = 1;
            float x = 0;
            string[] wrds = phrase.Trim().ToLower().Split(' ');
            foreach (string wrd in wrds)
            {
                Word w = new Word(x, wrd, size, xSpacing, ySpacing, color, alpha);
                foreach (var b in w.Boxes)
                {
                    bins.Add(b);
                }
                words.Add(w);
                x += w.Width + xSpacing + size;
            }
        }
        public static string GetBinaryChar(char c)
        {
            return char.IsWhiteSpace(c) ? "/" : Lookup[c][3..];
        }
        public static string[] GetBinaryArray(string input)
        {
            string output = "";
            foreach (char c in input)
            {
                output += GetBinaryChar(c) + " ";
            }
            return output.Trim().Split(' ');
        }
        public static string GetBinaryPhrase(string phrase)
        {
            string output = "";
            foreach (char c in phrase)
            {
                output += GetBinaryChar(c) + " ";
            }
            return output.Trim();
        }
        public struct Word
        {
            public List<Char> Chars = [];
            public List<ChipBox> Boxes = [];
            public string Text;
            public float Width;
            public Word(float xOffset, string word, int boxSize, int xSpacing, int ySpacing, Color color, float alpha = 1)
            {
                Text = word;
                float x = xOffset;
                foreach (char ch in word)
                {
                    string binary = GetBinaryChar(ch);
                    List<ChipBox> bxs = [];
                    if (!string.IsNullOrEmpty(binary) && binary != "/")
                    {
                        Char letter = new(x, ch, binary, boxSize, ySpacing, color, out bxs, alpha);
                        Chars.Add(letter);
                        foreach (var b in bxs)
                        {
                            Boxes.Add(b);
                        }
                    }
                    x += boxSize + xSpacing;
                }
                Width = x - xOffset;
            }
        }
        public struct Char
        {
            public List<ChipBox> Boxes = [];
            public char Key;
            public string Binary;
            public Char(float x, char c, string binary, int boxSize, int ySpacing, Color color, out List<ChipBox> boxes, float alpha = 1)
            {
                Key = c;
                Binary = binary;
                float y = 0;
                for (int i = 0; i < binary.Length; i++)
                {
                    if (binary[i] != '0')
                    {
                        ChipBox box = new ChipBox(new Vector2(x, y), boxSize, color, alpha);
                        Boxes.Add(box);
                    }
                    y += boxSize + ySpacing;
                }
                boxes = Boxes;
            }
        }

        public string Phrase;
        public List<Word> ChipWords = [];
        public string[] Words;

        public ChipPhrase(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("stringValue"), Vector2.Zero, 1, 4, Color.Lime)
        {
        }
        public ChipPhrase(Vector2 position, string phrase, Vector2 speed, float alpha, int boxSize, Color color) : base(position, speed, alpha, boxSize, color)
        {
            Phrase = phrase.Trim().ToLower();
            Words = Phrase.Split(' ');
            ParsePhrase(Phrase, boxSize, color, alpha, out Bins, out ChipWords);
            CreateCollider();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add([.. Bins]);
            Bake();
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
            Engine.Commands.Log(GetBinaryPhrase(value));
        }
    }
}
