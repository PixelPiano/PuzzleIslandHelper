using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [Tracked]
    public class CalidusButton : Actor
    {
        public Player Player;
        public bool HasGravity;
        public Vector2 Speed;
        public VertexPositionColor[] Vertices;
        public Vector2 VertexScale = Vector2.Zero;
        private Color[] colors = new Color[4] { Color.Cyan, Color.Cyan, Color.LightCyan, Color.Cyan };
        public readonly Vector2 ScaleTarget = new Vector2(20, 28);
        public Vector2[] Points = new Vector2[] { new(-0.5f, -1), new(0, -1), new(0, 0), new(0.5f, -1) };
        public int[] indices = new int[] { 0, 1, 2, 1, 3, 2 };
        public Calidus Calidus;
        public bool Loading;
        public Effect Shader;
        private float lineAmount;
        public CalidusButton(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
        public CalidusButton(Vector2 position) : base(position)
        {

            Collider = new Hitbox(8, 8);
            HasGravity = false;
            Vertices = new VertexPositionColor[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i] * Collider.Size, 0), Color.White);
            }
            Visible = false;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player = scene.GetPlayer();
            Calidus = scene.Tracker.GetEntity<Calidus>();
        }
        public override void Update()
        {
            base.Update();
            if (!Visible)
            {
                Position = Player.TopCenter + new Vector2(-Width / 2, -Height + 4);
            }
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position = new Vector3(TopCenter + Points[i] * VertexScale, 0);
                Vertices[i].Color = colors[i] * VertexAlpha;
            }
            MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
            Speed.Y = Calc.Approach(Speed.Y, HasGravity ? 240f : 0, 900f * Engine.DeltaTime);
        }
        public void OnCollideV(CollisionData hit)
        {
            if (HasGravity)
            {
                if (Math.Abs(Speed.Y) < 50f)
                {
                    Speed.Y = 0;
                }
                Speed.Y *= -0.6f;
            }
            else
            {
                Speed.Y = 0;
            }
        }
        public void Reveal()
        {
            Visible = true;
        }
        public void Hide()
        {
            Visible = false;
        }
        public float VertexAlpha;
        public void SetUpCalidus()
        {
            Calidus.Position = TopCenter - VertexScale.YComp() - Calidus.Collider.HalfSize;
            Calidus.Visible = Calidus.Active = true;
            Calidus.Update();
        }
        public void SpawnCalidus()
        {
            SetUpCalidus();
            Add(new Coroutine(Sequence()));
        }
        public IEnumerator LoadingRoutine()
        {
            while (Loading)
            {
                for (float i = 0; i < 1 && Loading; i += Engine.DeltaTime)
                {
                    lineAmount = i;
                    yield return null;
                }
                lineAmount = 1;
                for (float i = 0; i < 1 && Loading; i += Engine.DeltaTime)
                {
                    lineAmount = 1 - i;
                    yield return null;
                }
                lineAmount = 0;
            }
        }
        public IEnumerator Sequence()
        {
            bool high = true;
            for (int i = 1; i < 25; i++)
            {
                float wait = i / 10f * Engine.DeltaTime;
                if (high)
                {
                    Calidus.Alpha = i / 15f;
                }
                else
                {
                    Calidus.Alpha = 0;
                }
                yield return wait;
                high = !high;
            }
            Calidus.Alpha = 1;
            Loading = false;

            //yield return LoadingRoutine();
            yield return null;
            yield return 0.1f;
        }
        public void FadeOut()
        {
            Add(new Coroutine(FadeOutRoutine()));
        }
        public IEnumerator FadeOutRoutine()
        {
            bool high = false;
            for (int i = 1; i < 16; i++)
            {
                float wait = 0.1f - (i / 15f * 0.1f);
                if (high)
                {
                    VertexAlpha = Calc.Random.Range(0.6f, 0.9f);
                }
                else
                {
                    VertexAlpha = Calc.Random.Range(0.2f, 0.5f);
                }
                yield return wait;
                high = !high;
            }
            VertexAlpha = 0;
            yield return null;
        }
        public IEnumerator VertexFlickerRoutine()
        {
            bool high = true;
            for (int i = 1; i < 16; i++)
            {
                float wait = i / 15f * 0.1f;
                if (high)
                {
                    VertexAlpha = Calc.Random.Range(0.6f, 0.9f);
                }
                else
                {
                    VertexAlpha = Calc.Random.Range(0.2f, 0.5f);
                }
                yield return wait;
                high = !high;
            }
            VertexAlpha = 1;
            yield return null;
        }

        public void Press()
        {
            Loading = true;
            Reveal();
            Add(new Coroutine(VertexFlickerRoutine()));
            Tween.Set(this, Tween.TweenMode.Oneshot, 0.5f, Ease.Linear, t =>
            {
                VertexScale.X = Calc.LerpClamp(0, ScaleTarget.X, Ease.SineIn(t.Eased));
                VertexScale.Y = Calc.LerpClamp(0, ScaleTarget.Y, Ease.CubeIn(t.Eased * 2));
            }, t => SpawnCalidus());
        }
        public void Drop()
        {
            HasGravity = true;
        }
        public void ApplyParameters()
        {
            if (Shader != null)
            {
                Shader.ApplyCameraParams(Scene as Level);
                Shader.Parameters["LineOsc"]?.SetValue(lineAmount);
            }
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(Position, 8, 8, Color.Red);
            if (VertexScale != Vector2.Zero)
            {
                Draw.SpriteBatch.End();
                GFX.DrawIndexedVertices(SceneAs<Level>().Camera.Matrix, Vertices, 4, indices, 2);
                GameplayRenderer.Begin();
            }
        }
    }
}
