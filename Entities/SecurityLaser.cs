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
        private float timeDelay;
        private int electricBuffer;
        private bool isTimed;
        private bool onScreen;
        private Color stateColor
        {
            get
            {
                Color color;
                if (deactivated)
                {
                    color = goodColor;
                }
                else
                {
                    color = badColor;
                    if (dangerous)
                    {
                        color = Color.Lerp(Color.Orange, badColor, dangerColorLerp);
                    }

                }
                return color;
            }
        }
        private bool respectCollision;
        private Vector2 topBound;
        private Vector2 bottomBound;
        private Player player;
        private Entity baseEntity;
        private Entity nodeEntity;
        private float laserOpacity = 1;
        private float laserWidth = 2;
        private Sprite nodeSprite;
        private Sprite baseSprite;
        private Vector2 node;
        private Vector2 start;
        private Vector2 end;
        private bool rotateSprites;
        private bool dangerous;
        private bool deactivated
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
        private Color goodColor;
        private Color badColor;
        private bool inverted;
        private string gunID;
        private float colorLerp;
        private string flag;
        private bool detecting;
        private int randomize;
        private VertexLight light;
        private Color _G, _B;
        private List<Vector2> points = new();
        private int range;
        private float dangerColorLerp;
        private float duration;
        public Tween FlickerTween;
        public bool Alerting;
        private float opacityBeforeTween;
        public bool LaserActive => !isTimed || detecting;
        private Vector2 prevBase, prevNode;
        private FlagData flagToSet;
        private bool pauseCollisionChecks;
        public SecurityLaser(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Add(new TransitionListener()
            {
                OnOutBegin = () =>
                {
                    pauseCollisionChecks = true;
                }
            });
            Visible = data.Bool("visible", true);
            duration = data.Float("timer", 1);
            timeDelay = data.Float("WaitTime", 0);
            respectCollision = data.Bool("respectCollisions", true);
            flagToSet = new FlagData(data.Attr("flagOnCrossed"), !data.Bool("flagOnCrossedState"));
            dangerous = data.Bool("dangerous");
            isTimed = data.Bool("isTimed");
            rotateSprites = data.Bool("rotateSprites", false);
            Tag |= Tags.TransitionUpdate;
            goodColor = _G = data.HexColor("safeColor", Color.LightGreen);
            badColor = _B = data.HexColor("dangerousColor", Color.Red);

            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            node = data.Nodes[0] + offset;
            Depth = -10001;
            gunID = data.Attr("gunID");
            baseSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/securityLaser/");
            baseSprite.AddLoop("idle", "emitter", 0.1f);

            nodeSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/securityLaser/");
            nodeSprite.AddLoop("idle", "emitter", 0.1f);

            start = Position + new Vector2(4);
            end = node + new Vector2(4);
            light = new VertexLight(start - Position, Color.White, Visible ? 1 : 0, (int)laserWidth, (int)laserWidth + 5);
            setAngles();
            Collider = new Hitbox(Width, Height);
            baseEntity = new Entity(start);
            nodeEntity = new Entity(end);
            nodeEntity.Depth = -10003;
            baseEntity.Depth = -10002;
            baseEntity.Add(baseSprite);
            nodeEntity.Add(nodeSprite);
            baseSprite.CenterOrigin();
            nodeSprite.CenterOrigin();
            if (Visible)
            {
                baseSprite.Play("idle");
                nodeSprite.Play("idle");
            }

            baseEntity.Collider = new Hitbox(8, 8, -4, -4);
            nodeEntity.Collider = new Hitbox(8, 8, -4, -4);

            nodeEntity.Add(new StaticMover
            {
                OnShake = nodeOnShake,
                SolidChecker = nodeIsRiding,
                OnDestroy = nodeEntity.RemoveSelf
            });
            baseEntity.Add(new StaticMover
            {
                OnShake = baseOnShake,
                SolidChecker = baseIsRiding,
                OnDestroy = baseEntity.RemoveSelf
            });
            FlickerTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.Linear, .25f, false);
            FlickerTween.OnUpdate = t => laserOpacity = Calc.LerpClamp(opacityBeforeTween, 0, t.Eased);
            Add(FlickerTween);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(baseEntity);
            scene.Add(nodeEntity);
            if (Visible)
            {
                Tween LightTween = Tween.Create(Tween.TweenMode.Looping, Ease.SineInOut, 2);
                LightTween.OnUpdate = (t) =>
                {
                    if (isTimed)
                    {
                        light.Visible = detecting;
                    }
                    light.Position = Calc.LerpSnap(start, end, t.Eased) - Position;
                };
                Tween ColorTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 1);
                ColorTween.OnUpdate = (t) =>
                {
                    goodColor = Color.Lerp(_G, Color.White, t.Eased / 4f);
                    badColor = Color.Lerp(_B, Color.White, t.Eased / 4f);
                };
                Add(light);
                Add(LightTween, ColorTween);
                LightTween.Start();
                ColorTween.Start();

                if (isTimed)
                {
                    Add(new Coroutine(alarmRoutine(timeDelay)));
                }
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Collider = new Hitbox(8, 8);
            Level level = scene as Level;
            player = level.Tracker.GetEntity<Player>();
            if (Visible)
            {
                Tween colorTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.Follow(Ease.SineInOut, Ease.CubeInOut), 0.8f);
                colorTween.OnUpdate = (t) =>
                {
                    colorLerp = Calc.LerpClamp(0.1f, 0.4f, t.Eased);
                };
                Add(colorTween);
                colorTween.Start();
            }
            if (respectCollision)
            {
                setBounds(baseEntity.Position, nodeEntity.Position);
                Vector2? Ray = DoRaycast(scene, topBound, bottomBound);
                if (Ray is not null && !SceneAs<Level>().Transitioning)
                {
                    end = Ray.Value;
                }
                else
                {
                    end = nodeEntity.Position;
                }
            }
            prevBase = baseEntity.Position;
            prevNode = nodeEntity.Position;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;

            dangerColorLerp = Calc.Random.Choose(1, 0.5f, 0.3f, 0.8f, 0);
            nodeSprite.Color = Color.Lerp(stateColor, Color.Black, 0.3f);
            baseSprite.Color = Color.Lerp(stateColor, Color.White, 0.6f);
            start = baseEntity.Position;
            end = nodeEntity.Position;
            Camera camera = level.Camera;
            Rectangle c = new Rectangle((int)camera.X, (int)camera.Y, 320, 180);
            onScreen = Collide.RectToLine(c, start, end);
            if (respectCollision)
            {
                if (!pauseCollisionChecks)
                {
                    if ((prevBase != start || prevNode != end))
                    {
                        setBounds(start, end);
                    }
                    Vector2? ray = DoRaycast(Scene, topBound, bottomBound);
                    if (ray is not null && !SceneAs<Level>().Transitioning)
                    {
                        end = ray.Value;
                    }
                    else
                    {
                        end = nodeEntity.Position;
                    }
                }
            }
            else
            {
                end = nodeEntity.Position;
            }
            prevBase = baseEntity.Position;
            prevNode = nodeEntity.Position;
            if (Collidable && player != null && player.Collidable && Collide.RectToLine(player.Collider.Bounds, start, end))
            {
                OnPlayer(player);
            }
            if (onScreen)
            {
                electricBuffer--;
                if (LaserActive && !deactivated && electricBuffer <= 0)
                {
                    randomize = Math.Max(randomize - 1, 0);
                    if ((dangerous && randomize == 0) || (points == null || points.Count == 0))
                    {
                        range = Calc.Random.Range(1, 4);
                        points = GetPoints(start, end + Vector2.One, range);
                        randomize = 2;
                    }
                }
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            baseEntity.RemoveSelf();
            nodeEntity.RemoveSelf();
        }
        public override void Render()
        {
            base.Render();

            if (onScreen)
            {
                baseSprite.DrawOutline(stateColor * colorLerp);
                nodeSprite.DrawOutline(stateColor * colorLerp);
                if (LaserActive)
                {
                    Draw.Line(start, end, Color.Lerp(stateColor, Color.Black, 0.5f) * laserOpacity * 0.2f, laserWidth + 4);
                    Draw.Line(start, end, Color.Lerp(stateColor, Color.Black, 0.3f) * laserOpacity * 0.2f, laserWidth + 2);
                    Draw.Line(start, end, stateColor * laserOpacity, laserWidth);
                    if (!deactivated && dangerous && electricBuffer <= 0 && points != null && points.Count > 1)
                    {
                        for (int i = 1; i < points.Count; i++)
                        {
                            float Opacity = Calc.Random.Range(1f, 0.4f);
                            float Lerp = Calc.Random.Range(0, 1f);
                            int thicc = Calc.Random.Range(1, 3);
                            Draw.Line(points[i], points[i - 1], Color.Lerp(Color.OrangeRed, Color.Yellow, Lerp) * Opacity, thicc);
                        }
                    }
                }
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Line(start, end, Color.Yellow);
            Draw.Line(topBound, bottomBound, Color.Green * 0.6f, laserWidth);
            Draw.Point(topBound, Color.White);
            Draw.Point(bottomBound, Color.LightGreen);
        }
        public void OnPlayer(Player player)
        {
            if (LaserActive)
            {
                if (!deactivated)
                {
                    if (dangerous && !player.Dead)
                    {
                        player.Die(Vector2.Zero);
                    }
                    else if (!Alerting)
                    {
                        OnCrossed();
                    }
                }
            }

        }
        public void OnCrossed()
        {
            flagToSet.State = true;
            opacityBeforeTween = laserOpacity;
            FlickerTween.Start();
        }
        private IEnumerator alarmRoutine(float delay)
        {
            if (delay > 0) yield return delay;
            while (true)
            {
                yield return duration;
                detecting = !detecting;
            }
        }
        private void setBounds(Vector2 from, Vector2 to)
        {
            Vector2 position = from.Round();
            while (position != to)
            {
                if (!Scene.CollideCheck<Solid>(position))
                {
                    topBound = position;
                    position = to;
                    break;
                }
                position = Calc.Approach(position, to, 1);
            }
            while (position != from)
            {
                if (!Scene.CollideCheck<Solid>(position))
                {
                    bottomBound = position;
                    return;
                }
                position = Calc.Approach(position, from, 1);
            }
        }
        private bool nodeIsRiding(Solid solid)
        {
            return nodeEntity.CollideCheck(solid);
        }
        private void nodeOnShake(Vector2 pos)
        {
            nodeSprite.Position += pos;
        }
        private void baseOnShake(Vector2 pos)
        {
            baseSprite.Position += pos;
        }
        private bool baseIsRiding(Solid solid)
        {
            return baseEntity.CollideCheck(solid);
        }
        private void setAngles()
        {
            if (!rotateSprites)
            {
                return;
            }
            Vector2 a = node;
            Vector2 b = Position;
            float Angle = (float)Math.Atan2(b.Y - a.Y, b.X - a.X);
            baseSprite.Rotation = Angle - MathHelper.PiOver2;
            nodeSprite.Rotation = Angle + MathHelper.PiOver2;
        }
        private List<Vector2> GetPoints(Vector2 start, Vector2 end, int range)
        {
            List<Vector2> list = new();
            int points = (int)Vector2.Distance(start, end);
            for (int i = 0; i < points / 4; i++)
            {
                Vector2 position = Vector2.Lerp(start, end, i / (float)points * 4);
                int xVar = Calc.Random.Range(-range, range + 1);
                int yVar = Calc.Random.Range(-range, range + 1);
                list.Add(new Vector2((int)position.X + xVar, (int)position.Y + yVar));
            }

            return list;
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

            return 0 <= tmin && tmin <= tmax && tmin * tmin <= Vector2.DistanceSquared(start, end) ? hbox.AbsolutePosition + start + tmin * dir : null;
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
    }
}