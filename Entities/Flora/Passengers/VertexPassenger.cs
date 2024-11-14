using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{

    [Tracked]
    public abstract class VertexPassenger : Passenger
    {


        public List<Vector2> Points = new();
        public List<ColorShifter> Shifters = new();
        public List<float> OffsetMults = new();
        public List<int> Indices = new();
        public List<Vector2> WiggleMults = new();
        public float MinWiggleTime = 0.8f;
        public float MaxWiggleTime = 2;
        public Vector2 Scale;
        public int[] indices;
        public Vector2[] OgOffsets;
        public Vector2[] Offsets;
        public VertexPositionColor[] Vertices;
        private Tween[] tweens;
        public Vector2 ScaleOffset;
        private Vector2 BreathOffset;
        public Vector2 BreathDirection;
        public float BreathDuration;
        public Vector2 ScaleApproach = Vector2.One;
        public bool Baked;
        public bool IsInView;
        public bool CanJump => HasGravity && onGround && CannotJumpTimer <= 0;
        public int Primitives;
        public float Alpha = 1;
        public Color Color2;
        public float ColorMixLerp;


        public Facings Facing = Facings.Left;
        public VertexPassenger(Vector2 position, float width, float height, string cutscene, Vector2 scale) : base(position, width, height, cutscene)
        {
            Scale = scale;
            Position.Y -= (Height - 16);
        }
        public VertexPassenger(Vector2 position, float width, float height, string cutscene, Vector2 scale, Vector2 breathDirection, float breathDuration) : this(position, width, height, cutscene, scale)
        {
            BreathDirection = breathDirection;
            BreathDuration = breathDuration;
        }
        public void Face(Entity entity)
        {
            if (entity.CenterX > CenterX)
            {
                Facing = Facings.Right;
            }
            else
            {
                Facing = Facings.Left;
            }
        }
        public IEnumerator WalkX(float x, float speedMult = 1, bool walkBackwards = false)
        {
            yield return new SwapImmediately(WalkToX(X + x, speedMult, walkBackwards));
        }
        public IEnumerator WalkToX(float x, float speedMult = 1, bool walkBackwards = false)
        {
            x = (int)Math.Round(x);
            if (Position.X == x) yield break;
            int dir = Math.Sign(x - Position.X);
            if (walkBackwards) dir *= -1;
            Facing = (Facings)dir;
            while (Math.Abs(Position.X - x) > 2)
            {
                MoveTowardsX(x, 90f * speedMult * Engine.DeltaTime);
                yield return null;
            }
            Position.X = x;
        }
        private IEnumerator breathRoutine()
        {
            int skip = Calc.Random.Range(0, 30);

            while (true)
            {
                while (BreathDuration <= 0) yield return null;
                for (float i = 0; i < 1; i += Engine.DeltaTime / BreathDuration / 2f)
                {
                    BreathOffset = Vector2.Lerp(Vector2.Zero, BreathDirection, Ease.QuadInOut(i));
                    if (skip > 0) skip--;
                    else yield return null;

                }
                while (BreathDuration <= 0) yield return null;
                BreathOffset = BreathDirection;
                for (float i = 0; i < 1; i += Engine.DeltaTime / BreathDuration / 2f)
                {
                    BreathOffset = Vector2.Lerp(BreathDirection, Vector2.Zero, Ease.QuadInOut(i));
                    if (skip > 0) skip--;
                    else yield return null;
                }
                BreathOffset = Vector2.Zero;
            }
        }
        public void AddQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float mult, Vector2 wiggleMult, ColorShifter shifter = null)
        {
            AddPoints(new Vector2[] { a, b, c }, mult, wiggleMult, shifter);
            AddPoints(new Vector2[] { b, c, d }, mult, wiggleMult, null);
        }
        public void AddPoints(Vector2[] points, float mult, Vector2 wiggleMult, ColorShifter shifter)
        {
            foreach (Vector2 p in points)
            {
                AddPoint(p, mult, wiggleMult, shifter);
            }
        }
        public void AddQuad(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, float mult, Vector2 wiggleMult, ColorShifter shifter = null)
        {
            AddQuad(new(x1, y1), new(x2, y2), new(x3, y3), new(x4, y4), mult, wiggleMult, shifter);
        }
        public void AddTriangle(Vector2 a, Vector2 b, Vector2 c, float multiplier, Vector2 wiggleMult, ColorShifter shifter = null, bool mirrorX = false)
        {
            AddPoints(new Vector2[] { a, b, c }, multiplier, wiggleMult, shifter);
        }
        public void AddTriangle(float x1, float y1, float x2, float y2, float x3, float y3, float mult, Vector2 wiggleMult, ColorShifter shifter = null, bool mirrorX = false)
        {
            AddTriangle(new(x1, y1), new(x2, y2), new(x3, y3), mult, wiggleMult, shifter);
            if (mirrorX)
            {
                AddTriangle(new(Width - x1, y1), new(Width - x2, y2), new(Width - x3, y3), mult, wiggleMult, shifter);
            }
        }

        public void AddTriangle(int indiceA, int indiceB, int indiceC)
        {
            Indices.Add(indiceA);
            Indices.Add(indiceB);
            Indices.Add(indiceC);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(breathRoutine()));
        }
        public override void Jump()
        {
            base.Jump();
            ScaleApproach.X = 0.8f;
            ScaleApproach.Y = 0.9f;
        }
        public override void Update()
        {
            IsInView = InView();
            if (ScaleApproach.X != 1)
            {
                ScaleApproach.X = Calc.Approach(ScaleApproach.X, 1, Engine.DeltaTime);
            }
            if (ScaleApproach.Y != 1)
            {
                ScaleApproach.Y = Calc.Approach(ScaleApproach.Y, 1, Engine.DeltaTime);
            }
            if (IsInView)
            {
                base.Update();
                UpdateVertices();
            }
        }

        public void UpdateVertices()
        {
            if (!Baked) return;

            for (int i = 0; i < Points.Count; i++)
            {
                Vector2 point = Points[i] - Collider.HalfSize;
                point.X *= -(int)Facing;
                Vector2 position = Center + (point * Scale * ScaleApproach);
                Vector2 wiggleOffset = OgOffsets[i] * tweens[i].Eased * WiggleMults[i] * OffsetMults[i];
                Vector2 breath = BreathOffset;
                Vertices[i].Position = new Vector3(position + (wiggleOffset + Offsets[i]), 0);
                if (Shifters[i] != null)
                {
                    Vertices[i].Color = Color.Lerp(Shifters[i][i % Shifters[i].Colors.Length], Color2, ColorMixLerp) * Alpha;
                }
            }
        }
        public void AddPoint(Vector2 p, float mult, Vector2 wiggleMult, ColorShifter shifter = null)
        {
            if (Points.Contains(p))
            {
                int index = Points.IndexOf(p);

                OffsetMults[index] = Calc.Max(OffsetMults[index], mult);
                Indices.Add(index);
            }
            else
            {
                Indices.Add(Points.Count);
                Points.Add(p);
                if (shifter == null)
                {
                    if (Shifters.Count > 0)
                    {
                        Shifters.Add(Shifters.Last());
                    }
                    else
                    {
                        Shifters.Add(null);
                    }
                }
                else
                {
                    Shifters.Add(shifter);
                }
                OffsetMults.Add(mult);
                WiggleMults.Add(wiggleMult);
            }
        }
        public void Bake()
        {
            Vector2[] p = new Vector2[Points.Count];
            indices = new int[Points.Count];
            tweens = new Tween[Points.Count];
            OgOffsets = new Vector2[Points.Count];
            Offsets = new Vector2[Points.Count];
            for (int i = 0; i < Points.Count; i++)
            {
                p[i] = Points[i];
                Offsets[i] = Vector2.Zero;
                OgOffsets[i] = Vector2.UnitY;// * Calc.Random.Choose(-1, 1);

                Tween t = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.QuadInOut, Calc.Random.Range(MinWiggleTime, MaxWiggleTime), true);
                t.Randomize();
                Add(t);
                tweens[i] = t;
            }
            for (int i = 0; i < Shifters.Count; i++)
            {
                if (Shifters[i] != null && !Shifters[i].HasBeenAdded)
                {
                    Add(Shifters[i]);
                    Shifters[i].HasBeenAdded = true;
                }
            }
            Vertices = p.CreateVertices(Scale, out indices, Color.Lime);
            indices = Indices.ToArray();
            Baked = true;
        }

        public bool InView()
        {
            Camera camera = (Scene as Level).Camera;
            float xPad = Width;
            float yPad = Height;
            if (X > camera.X - xPad && Y > camera.Y - yPad && X < camera.X + 320f + xPad)
            {
                return Y < camera.Y + 180f + yPad;
            }

            return false;
        }
        public override void Render()
        {
            base.Render();
            if (!Baked || Scene is not Level level || !IsInView) return;
            Draw.SpriteBatch.End();
            DrawVertices(level);
            GameplayRenderer.Begin();
        }
        public virtual void DrawVertices(Level level)
        {
            GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, Vertices.Length, indices, indices.Length / 3);
        }
        public void DrawLines<T>(Matrix matrix, T[] vertices, int vertexCount, int[] indices, int primitiveCount, Effect effect = null, BlendState blendState = null) where T : struct, IVertexType
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
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, vertexCount, indices, 0, primitiveCount);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Vertices = null;
        }
        public IEnumerator PlayerStepBack(Player player, Vector2 screenSpaceFocusPoint, float zoom, float duration)
        {
            Coroutine zoomRoutine = new Coroutine(SceneAs<Level>().ZoomTo(screenSpaceFocusPoint, zoom, duration));
            Add(zoomRoutine);
            yield return new SwapImmediately(PlayerStepBack(player));
            while (!zoomRoutine.Finished)
            {
                yield return null;
            }
        }
        public void FacePlayer(Player player)
        {
            if (player.CenterX > CenterX)
            {
                Facing = Facings.Right;
            }
            else
            {
                Facing = Facings.Left;
            }
        }
        public IEnumerator PlayerStepBack(Player player, Facings facing)
        {
            float xTarget = CenterX + (int)facing * (16 + Width / 2);
            yield return new SwapImmediately(player.DummyWalkTo(xTarget));
            player.Facing = (Facings)(-(int)facing);
        }
        public IEnumerator PlayerStepBack(Player player)
        {
            yield return new SwapImmediately(PlayerStepBack(player, Facing));
        }
        public static string GetPositionString(VertexPassenger p)
        {
            string output = "";

            foreach (VertexPositionColor v in p.Vertices.OrderByDescending(item => item.Position.Y))
            {
                output += "{" + v.Position.X + "," + v.Position.Y + "} " + '\n';
            }
            return output;
        }
    }
}
