using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.WIP.PotionFluid;
// PuzzleIslandHelper.FluidBottle
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/FluidBottle")]
    [Tracked]
    public class FluidBottle : Actor
    {

        private Vector2 ShatterPosition;
        public enum Side
        {
            L,
            R,
            U,
            D,
            UR,
            UL,
            DR,
            DL
        }
        private List<Vector2> TilePositions = new();
        private List<Side> TileSides = new();
        private Level l;
        private Player player;
        private PotionEffects Effect;
        private Sprite Casing;
        private Sprite Fluid;
        private int Hits;
        private List<Vector2> VecPositions = new();
        private bool Broken;
        private bool HitWall;
        private bool HitGround;
        private bool Reinforced;
        private Collision onCollideV;
        private Collision onCollideH;
        public HoldableCollider hitSeeker;
        public bool IsHeld = false;
        private bool Released;
        private float noGravityTimer;
        private float swatTimer;
        private float hardVerticalHitSoundCooldown;
        private static Vector2 StoolJustify = new Vector2(0.5f, 1f);
        public Vector2 Speed;
        private VertexLight Light;
        private Vector2 ReleasedPosition;
        private Vector2 prevLiftSpeed;
        private Holdable Hold;
        private Hitbox HoldingHitbox;
        private EntityID id;
        private Color Color;
        private Vector2 SavedSpeed;
        private Rectangle bounds;
        private List<Vector2> TrackedPositions = new();
        private bool WasHeld;
        private int Area = 48;
        private float ThrowTimeDelay;
        private bool InTightSpaceWhenThrown;
        #region Holdable Methods
        private void OnPickup()
        {
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            SceneAs<Level>().Session.DoNotLoad.Add(id);
        }
        public void OnRelease(Vector2 force)
        {
            RemoveTag(Tags.Persistent);
            if (SceneAs<Level>().Session.DoNotLoad.Contains(id))
            {
                SceneAs<Level>().Session.DoNotLoad.Remove(id);
            }
            ThrowTimeDelay = Engine.DeltaTime * 2;
            Released = true;
            ReleasedPosition = Position;
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            bool condition1 = player.CollideCheck<Solid>(player.TopCenter - Vector2.UnitY * 3);
            bool condition2 = player.CollideCheck<Solid>(player.BottomCenter + Vector2.UnitY * 3);
            bool condition3 = player.CollideCheck<Solid>(player.CenterLeft + Vector2.UnitX * 3);
            bool condition4 = player.CollideCheck<Solid>(player.CenterRight - Vector2.UnitY * 3);
            InTightSpaceWhenThrown = condition1 && condition2 || condition3 && condition4;
            if (InTightSpaceWhenThrown)
            {
                Casing.Play("shine");
                Fluid.Play("shine");
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
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
        /// <summary>
        /// Rotates one point around another
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="centerPoint">The center point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        private void FilterEdges(Grid grid)
        {
            ShatterPosition = Collider.Center + Position;
            //This method fucking sucks fuck you
            //ONLY CHANGE THIS METHOD IF YOU REALLY REALLY NEED TO
            int index = 0;
            if (TrackedPositions.Count - 1 >= 0)
            {
                index = TrackedPositions.Count - 1;
            }
            Vector2 FilterOffset = Vector2.Zero;

            if (HitWall)
            {
                FilterOffset.X = SavedSpeed.X >= 0 ? -16 : 16;

            }
            if (HitGround)
            {
                FilterOffset.Y = SavedSpeed.Y >= 0 ? -16 : 16;
            }

            List<Vector2> RayCasts = new();
            for (int i = 0; i < 360; i += 10)
            {
                Vector2? Ray = DoRaycast(Scene, ShatterPosition, RotatePoint(ShatterPosition + FilterOffset, ShatterPosition, i));
                if (Ray is not null)
                {
                    Vector2 round = new Vector2((int)Ray.Value.X % grid.CellWidth, (int)Ray.Value.Y % grid.CellHeight);
                    Vector2 a = new Vector2((int)Ray.Value.X, (int)Ray.Value.Y) - round;
                    RayCasts.Add(a);
                }
            }
            bool PreferLeft = TrackedPositions[index].X > ShatterPosition.X;
            bool PreferUp = TrackedPositions[index].Y < ShatterPosition.Y;
            bool EqualX = TrackedPositions[index].X == ShatterPosition.X;

            foreach (var vector in VecPositions)
            {
                Vector2 center = vector + Vector2.One * 4;
                bool Top = !grid.Collide(center + new Vector2(0, -8)); //x1x, x0x
                bool Bottom = !grid.Collide(center + new Vector2(0, 8)); //x0x x1x
                bool Left = !grid.Collide(center + new Vector2(8, 0)); // 01x xxx
                bool Right = !grid.Collide(center + new Vector2(-8, 0)); // xxx x10
                Vector2 Justify = new Vector2(Math.Sign(ShatterPosition.X + FilterOffset.X - center.X), Math.Sign(ShatterPosition.Y + FilterOffset.Y - center.Y));
                Vector2 StartLine = center + Vector2.One * 8 * Justify;

                Rectangle r = new Rectangle((int)vector.X, (int)vector.Y, 8, 8);

                bool Continue = true;
                foreach (Vector2 vec in RayCasts)
                {
                    if (!Collide.RectToPoint(r, vec))
                    {
                        Continue = false;
                    }
                }
                bool Condition2 = grid.Collide(StartLine, ShatterPosition + FilterOffset);
                if (Continue || Condition2)
                {
                    continue;
                }
                #region Check Sides
                if (Top)
                {
                    TilePositions.Add(vector);
                    if (Left)
                    {
                        TileSides.Add(Side.UL);
                    }
                    else if (Right)
                    {
                        TileSides.Add(Side.UR);
                    }
                    else
                    {
                        if (!PreferUp/* || HitGround*/)
                        {
                            TileSides.Add(Side.U);
                        }
                        else
                        {
                            TileSides.Add(Side.D);
                        }
                        //TileSides.Add(Side.U);
                    }
                    continue;
                }
                if (Bottom)
                {
                    TilePositions.Add(vector);
                    if (Left)
                    {
                        TileSides.Add(Side.DL);
                    }
                    else if (Right)
                    {
                        TileSides.Add(Side.DR);
                    }
                    else
                    {
                        if (PreferUp/* || HitGround*/)
                        {

                            TileSides.Add(Side.U);
                        }
                        else
                        {
                            TileSides.Add(Side.D);
                        }
                        //TileSides.Add(Side.D);
                    }
                    continue;
                }
                if (Left)
                {
                    TilePositions.Add(vector);
                    if (!EqualX)
                    {
                        if (PreferLeft)
                        {
                            TileSides.Add(Side.L);
                        }
                        else
                        {
                            TileSides.Add(Side.R);
                        }
                    }
                    else
                    {
                        TileSides.Add(Side.L);
                    }
                    continue;
                }
                if (Right)
                {
                    TilePositions.Add(vector);
                    if (!EqualX)
                    {
                        if (PreferLeft)
                        {
                            TileSides.Add(Side.L);
                        }
                        else
                        {
                            TileSides.Add(Side.R);
                        }
                    }
                    else
                    {
                        TileSides.Add(Side.R);
                    }
                    continue;
                }
            }
            #endregion
            SceneAs<Level>().Add(new PotionFluid(TilePositions, TileSides, Effect, permanent: false));
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            foreach (Vector2 vec in VecPositions)
            {
                Draw.Point(vec, Color.Red);

            }
            foreach (Vector2 vec in TilePositions)
            {
                Draw.Point(vec, Color.Blue);
            }
        }
        private void CoatSurface()
        {
            ShatterPosition = Collider.Center + Position;
            VecPositions.Clear();
            Grid grid = SceneAs<Level>().SolidTiles.Grid;
            int roundX = (int)ShatterPosition.X % 8;
            int roundY = (int)ShatterPosition.Y % 8;
            Rectangle bounds = new Rectangle((int)ShatterPosition.X - Area / 2 - roundX, (int)ShatterPosition.Y - Area / 2 - roundY, Area, Area);
            for (int i = 0; i < bounds.Width / 8; i++)
            {
                for (int j = 0; j < bounds.Height / 8; j++)
                {
                    Vector2 point = new Vector2(bounds.X + i * 8, bounds.Y + j * 8);
                    if (grid.Collide(point))
                    {
                        VecPositions.Add(point);
                    }
                }
            }
            FilterEdges(grid);
            Collider = null;
        }
        private void BreakBottle()
        {
            Released = false;
            Vector2 s = Speed;
            if (HitGround)
            {
                s.Y = -s.Y / 4f;
                if (s.X == 0)
                {
                    s.X = Calc.Random.Range(-5f, 6f);
                }
            }
            if (HitWall)
            {
                s.X = -s.X / 4f;
            }
            ReleasedPosition = Vector2.Zero;
            if (Reinforced && Hits < 1)
            {
                Casing.Play("crack");
                Hits++;

                for (int i = 0; i < 4; i++)
                {
                    Vector2 Random = Center;
                    Random.X += Calc.Random.Range(-Width / 2, Width / 2);
                    Random.Y += Calc.Random.Range(-Height / 2, Height / 2);
                    Vector2 sMult = s * Calc.Random.Range(-1f, 3f);
                    l.Add(new GravityParticle(Random, sMult, Color));
                }
            }
            else
            {
                Broken = true;
                for (int i = 0; i < 12; i++)
                {
                    Vector2 Random = Center;
                    Random.X += Calc.Random.Range(-Width / 2, Width / 2);
                    Random.Y += Calc.Random.Range(-Height / 2, Height / 2);
                    Vector2 sMult = s * Calc.Random.Range(-2f, 3f);
                    l.Add(new GravityParticle(Random, sMult, Color));
                }
                Light.Alpha = 0;
                Remove(Fluid);
                Casing.Play("shatter");
                CoatSurface();
                Casing.OnLastFrame = (s) =>
                {
                    Remove(Casing);
                    Remove(Hold);
                };
            }
        }
        private void OnCollideV(CollisionData data)
        {
            HitGround = true;
            SavedSpeed = Speed;
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            Vector2 Current = Position;
            bool condition1 = Current.Y < ReleasedPosition.Y; //aka if it hit the ceiling 
            bool condition2 = ReleasedPosition != Vector2.Zero; //if it opened at all
            bool condition3 = MathHelper.Distance(Current.Y, ReleasedPosition.Y) > 17; //if the Player isn't just dropping it
            bool condition4 = MathHelper.Distance(Current.X, ReleasedPosition.X) > 0;
            if (Released && condition2 && !InTightSpaceWhenThrown && (condition3 || condition1 || condition4))
            {
                BreakBottle();
            }
            else if (!Broken)
            {
                if (Speed.Y > 0f)
                {
                    if (hardVerticalHitSoundCooldown <= 0f)
                    {
                        //Event.PlayEvent("event:/PianoBoy/stool_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                        hardVerticalHitSoundCooldown = 0.5f;
                    }
                    else
                    {
                        //Event.PlayEvent("event:/PianoBoy/stool_hit_ground", Position, "crystal_velocity", 0f);
                    }
                }
                if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
                {
                    Speed.Y *= -0.6f;
                }
                else
                {
                    Speed.Y = 0f;
                }
            }
            Released = false;
        }
        private void OnCollideH(CollisionData data)
        {
            HitWall = true;
            SavedSpeed = Speed;
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            if (InTightSpaceWhenThrown)
            {

            }
            if (Released && ReleasedPosition != Vector2.Zero && !InTightSpaceWhenThrown)
            {
                BreakBottle();
            }
            else if (!Broken)
            {
                //Event.PlayEvent("event:/PianoBoy/stool_hit_side", Position);
                //Speed.X *= -0.4f;
            }
            Released = false;
        }
        public void Swat(HoldableCollider hc, int dir)
        {
            if (Hold.IsHeld && hitSeeker == null)
            {
                swatTimer = 0.1f;
                hitSeeker = hc;
                Hold.Holder.Swat(dir);
            }
        }

        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }

        public void HitSpinner(Entity spinner)
        {
        }
        public void HitSeeker(Seeker seeker)
        {
            if (!Hold.IsHeld)
            {
                Speed = (Center - seeker.Center).SafeNormalize(120f);
            }
            Audio.Play("event:/PianoBoy/stool_hit_side", Position);
        }
        public bool Dangerous(HoldableCollider holdableCollider)
        {
            if (!Hold.IsHeld && Speed != Vector2.Zero)
            {
                return hitSeeker != holdableCollider;
            }
            return false;
        }
        #endregion
        public FluidBottle(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            string path = "objects/PuzzleIslandHelper/potion/";
            this.id = id;
            Reinforced = data.Bool("reinforced");
            Effect = data.Enum<PotionEffects>("effect");
            Collider = new Hitbox(16, 16);
            Casing = new Sprite(GFX.Game, path);
            Fluid = new Sprite(GFX.Game, path);
            Fluid.AddLoop("idle", "fluid", 0.1f);
            Casing.AddLoop("idle", "case", 0.1f);
            Casing.Add("shatter", "caseShatter", 0.05f);
            Casing.Add("crack", "caseCrack", 0.1f);
            Casing.Add("shine", "shine", 0.1f, "idle");
            Fluid.Add("shine", "liquidShine", 0.05f, "idle");
            Casing.Position.Y = Fluid.Position.Y -= 1;
            Casing.Justify = StoolJustify;
            Fluid.Justify = StoolJustify;
            Casing.JustifyOrigin(StoolJustify);
            Fluid.JustifyOrigin(StoolJustify);
            //Fluid.From = Color or smthn

            Fluid.Color = Effect switch
            {
                PotionEffects.Hot => Color.Red,
                PotionEffects.Refill => Color.Pink,
                PotionEffects.Bouncy => Color.Cyan,
                PotionEffects.Sticky => Color.Green,
                PotionEffects.Invert => Color.White,
                _ => Color.White
            };
            Add(Fluid, Casing);

            Fluid.Play("idle");
            Casing.Play("idle");

            #region Holdable
            Add(Hold = new Holdable(0.1f)
            {
                SlowFall = false,
                SlowRun = false,
                OnPickup = OnPickup,
                OnRelease = OnRelease,
                DangerousCheck = Dangerous,
                OnHitSeeker = HitSeeker,
                OnSwat = Swat,
                OnHitSpring = HitSpring,
                OnHitSpinner = HitSpinner,
                SpeedGetter = () => Speed,
                SpeedSetter = delegate (Vector2 speed)
                {
                    Speed = speed;
                },
                PickupCollider = new Hitbox(11, 11, -11 * StoolJustify.X, -11 * StoolJustify.Y)

            });
            Collider = new Hitbox(11, 11, -11 * StoolJustify.X, -11 * StoolJustify.Y);
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            LiftSpeedGraceTime = 0.1f;
            Add(Light = new VertexLight(Collider.Center, Color.White, 0.7f, 32, 64));
            Add(new MirrorReflection());
            HoldingHitbox = new Hitbox(11 - 4, 11, -11 * StoolJustify.X + 2, -11 * StoolJustify.Y);
            Collider = HoldingHitbox;
            #endregion
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            PianoModule.Session.PotionSpeedMult = Vector2.One;
            PianoModule.Session.PotionJumpMult = 1;
            Position += Vector2.One * 8;
            Position.Y += 3;
            l = scene as Level;
        }
        public override void Update()
        {
            if (Released)
            {
                TrackedPositions.Add(Position);
            }
            WasHeld = Hold.IsHeld;
            base.Update();
            if (player is null)
            {
                return;
            }
            if (ThrowTimeDelay > 0 && Released)
            {
                ThrowTimeDelay -= Engine.DeltaTime;
            }
            /*            Player.Border.Rate = Effect == PotionEffects.Sticky && PlayerOnTop ? PianoModule.Session.PotionSpeedMult.X == Slowed ? 0.4f : PianoModule.Session.PotionSpeedMult.X == Slower ? 0.2f : 0 : 1;
                        PianoModule.Session.PotionJumpMult = Effect == PotionEffects.Sticky && PlayerOnTop ? 0.4f : 1;*/

            if (Broken)
            {
                return;
            }
            Hold.CheckAgainstColliders();
            #region Copied
            if (swatTimer > 0f)
            {
                swatTimer -= Engine.DeltaTime;
            }
            hardVerticalHitSoundCooldown -= Engine.DeltaTime;
            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
            }
            else
            {
                if (OnGround())
                {
                    float target = !OnGround(Position + Vector2.UnitX * 3f) ? 20f : OnGround(Position - Vector2.UnitX * 3f) ? 0f : -20f;
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f)
                        {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f)
                        {
                            noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f)
                        {
                            Speed.Y = 0f;
                        }
                    }
                }
                else if (Hold.ShouldHaveGravity)
                {
                    float num = 800f;
                    if (Math.Abs(Speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = 350f;
                    if (Speed.Y < 0f)
                    {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f)
                    {
                        noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                if (Center.X > l.Bounds.Right)
                {
                    MoveH(32f * Engine.DeltaTime);
                    if (Left - 8f > l.Bounds.Right)
                    {
                        RemoveSelf();
                    }
                }
                else if (Left < l.Bounds.Left)
                {
                    Left = l.Bounds.Left;
                    Speed.X *= -0.4f;
                }
                else if (Top < l.Bounds.Top - 4)
                {
                    Top = l.Bounds.Top + 4;
                    Speed.Y = 0f;
                }
                else if (Bottom > l.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
                {
                    Bottom = l.Bounds.Bottom;
                    Speed.Y = -300f;
                    Audio.Play("event:/game/general/assist_screenbottom", Position);
                }
                if (X < l.Bounds.Left + 10)
                {
                    MoveH(32f * Engine.DeltaTime);
                }
                Player entity = Scene.Tracker.GetEntity<Player>();
                TempleGate templeGate = CollideFirst<TempleGate>();
                if (templeGate != null && entity != null)
                {
                    templeGate.Collidable = false;
                    MoveH(Math.Sign(entity.X - X) * 32 * Engine.DeltaTime);
                    templeGate.Collidable = true;
                }
            }
            if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
            {
                hitSeeker = null;
            }
            #endregion
        }

        #region Annoying stinky blegh
        public static void Load()
        {
            /*            speedHook = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Public | BindingFlags.Instance), modSpeed);
                        wallJumpHook = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic), modWallJump);
                        IL.Celeste.Player.Jump += modJump;*/
        }
        public static void Unload()
        {
            /*            speedHook?.Dispose();
                        wallJumpHook?.Dispose();
                        speedHook = null;
                        wallJumpHook = null;
                        IL.Celeste.Player.Jump -= modJump;*/
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            PianoModule.Session.PotionSpeedMult = Vector2.One;
            PianoModule.Session.PotionJumpMult = 1;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            PianoModule.Session.PotionSpeedMult = Vector2.One;
            PianoModule.Session.PotionJumpMult = 1;
        }
        private static void modJump(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f)))
            {
                Logger.Log("ExtendedVariantMode/JumpHeight", $"Modding constant at {cursor.Index} in CIL code for Jump to make jump height editable");

                cursor.EmitDelegate(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }
        private static void modWallJump(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply -105f (height given by a superdash) with the jump height factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f)))
            {
                cursor.EmitDelegate(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }
        private static bool ShouldContinue()
        {
            if (Engine.Scene.Tracker.GetEntity<FluidBottle>() == null)
            {
                return false;
            }
            return true;
        }
        private static float determineJumpHeightFactor()
        {
            if (!ShouldContinue())
            {
                return 1;
            }
            return PianoModule.Session.PotionJumpMult;
        }
        private static float getSpeedYMultiplier()
        {
            if (!ShouldContinue())
            {
                return 1;
            }
            return PianoModule.Session.PotionSpeedMult.Y;
        }
        private static float getSpeedXMultiplier()
        {
            if (!ShouldContinue())
            {
                return 1;
            }
            return PianoModule.Session.PotionSpeedMult.X;
        }

        private static void modSpeed(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<Actor>("MoveH")))
            {
                if (cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdfld<Vector2>("X")))
                {
                    Logger.Log("PuzzleIslandHelper/FluidMachine", $"Modding dash speed at index {cursor.Index} in CIL code for {cursor.Method.Name}");

                    cursor.EmitDelegate(getSpeedXMultiplier);
                    cursor.Emit(OpCodes.Mul);

                }
            }
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<Actor>("MoveV")))
            {
                if (cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdfld<Vector2>("Y")))
                {
                    Logger.Log("PuzzleIslandHelper/FluidMachine", $"Modding dash speed at index {cursor.Index} in CIL code for {cursor.Method.Name}");

                    cursor.EmitDelegate(getSpeedYMultiplier);
                    cursor.Emit(OpCodes.Mul);

                }
            }
        }
        #endregion
    }
}