using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using FactoryHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities
{
    [Tracked(false)]
    [CustomEntity("PuzzleIslandHelper/GearCircuit")]
    public class GearCircuit : Entity
    {
        public string Flag;
        public List<Image> Images = [];
        private class Line : Component
        {
            public Vector2 A;
            public Vector2 B;
            public Vector2 LineStart;
            public Vector2 LineEnd;
            public Color Color;

            public Line(Vector2 start, Vector2 end, Color color) : base(true, true)
            {
                A = LineStart = LineEnd = start;
                B = end;
                Color = color;
            }

            public override void Render()
            {
                if (LineEnd == A) return;
                Draw.Line(LineStart, LineEnd, Color, 2);
            }
        }
        private Line[] Lines;
        public Vector2[] Nodes;
        private float moveMult = 1;
        private enum approachModes
        {
            None,
            On,
            Off
        }
        private SnakeLine line;
        private approachModes approachMode;
        public GearCircuit(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Flag = data.Attr("flag");
            Nodes = data.NodesWithPosition(offset);
            Depth = 2;
            removeRedundantNodes();
            Add(coroutine = new Coroutine(false));
            AddTag(Tags.TransitionUpdate);

            line = new SnakeLine(Vector2.Zero, [.. Nodes.Select(item => item - Position)], 0, 100, 40, 50, 1, Color.Cyan, Color.Red);
            line.WrapAround = true;

            Add(line);
        }
        public override void Update()
        {
            base.Update();
            bool completed = true;
            if (moveMult != 1)
            {
                moveMult = Calc.Approach(moveMult, 1, Engine.DeltaTime * 2f);
            }
            switch (approachMode)
            {
                case approachModes.None:
                    Speed = 0;
                    break;
                case approachModes.On:
                    if (coroutine.Finished) Speed = Calc.Approach(Speed, 200f, 100f * Engine.DeltaTime * moveMult);
                    foreach (Line line in Lines)
                    {
                        if (line.LineEnd == line.B) continue;
                        completed = false;
                        line.LineEnd = Calc.Approach(line.LineEnd, line.B, Speed * Engine.DeltaTime);
                        break;
                    }
                    break;
                case approachModes.Off:

                    if (coroutine.Finished) Speed = Calc.Approach(Speed, 200f, 100f * Engine.DeltaTime * moveMult);
                    foreach (Line line in Lines.Reverse())
                    {
                        if (line.LineEnd == line.A) continue;
                        completed = false;
                        line.LineEnd = Calc.Approach(line.LineEnd, line.A, Speed * Engine.DeltaTime);
                        break;
                    }
                    break;
            }
            if (completed)
            {
                approachMode = approachModes.None;
            }
        }
        private Coroutine coroutine;
        private IEnumerator speedRoutine(approachModes next)
        {
            while (Math.Abs(Speed) > 0.8f)
            {
                Speed = Calc.Approach(Speed, 0f, Speed * 4 * Engine.DeltaTime);
                yield return null;
            }
            moveMult = 0;
            approachMode = next;
        }
        public override void Awake(Scene scene)
        {
            if (Nodes.Length < 2)
            {
                RemoveSelf();
                return;
            }
            Lines = new Line[Nodes.Length - 1];
            Vector2 prev = Nodes[0];

            for (int i = 1; i < Nodes.Length; i++)
            {
                Vector2 current = Nodes[i];
                Lines[i - 1] = new Line(prev, current, Color.White);
                prev = current;
            }
            Add(Lines);
            if ((scene as Level).Session.GetFlag(Flag))
            {
                Activate(true);
            }
            base.Awake(scene);
        }
        [Command("test_activate", "")]
        public static void test()
        {
            foreach (GearCircuit c in Engine.Scene.Tracker.GetEntities<GearCircuit>())
            {
                c.Activate(false);
            }
        }
        [Command("test_deactivate", "")]
        public static void testde()
        {
            foreach (GearCircuit c in Engine.Scene.Tracker.GetEntities<GearCircuit>())
            {
                c.Deactivate(false);
            }
        }
        private void removeRedundantNodes()
        {
            List<Vector2> list = new List<Vector2>();
            Vector2 vector = Vector2.Zero;
            Vector2 vector2 = Vector2.Zero;
            bool flag = false;
            Vector2[] array = Nodes;
            foreach (Vector2 vector3 in array)
            {
                if (flag)
                {
                    Vector2 vector4 = (vector - vector3).SafeNormalize();
                    if ((double)Math.Abs(vector4.X - vector2.X) > 0.0005 || (double)Math.Abs(vector4.Y - vector2.Y) > 0.0005)
                    {
                        list.Add(vector);
                    }

                    vector2 = vector4;
                }

                flag = true;
                vector = vector3;
            }

            list.Add(Nodes.Last());
            Nodes = [.. list];
        }
        public void Activate(bool instant)
        {
            Flag.SetFlag(true);
            if (instant)
            {
                approachMode = approachModes.None;
                foreach (Line l in Lines)
                {
                    l.LineEnd = l.B;
                }
                onCircuitEnd(true);
            }
            else if (approachMode != approachModes.On)
            {
                coroutine.Replace(speedRoutine(approachModes.On));
                //Add(new Coroutine(LineRoutine()));
            }
        }
        public void Deactivate(bool instant)
        {
            Flag.SetFlag(false);
            if (instant)
            {
                approachMode = approachModes.None;
                foreach (Line l in Lines)
                {
                    l.LineEnd = l.A;
                }
            }
            else if (approachMode != approachModes.Off)
            {
                coroutine.Replace(speedRoutine(approachModes.Off));
            }
        }
        public float Speed;
        public Coroutine Coroutine;
        private void onCircuitEnd(bool instant)
        {

        }

    }
}