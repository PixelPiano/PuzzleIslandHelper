using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TowerStairs")]
    [Tracked]
    public class Stairs : Entity
    {
        public bool RenderOnce = true;
        [CustomEntity("PuzzleIslandHelper/TowerPlatform")]
        [TrackedAs(typeof(JumpThru))]
        [Tracked]
        public class CustomPlatform : JumpThru
        {
            public float Timer;
            public bool Disabled;
            public bool InElevator;
            private bool timerBlocked;
            public Tower Tower;

            public CustomPlatform(Vector2 position, int width) : base(position, width, true)
            {
            }
            public CustomPlatform(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, true)
            {

            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Tower = scene.Tracker.GetEntity<Tower>();
                if (Tower == null)
                {
                    RemoveSelf();
                    return;
                }
                Collidable = Tower.Inside;
            }
            public override void Render()
            {
                base.Render();
                Draw.Rect(Collider, Color.LightGray);
            }
            public override void Update()
            {
                base.Update();

                if (Timer > 0)
                {
                    timerBlocked = true;
                    Timer -= Engine.DeltaTime;
                    if (Timer <= 0)
                    {
                        Timer = 0;
                        timerBlocked = false;
                    }
                }
                Collidable = !timerBlocked && !Disabled && (Tower.Inside || Tower.Ending.CollideCheck<Player>());
                if (Collidable)
                {
                    if (Input.MoveY == 1 && Input.MoveX == 0)
                    {
                        if (GetPlayerRider() is Player player && player.CollideCheck<Platform, CustomPlatform>(player.Position + Vector2.UnitY * player.Height))
                        {
                            Timer = Engine.DeltaTime * 15;
                        }
                    }
                }


            }
        }
        public float AngleOffset
        {
            get => angleOffset;
            set
            {
                if (angleOffset != value)
                {
                    angleOffset = value;
                    RecalculatePoints();
                }
            }
        }
        public float FlowRateMult;
        private int stepOffset;
        private int stepSkip = 4;
        private float angleOffset;
        public bool Enabled;
        public Tower Parent;
        public int Revolutions;
        public bool RidingPlatform;
        public CustomPlatform TopPlatform;
        public float TopPlatformCollisionTimer;
        public JumpThru Platform;
        private float NoCollideTimer;
        private float allowDescentTimer;
        private FlagList CollidableFlag;
        public bool DisablePlatform;
        private Vector2 PrevPosition;
        public Vector3[] Points = [];
        public Vector3[] OuterPoints = [];
        public Vector3[] InnerPoints = [];
        public float Radius => Width / 2;
        public float HalfWave => (Height / Revolutions);
        public float ZThreshHigh => Radius;
        public float ZThreshLow
        {
            get => GetLowThresh(Radius);
        }
        public int XFlipScale;
        public int ZFlipScale;
        public float GetLowThresh(float radius)
        {
            float halfCol = Parent.Col.Width / 2;
            float dist = (radius + 1) - halfCol;
            return -(radius + 1) + (dist / 2);
        }
        public Color BGColor = Color.Black;
        public Color FGColor = Color.White;
        public int? LastRiddenFloor;
        public int LastFloor;
        private float forceAscentTimer;
        public Vector2 LastSign;
        public bool WaitForUpInput;
        public VirtualRenderTarget target;
        private bool Rendered
        {
            get => rendered;
            set
            {
                if (!value)
                {
                    foreach (var r in Renderers)
                    {
                        r.Rendered = false;
                    }
                    rendered = value;
                }
            }
        }
        private bool rendered;
        public int CurrentFloor;
        public float CurrentZ;
        public bool HidingEnabled;
        public bool Initialized;
        public int YOffset;
        public bool WasRidingPlatform;
        public bool AtTop;
        public bool WasAtTop;
        private bool waitUntilNotCollidingWithTopPlatform;
        public float ShadeValue;
        public float XMult = 1;
        public class Renderer : Entity
        {
            public VirtualRenderTarget Target;
            public bool Rendered;
            public Stairs Parent;
            public Renderer(Stairs parent, int depth, float width, float height)
            {
                Depth = depth;
                Tag |= Tags.TransitionUpdate;
                Parent = parent;
                Target = VirtualContent.CreateRenderTarget("stairsRenderer", (int)width, (int)height);
                Add(new BeforeRenderHook(BeforeRender));
            }
            public void BeforeRender()
            {
                if (Rendered || Parent.OuterPoints == null || Parent.InnerPoints == null || Parent.Points == null) return;
                float radius = Parent.Radius;
                float flowOffset = Parent.FlowTimer;
                Rendered = true;
                Target.SetAsTarget(true);
                Draw.SpriteBatch.Begin();
                Vector2 topCenter = new Vector2(radius + 16, 0);
                float lineWidth = 8;
                //DrawLines(OuterPoints, topCenter, 1, Radius + lineWidth / 2);
                //DrawLines(Points, topCenter, 1, Radius);
                //DrawLines(InnerPoints, topCenter, 1, Radius - lineWidth / 2);
                if (Depth > 0)
                {
                    DrawCurve(Parent.OuterPoints, topCenter, 1, radius + lineWidth / 2, flowOffset);
                    DrawCurve(Parent.Points, topCenter, 1, radius, flowOffset + 0.33f);
                    DrawCurve(Parent.InnerPoints, topCenter, 1, radius - lineWidth / 2, flowOffset + 0.66f);
                }
                else
                {
                    DrawCurve(Parent.InnerPoints, topCenter, 1, radius + lineWidth / 2, flowOffset + 0.66f);
                    DrawCurve(Parent.Points, topCenter, 1, radius, flowOffset + 0.33f);
                    DrawCurve(Parent.OuterPoints, topCenter, 1, radius - lineWidth / 2, flowOffset);
                }
                Draw.SpriteBatch.End();
            }

            public void DrawCurve(Vector3[] points, Vector2 topCenter, float scale, float radius, float flowOffset)
            {
                float zThreshHigh = radius + 1;
                float zThreshLow = Parent.GetLowThresh(radius);
                float flow = flowOffset;
                int sign = Math.Sign(Depth);
                Color[] colors = [Color.Magenta, Color.Blue, Color.Cyan, Color.Magenta];
                for (int i = 1; i < points.Length; i++)
                {
                    float currentZ = points[i].Z;
                    float prevZ = points[i - 1].Z;
                    if (Math.Sign(currentZ - prevZ) == sign)
                    {
                        int index = (int)(flow % (colors.Length - 1));
                        int indexA = index;
                        int indexB = index + 1;
                        if (currentZ < zThreshHigh && currentZ > zThreshLow && prevZ < zThreshHigh && prevZ > zThreshLow)
                        {
                            Color fg = Color.Lerp(colors[indexA], colors[indexB], Ease.SineInOut(flow % 1));
                            float lerp = 0;
                            if (currentZ < 0)
                            {
                                lerp = Math.Abs(currentZ) / radius;
                            }
                            Color color = Color.Lerp(fg, Parent.BGColor, lerp);
                            int thickness = (int)Calc.LerpClamp(1, 3, (currentZ + radius) / (radius * 2));
                            Draw.Line(points[i - 1].XY() * scale + topCenter, points[i].XY() * scale + topCenter, color, thickness);
                        }
                    }
                    flow += Engine.DeltaTime;
                }
            }
            public override void Render()
            {
                base.Render();
                if (Parent.Parent.OutsideAlpha < 1)
                {
                    Draw.SpriteBatch.Draw(Target, Parent.Position - Vector2.UnitX * 16, Color.White * (1 - Parent.Parent.OutsideAlpha));
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Target?.Dispose();
            }
        }
        public float FlowTimer
        {
            get => flowTimer;
            set
            {
                if (flowTimer != value)
                {
                    Rendered = false;
                    flowTimer = value;
                }
            }
        }
        private float flowTimer;
        private Renderer[] Renderers = new Renderer[2];
        public bool InElevator;
        public Stairs(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Int("halfRevolutions", 3), data.Bool("flipX") ? -1 : 1, data.Bool("flipZ") ? -1 : 1)
        {

        }
        public Stairs(Vector2 position, float width, float height, int revolutions, int xFlipScale, int zFlipScale) : base(position)
        {
            XFlipScale = xFlipScale;
            ZFlipScale = zFlipScale;
            AddTag(Tags.TransitionUpdate);
            Collider = new Hitbox(width, height);
            Revolutions = revolutions;
            Renderers[0] = new Renderer(this, -1, width + 32, height);
            Renderers[1] = new Renderer(this, 1, width + 32, height);
            Depth = 2;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Renderers);
        }
        public float GetY(float y, float x, float radius, float halfWave)
        {
            float xPosInCollider = Calc.Clamp(x - CenterX, -radius, radius) * XFlipScale;
            float yPosInCollider = Calc.Clamp(y - Top, 0, Height);
            float halfWavesFromTop = (yPosInCollider + halfWave / 2) / halfWave;
            if ((int)halfWavesFromTop % 2 != 0) xPosInCollider *= -1;
            y = halfWave * (float)Math.Asin(xPosInCollider / radius) / (float)Math.PI;
            y += (int)halfWavesFromTop * halfWave;
            y = Calc.Clamp(y, 0, Height);
            return y;
        }
        public override void Update()
        {
            if (Scene.GetPlayer() is not Player player || !Initialized) return;
            base.Update();
            FlowTimer += Engine.DeltaTime * FlowRateMult;
            if (Enabled)
            {
                if (PrevPosition.Y != Bottom && Platform.Top == Bottom)
                {
                    WaitForUpInput = true;
                }
                if (WaitForUpInput)
                {
                    PlatformTo(Bottom, true);
                    if (Input.MoveY == -1 && Math.Abs(Platform.CenterX - player.CenterX) < Platform.Width)
                    {
                        WaitForUpInput = false;
                    }
                    else
                    {
                        return;
                    }
                }
                WasRidingPlatform = RidingPlatform;
                RidingPlatform = player.IsRiding(Platform);
                if (WasRidingPlatform && RidingPlatform)
                {
                    stepOffset = (stepOffset + Math.Sign(Platform.Top - PrevPosition.Y)) % stepSkip;
                    if (stepOffset < 0) stepOffset = stepSkip - Math.Abs(stepOffset);
                    RecalculatePoints();
                }
                float xPosInCollider = Calc.Clamp(player.X - CenterX, -Radius, Radius) * XFlipScale;
                float yPosInCollider = Calc.Clamp(player.Bottom - Top, 0, Height);
                float halfWavesFromTop = (yPosInCollider + HalfWave / 2) / HalfWave;
                float distToInt = Math.Abs(float.Round(halfWavesFromTop) - halfWavesFromTop);
                CurrentFloor = ((int)halfWavesFromTop);
                if (RidingPlatform)
                {
                    LastRiddenFloor = ((int)halfWavesFromTop);
                    if (distToInt < (3 / HalfWave))
                    {
                        if (Input.MoveY == 1 || (LastSign == new Vector2(-1, 1) && player.Facing == Facings.Left)
                        || (LastSign == Vector2.One && player.Facing == Facings.Right))
                        {
                            allowDescentTimer = Engine.DeltaTime * 16;
                        }
                        else if (halfWavesFromTop % 1 < 0.01f && ((LastSign == new Vector2(1, -1) && player.Facing == Facings.Right)
                        || (LastSign == -Vector2.One && player.Facing == Facings.Left)))
                        {
                            forceAscentTimer = Engine.DeltaTime * 16;
                        }
                    }
                }
                else if (!Parent.CollideCheck<Player>())
                {
                    LastRiddenFloor = null;
                }
                if (Input.MoveY == -1 || !RidingPlatform)
                {
                    allowDescentTimer = 0;
                }
                if (Input.MoveY == 1 || !RidingPlatform)
                {
                    forceAscentTimer = 0;
                }
                if (distToInt < (1 / HalfWave))
                {
                    if (allowDescentTimer > 0)
                    {
                        allowDescentTimer -= Engine.DeltaTime;
                        if (allowDescentTimer <= 0)
                        {
                            allowDescentTimer = 0;
                        }
                        else
                        {
                            halfWavesFromTop = (yPosInCollider + 4 + HalfWave / 2) / HalfWave;
                        }
                    }
                    else if (forceAscentTimer > 0)
                    {

                        forceAscentTimer -= Engine.DeltaTime;
                        if (forceAscentTimer <= 0)
                        {
                            forceAscentTimer = 0;
                        }
                        else
                        {
                            halfWavesFromTop = (yPosInCollider - 4 + HalfWave / 2) / HalfWave;
                        }

                    }
                }
                else
                {
                    allowDescentTimer = 0;
                    forceAscentTimer = 0;
                }

                if ((int)halfWavesFromTop % 2 != 0) xPosInCollider *= -1;

                float lowThresh = Parent.Col.Width / 2;
                float topThresh = Width / 2;
                float distFromCenter = Math.Abs(xPosInCollider);
                XMult = 1;
                if (RidingPlatform && distFromCenter > lowThresh)
                {
                    float percent = 1 - (distFromCenter - lowThresh) / (topThresh - lowThresh);
                    XMult = 0.5f + 0.5f * percent;
                }
                float y = HalfWave * (float)Math.Asin(xPosInCollider / Radius) / (float)Math.PI;
                y += (int)halfWavesFromTop * HalfWave;
                y = Calc.Clamp(y, 0, Height);

                Vector2 prevPosition = new Vector2(Platform.CenterX, Platform.Top);
                if (PrevPosition.X != prevPosition.X)
                {
                    LastSign.X = Math.Sign(prevPosition.X - PrevPosition.X);
                }
                if (PrevPosition.Y != prevPosition.Y)
                {
                    LastSign.Y = Math.Sign(prevPosition.Y - PrevPosition.Y);
                }
                PrevPosition = prevPosition;
                if (NoCollideTimer > 0)
                {
                    NoCollideTimer -= Engine.DeltaTime;
                    if (NoCollideTimer < 0)
                    {
                        NoCollideTimer = 0;
                    }
                }

                Platform.Collidable = !InElevator && Collidable && NoCollideTimer == 0 && CollidableFlag && !DisablePlatform;
                distFromCenter = Math.Abs(player.CenterX - CenterX);
                CurrentZ = GetZ(y);

                HidingEnabled = false;
                ShadeValue = 0;
                if (player.Bottom >= Top && player.Top <= Bottom)
                {
                    if (LastRiddenFloor.HasValue && LastRiddenFloor.Value % 2 == CurrentFloor % 2 && CurrentZ < 0)
                    {
                        ShadeValue = Math.Abs(CurrentZ) / Radius;
                    }
                    HidingEnabled = false;
                    if (LastRiddenFloor.HasValue)
                    {
                        if (LastRiddenFloor.Value % 2 != CurrentFloor % 2 && distFromCenter < Parent.Col.Width / 2)
                        {
                            Platform.Collidable = false;
                        }
                        int val = Math.Max(0, XFlipScale);
                        HidingEnabled = distFromCenter < Parent.Col.Width / 2 + 4 && LastRiddenFloor.Value % 2 == val && !Parent.Col.InElevator;
                    }
                }

                float top = float.Floor(Top);
                float platY = float.Floor(Platform.Top);
                float prev = float.Floor(PrevPosition.Y);
                if (platY == top && prev != top)
                {
                    YOffset = -1;
                }
                else if (YOffset != 0)
                {
                    if (player.IsRiding(TopPlatform) && !player.IsRiding(Platform))
                    {
                        YOffset = 0;
                    }
                }
                else
                {
                    YOffset = 0;
                }
                Vector2 pos = new Vector2(GetX(y) + CenterX, Calc.Clamp(y + Top + YOffset, Top + YOffset, Bottom));
                Platform.CenterX = pos.X;

                if (player.CollideCheck(Platform))
                {
                    player.Bottom = Platform.Top;
                    //prevents the player from magically falling through the stairs for no apparent reason
                    //it's sloppy but i genuinely tried my best to fix it intuitively and I couldn't figure that shit out
                }
                if (LastFloor != CurrentFloor && Platform.Collidable && !RidingPlatform)
                {
                    Platform.Collidable = false;
                    Platform.MoveToY(pos.Y, 0);
                    Platform.Collidable = true;
                }
                else
                {
                    Platform.MoveToY(pos.Y, 0);
                }
                LastFloor = CurrentFloor;
            }
            else
            {
                ShadeValue = 0;
                HidingEnabled = false;
            }
            if (Collidable)
            {
                if (player.Y < Top)
                {
                    TopPlatform.Collidable = true;
                }
                if ((Input.MoveY == 1 && TopPlatform.HasPlayerRider() && player.CenterX >= CenterX && player.CenterX < CenterX + Platform.Width * 2))
                {
                    Platform.Collidable = true;
                    TopPlatformCollisionTimer = 0.3f;
                }

                if (TopPlatformCollisionTimer > 0)
                {
                    TopPlatform.Collidable = false;
                    TopPlatformCollisionTimer -= Engine.DeltaTime;
                    if (TopPlatformCollisionTimer < 0)
                    {
                        TopPlatformCollisionTimer = 0;
                        waitUntilNotCollidingWithTopPlatform = true;
                    }
                }
                else if (waitUntilNotCollidingWithTopPlatform)
                {
                    if (player.CollideCheck(TopPlatform))
                    {
                        TopPlatform.Collidable = false;
                    }
                    else
                    {
                        TopPlatform.Collidable = true;
                        waitUntilNotCollidingWithTopPlatform = false;
                    }
                }
            }

            if (RidingPlatform && !player.IsRiding(TopPlatform))
            {
                FlowRateMult = Calc.Approach(FlowRateMult, 1, Engine.DeltaTime);
            }
            else
            {
                FlowRateMult = Calc.Approach(FlowRateMult, 0, Engine.DeltaTime);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform.RemoveSelf();
            TopPlatform.RemoveSelf();
            target?.Dispose();
            Renderers.RemoveSelves();
        }
        public void Initialize(Scene scene)
        {
            if (Initialized) return;
            Initialized = true;
            RecalculatePoints(Width / 2, 8, Height, Revolutions, 0);
            Platform = new(Points[^1].XY() + TopCenter, 16, true);
            TopPlatform = new CustomPlatform(new Vector2(Parent.X, Y), (int)(Parent.Width));

            scene.Add(Platform);
            scene.Add(TopPlatform);
        }
        public void RecalculatePoints(float radius, float width, float height, int revolutions, float rotationalOffset)
        {
            Points = GetPoints(radius, height, height / revolutions, rotationalOffset);
            OuterPoints = GetPoints(radius + width / 2, height, height / revolutions, rotationalOffset);
            InnerPoints = GetPoints(radius - width / 2, height, height / revolutions, rotationalOffset);
            Rendered = false;
        }
        public void RecalculatePoints() => RecalculatePoints(Radius, 8, Height, Revolutions, AngleOffset);
        public void Disable()
        {
            YOffset = 0;
            Enabled = false;
            RidingPlatform = false;
            LastRiddenFloor = null;
            XMult = 1;
            LastSign = Vector2.Zero;
            PrevPosition = Vector2.Zero;
            LastFloor = 0;
            Platform.Collidable = false;
            TopPlatform.Collidable = true;
            if (Parent != null && Parent.Col != null)
            {
                Parent.Col.HidesPlayer = false;
                Parent.Col.Collidable = false;
            }
            forceAscentTimer = 0;
            AtTop = WasAtTop = false;
        }
        public void Enable()
        {
            XMult = 1;
            YOffset = 0;
            Enabled = true;
        }
        public void PlatformTo(float y, bool naive = false)
        {
            Platform.CenterX = GetX(y - Top) + CenterX;
            if (naive)
            {
                Platform.Y = Calc.Clamp(y, Top, Bottom);
            }
            else
            {
                Platform.MoveToY(Calc.Clamp(y, Top, Bottom));
            }
        }
        public void DrawCurve(Vector3[] points, Vector2 topCenter, float scale, float radius, float flowOffset)
        {
            float zThreshHigh = radius + 1;
            float zThreshLow = GetLowThresh(radius);
            float flow = flowOffset;
            Color[] colors = [Color.Magenta, Color.Blue, Color.Cyan, Color.Magenta];
            for (int i = 1; i < points.Length; i++)
            {
                float z1 = points[i].Z;
                float z2 = points[i - 1].Z;
                int index = (int)(flow % (colors.Length - 1));
                int indexA = index;
                int indexB = index + 1;
                if (z1 < zThreshHigh && z1 > zThreshLow && z2 < zThreshHigh && z2 > zThreshLow)
                {
                    Color fg = Color.Lerp(colors[indexA], colors[indexB], Ease.SineInOut(flow % 1));
                    Color color = points[i].Z >= 0 ? fg : Color.Lerp(fg, BGColor, Math.Abs(points[i].Z) / radius);
                    Draw.Line(points[i - 1].XY() * scale + topCenter, points[i].XY() * scale + topCenter, color, 1);
                    flow += Engine.DeltaTime;
                }
            }
        }
        public void DrawCurve(Vector3[] points, Vector2 topCenter, float scale, float radius)
        {
            float zThreshHigh = radius;
            float zThreshLow = GetLowThresh(radius);
            for (int i = 1; i < points.Length; i++)
            {
                float z1 = points[i].Z;
                float z2 = points[i - 1].Z;
                if (z1 < zThreshHigh && z1 > zThreshLow && z2 < zThreshHigh && z2 > zThreshLow)
                {
                    Color color = points[i].Z >= 0 ? FGColor : Color.Lerp(FGColor, BGColor, Math.Abs(points[i].Z) / radius);
                    Draw.Line(Points[i - 1].XY() * scale + topCenter, points[i].XY() * scale + topCenter, color);
                }
            }
        }
        public void DrawLines(Vector3[] points, Vector2 topCenter, float scale, float radius)
        {
            float zThreshHigh = radius;
            float zThreshLow = GetLowThresh(radius);

            for (int i = stepOffset % stepSkip; i < points.Length; i += stepSkip)
            {
                float z1 = points[i].Z;
                if (z1 < zThreshHigh && z1 > zThreshLow)
                {
                    Color color = points[i].Z >= 0 ? FGColor : Color.Lerp(FGColor, BGColor, Math.Abs(points[i].Z) / radius);
                    Vector2 pos = points[i].XY() * scale + topCenter;
                    float lerp = (pos.X - topCenter.X) / Radius;
                    Vector2 to = new Vector2(topCenter.X + Parent.Col.Width / 2 * lerp, pos.Y + 10);
                    if (z1 < 0)
                    {
                        if (to.X > topCenter.X)
                        {
                            if (TryFindIntersection(pos, to, Parent.Col.TopRight - Parent.Position, Parent.Col.BottomRight - Parent.Position, out Vector2 point))
                            {
                                to = point;
                            }
                        }
                        else
                        {
                            if (TryFindIntersection(pos, to, Parent.Col.Position - Parent.Position, Parent.Col.BottomLeft - Parent.Position, out Vector2 point))
                            {
                                to = point;
                            }
                        }

                    }
                    Draw.Line(pos, to, color);
                }
            }
        }
        public static bool TryFindIntersection(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End, out Vector2 intersection)
        {
            intersection = Vector2.Zero;
            double x1 = line1Start.X, y1 = line1Start.Y;
            double x2 = line1End.X, y2 = line1End.Y;
            double x3 = line2Start.X, y3 = line2Start.Y;
            double x4 = line2End.X, y4 = line2End.Y;

            double denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            if (Math.Abs(denominator) < 0.0001)
            {
                return false;
            }

            double x = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / denominator;
            double y = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / denominator;
            intersection = new Vector2((float)x, (float)y);
            return true;
        }

        public Vector3 GetPoint(float radius, float y, float halfWavelength, float angleOffset = 0)
        {
            return new Vector3(GetX(radius, y, halfWavelength, angleOffset), y, GetZ(radius, y, halfWavelength, angleOffset));
        }
        public float GetX(float radius, float y, float halfWavelength, float angleOffset = 0)
        {
            return (float)(double)(radius * Math.Sin(angleOffset + y * Math.PI / halfWavelength)) * XFlipScale;
        }
        public float GetZ(float radius, float y, float halfWavelength, float angleOffset = 0)
        {
            return (float)(double)(radius * Math.Cos(angleOffset + y * Math.PI / halfWavelength)) * ZFlipScale;
        }
        public Vector3 GetPoint(float y) => GetPoint(Radius, y, HalfWave, AngleOffset);
        public float GetX(float y) => GetX(Radius, y, HalfWave, AngleOffset);
        public float GetZ(float y) => GetZ(Radius, y, HalfWave, AngleOffset);
        public Vector3[] GetPoints(float radius, float height, float halfWave, float angleOffset = 0)
        {
            List<Vector3> points = [];
            int floors = (int)(height / halfWave);
            for (int i = 0; i < floors; i++)
            {
                for (int j = 0; j < halfWave; j++)
                {
                    Vector3 point = GetPoint(radius, i * halfWave + j, halfWave, angleOffset);
                    points.Add(point);
                }
            }
            return [.. points];
        }
        private static ILHook speedHook;
        [OnLoad]
        public static void Load()
        {
            speedHook = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Public | BindingFlags.Instance), modSpeed);
        }
        [OnUnload]
        public static void Unload()
        {
            speedHook.Dispose();
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
        }
        private static float getSpeedXMultiplier()
        {
            float mult = 1;
            foreach (Stairs s in Engine.Scene.Tracker.GetEntities<Stairs>())
            {
                if (s.RidingPlatform && s.Enabled)
                {
                    mult *= s.XMult;
                }
            }
            return mult;
        }
    }
    /*          __
     *          (( <--- 0 halfwaves from top
     *           \\    
     *            )) <- 1 halfwave from top
     *           //
     *          (( <--- 2 halfwaves from top
     *           \\
     *            )) <- 3 halfwaves from top
     *            --
     */
}