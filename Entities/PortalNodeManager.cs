using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [ConstantEntity("PuzzleIslandHelper/PortalNodeManager")]
    [Tracked]
    public class PortalNodeManager : Entity
    {
        public AreaKey AreaKey => SceneAs<Level>().Session.Area;
        public static Dictionary<string,List<PortalNodeData>> DataPerArea => PianoMapDataProcessor.PortalNodes;
        public List<PortalNodeData> Data;
        private Player Player;
        public Dictionary<string, Vector2> NodePositions => PianoModule.Session.PortalNodePositions;
        public Dictionary<string, ParticleSystem> ParticleSystems = new();
        public ParticleType P_Beam = new()
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/portalNode/particle"],
            Size = 1,
            Color = Color.Yellow,
            SpeedMin = 10f,
            SpeedMax = 15f,
            FadeMode = ParticleType.FadeModes.InAndOut,
            LifeMin = 1f,
            LifeMax = 1.5f,
            RotationMode = ParticleType.RotationModes.SameAsDirection
        };
        public PortalNodeManager() : base()
        {
            Depth = -100000;
            Tag |= Tags.Global | Tags.TransitionUpdate;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (DataPerArea.TryGetValue(scene.GetAreaKey(), out List<PortalNodeData> value) && value.Count > 0)
            {
                Data = value;
            }
            else
            {
                RemoveSelf();
                return;
            }
            Collider = new Hitbox(20, 16, -10, -8);
            foreach (var d in Data)
            {
                if (ParticleSystems.ContainsKey(d.Flag)) continue;
                ParticleSystem PSystem = new ParticleSystem(Depth, 300);
                PSystem.AddTag(Tags.Global);
                PSystem.AddTag(Tags.TransitionUpdate);
                scene.Add(PSystem);
                ParticleSystems.Add(d.Flag, PSystem);
                Add(new Coroutine(ParticleRoutine(d)));
            }
            Player = scene.GetPlayer();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (var d in ParticleSystems)
            {
                d.Value.RemoveSelf();
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            Player = level.GetPlayer();
            if (Player is null) return;
            RotateOffset += Engine.DeltaTime;
            RotateOffset %= 1;
            Rectangle b = level.Camera.GetBounds();
            Position = Player.Position;

            foreach (var p in Data)
            {
                if (CheckFlag(level, p))
                {
                    float pad = 16;
                    Vector2 s = p.Center;
                    Vector2 e = NodePositions[p.Flag];
                    if (lineRect(s.X, s.Y, e.X, e.Y, b.X - pad, b.Y - pad, b.Width + pad * 2, b.Height + pad * 2, out var start, out var end))
                    {
                        p.Start = start;
                        p.End = end;
                    }

                }
            }
        }
        public IEnumerator ParticleRoutine(PortalNodeData data)
        {
            Level level = Scene as Level;
            while (true)
            {
                yield return 0.05f;
                if (CheckFlag(level, data) && data.Start.HasValue && data.End.HasValue)
                {
                    float angle = (data.Start.Value - data.End.Value).Angle();
                    float normal = angle + (float)(Math.PI / 2f);
                    Vector2 offset = -Calc.AngleToVector(normal, (data.Thickness / 2) - 1);
                    Vector2 pos = Vector2.Lerp(data.Start.Value + offset, data.End.Value + offset, Calc.Random.Range(0f, 1f));
                    pos += Calc.AngleToVector(normal, Calc.Random.Range(0, data.Thickness - 2)).Floor();
                    ParticleSystems[data.Flag].Emit(P_Beam, pos, Color.Yellow, angle);
                }
            }

        }
        public bool CheckFlag(Level level, PortalNodeData data)
        {
            if (NodePositions.ContainsKey(data.Flag) && !level.Session.GetFlag(data.Flag))
            {
                return true;
            }
            return false;
        }
        public float RotateOffset;
        public void RenderData(Level level, Rectangle b, PortalNodeData data)
        {
            if (CheckFlag(level, data) && data.Start.HasValue && data.End.HasValue)
            {
                float angle = (data.Start.Value - data.End.Value).Angle();
                float normal = angle + (float)(Math.PI / 2f);
                float amount = (float)(Math.Sin(RotateOffset) + 1) / 2f;
                Vector2 offset = Calc.AngleToVector(normal, data.Thickness / 2);
                Draw.Line(data.Start.Value, data.End.Value, Color.Yellow * 0.7f, data.Thickness);

            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Rectangle b = level.Camera.GetBounds();
            foreach (PortalNodeData data in Data)
            {
                RenderData(level, b, data);
            }

        }
        // LINE/RECTANGLE
        public bool lineRect(float x1, float y1, float x2, float y2, float rx, float ry, float rw, float rh, out Vector2? start, out Vector2? end)
        {
            Rectangle rect = new Rectangle((int)rx, (int)ry, (int)(rx + rw), (int)(ry + rh));
            start = end = null;
            // check if the line has hit any of the rectangle's sides
            // uses the Line/Line function below
            bool left = lineLine(x1, y1, x2, y2, rx, ry, rx, ry + rh, out var l);
            bool right = lineLine(x1, y1, x2, y2, rx + rw, ry, rx + rw, ry + rh, out var r);
            bool top = lineLine(x1, y1, x2, y2, rx, ry, rx + rw, ry, out var t);
            bool bottom = lineLine(x1, y1, x2, y2, rx, ry + rh, rx + rw, ry + rh, out var b);

            // if ANY of the above are true, the line
            // has hit the rectangle
            if (left)
            {
                start = l;
            }
            if (right)
            {
                if (start.HasValue) end = r;
                else start = r;
            }
            if (top)
            {
                if (start.HasValue) end = t;
                else start = t;
            }
            if (bottom)
            {
                if (start.HasValue) end = b;
                else start = b;
            }
            if (rect.Contains(new Point((int)x1, (int)y1)))
            {
                start = new Vector2(x1, y1);
            }
            if (rect.Contains(new Point((int)x2, (int)y2)))
            {
                end = new Vector2(x2, y2);
            }
            return start.HasValue && end.HasValue;
        }


        // LINE/LINE
        bool lineLine(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, out Vector2? a)
        {
            a = null;
            // calculate the direction of the lines
            float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
            float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

            // if uA and uB are between 0-1, lines are colliding
            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
            {

                // optionally, draw a circle where the lines meet
                float intersectionX = x1 + (uA * (x2 - x1));
                float intersectionY = y1 + (uA * (y2 - y1));
                a = new Vector2(intersectionX, intersectionY);

                return true;
            }
            return false;
        }
    }
}