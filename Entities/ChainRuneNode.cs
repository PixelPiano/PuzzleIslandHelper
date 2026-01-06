using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ChainRuneNode")]
    [Tracked]
    public class ChainRuneNode : Entity
    {
        public class Lightning : Entity
        {
            public Vector2 Start;
            public Vector2 End;
            public float MaxWidth;
            public Vector2[] Points;
            public Color ColorA, ColorB;
            public float Interval;
            private float timer;
            private Coroutine coroutine;
            public Lightning(int depth, Vector2 start, Vector2 end, int steps, float maxWidth, Color colorA, Color colorB, float interval, float delay = 0) : base(Vector2.Zero)
            {
                Depth = depth;
                Interval = interval;
                timer = delay;
                Start = start;
                End = end;
                MaxWidth = maxWidth;
                Points = new Vector2[steps + 2];
                Points[0] = Start;
                Points[^1] = End;
                ColorA = colorA;
                ColorB = colorB;
                GenerateNewPoints();
                Add(coroutine = new Coroutine(false));
            }
            public override void Update()
            {
                base.Update();
                if (timer > 0)
                {
                    timer -= Engine.DeltaTime;
                    if (timer <= 0)
                    {
                        GenerateNewPoints();
                        timer = Interval;
                    }
                }
            }
            public void GenerateNewPoints()
            {
                for (int i = 1; i < Points.Length - 1; i++)
                {
                    Vector2 lerp = Vector2.Lerp(Start, End, (float)i / Points.Length);
                    lerp.X += Calc.Random.Range(-4, 4);
                    lerp.Y += Calc.Random.Range(-4, 4);
                    Points[i] = lerp;
                }
            }
            public override void Render()
            {
                base.Render();
                for (int i = 1; i < Points.Length; i++)
                {
                    Draw.Line(Points[i - 1], Points[i], Color.Lerp(ColorA, ColorB, Calc.Random.NextFloat()), Math.Max(Calc.Random.NextFloat() * MaxWidth, 1));
                }
            }
            public void TurnOn(bool instant = false)
            {
                if (instant)
                {
                    Visible = true;
                    coroutine.Cancel();
                }
                else
                {
                    StartRoutine(true);
                }
            }
            public void TurnOff(bool instant = false)
            {
                if (instant)
                {
                    Visible = false;
                    coroutine.Cancel();
                }
                else
                {
                    StartRoutine(false);
                }
            }
            public void StartRoutine(bool state)
            {
                coroutine.Replace(stateChangeRoutine(state));
            }
            private IEnumerator stateChangeRoutine(bool state)
            {
                float interval = 0.7f + Calc.Random.NextFloat() * 0.3f;
                while (interval > 0.1f)
                {
                    Visible = !Visible;
                    yield return interval;
                    Visible = !Visible;
                    interval *= 0.7f;
                }
                Visible = state;
            }

        }
        public string[] ConnectNodes;
        public string ID;
        public string WarpID;
        public int Steps;
        public bool Enabled => SceneAs<Level>().Session.GetFlag(Flag);
        public string Flag => "ChainRuneNode" + WarpID + ID;
        public Dictionary<ChainRuneNode, List<Lightning>> Connections = [];
        public ChainRuneNode(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = -10001;
            ID = data.Attr("nodeID");
            WarpID = data.Attr("warpID");
            ConnectNodes = data.Attr("connectTo").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/digiSquare"]);
            Add(image);
            Collider = image.Collider();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new WARP.WarpCapsule.OnWarpEndComponent((p, w) =>
            {
                if (!string.IsNullOrEmpty(w.WarpID))
                {
                    if (w.WarpID == WarpID && !Enabled)
                    {
                        TurnOn(true);
                    }
                }
            }));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (ChainRuneNode node in Scene.Tracker.GetEntities<ChainRuneNode>())
            {
                if (node != this && ConnectNodes.Contains(node.ID) && !node.Connections.ContainsKey(this) && !Connections.ContainsKey(node))
                {
                    Connections.Add(node, new());
                    int steps = (int)(Math.Abs((Center - node.Center).Length()) / 8);
                    for (int i = 0; i < 4; i++)
                    {
                        Lightning l = new Lightning(Depth - 1, Center, node.Center, steps, 4, Color.Green, Color.Turquoise, 0.2f, Calc.Random.NextFloat());
                        Connections[node].Add(l);
                        Scene.Add(l);
                    }
                }
            }
            if ((scene as Level).Session.GetFlag(Flag))
            {
                TurnOn(true);
            }
            else
            {
                TurnOff(true);
            }

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (var v in Connections)
            {
                v.Value.RemoveSelves();
            }
        }
        public void TurnOn(bool instant = false)
        {
            SceneAs<Level>().Session.SetFlag(Flag);
            if (!instant)
            {
                Audio.Play("event:/PianoBoy/TubeLightSparks", Center);
            }
            foreach (var v in Connections)
            {
                foreach (var l in v.Value)
                {
                    l.TurnOn(instant);
                }
            }
        }
        public void TurnOff(bool instant = false)
        {
            if (!instant)
            {
                Audio.Play("event:/PianoBoy/TubeLightSparks", Center);
            }
            foreach (var v in Connections)
            {
                foreach (var l in v.Value)
                {
                    l.TurnOff(instant);
                }
            }
        }
    }
}