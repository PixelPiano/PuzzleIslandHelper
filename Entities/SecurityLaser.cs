using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// PuzzleIslandHelper.SecurityLaser
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/SecurityLaser")]
    [Tracked]
    public class SecurityLaser : Entity
    {
        #region Variables

        private float timeDelay;
        private int ElectricBuffer;
        private bool IsTimed;
        private bool OnScreen;
        private Color StateColor
        {
            get
            {
                Color color;
                if (State)
                {
                    color = GoodColor;
                }
                else
                {
                    color = BadColor;
                    if (Dangerous)
                    {
                        color = Color.Lerp(Color.Orange, BadColor, dangerColorLerp);
                    }

                }
                return color;
            }
        }
        private bool RespectCollision;

        private Entity CollisionCheck;
        private Vector2 TopBound;
        private Vector2 _End;
        private Vector2 BottomBound;
        private Player player;
        private Entity BaseEntity;
        private Entity NodeEntity;
        public static bool Alert;
        private float LaserOpacity = 1;
        private float LaserWidth = 2;
        private Sprite NodeSprite;
        private Sprite BaseSprite;
        private Vector2 Node;
        private bool AlarmGuns;
        private Vector2 Start;
        private Vector2 End;
        private bool RotateSprites;
        private bool Dangerous;
        private Vector2 offset;
        private bool State
        {
            get
            {
                Level level = Scene as Level;
                if (level is null)
                {
                    return false;
                }

                if (inverted)
                {
                    return level.Session.GetFlag(flag);
                }
                else
                {
                    return !level.Session.GetFlag(flag);
                }

            }
        }
        private bool CollideRoutine;
        private Color GoodColor;
        private Color BadColor;
        private bool inverted;
        private string GunID;
        private float colorLerp;
        private string flag;
        private Alarm Alarm;
        private bool On;
        private int randomize;
        private Vector2[] Lines = new Vector2[4];
        private VertexLight Light;
        private Color _G, _B;
        private List<Point> Points = new();
        private int range;
        private string crossedFlag;
        private bool crossedFlagState;
        private float dangerColorLerp;
        private float Timer;
        private bool GunState;
        #endregion
        private List<Point> GetPoints(Vector2 start, Vector2 end, int range)
        {
            List<Point> list = new();
            int points = (int)Vector2.Distance(start, end);
            for (int i = 0; i < points / 4; i++)
            {
                Vector2 position = Vector2.Lerp(start, end, i / (float)points * 4);
                int xVar = Calc.Random.Range(-range, range + 1);
                int yVar = Calc.Random.Range(-range, range + 1);
                list.Add(new Point((int)position.X + xVar, (int)position.Y + yVar));
            }

            return list;
        }

        public override void Update()
        {
            base.Update();
            if(Scene is not Level level) return;
            Camera camera = level.Camera;
            Rectangle c = new Rectangle((int)camera.X, (int)camera.Y, 320, 180);
            OnScreen = Collide.RectToLine(c, Start, End);

            dangerColorLerp = Calc.Random.Choose(1, 0.5f, 0.3f, 0.8f, 0);
            NodeSprite.Color = Color.Lerp(StateColor, Color.Black, 0.3f);
            BaseSprite.Color = Color.Lerp(StateColor, Color.White, 0.6f);
            Start = BaseEntity.Position;
            End = NodeEntity.Position;

            SetBounds(BaseEntity.Center, NodeEntity.Center, CollisionCheck);

            if (RespectCollision)
            {
                Vector2? Ray = DoRaycast(Scene, TopBound, BottomBound);
                if (Ray is not null && !SceneAs<Level>().Transitioning)
                {
                    End = Ray.Value;
                }
                else
                {
                    End = NodeEntity.Position;
                }
            }
            else
            {
                End = NodeEntity.Position;
            }

            if (PlayerCollide(player) && !CollideRoutine && (!IsTimed || On))
            {
                Add(new Coroutine(PlayerCollide()));
            }

        }
        public static Vector2? DoRaycast(Scene scene, Vector2 start, Vector2 end)
        => DoRaycast(scene.Tracker.GetEntities<Solid>().Select(s => s.Collider), start, end);

        public static Vector2? DoRaycast(IEnumerable<Collider> cols, Vector2 start, Vector2 end)
        {
            Vector2? curPoint = null;
            float curDst = float.PositiveInfinity;
            foreach (Collider c in cols)
            {
                if (!(DoRaycast(c, start, end) is Vector2 intersectionPoint)) continue;
                float dst = Vector2.DistanceSquared(start, intersectionPoint);
                if (dst < curDst)
                {
                    curPoint = intersectionPoint;
                    curDst = dst;
                }
            }
            return curPoint;
        }

        public static Vector2? DoRaycast(Collider col, Vector2 start, Vector2 end) => col switch
        {
            Hitbox hbox => DoRaycast(hbox, start, end),
            Grid grid => DoRaycast(grid, start, end),
            ColliderList colList => DoRaycast(colList.colliders, start, end),
            _ => null //Unknown collider type
        };

        public static Vector2? DoRaycast(Hitbox hbox, Vector2 start, Vector2 end)
        {
            start -= hbox.AbsolutePosition;
            end -= hbox.AbsolutePosition;

            Vector2 dir = Vector2.Normalize(end - start);
            float tmin = float.NegativeInfinity, tmax = float.PositiveInfinity;

            if (dir.X != 0)
            {
                float tx1 = (hbox.Left - start.X) / dir.X, tx2 = (hbox.Right - start.X) / dir.X;
                tmin = Math.Max(tmin, Math.Min(tx1, tx2));
                tmax = Math.Min(tmax, Math.Max(tx1, tx2));
            }
            else if (start.X < hbox.Left || start.X > hbox.Right) return null;

            if (dir.Y != 0)
            {
                float ty1 = (hbox.Top - start.Y) / dir.Y, ty2 = (hbox.Bottom - start.Y) / dir.Y;
                tmin = Math.Max(tmin, Math.Min(ty1, ty2));
                tmax = Math.Min(tmax, Math.Max(ty1, ty2));
            }
            else if (start.Y < hbox.Top || start.Y > hbox.Bottom) return null;

            return (0 <= tmin && tmin <= tmax && tmin * tmin <= Vector2.DistanceSquared(start, end)) ? hbox.AbsolutePosition + start + tmin * dir : null;
        }
        public static Vector2? DoRaycast(Grid grid, Vector2 start, Vector2 end)
        {

            start = (start - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
            end = (end - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
            Vector2 dir = Vector2.Normalize(end - start);
            int xDir = Math.Sign(end.X - start.X), yDir = Math.Sign(end.Y - start.Y);
            if (xDir == 0 && yDir == 0) return null;
            int gridX = (int)start.X, gridY = (int)start.Y;
            float nextX = xDir < 0 ? (float)Math.Ceiling(start.X) - 1 : xDir > 0 ? (float)Math.Floor(start.X) + 1 : float.PositiveInfinity;
            float nextY = yDir < 0 ? (float)Math.Ceiling(start.Y) - 1 : yDir > 0 ? (float)Math.Floor(start.Y) + 1 : float.PositiveInfinity;
            while (Math.Sign(end.X - start.X) != -xDir || Math.Sign(end.Y - start.Y) != -yDir)
            {
                if (grid[gridX, gridY])
                {
                    return grid.AbsolutePosition + start * new Vector2(grid.CellWidth, grid.CellHeight);
                }
                if (Math.Abs((nextX - start.X) * dir.Y) < Math.Abs((nextY - start.Y) * dir.X))
                {
                    start.Y += Math.Abs((nextX - start.X) / dir.X) * dir.Y;
                    start.X = nextX;
                    nextX += xDir;
                    gridX += xDir;
                }
                else
                {
                    start.X += Math.Abs((nextY - start.Y) / dir.Y) * dir.X;
                    start.Y = nextY;
                    nextY += yDir;
                    gridY += yDir;
                }
            }
            return null;
        }
        public SecurityLaser(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Visible = data.Bool("visible", true);
            this.offset = offset;
            #region Finished
            GunState = data.Bool("gunState");
            Timer = data.Float("timer", 1);
            timeDelay = data.Float("WaitTime", 0);
            RespectCollision = data.Bool("respectCollisions", true);
            crossedFlag = data.Attr("flagOnCrossed");
            crossedFlagState = data.Bool("flagOnCrossedState");
            Dangerous = data.Bool("dangerous");
            IsTimed = data.Bool("isTimed");
            RotateSprites = data.Bool("rotateSprites", false);
            Tag |= Tags.TransitionUpdate;
            GoodColor = _G = data.HexColor("safeColor", Color.LightGreen);
            BadColor = _B = data.HexColor("dangerousColor", Color.Red);

            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            Node = data.Nodes[0] + offset;
            Depth = -10001;
            AlarmGuns = data.Bool("alertAllGuns");
            GunID = data.Attr("gunID");
            BaseSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/securityLaser/");
            BaseSprite.AddLoop("idle", "emitter", 0.1f);

            NodeSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/securityLaser/");
            NodeSprite.AddLoop("idle", "emitter", 0.1f);

            Start = Position + new Vector2(4);
            End = _End = Node + new Vector2(4);
            Light = new VertexLight(Start - Position, Color.White, Visible ? 1 : 0, (int)LaserWidth, (int)LaserWidth + 5);
            SetAngles();
            Collider = new Hitbox(Width, Height);
            BaseEntity = new Entity(Start);
            NodeEntity = new Entity(End);
            NodeEntity.Depth = -10003;
            BaseEntity.Depth = -10002;
            BaseEntity.Add(BaseSprite);
            NodeEntity.Add(NodeSprite);
            BaseSprite.CenterOrigin();
            NodeSprite.CenterOrigin();
            if (Visible)
            {
                BaseSprite.Play("idle");
                NodeSprite.Play("idle");
            }

            BaseEntity.Collider = new Hitbox(8, 8, -4, -4);
            NodeEntity.Collider = new Hitbox(8, 8, -4, -4);

            NodeEntity.Add(new StaticMover
            {
                OnShake = NodeOnShake,
                SolidChecker = NodeIsRiding,
                OnDestroy = NodeEntity.RemoveSelf
            });
            BaseEntity.Add(new StaticMover
            {
                OnShake = BaseOnShake,
                SolidChecker = BaseIsRiding,
                OnDestroy = BaseEntity.RemoveSelf
            });
            #endregion
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            #region Finished
            scene.Add(BaseEntity);
            scene.Add(NodeEntity);
            scene.Add(CollisionCheck = new Entity(Position));
            CollisionCheck.Collider = new Hitbox(1, 1);
            if (Visible)
            {
                Tween LightTween = Tween.Create(Tween.TweenMode.Looping, Ease.SineInOut, 2);
                LightTween.OnUpdate = (Tween t) =>
                {
                    if (IsTimed)
                    {
                        Light.Visible = On;
                    }
                    Light.Position = Calc.LerpSnap(Start, End, t.Eased) - Position;
                };
                Tween ColorTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 1);
                ColorTween.OnUpdate = (Tween t) =>
                {
                    GoodColor = Color.Lerp(_G, Color.White, t.Eased / 4f);
                    BadColor = Color.Lerp(_B, Color.White, t.Eased / 4f);
                };
                Add(Light);
                Add(LightTween, ColorTween);
                LightTween.Start();
                ColorTween.Start();

                if (IsTimed)
                {
                    bool Condition = timeDelay <= 0;
                    Alarm = Alarm.Create(Alarm.AlarmMode.Looping, delegate { On = !On; }, Timer, Condition);
                    Add(Alarm);
                    if (!Condition)
                    {
                        Alarm Delay = Alarm.Create(Alarm.AlarmMode.Oneshot, Alarm.Start, timeDelay, true);
                        Add(Delay);
                    }
                }
            }
            #endregion
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            #region Finished
            Alert = false;
            player = level.Tracker.GetEntity<Player>();
            if (Visible)
            {
                Tween colorTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.Follow(Ease.SineInOut, Ease.CubeInOut), 0.8f);
                colorTween.OnUpdate = (Tween t) =>
                {
                    colorLerp = Calc.LerpClamp(0.1f, 0.4f, t.Eased);
                };
                Add(colorTween);
                colorTween.Start();
            }
            SetBounds(BaseEntity.Center, NodeEntity.Center, CollisionCheck);
            Collider = new Hitbox(8, 8);
            if (RespectCollision)
            {
                Vector2? Ray = DoRaycast(scene, TopBound, BottomBound);
                if (Ray is not null && !SceneAs<Level>().Transitioning)
                {
                    End = Ray.Value;
                }
                else
                {
                    End = NodeEntity.Position;
                }
            }
            else
            {
                End = NodeEntity.Position;
            }
            #endregion
        }

        public override void Render()
        {
            base.Render();

            if (OnScreen)
            {
                ElectricBuffer--;
                BaseSprite.DrawOutline(StateColor * colorLerp);
                NodeSprite.DrawOutline(StateColor * colorLerp);
                if (!IsTimed || On)
                {
                    Draw.Line(Start, End, Color.Lerp(StateColor, Color.Black, 0.5f) * LaserOpacity * 0.2f, LaserWidth + 4);
                    Draw.Line(Start, End, Color.Lerp(StateColor, Color.Black, 0.3f) * LaserOpacity * 0.2f, LaserWidth + 2);
                    Draw.Line(Start, End, StateColor * LaserOpacity, LaserWidth);
                    if (!State && ElectricBuffer <= 0)
                    {
                        if (Dangerous && randomize == 0)
                        {
                            range = Calc.Random.Range(1, 4);
                            Points = GetPoints(Start, End + Vector2.One, range);
                            randomize = 3;
                        }
                        randomize--;
                        for (int i = 1; i < Points.Count; i++)
                        {
                            float Opacity = Calc.Random.Range(1f, 0.4f);
                            float Lerp = Calc.Random.Range(0, 1f);
                            int thicc = Calc.Random.Range(1, 3);
                            Draw.Line(Points[i].ToVector2(), Points[i - 1].ToVector2(), Color.Lerp(Color.OrangeRed, Color.Yellow, Lerp) * Opacity, thicc);
                        }
                    }
                }
            }
            else
            {
                randomize = 0;
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Line(Start, End, Color.Yellow);
            Draw.Line(TopBound, BottomBound, Color.Green * 0.6f, LaserWidth);
            Draw.Point(TopBound, Color.White);
            Draw.Point(BottomBound, Color.LightGreen);
        }


        private void SetBounds(Vector2 from, Vector2 to, Entity c)
        {
            if (!RespectCollision || !InView())
            {
                return;
            }
            bool CheckForCollision = false;
            c.Position = from;
            for (float i = 0; i < 1; i += 0.01f)
            {
                if (!CheckForCollision && !c.CollideCheck<Solid>())
                {
                    TopBound = c.Position;
                    c.Position = to;
                    break;
                }
                c.X = Calc.Approach(from.X, to.X, i * MathHelper.Distance(from.X, to.X));
                c.Y = Calc.Approach(from.Y, to.Y, i * MathHelper.Distance(from.Y, to.Y));
                //c.Position = Vector2.Lerp(from, to, i * Vector2.Distance(from, to));
            }
            for (float i = 0; i < 1; i += 0.01f)
            {
                if (!CheckForCollision && !c.CollideCheck<Solid>())
                {
                    BottomBound = c.Position;
                    c.Position = from;
                    break;
                }
                c.X = Calc.Approach(to.X, from.X, i * MathHelper.Distance(to.X, from.X));
                c.Y = Calc.Approach(to.Y, from.Y, i * MathHelper.Distance(to.Y, from.Y));
            }
        }

        ////////////////////////////////////////////////////////////

        private bool InView()
        {
            if (!Visible)
            {
                return true;
            }
            Camera camera = (Scene as Level).Camera;
            Rectangle c = new Rectangle((int)camera.X, (int)camera.Y, 320, 180);
            OnScreen = Collide.RectToLine(c, Start, End);
            return OnScreen;
        }
        private bool PlayerCollide(Player player)
        {
            if (player is null)
            {
                return false;
            }
            return player.CollideLine(Start, End);
        }
        private bool NodeIsRiding(Solid solid)
        {
            return NodeEntity.CollideCheck(solid);
        }
        private void NodeOnShake(Vector2 pos)
        {
            NodeSprite.Position += pos;
        }
        private void BaseOnShake(Vector2 pos)
        {
            BaseSprite.Position += pos;
        }
        private bool BaseIsRiding(Solid solid)
        {
            return BaseEntity.CollideCheck(solid);
        }
        private IEnumerator PlayerCollide()
        {

            CollideRoutine = true;
            yield return null;
            if (player is null)
            {
                yield break;
            }
            if (State)
            {
                CollideRoutine = false;
                yield break;
            }
            SceneAs<Level>().Session.SetFlag(crossedFlag, crossedFlagState);
            float Opacity = LaserOpacity;
            if (Dangerous)
            {
                if (!player.Dead)
                {
                    player.Die(Vector2.Zero);
                }
            }
            else
            {
                if (AlarmGuns)
                {
                    Alert = true;
                }
                foreach (PassiveSecurity gun in SceneAs<Level>().Tracker.GetEntities<PassiveSecurity>())
                {
                    if (gun.LaserID == GunID && gun.mode == PassiveSecurity.Mode.LaserActivated)
                    {
                        gun.ForceState = true;
                        gun.ForcedState = GunState;
                    }
                }
            }
            while (player is not null && !player.Dead && Visible)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime * 15)
                {
                    LaserOpacity = Calc.LerpClamp(Opacity, 0, i);
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime * 15)
                {
                    LaserOpacity = Calc.LerpClamp(0, Opacity, i);
                    yield return null;
                }
            }
            LaserOpacity = Opacity;
            CollideRoutine = false;
        }

        private void SetAngles()
        {
            if (!RotateSprites)
            {
                return;
            }
            Vector2 a = Node;
            Vector2 b = Position;
            float Angle = (float)Math.Atan2(b.Y - a.Y, b.X - a.X);
            BaseSprite.Rotation = Angle - MathHelper.PiOver2;
            NodeSprite.Rotation = Angle + MathHelper.PiOver2;
        }
        public Vector2[] GetPaddedCorners(Vector2 point, Vector2 normal, Vector2 padding)
        {
            normal = Vector2.Normalize(normal);
            var cross3 = Vector3.Cross(new Vector3(normal, 0), new Vector3(0, 0, 1));
            var cross = new Vector2(cross3.X, cross3.Y);  // cross3.Z is always 0 idc
            cross.Normalize();

            var p1 = point + normal * padding.Y + cross * padding.X;
            var p3 = point - normal * padding.Y - cross * padding.X;
            var p2 = point + normal * padding.Y - cross * padding.X;
            var p4 = point - normal * padding.Y + cross * padding.X;

            return new Vector2[] { p1, p2, p3, p4 };
        }
    }
}