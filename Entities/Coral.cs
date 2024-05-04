using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/Coral")]
    [Tracked]
    public class Coral : Entity
    {
        public Color BaseColor;
        public Color SecondaryColor;
        public Segment Base;
        private float baseAngle;
        public List<SegmentList> Limbs = new();
        public int MaxSegments = 50;
        public int MaxLimbs = 7;
        public VirtualRenderTarget Target;
        private bool initialized;
        private bool onScreen;
        public enum ColorModes
        {
            Gradient,
            SolidStem
        }
        public ColorModes ColorMode;
        public Coral(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Calc.PushRandom((int)(Position.X + Position.Y));
            Depth = 1;
            MaxLimbs = data.Int("maxLimbs", 7);
            MaxSegments = data.Int("maxSegments", 30);
            BaseColor = data.HexColor("color");
            SecondaryColor = data.HexColor("color2", Color.Blue);
            baseAngle = data.Float("angle");
            float length = data.Int("segmentLength",2);
            Position +=  Vector2.One * 4 + Calc.AngleToVector(-baseAngle.ToRad(), 4);
            Base = new Segment(Position, BaseColor, baseAngle);
            for (int i = 0; i < MaxLimbs; i++)
            {
                float progress = (float)i / MaxLimbs;
                Color b = ColorMode switch
                {
                    ColorModes.Gradient => Color.Lerp(BaseColor, Color.Lerp(BaseColor, Color.Black, 0.3f), 1 - progress),
                    ColorModes.SolidStem => Color.Lerp(BaseColor, SecondaryColor, progress)
                };
                Color s = ColorMode switch
                {
                    ColorModes.Gradient => Color.Lerp(SecondaryColor, Color.Lerp(SecondaryColor, Color.Black, 0.3f), 1 - progress),
                    ColorModes.SolidStem => SecondaryColor
                };
                SegmentList list = new SegmentList(Base, (int)length, b, s, MaxSegments, ColorMode);
                Limbs.Add(list);
                Add(list);

            }
            Calc.PopRandom();
            float left, top, right, bottom;
            left = top = int.MaxValue;
            right = bottom = int.MinValue;

            for (int i = 0; i < Limbs.Count; i++)
            {
                foreach (Segment s in Limbs[i].Segments)
                {
                    left = Calc.Min(left, s.End.X);
                    right = Calc.Max(right, s.End.X);
                    top = Calc.Min(top, s.End.Y);
                    bottom = Calc.Max(bottom, s.End.Y);
                }
            }
            float width = right - left;
            float height = bottom - top;
            Collider = new Hitbox(width, height);
            Collider.Position = new Vector2(left, top) - Position;
            Target = VirtualContent.CreateRenderTarget("CoralTarget", (int)width, (int)height);
            Add(new BeforeRenderHook(BeforeRender));
            Tag |= Tags.TransitionUpdate;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target.Dispose();
        }
        private void BeforeRender()
        {

            if (initialized) return;
            Vector2 offset = -TopLeft;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            foreach (SegmentList list in Limbs)
            {
                list.RenderAt(offset);
            }
            Draw.SpriteBatch.End();
            initialized = true;
        }
        public override void Update()
        {
            base.Update();
            onScreen = Scene is Level level && Collide.CheckRect(this, level.Camera.GetBounds());
        }
        public override void Render()
        {
            if (onScreen)
            {
                Draw.SpriteBatch.Draw(Target, Collider.AbsolutePosition, Color.White);
            }
        }
        public class Segment
        {
            public const float EvenAngle = 15f;
            public const float OddAngle = -15f;
            public float Z;
            public float Length;
            public Vector2 Start;
            public Vector2 End;
            public Segment Prev;
            public float AlgAngle;
            public float Angle;
            public Color Color;
            public Color DepthColor;
            private int thickness;
            public Segment(Vector2 start, Color color, float angle)
            {
                Start = End = start;
                Angle = angle.ToRad();
                Color = color;
                DepthColor = Color;
            }
            public Segment(Segment prev, float length, bool isEven, Color color, float mult, int thickness)
            {
                Prev = prev;
                Length = length;
                AlgAngle = (isEven ? EvenAngle : OddAngle).ToRad() * mult;
                Angle = Prev.Angle + AlgAngle;
                this.thickness = thickness;
                Start = Prev.End;
                End = Start + Calc.AngleToVector(Angle, Length).Round();
                Z = prev.Z + (isEven ? 0.025f : -0.025f);
                Color = Color.Lerp(color, Z > 0 ? Color.White : Color.Black, Math.Abs(Z));
            }

            public void RenderAt(Vector2 offset)
            {
                
                Draw.Line(Start + offset, End + offset, Color, thickness);
            }
        }
        public class SegmentList : Component
        {
            public List<Segment> Segments = new();
            public Segment Base;
            private int length;
            private Color baseColor;
            private Color secondaryColor;
            private int maxSegments;
            private float mult;
            private int startIndex;
            private ColorModes Mode;
            public SegmentList(Segment baseSegment, int length, Color baseColor, Color secondaryColor, int maxSegments, ColorModes mode, int startIndex = 0) : base(true, true)
            {
                Mode = mode;
                this.startIndex = startIndex;
                Base = baseSegment;
                this.length = length;
                this.baseColor = baseColor;
                this.secondaryColor = secondaryColor;
                this.maxSegments = maxSegments;
                mult = Calc.Random.Range(0.15f, 1f) * Calc.Random.Choose(-1, 1);
                Generate();
            }
            public void Generate()
            {
                Segments.Clear();
                Segments.Add(Base);
                int count = startIndex;
                Segment last = Base;
                int lastResult = Calc.Random.Range(7, int.MaxValue);
                int baseThickness = Calc.Random.Range(2, 5);
                while (count < maxSegments)
                {
                    bool isEven = lastResult % 2 == 0;
                    int result = isEven ? lastResult / 2 : (lastResult * 3) + 1;
                    if (lastResult == 2 && result == 1)
                    {
                        break;
                    }
                    lastResult = result;
                    float progress = count / (float)maxSegments;
                    
                    int thickness = (int)(baseThickness - progress * (baseThickness - 1));
                    Color c = Mode switch
                    {
                        ColorModes.Gradient => Color.Lerp(baseColor, secondaryColor, progress),
                        ColorModes.SolidStem => baseColor
                    };
                    float newLength = length - progress * (length - 1);
                    if (newLength <= 1.4f) break;
                    Segment next = new Segment(last, newLength, isEven, c, mult, thickness);
                    Segments.Add(next);
                    last = next;
                    count++;
                }
            }
            public void RenderAt(Vector2 offset)
            {
                foreach (Segment segment in Segments)
                {
                    segment.RenderAt(offset);
                }
            }
        }
    }

}