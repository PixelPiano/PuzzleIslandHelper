using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class GlitchSquare : Component
    {
        public readonly int[] Indices = { 0, 1, 4, 0, 2, 4, 2, 3, 4, 1, 3, 4 };
        public readonly Vector2[] Points = { new(-1, -1), new(1, -1), new(-1, 1), new(1, 1), new(0) };
        public int Size;
        public bool ScaleWidth;
        public VertexPositionColor[] Vertices;
        public float Alpha = 1;
        public Vector2 Position;
        public Vector2 RenderPosition
        {
            get
            {
                return ((Entity == null) ? Vector2.Zero : Entity.Position) + Position;
            }
            set
            {
                Position = value - ((Entity == null) ? Vector2.Zero : Entity.Position);
            }
        }
        public Vector2 Scale;
        public bool InFront;
        private float waitTime = 0.7f;
        private float scaleTime;
        private float startDelay;
        public Color color = Color.Purple;
        private bool addedRoutine;
        private float colorLerp;
        private bool Inside;
        private float intervalOffset;
        private float width, height;
        public enum States
        {
            Growing,
            Waiting,
            Shrinking,
            Randomizing
        }
        public States State = States.Growing;
        public GlitchSquare(float widthRange, float heightRange, bool visible = true, float startDelayRange = 0.1f) : base(true, visible)
        {
            width = widthRange;
            height = heightRange;
            startDelay = Calc.Random.Range(0, startDelayRange);
            intervalOffset = Calc.Random.Range(0f, 0.5f);
            Randomize();
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            AddMainRoutine();
        }

        private void AddMainRoutine()
        {
            if (Entity is not null)
            {
                Entity.Add(new Coroutine(Routine()));
                addedRoutine = true;
            }
        }
        private IEnumerator Flicker()
        {
            bool abort = false;
            bool subtract = false;
            float start;
            while (!abort)
            {
                start = Calc.Clamp(Alpha, 0, 1);
                float time = Calc.Random.Range(0.05f, 0.2f);
                float to = Calc.Random.Range(0.1f, 0.5f) * (subtract ? -1 : 1);
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    if (State != States.Waiting)
                    {
                        abort = true;
                        break;
                    }
                    Alpha = Calc.LerpClamp(start, 0.5f + to, i);
                    yield return null;
                }
                subtract = !subtract;
                yield return null;
            }
        }
        public void Randomize()
        {
            Size = Calc.Random.Range(4, 12);
            ScaleWidth = Calc.Random.Chance(0.5f);
            InFront = Calc.Random.Chance(0.5f);
            scaleTime = Calc.Random.Range(0.2f, 0.5f);
            float width = this.width * 1.3f;
            float height = this.height * 1.5f;
            Position = PianoUtils.Random(-width, width, -height, height).Round();
            Vertices = new VertexPositionColor[5];
            Vector2 scaleOffset = (Scale * Size / 2);
            for (int i = 0; i < 5; i++)
            {
                Vertices[i] = PianoUtils.Create(Vector2.Zero, (Inside ? i != 4 : i == 4) ? Color.Transparent : color);
            }
            UpdateVertices();
        }
        private IEnumerator Routine()
        {
            yield return startDelay;
            while (true)
            {
                State = States.Growing;
                for (float i = 0; i < 1; i += Engine.DeltaTime / scaleTime)
                {
                    float amount = (float)Math.Round(Ease.QuintIn(i), 2);
                    Scale = new Vector2(ScaleWidth ? 1 + amount / 2 : amount, ScaleWidth ? amount : 1 + amount / 2);
                    colorLerp = Calc.Clamp(amount * 2, 0, 1);
                    yield return null;
                }
                Scale = Vector2.One;
                State = States.Waiting;
                AddRoutine(Flicker());
                yield return Calc.Random.Range(0.2f, 1);
                State = States.Shrinking;
                for (float i = 0; i < 1; i += Engine.DeltaTime / scaleTime)
                {
                    float amount = (float)Math.Round(Ease.QuintIn(i), 2);
                    Scale = new Vector2(ScaleWidth ? 1 - amount : 1 + amount / 2, ScaleWidth ? 1 + amount / 2 : 1 - amount);
                    colorLerp = Calc.Clamp(1 - (amount * 2), 0, 1);
                    yield return null;
                }
                Scale = Vector2.Zero;
                State = States.Randomizing;
                yield return waitTime;
                Randomize();

            }
        }
        private void AddRoutine(IEnumerator method)
        {
            if (Entity is not null)
            {
                Entity.Add(new Coroutine(method));
            }
        }
        public void UpdateVertices()
        {
            Vector2 scaleOffset = (Scale * Size / 2);

            for (int i = 0; i < 5; i++)
            {
                Vertices[i].Position = new Vector3((Points[i] * scaleOffset) + RenderPosition, 0);
                Vertices[i].Color = (Inside ? i != 4 : i == 4) ? Color.Transparent : Color.Lerp(Color.White, color, colorLerp) * Alpha;
            }
        }

        public override void Update()
        {
            base.Update();
            UpdateVertices();
            if (!addedRoutine)
            {
                AddMainRoutine();
            }
            if (Scene.OnInterval((15 / 60f) + intervalOffset))
            {
                Inside = !Inside;
            }
        }
        public void DrawSquare(Level level)
        {
            GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, 5, Indices, 4);
        }
    }
}
