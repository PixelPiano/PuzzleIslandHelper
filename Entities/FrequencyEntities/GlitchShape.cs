using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.BatterySystem;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    public class GlitchCircle : GlitchShape
    {
        public float Radius;
        private Vector2[] offsets;
        public GlitchCircle(Vector2 position, float radius, Color color, Color color2) : base(position, color, color2)
        {
            Radius = radius;
            offsets = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                offsets[i] = Calc.AngleToVector(MathHelper.TwoPi / 4 * i, radius);
            }

        }
        public override void RenderPart(float from, float to, float offset, Color color, int thickness)
        {
            offset *= 0.3f;
            thickness = Math.Max(3, (int)(thickness * 0.5f));
            int index = (int)(from / 0.25f);
            float lerp = (from % 0.25f) / 0.25f;
            Vector2 vectorOffset = Vector2.Lerp(offsets[index] * (1 + offset), offsets[(index + 1) % 4] * (1 + offset), lerp);
            Vector2 start = Position + vectorOffset;
            float i = from + 0.05f;
            for (; i < to; i += 0.05f)
            {
                index = (int)(i / 0.25f);
                lerp = (i % 0.25f) / 0.25f;
                vectorOffset = Vector2.Lerp(offsets[index] * (1 + offset), offsets[(index + 1) % 4] * (1 + offset), lerp);
                Vector2 position = Position + vectorOffset;
                Draw.Line(start, position, color * Alpha, thickness);
                start = position;
            }
        }
    }
    public class GlitchLine : GlitchShape
    {
        public Vector2 From, To;
        public GlitchLine(Vector2 from, Vector2 to) : base(from, Color.Lime, Color.Green)
        {
            From = from;
            To = to;
        }
        public override void RenderPart(float from, float to, float offset, Color color, int thickness)
        {
            Draw.Line(From.X + (To.X - From.X) * from, Y + offset, From.X + (To.X - From.X) * to, Y + offset, color, thickness);
        }
    }
    public abstract class GlitchShape : Entity
    {
        private class node : Component
        {
            public float Percent;
            public float Timer;
            public node(float percent, float time) : base(true, true)
            {
                Percent = percent;
                Timer = time;
            }
            public override void Update()
            {
                base.Update();
                Timer -= Engine.DeltaTime;
                if (Timer <= 0)
                {
                    RemoveSelf();
                }
            }
            public override void Removed(Entity entity)
            {
                if (entity is GlitchShape shape && shape.nodes.Contains(this))
                {
                    shape.nodes.Remove(this);
                }
                base.Removed(entity);
            }
        }
        private class thicknessOffset : node
        {
            public int Offset;
            public thicknessOffset(float percent, int offset, float time) : base(percent, time)
            {
                Offset = offset;
            }
        }
        private class offset : node
        {
            public float Low
            {
                get => Percent;
                set => Percent = value;
            }
            public float High;
            public int Offset;
            public float Duration;
            public offset(float low, float high, int direction, float time) : base(low, time)
            {
                Duration = time;
                High = high;
                Offset = direction * Calc.Random.Range(0, 6);
            }
        }
        private List<node> nodes = [];
        public int Thickness = 15;
        public Color Color, Color2;
        public float Alpha = 1;
        public string Text = "";
        public Vector2 TextOffset;
        public GlitchShape(Vector2 position, Color color, Color color2) : base(position)
        {
            Tag |= TagsExt.SubHUD;
            Color = color;
            Color2 = color2;
        }
        public void AddOffset(float low, float high, int direction)
        {
            offset n = new offset(low, high, direction, Calc.Random.Range(0.1f, 0.4f));
            List<offset> toRemove = [];
            foreach (node node in nodes)
            {
                if (node is offset o)
                {
                    if (n.Low < o.Low)
                    {
                        if (n.High > o.Low)
                        {
                            o.Low = n.High;
                        }
                    }
                    if (n.High > o.High)
                    {
                        if (n.Low < o.High)
                        {
                            o.High = n.Low;
                        }
                    }
                    if (o.Low > n.Low && o.High < n.High)
                    {
                        toRemove.Add(o);
                    }
                }
            }
            foreach (offset node in toRemove)
            {
                node.RemoveSelf();
            }
            nodes.Add(n);
            Add(n);


        }
        public override void Update()
        {
            base.Update();
            if (Calc.Random.Chance(0.2f))
            {
                float low = Calc.Random.Range(0, 1f);
                float to = Math.Min(Calc.Random.Range(low + 0.05f, low + 0.4f), 1);
                if (to > low)
                {
                    AddOffset(low, to, Calc.Random.Sign());
                }
            }
            if (Calc.Random.Chance(0.2f))
            {
                float from = Calc.Random.Range(0, 1f);
                nodes.Add(new thicknessOffset(from, Calc.Random.Choose(1, -1), Calc.Random.Range(0.1f, 0.4f)));
            }
            nodes = [.. nodes.OrderBy(item => item.Percent)];
        }
        public abstract void RenderPart(float from, float to, float offset, Color color, int thickness);
        public override void Render()
        {
            base.Render();
            float low = 0;
            float high = 1f;
            int thickness = Thickness;
            Color c = Color;
            foreach (node node in nodes)
            {
                if (node is offset o)
                {
                    if (o.Low < high && o.Low >= low)
                    {
                        if (low < o.Low)
                        {
                            RenderPart(low, o.Low, 0, c, thickness);
                        }
                        c = Color.Lerp(Color2, Color, o.Timer / o.Duration);
                        RenderPart(o.Low, o.High, o.Offset, c, thickness);
                        low = o.High;
                    }
                }
                else if (node is thicknessOffset thicknessOffset)
                {
                    thickness = Math.Clamp(thickness + thicknessOffset.Offset, 6, Thickness);
                }
            }
            if (low != high)
            {
                RenderPart(low, high, 0, c, thickness);
            }
            if (!string.IsNullOrEmpty(Text))
            {
                ActiveFont.Draw(Text, Position + TextOffset, new Vector2(0.5f), Vector2.One, Color.White);
            }
        }
    }

}
