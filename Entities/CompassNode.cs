using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.InvertAuth;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CompassNode")]
    [Tracked]
    public class CompassNode : Entity
    {
        public CompassManager Manager;
        public static HashSet<CompassData> CompassData => PianoModule.Session.CompassData;
        private CompassNodeData _data;
        public Image Outline;
        public CompassData Parent;
        public CompassNodeOrb Orb;
        public bool On => !Empty && _data.On;
        public string debug => _data.ToString();
        public bool Empty => StartEmpty && orbIsNull;
        private bool orbIsNull => Orb == null;
        public bool StartEmpty;
        public string ParentID;
        public string ID;
        public Directions Direction;
        public bool OrbAtCenter;
        private float lineLerp;
        private float lockTimer;
        public EntityID EntityID;
        private float maxShakeTime = 1.3f;
        private float shakeMult => shakeTime / maxShakeTime;
        private BetterShaker shaker;
        public Vector2 ShakeOffset;
        private Vector2 shakeOffset;

        public CompassNode(Vector2 position, EntityID id, Directions direction, string nodeId, string compassId, bool startEmpty) : base(position)
        {
            Depth = 3;
            EntityID = id;
            Direction = direction;
            ID = nodeId;
            ParentID = compassId;
            this.StartEmpty = startEmpty;
            Add(Outline = new Image(GFX.Game["objects/PuzzleIslandHelper/wallbuttonOutline"]));
            Collider = Outline.Collider();
            Collider holdCollider = new Hitbox(Width - 6, Height - 6, 3, 3);
            Tag |= Tags.TransitionUpdate | Tags.Persistent;
            Add(new HoldableCollider((Holdable h) =>
            {
                if (Empty && !h.IsHeld && h.Entity is CompassNodeOrb orb)
                {
                    Orb = orb;
                    Orb.Holder = this;
                    Orb.ResetCollider();
                    Orb.noGravityTimer = Engine.DeltaTime;
                    Orb.Sprite.Play("idle");
                    OrbAtCenter = false;
                    lockTimer = Engine.DeltaTime * 30;
                }
            }, holdCollider));
            Add(shaker = new BetterShaker(OnShake));
        }
        public CompassNode(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, id, data.Enum<Directions>("direction", default), data.Attr("nodeID"), data.Attr("compassID"), data.Bool("startEmpty"))
        {

        }
        public void OnShake(Vector2 shake)
        {
            shakeOffset += shake;
        }
        private float shakeTime = 0;
        private int timeBuffer = 3;
        private int currentTimeBuffer;
        public void OnCompassPulseCollide()
        {
            if (_data.CanTurnOn)
            {
                shakeTime += 2.5f * Engine.DeltaTime;
                if (shakeTime > maxShakeTime)
                {
                    _data.Broken = true;
                    shaker.StopShaking();
                }
                else
                {
                    shaker.StartShaking(shakeTime);
                }
            }
        }
        public void OnOrbTaken()
        {
            Orb.Holder = null;
            Orb = null;
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            (scene as Level).Session.DoNotLoad.Remove(EntityID);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            bool foundNode = false;
            foreach (var v in CompassData)
            {
                if (!foundNode)
                {
                    foreach (var n in v.Nodes[Direction])
                    {
                        if (n.ID == ID && n.ParentID == ParentID)
                        {
                            _data = n;
                            foundNode = true;
                            break;
                        }
                    }
                }
                else break;
            }
            if (!foundNode)
            {
                RemoveSelf();
                return;
            }
            (scene as Level).Session.DoNotLoad.Add(EntityID);
            if (!StartEmpty)
            {
                CompassNodeOrb orb = new CompassNodeOrb(Position, new EntityID(Guid.NewGuid().ToString(), 0), false);
                scene.Add(orb);
                Orb = orb;
                Orb.Center = Center;
                OrbAtCenter = true;
                Orb.Sprite.Color = On ? Color.Lime : Color.Red;
                Orb.Holder = this;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Manager = scene.Tracker.GetEntity<CompassManager>();
            if (Manager == null)
            {
                scene.Add(Manager = new CompassManager());
            }
            if (_data != null)
            {
                _data.Empty = Empty;
            }
        }
        public override void Update()
        {
            if (Scene.OnInterval(0.35f))
            {
                shakeTime = Calc.Approach(shakeTime, 0, Engine.DeltaTime * 2);
            }
            base.Update();
            if (Orb != null)
            {
                Orb.noGravityTimer = Engine.DeltaTime;
                float dist = Vector2.DistanceSquared(Center, Orb.Center);
                if (dist > 1 || lockTimer > 0)
                {
                    Vector2 orbSpeed = Orb.Speed;
                    float angle = (Orb.Center - Center).Angle();
                    Vector2 offset = -Calc.AngleToVector(angle, Math.Max(dist, 20));
                    Orb.Speed = Calc.Approach(Orb.Speed, offset, 700f * Engine.DeltaTime);
                    lineLerp = Calc.Approach(lineLerp, 1, Engine.DeltaTime / 0.4f);
                    Orb.MoveH(Orb.Speed.X * Engine.DeltaTime);
                    Orb.MoveV(Orb.Speed.Y * Engine.DeltaTime);
                }
                else
                {
                    if (!OrbAtCenter && !Orb.Hold.IsHeld && !_data.Broken)
                    {
                        Pulse.Circle(this, Pulse.Fade.Linear, Pulse.Mode.Oneshot, Collider.HalfSize, Width / 2f, Width, 1, true, Color.White, default, null, Ease.CubeIn);
                        Pulse.Circle(this, Pulse.Fade.Linear, Pulse.Mode.Oneshot, Collider.HalfSize, Width / 2.5f, Width, 1, true, Color.White, default, null, Ease.CubeIn);
                        Pulse.Diamond(this, Pulse.Fade.Linear, Pulse.Mode.Oneshot, Collider.HalfSize, Width / 2f, Width, 1f, true, Color.White, default, null, Ease.CubeIn);
                        Pulse.Diamond(this, Pulse.Fade.Linear, Pulse.Mode.Oneshot, Collider.HalfSize, Width / 2f, Width * 1.5f, 0.7f, true, Color.White, default, null, Ease.QuintIn);
                    }
                    Orb.Sprite.Color = Color.Lerp(Orb.Sprite.Color, _data.Broken ? Color.DarkSlateGray : On ? Color.Lime : Color.Red, 5 * Engine.DeltaTime);
                    Orb.Center = Center;
                    OrbAtCenter = true;
                }
            }
            else
            {
                OrbAtCenter = false;
            }
            if (_data != null)
            {
                _data.Empty = !OrbAtCenter || Empty;
            }
            if (lockTimer > 0)
            {
                lockTimer -= Engine.DeltaTime;
            }
        }
        public override void Render()
        {
            Position += shakeOffset * shakeMult;
            if (Orb != null)
            {
                if (!OrbAtCenter)
                {
                    float s = 360 / 8;
                    int inc = (360 / 4);
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = s + inc * i;
                        Vector2 p1 = PianoUtils.RotateAroundDeg(CenterRight, Center, angle);
                        Vector2 p2 = PianoUtils.RotateAroundDeg(Orb.CenterRight, Orb.Center, angle);
                        Draw.Line(p1, Vector2.Lerp(p1, p2, Ease.CubeOut(lineLerp)), Color.White);
                    }
                }
                else
                {
                    Orb.Position += shakeOffset * shakeMult;
                    Orb.DrawOrb();
                    Orb.Position -= shakeOffset * shakeMult;
                }
            }
            base.Render();
            Position -= shakeOffset * shakeMult;
        }
    }
}