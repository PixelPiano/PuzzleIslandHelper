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
using static Celeste.Mod.PuzzleIslandHelper.Beta.ArtifactSlot;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TowerStairs")]
    [Tracked]
    public class Stairs : Entity
    {
        [TrackedAs(typeof(JumpThru))]
        public class CustomPlatform : JumpThru
        {
            public CustomPlatform(Vector2 position, int width) : base(position, width, true)
            {

            }
            public override void Render()
            {
                base.Render();
                Draw.Rect(Collider,Color.LightGray);
            }
        }
        public bool Enabled;
        public Tower Parent;
        public InvisibleBarrier[] Safeguards = new InvisibleBarrier[2];
        public int Floors;
        public bool RidingPlatform;
        public bool PlayerFading;
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
        public float HalfWave => (Height / Floors) / 2;
        public float ZThreshHigh => Radius;
        public float ZThreshLow
        {
            get => GetLowThresh(Radius);
        }
        public float GetLowThresh(float radius)
        {
            float halfCol = Parent.Col.Width / 2;
            float dist = radius - halfCol;
            return -radius + (dist / 2);
        }
        public Color BGColor = Color.Black;
        public Color FGColor = Color.White;
        public int? LastRodeFloor;
        public int LastFloor;
        private float forceAscentTimer;
        public Vector2 LastSign;
        public bool WaitForUpInput;
        public VirtualRenderTarget target;
        private bool rendered;
        public int CurrentFloor;
        public float CurrentZ;
        public bool HidingEnabled;
        public bool Initialized;
        public Stairs(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Int("levels", 3))
        {

        }
        public Stairs(Vector2 position, float width, float height, int floors) : base(position)
        {
            AddTag(Tags.TransitionUpdate);
            Collider = new Hitbox(width, height);
            Floors = floors;
            target = VirtualContent.CreateRenderTarget("stairs", (int)width + 32, (int)height);
            Add(new BeforeRenderHook(BeforeRender));
        }
        private void BeforeRender()
        {
            if (rendered || OuterPoints == null || InnerPoints == null || Points == null) return;
            rendered = true;
            target.SetAsTarget(true);
            Draw.SpriteBatch.Begin();
            DrawCurve(OuterPoints, new Vector2(Width / 2 + 16, 0), 1, Width / 2 + 4);
            DrawCurve(Points, new Vector2(Width / 2 + 16, 0), 1, Width / 2);
            DrawCurve(InnerPoints, new Vector2(Width / 2 + 16, 0), 1, Width / 2 - 4);
            Draw.SpriteBatch.End();
        }
        public override void Render()
        {
            base.Render();
            if (Enabled)
            {
                Draw.SpriteBatch.Draw(target, Position - Vector2.UnitX * 16, Color.White);
            }
        }
        public void Initialize(Scene scene)
        {
            if (Initialized) return;
            Initialized = true;
            Points = GetPoints(Width / 2, Height, Height / Floors);
            OuterPoints = GetPoints(Width / 2 + 4, Height, Height / Floors);
            InnerPoints = GetPoints(Width / 2 - 4, Height, Height / Floors);
            Platform = new(Points[^1].XY() + TopCenter, 16, true);
            TopPlatform = new CustomPlatform(Position, (int)Width);
            Safeguards[0] = new InvisibleBarrier(TopLeft - Vector2.UnitX * (Platform.Width / 2f + 8), 8, Height);
            Safeguards[1] = new InvisibleBarrier(TopRight + Vector2.UnitX * (Platform.Width / 2f), 8, Height);
            scene.Add(Platform);
            scene.Add(TopPlatform);
            scene.Add(Safeguards);
        }

        public void Disable()
        {
            Enabled = false;
            SetSafeguards(false);
            RidingPlatform = false;
            LastRodeFloor = null;
            xMult = 1;
            LastSign = Vector2.Zero;
            PrevPosition = Vector2.Zero;
            LastFloor = 0;
            Platform.Collidable = false;
            TopPlatform.Collidable = true;
            Parent.Col.HidesPlayer = false;
            Parent.Col.Collidable = false;
            forceAscentTimer = 0;
        }
        public void Enable()
        {
            SetSafeguards(true);
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
        public override void Update()
        {
            if (Scene.GetPlayer() is not Player player || !Initialized) return;
            base.Update();
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
                RidingPlatform = player.IsRiding(Platform);
                float xPosInCollider = Calc.Clamp(player.X - CenterX, -Radius, Radius);
                float yPosInCollider = Calc.Clamp(player.Bottom - Top, 0, Height);
                float halfWavesFromTop = (yPosInCollider + HalfWave / 2) / HalfWave;
                float distToInt = Math.Abs(float.Round(halfWavesFromTop) - halfWavesFromTop);
                CurrentFloor = ((int)halfWavesFromTop);
                if (RidingPlatform)
                {
                    LastRodeFloor = ((int)halfWavesFromTop);
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
                    LastRodeFloor = null;
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
                xMult = 1;
                if (RidingPlatform && distFromCenter > lowThresh)
                {
                    float percent = 1 - (distFromCenter - lowThresh) / (topThresh - lowThresh);
                    xMult = 0.5f + 0.5f * percent;
                }
                float y = HalfWave * (float)Math.Asin(xPosInCollider / Radius) / (float)Math.PI;
                y += (int)halfWavesFromTop * HalfWave;
                y = Calc.Clamp(y, 0, Height);
                Vector2 pos = new Vector2(Platform.CenterX, Platform.Top);
                if (PrevPosition.X != pos.X)
                {
                    LastSign.X = Math.Sign(pos.X - PrevPosition.X);
                }
                if (PrevPosition.Y != pos.Y)
                {
                    LastSign.Y = Math.Sign(pos.Y - PrevPosition.Y);
                }
                PrevPosition = pos;
                pos.X = GetX(y) + CenterX;
                pos.Y = Calc.Clamp(y + Top, Top, Bottom);

                if (Safeguards[0].CollideCheck(player))
                {
                    player.Left = Safeguards[0].Right;
                }
                if (Safeguards[1].CollideCheck(player))
                {
                    player.Right = Safeguards[1].Left;
                }
                if (NoCollideTimer > 0)
                {
                    NoCollideTimer -= Engine.DeltaTime;
                    if (NoCollideTimer < 0)
                    {
                        NoCollideTimer = 0;
                    }
                }
                Platform.Collidable = Collidable && NoCollideTimer == 0 && CollidableFlag && !DisablePlatform;
                distFromCenter = Math.Abs(player.CenterX - CenterX);
                CurrentZ = GetZ(y);

                HidingEnabled = false;
                ShadeValue = 0;
                if (player.Bottom >= Top && player.Top <= Bottom)
                {
                    if (LastRodeFloor.HasValue && LastRodeFloor.Value % 2 == CurrentFloor % 2 && CurrentZ < 0)
                    {
                        ShadeValue = Math.Abs(CurrentZ) / Radius;
                    }
                    HidingEnabled = false;
                    if (LastRodeFloor.HasValue)
                    {
                        if (LastRodeFloor.Value % 2 != CurrentFloor % 2 && distFromCenter < Parent.Col.Width / 2)
                        {
                            Platform.Collidable = false;
                        }
                        HidingEnabled = distFromCenter < Parent.Col.Width / 2 + 4 && LastRodeFloor.Value % 2 == 1 && !Parent.Col.InColumn;
                    }
                }

                Platform.CenterX = pos.X;
                if (player.CollideCheck(Platform))
                {
                    player.Bottom = Platform.Top;
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
            if (player.Bottom - 1 < Top)
            {
                TopPlatform.Collidable = Collidable;
            }
            if ((Input.MoveY == 1 && TopPlatform.HasPlayerRider() && player.CenterX >= CenterX && player.CenterX < CenterX + Platform.Width * 2))
            {
                TopPlatformCollisionTimer = 0.3f;
                //Parent.CanEnter = true;
            }
            if (TopPlatformCollisionTimer > 0)
            {
                TopPlatform.Collidable = false;
                TopPlatformCollisionTimer -= Engine.DeltaTime;
                if (TopPlatformCollisionTimer < 0)
                {
                    TopPlatformCollisionTimer = 0;
                }
            }
            else if (player.CollideCheck(TopPlatform))
            {
                TopPlatform.Collidable = false;
            }
        }
        public float ShadeValue;
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
                    Color color = points[i].Z >= 0 ? FGColor : Color.Lerp(FGColor, BGColor, Math.Abs(Points[i].Z) / radius);
                    Draw.Line(Points[i - 1].XY() * scale + topCenter, points[i].XY() * scale + topCenter, color);
                }
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform.RemoveSelf();
            TopPlatform.RemoveSelf();
            Safeguards.RemoveSelf();
            target?.Dispose();
        }
        public void SetSafeguards(bool state)
        {
            Safeguards[0].Active = state;
            Safeguards[1].Active = state;
            Safeguards[0].Collidable = state;
            Safeguards[1].Collidable = state;
        }

        public static Vector3 GetPoint(float radius, float y, float halfWavelength)
        {
            return new Vector3(GetX(radius, y, halfWavelength), y, GetZ(radius, y, halfWavelength));
        }
        public static float GetX(float radius, float y, float halfWavelength)
        {
            return (float)(double)(radius * Math.Sin(y * Math.PI / halfWavelength));
        }
        public static float GetZ(float radius, float y, float halfWavelength)
        {
            return (float)(double)(radius * Math.Cos(y * Math.PI / halfWavelength));
        }
        public Vector3 GetPoint(float y) => GetPoint(Radius, y, HalfWave);
        public float GetX(float y) => GetX(Radius, y, HalfWave);
        public float GetZ(float y) => GetZ(Radius, y, HalfWave);
        public Vector3[] GetPoints(float radius, float height, float waveHeight)
        {
            List<Vector3> points = [];
            float halfWave = waveHeight / 2;
            int floors = (int)(height / halfWave);
            for (int i = 0; i < floors; i++)
            {
                for (int j = 0; j < halfWave; j++)
                {
                    points.Add(GetPoint(radius, i * halfWave + j, halfWave));
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
        private static float xMult = 1;
        private static float getSpeedXMultiplier()
        {
            return xMult;
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