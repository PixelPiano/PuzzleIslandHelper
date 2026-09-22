using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Singularity
{
    [CustomEntity("PuzzleIslandHelper/MemoryOrbPath")]
    [Tracked]
    public class SingularityPath : Entity
    {
        public enum PathStartModes
        {
            EntityOnScreen,
            Awake,
            Triggered
        }
        public PathStartModes PathStartMode;
        public bool Triggered;
        public int Points => Nodes.Length;
        public Node[] Nodes;
        public class Node
        {
            public Vector2 Position;
        }
        public float[] Lengths;
        public float TotalLength
        {
            get
            {
                float f = 0;
                foreach (float f2 in Lengths)
                {
                    f += f2;
                }
                return f;
            }
        }
        public float ExitRadius;
        public string ID;
        public bool RemoveEntityOnEnd;
        public FlagList FlagsOnEnd;
        public int MaxLoops;
        public float TargetSpeed;
        public float Speed;
        public float ApproachSpeed;
        public bool Running;
        private float origMaxSpeed;
        public SingularityPath(EntityData data, Vector2 offset) : this(data.Position + offset, data.NodesWithPosition(offset))
        {
            //targetSpeed, maxSpeed, approachSpeed
            PathStartMode = data.Enum<PathStartModes>("activationMode");
            ID = data.Attr("pathID");
            TargetSpeed = data.Float("targetSpeed", 200f);
            origMaxSpeed = Speed = data.Float("speed", 200f);
            ApproachSpeed = data.Float("approachSpeed", 800f);
            ExitRadius = data.Float("nodeRadius");
            MaxLoops = data.Int("loops");
            RemoveEntityOnEnd = data.Bool("removeEntityOnEnd");
            FlagsOnEnd = data.FlagList("flagsOnEnd");
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(8, 8);
        }
        public SingularityPath(Vector2 position, params Vector2[] positions) : base(position)
        {
            Vector2[] modified = [.. positions.Select(p => p + Vector2.One * 4)];
            SetPoints(modified);
        }
        public void Start()
        {
            Running = true;
            Speed = origMaxSpeed;

        }
        public void Stop()
        {
            Running = false;
        }
        public override void Update()
        {
            base.Update();
            if (Running)
            {
                Speed = Calc.Approach(Speed, TargetSpeed, ApproachSpeed * Engine.DeltaTime);
            }
            else
            {
                Speed = 0;
            }
        }
        public void SetPoints(params Vector2[] points)
        {
            Nodes = new Node[points.Length];
            Lengths = new float[points.Length - 1];
            Vector2 prev = default;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 current = points[i];
                Nodes[i] = new Node() { Position = current };
                if (i > 0)
                {
                    Lengths[i - 1] = (current - prev).Length();
                }
                prev = current;
            }
        }
        public Vector2 GetPoint(float progress, bool loop = false)
        {
            if (progress == 0) return Nodes[0].Position;
            if (progress < 0)
            {
                if (!loop) return Nodes[0].Position;
                else
                {
                    progress += TotalLength;
                }
            }
            float currentLength = progress;
            while (true)
            {
                for (int i = 0; i < Lengths.Length; i++)
                {
                    float l = Lengths[i];
                    if (currentLength >= l)
                    {
                        currentLength -= l;
                    }
                    else
                    {
                        return Calc.Approach(Nodes[i].Position, Nodes[i + 1].Position, currentLength);
                    }
                }
                if (!loop) break;
            }
            return Nodes[^1].Position;
        }
        public Vector2 GetPoint(Vector2 prevPoint)
        {
            for (int i = 1; i < Nodes.Length; i++)
            {
                if (Nodes[i - 1].Position == prevPoint) return Nodes[i].Position;
            }
            return Nodes[0].Position;
        }
        public Vector2 GetPoint(int index) => Nodes[index].Position;
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            for (int i = 1; i < Nodes.Length; i++)
            {
                Draw.Line(Nodes[i - 1].Position, Nodes[i].Position, Color.Yellow);

            }
            for (int i = 0; i < Nodes.Length; i++)
            {
                Draw.Circle(Nodes[i].Position, ExitRadius, Color.Orange, (int)ExitRadius);

            }
        }
    }

    [CustomEntity("PuzzleIslandHelper/SingularityPathTrigger")]
    public class SingularityPathTrigger : Trigger
    {
        public string PathID;
        public FlagList Flag;
        public bool OnlyOnce;
        public bool TriggerOnEnter, TriggerOnLeave, TriggerOnStay;
        public SingularityPathTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            PathID = data.Attr("pathID");
            Flag = data.FlagList("flag");
            OnlyOnce = data.Bool("onlyOnce", true);
            TriggerOnEnter = data.Bool("onEnter");
            TriggerOnLeave = data.Bool("onLeave");
            TriggerOnStay = data.Bool("onStay");
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (TriggerOnEnter) TryActivate();
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (TriggerOnLeave) TryActivate();
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (TriggerOnStay) TryActivate();
        }
        public void TryActivate()
        {
            if (Flag && !(Triggered && OnlyOnce))
            {
                foreach (SingularityPath path in Scene.Tracker.GetEntities<SingularityPath>())
                {
                    if (path.ID == PathID)
                    {
                        path.Triggered = true;
                        Triggered = true;
                        break;
                    }
                }
            }
        }
    }
}