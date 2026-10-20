using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TowerStairs")]
    [Tracked]
    public class Stairs : Entity
    {
        [CustomEntity("PuzzleIslandHelper/TowerPlatform")]
        [TrackedAs(typeof(JumpThru))]
        [Tracked]
        public class CustomPlatform : JumpThru
        {
            public float NoCollideTimer;
            public bool Disabled;
            private bool timerBlocked;
            public CustomPlatform(Vector2 position, int width) : base(position, width, true)
            {
            }
            public CustomPlatform(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, true)
            {

            }
            public override void Render()
            {
                base.Render();
                Draw.Rect(Collider, Color.LightGray);
            }
            public override void Update()
            {
                base.Update();
                UpdateCollidableTimer();
                Collidable = !timerBlocked && !Disabled;
                if (Collidable && Input.MoveY == 1)
                {
                    CheckForPlayerCollision();
                }
            }
            public void UpdateCollidableTimer()
            {
                if (NoCollideTimer > 0)
                {
                    timerBlocked = true;
                    NoCollideTimer -= Engine.DeltaTime;
                    if (NoCollideTimer <= 0)
                    {
                        NoCollideTimer = 0;
                        timerBlocked = false;
                    }
                }
            }
            public void CheckForPlayerCollision()
            {
                if (GetPlayerRider() is Player player && player.CollideCheck<Platform, CustomPlatform>(player.Position + Vector2.UnitY * player.Height))
                {
                    //if player is riding this custom platform, but they are also colliding with another seperate platform, make uncollidable.
                    NoCollideTimer = Engine.DeltaTime * 15;
                }
            }
        }
        public class VertexPoint
        {
            public VertexPoint(Vector3 point, int index, Types type)
            {
                SpiralOffset = point;
                Index = index;
                Type = type;
            }
            public enum Types
            {
                Rim,
                Stairs,
                Inside
            }
            public Renderer.Depths Depth;
            public Types Type;
            public Vector3 SpiralOffset;

            public int Index;
            public Vector3 WorldOffset;
            public Vector3 SpiralMult = Vector3.One;
            public Color Tint = Color.White;
            public float LowMultAlpha = 1;
            public float LowMultLerpMax = 1;
            public float Alpha = 1;
            public Color Color;
            public Vector3 Position;
            public Vector3 Offset;
            private Vector3 prevOffset;
            private Vector3 prevPosition;
            private Color prevColor;
            public bool Dirty = true;
            public VertexPoint Next;
            public VertexPoint Prev;
            public bool Visible = true;
            public float Expand; //concept to turn lines into double-polygons
            public bool InColumn, InFront, InLeftHalf;
            public void PreUpdate(Column column)
            {
                Vector3 spiralPosition = SpiralOffset * SpiralMult;
                Position = spiralPosition + WorldOffset;
                Color = Tint * Alpha;
                InColumn = Math.Abs(spiralPosition.X) <= column.Radius;
                InFront = spiralPosition.Z >= 0;
                InLeftHalf = spiralPosition.X < 0;
                if ((InFront && !InLeftHalf) || (Type == Types.Inside && InFront))
                {
                    Depth = Renderer.Depths.JustAbove;
                }
                else if (InFront || (InLeftHalf && !InColumn))
                {
                    Depth = Renderer.Depths.FG;
                }
                else
                {
                    Depth = Renderer.Depths.BG;
                }
            }
            public void Update(Column column)
            {
                if (Type == Types.Stairs && InLeftHalf)
                {
                    Color = Color.Lerp(Color, Color.Black, 0.5f);
                }
                float lerp = (1 - (SpiralMult.X - LowMultThresh) / (1 - LowMultThresh)) * LowMultLerpMax;
                if (lerp > 0)
                {
                    Color = Color.Lerp(Color, Color.Black, lerp);
                }
                if (Prev != null && Prev.Depth == Renderer.Depths.FG && Depth == Renderer.Depths.BG)
                {
                    if (InLeftHalf && InColumn)
                    {
                        Offset.X = column.Left - Position.X;
                    }
                    else
                    {
                        Offset.X = 0;
                    }
                }
                if (prevColor != Color || prevPosition != Position || prevOffset != Offset)
                {
                    Dirty = true;
                }
                prevColor = Color;
                prevPosition = Position;
            }
        }
        public const float LowMultThresh = 0.4333f;
        public class Line : IComparable<Line>
        {
            public int AddIndex;
            public bool Visible = true;
            public Renderer.Depths Type => A.Depth;
            public bool IsRim;
            public float Depth => (A.Position.Z + B.Position.Z) / 2f;
            public VertexPoint A, B;
            public int CompareTo(Line other)
            {
                if ((A.Type == VertexPoint.Types.Inside || B.Type == VertexPoint.Types.Inside) && !(other.A.Type == VertexPoint.Types.Inside || other.B.Type == VertexPoint.Types.Inside))
                {
                    return -1;
                }
                int sign = Math.Sign(Depth - other.Depth);
                if (sign == 0)
                {
                    if (IsRim == other.IsRim)
                    {
                        if (AddIndex < other.AddIndex) return -1;
                        else return 1;
                    }
                    else
                    {
                        if (Depth < 0) return -1;
                        else return 1;
                    }
                }
                return sign;
            }
            public Line(int addIndex, VertexPoint a, VertexPoint b)
            {
                AddIndex = addIndex;
                A = a;
                B = b;
                A.Next = b;
                B.Prev = a;
            }
            /// <summary>
            /// Updates the Indices in the passed in <paramref name="array"/>, starting at <paramref name="index"/>.
            /// </summary>
            /// <param name="array"></param>
            /// <param name="index"></param>
            /// <returns>The number of indices that were set.
            /// If the return value is less than or equal to 0, the line will not be rendered.</returns>
            public int UpdateIndices(int[] array, int index)
            {
                if (Visible && A.Visible && B.Visible)
                {
                    array[index] = A.Index;
                    array[index + 1] = B.Index;
                    return 2;
                }
                else
                {
                    return 0;
                }
            }
        }
        private int testIndex;
        public class Renderer : Entity
        {
            public int[] Indices;
            public int Primitives;
            public Stairs Parent;
            public enum Depths
            {
                BG,
                JustAbove,
                FG
            }
            public Depths Type;
            public Renderer(Stairs parent, Depths type, int depth, int width, int height)
            {
                Parent = parent;
                Type = type;
                Depth = depth;
            }
            public void RenderVertices(Matrix matrix)
            {
                Effect e = ShaderHelperIntegration.TryGetEffect("PuzzleIslandHelper/Shaders/towerStairs");
                PianoUtils.DrawUserIndexedPrimitives(PrimitiveType.LineList, matrix, Parent.Vertices, Parent.VertCount, Indices, Primitives, e);
            }
            public override void Render()
            {
                base.Render();
                if (Indices == null || Indices.Length == 0 || Primitives <= 0) return;
                Draw.SpriteBatch.End();
                RenderVertices(SceneAs<Level>().Camera.Matrix);
                GameplayRenderer.Begin();
            }
        }
        public bool RenderOnce = true;
        public bool Enabled = true;
        public int Revolutions;
        public bool RidingPlatform;
        public CustomPlatform TopPlatform;
        public float TopPlatformCollisionTimer;
        public JumpThru Platform;
        private float NoCollideTimer;
        private float allowDescentTimer;
        public bool DisablePlatform;
        private Vector2 PrevPosition;
        public float Radius => Width / 2;
        public float HalfWave => (Height / Revolutions);
        public int XScale;
        public int ZScale;

        public Color BGColor = Color.Black;
        public Color FGColor = Color.White;
        public int? LastRiddenFloor;
        private string lastRiddenFloor
        {
            get
            {
                if (LastRiddenFloor.HasValue)
                {
                    return '[' + LastRiddenFloor.Value.ToString() + ']';
                }
                return "[N/A]";
            }
        }
        public int LastFloor;
        private float forceAscentTimer;
        public Vector2 LastSign;
        public bool WaitForUpInput;
        public int CurrentFloor;
        public float CurrentZ;
        public bool Initialized;
        public int PlatformYOffset;
        public bool WasRidingPlatform;
        public bool AtTop;
        public bool WasAtTop;
        private bool waitUntilNotCollidingWithTopPlatform;
        public float XMult = 1;

        private FlagList collidableFlag;
        public Renderer[] Renderers = new Renderer[3];
        private List<VertexPoint> rimPoints = [];
        private List<VertexPoint> stairAPoints = [];
        private List<VertexPoint> stairBPoints = [];
        private List<VertexPoint> columnPoints = [];
        public List<Line> Lines = [];
        public VertexPositionColor[] Vertices;
        public int VertCount;
        public Column Column;
        public Vector2 AutoMoveDirection;
        public bool AutoAscend; //wip, automatically moves the player up the spiral staircase without input
        public bool HidePlayer;
        public float ShadeAmount;
        public Vector3[] Points;
        private static ILHook speedHook;
        public Stairs(Column column, Vector2 position, float width, float height, int revolutions, int xFlipScale, int zFlipScale, FlagList collidableFlag = default) : base(position)
        {
            Add(popCoroutine = new Coroutine(false));
            Column = column;
            this.collidableFlag = collidableFlag;
            XScale = xFlipScale;
            ZScale = zFlipScale;
            AddTag(Tags.TransitionUpdate);
            Collider = new Hitbox(width, height);
            Revolutions = revolutions;
            Renderers[0] = new Renderer(this, Renderer.Depths.BG, 4, (int)width, (int)height);
            Renderers[1] = new Renderer(this, Renderer.Depths.JustAbove, 1, (int)width, (int)height);
            Renderers[2] = new Renderer(this, Renderer.Depths.FG, -4, (int)width, (int)height);
            Depth = 2;
            KeyComponent.ForKey(entity: this, Keys.G, () =>
            {
                PopStairs(0.5f, 0.4f, 0.05f);
            });
        }
        public bool matrixTest;
        public float LastActiveTime;
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Renderers);
        }
        public override void Update()
        {
            if (Scene.GetPlayer() is not Player player || !Initialized) return;
            base.Update();
            int moveX = AutoAscend ? (int)AutoMoveDirection.X : Input.MoveX;
            int moveY = AutoAscend ? (int)AutoMoveDirection.Y : Input.MoveY;
            if (Enabled)
            {
                if (PrevPosition.Y != Bottom && Platform.Top == Bottom)
                {
                    WaitForUpInput = true;
                }
                if (WaitForUpInput)
                {
                    PlatformTo(Bottom, true);
                    if (moveY == -1 && player.Bottom > Bottom - HalfWave / 2)
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
                UpdateVertices(false);

                float xPosInCollider = Calc.Clamp(player.X - CenterX, -Radius, Radius) * XScale;
                float yPosInCollider = Calc.Clamp(player.Bottom - Top, 0, Height);
                float halfWavesFromTop = (yPosInCollider + HalfWave / 2) / HalfWave;
                float distToInt = Math.Abs(float.Round(halfWavesFromTop) - halfWavesFromTop);

                CurrentFloor = ((int)halfWavesFromTop);
                if (RidingPlatform)
                {
                    LastRiddenFloor = ((int)halfWavesFromTop);
                    if (distToInt < (3 / HalfWave))
                    {
                        if (moveY == 1 || (LastSign == new Vector2(-1, 1) && player.Facing == Facings.Left)
                        || (LastSign == Vector2.One && player.Facing == Facings.Right))
                        {
                            allowDescentTimer = Engine.DeltaTime * 24;
                        }
                        else if (halfWavesFromTop % 1 < 0.01f && ((LastSign == new Vector2(1, -1) && player.Facing == Facings.Right)
                        || (LastSign == -Vector2.One && player.Facing == Facings.Left)))
                        {
                            forceAscentTimer = Engine.DeltaTime * 24;
                        }
                    }
                }
                else
                {
                    bool outsideColumn = player.Right <= Column.Left || player.Left >= Column.Right;
                    if (outsideColumn)
                    {
                        HidePlayer = false;
                    }
                    if (!CollideCheck<Player>())
                    {
                        LastRiddenFloor = Revolutions;
                    }
                }
                if (moveY == -1 || !RidingPlatform)
                {
                    allowDescentTimer = 0;
                }
                if (moveY == 1 || !RidingPlatform)
                {
                    forceAscentTimer = 0;
                }
                if (distToInt < (1 / HalfWave)) //used to be 1!!! just testing out 2
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

                //Player Speed Manipulation
                float lowThresh = Column.Width / 2;
                float topThresh = Width / 2;
                float distFromCenter = Math.Abs(xPosInCollider);
                if (RidingPlatform && distFromCenter > lowThresh)
                {
                    float percent = 1 - (distFromCenter - lowThresh) / (topThresh - lowThresh);
                    XMult = 0.5f + 0.5f * percent;
                }
                else XMult = 1;
                //

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
                if (Platform != null)
                {
                    Platform.Collidable = player.Speed.Y >= 0 && Collidable && NoCollideTimer == 0 && !DisablePlatform && collidableFlag;
                    if (!RidingPlatform)
                    {
                        if (distFromCenter < Column.Width / 2)
                        {
                            if (LastRiddenFloor.HasValue && LastRiddenFloor.Value % 2 != CurrentFloor % 2)
                            {
                                Platform.Collidable = false;
                            }
                        }
                    }
                }
                distFromCenter = Math.Abs(player.CenterX - CenterX);
                CurrentZ = GetZ(y);
                bool collidingWithStairs = player.CollideCheck(this);

                if (player.Bottom >= Bottom || distFromCenter < Column.Width / 2 + 6)
                {
                    ShadeAmount = 0;
                }
                if (player.Bottom > Top && player.Top <= Bottom)
                {
                    if (LastRiddenFloor.HasValue)
                    {
                        if (LastRiddenFloor.Value % 2 == CurrentFloor % 2)
                        {
                            if (CurrentZ < 0) ShadeAmount = Math.Abs(CurrentZ) / Radius;
                        }
                        else if (distFromCenter < Column.Width / 2 + 3)
                        {
                            Platform.Collidable = false;
                            RidingPlatform = false;
                        }
                        int val = Math.Clamp(Math.Abs(XScale), 0, 1);

                        //if in column AND last floor would have hidden player
                        HidePlayer = distFromCenter < Column.Width / 2 + 6 && LastRiddenFloor.Value % 2 != val;

                    }
                }

                float top = float.Floor(Top);
                float platY = float.Floor(Platform.Top);
                float prev = float.Floor(PrevPosition.Y);

                if (platY == top && prev != top)
                {
                    PlatformYOffset = -1;
                }
                else if (PlatformYOffset != 0)
                {
                    if (player.IsRiding(TopPlatform) && !player.IsRiding(Platform))
                    {
                        PlatformYOffset = 0;
                    }
                }
                else
                {
                    PlatformYOffset = 0;
                }
                Vector2 pos = new Vector2(GetX(y) + CenterX, Calc.Clamp(y + Top + PlatformYOffset, Top + PlatformYOffset, Bottom));
                Platform.CenterX = pos.X;

                if (RidingPlatform)
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
                RidingPlatform = Platform.HasPlayerRider();
                //push the player onto the top platform so they don't fall through it when they reach the top.
                if (RidingPlatform && PrevPosition.Y != Top && Platform.ExactPosition.Y == Top)
                {
                    player.MoveV(TopPlatform.Top - player.Bottom);
                }
                LastFloor = CurrentFloor;
            }
            else
            {
                float posInCollider = Math.Clamp(player.Bottom - Y, 0, Height);
                LastFloor = CurrentFloor = (int)Math.Max(0, (posInCollider + HalfWave / 2) / HalfWave);
                LastRiddenFloor = LastFloor;
                WasRidingPlatform = false;
                RidingPlatform = false;
                CurrentZ = GetZ(posInCollider);
                LastSign.X = Math.Sign(GetX(posInCollider - 1));
                LastSign.Y = -1;
                PrevPosition = BottomCenter;
                ShadeAmount = 0;
                HidePlayer = false;
            }
            if (Collidable)
            {
                if (player.Y < Top)
                {
                    TopPlatform.Collidable = true;
                }
                //let player descend from top of stairs if they are holding down
                if ((moveY == 1 && TopPlatform.HasPlayerRider() && player.CenterX >= CenterX && player.CenterX < CenterX + Platform.Width * 2))
                {
                    Platform.Collidable = true;
                    TopPlatformCollisionTimer = 0.3f;
                }

                //top platform is intangible until the timer reaches 0 AND the player is no longer colliding with it.
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
            if (RidingPlatform && Platform.ExactPosition.Y != Top)
            {
                LastActiveTime = Scene.TimeActive;
            }

        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            foreach (Line line in Lines)
            {
                if (line.A.Position.Z == 0 || line.B.Position.Z == 0)
                {
                    Draw.Line(line.A.Position.XY(), line.B.Position.XY(), Color.Magenta);
                }
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform.RemoveSelf();
            TopPlatform.RemoveSelf();
            Renderers.RemoveSelves();
        }
        public IEnumerator PopStairOut(int index, float time)
        {
            VertexPoint rim = rimPoints[index];
            VertexPoint stair = stairAPoints[index];
            VertexPoint stairB = stairBPoints[index];
            VertexPoint inside = columnPoints[index];
            rim.Visible = true;
            stair.Visible = true;
            stairB.Visible = true;
            inside.Visible = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                float ease = Ease.BounceOut(i);
                float mult = Calc.LerpClamp(LowMultThresh, 1, ease);
                rim.SpiralMult.X = stair.SpiralMult.X = mult;
                stairB.Alpha = Calc.LerpClamp(0, 1, i);
                inside.Alpha = Calc.LerpClamp(1f, 0, i);
                yield return null;
            }
            rim.SpiralMult.X = stair.SpiralMult.X = 1;
            stairB.Alpha = 1;
            inside.Alpha = 0;
            inside.Visible = false;
        }
        public IEnumerator PopAllStairsOut(float singleStairDuration, float startDelay, float endDelay, float delayLerpMult = 1)
        {
            float delay = startDelay;
            for (int i = 0; i < rimPoints.Count; i++)
            {
                Add(new Coroutine(PopStairOut(i, singleStairDuration)));
                yield return Calc.LerpClamp(startDelay, endDelay, (float)i / rimPoints.Count * delayLerpMult);

            }
        }
        private Coroutine popCoroutine;
        public void PopStairs(float singleStairDuration, float startDelay, float endDelay)
        {
            foreach (VertexPoint p in rimPoints)
            {
                p.SpiralMult.X = LowMultThresh;
                p.Visible = false;
            }
            foreach (VertexPoint p in stairAPoints)
            {
                p.SpiralMult.X = LowMultThresh;
                p.Visible = false;
            }
            foreach (VertexPoint p in stairBPoints)
            {
                p.Alpha = 0;
                p.Visible = false;
            }
            foreach(VertexPoint p in columnPoints)
            {
                p.Alpha = 1;
                p.Visible = true;
            }
            popCoroutine.Replace(PopAllStairsOut(singleStairDuration, startDelay, endDelay, 3));
        }
        public void Initialize(Scene scene)
        {
            if (Initialized) return;
            Initialized = true;
            RecalculatePoints(Width / 2, Height, Revolutions, 0);
            Platform = new(Points[^1].XY() + TopCenter, 16, true);
            TopPlatform = new CustomPlatform(new Vector2(Column.X, Y), (int)(Column.Width));

            scene.Add(Platform);
            scene.Add(TopPlatform);
        }
        public void CreateVertices(float columnRadius, float stairsRadiusOffset, int step = 1)
        {
            VertCount = 0;
            rimPoints.Clear();
            stairAPoints.Clear();
            stairBPoints.Clear();
            columnPoints.Clear();
            Lines.Clear();
            for (int i = 0; i < Points.Length; i += step)
            {
                Vector3 rimPoint = Points[i];
                Vector3 insidePoint = new Vector3(rimPoint.X * (columnRadius / Radius), rimPoint.Y, rimPoint.Z * (columnRadius / Radius));
                rimPoint.X *= 1.4f;
                VertexPoint rim = new(rimPoint, VertCount, VertexPoint.Types.Rim);
                VertexPoint stair = new(rimPoint, VertCount + 1, VertexPoint.Types.Stairs);
                VertexPoint inside = new(insidePoint, VertCount + 2, VertexPoint.Types.Stairs);
                VertexPoint column = new(insidePoint, VertCount + 3, VertexPoint.Types.Inside);
                rimPoints.Add(rim);
                stairAPoints.Add(stair);
                stairBPoints.Add(inside);
                columnPoints.Add(column);
                VertCount += 4;
            }
            for (int i = 0; i < rimPoints.Count; i++)
            {
                Line line = new Line(Lines.Count, stairAPoints[i], stairBPoints[i]) { IsRim = false };
                Lines.Add(line);
                if (i > 0)
                {
                    Lines.Add(new Line(Lines.Count, rimPoints[i - 1], rimPoints[i]) { IsRim = true });
                    Lines.Add(new Line(Lines.Count, columnPoints[i - 1], columnPoints[i]));
                }
            }
            Vertices = new VertexPositionColor[VertCount];
            worldOffset = TopCenter.ToVec3();
            UpdateVertices(true);
        }
        public const int RenderPadX = 8;
        public Vector3 WorldOffset
        {
            get => worldOffset;
            set
            {
                if (worldOffset != value)
                {
                    Dirty = true;
                }
                worldOffset = value;
            }
        }
        public bool Dirty;
        private Vector3 worldOffset;
        private int[] indexTypeCount = new int[3];
        public void UpdateVertices(bool allowLineSort)
        {
            int index = 0;
            for (int i = 0; i < VertCount; i += 4, index++)
            {
                VertexPoint rim = rimPoints[index];
                rim.WorldOffset = worldOffset;
                rim.PreUpdate(Column);
                VertexPoint stair = stairAPoints[index];
                stair.WorldOffset = worldOffset;
                stair.PreUpdate(Column);
                VertexPoint rimToInside = stairBPoints[index];
                rimToInside.WorldOffset = worldOffset;
                rimToInside.PreUpdate(Column);
                VertexPoint columnPoint = columnPoints[index];
                columnPoint.WorldOffset = worldOffset;
                columnPoint.PreUpdate(Column);
            }
            bool dirty = false;
            index = 0;
            for (int i = 0; i < VertCount; i += 4, index++)
            {
                VertexPoint rim = rimPoints[index];
                rim.Update(Column);
                if (rim.Dirty)
                {
                    Vertices[i].Position = rim.Position + rim.Offset;
                    Vertices[i].Color = rim.Color;
                    rim.Dirty = false;
                    dirty = true;
                }
                VertexPoint stair = stairAPoints[index];
                stair.Update(Column);
                if (stair.Dirty)
                {
                    Vertices[i + 1].Position = stair.Position + stair.Offset;
                    Vertices[i + 1].Color = stair.Color;
                    stair.Dirty = false;
                    dirty = true;
                }
                VertexPoint rimToInside = stairBPoints[index];
                rimToInside.Update(Column);
                if (rimToInside.Dirty)
                {
                    Vertices[i + 2].Position = rimToInside.Position + rimToInside.Offset;
                    Vertices[i + 2].Color = rimToInside.Color;
                    rimToInside.Dirty = false;
                    dirty = true;
                }
                VertexPoint columnPoint = columnPoints[index];
                columnPoint.Update(Column);
                if (columnPoint.Dirty)
                {
                    Vertices[i + 3].Position = columnPoint.Position + columnPoint.Offset;
                    Vertices[i + 3].Color = columnPoint.Depth switch
                    {
                        Renderer.Depths.JustAbove => Color.Black,
                        _ => Color.Transparent
                    };
                    columnPoint.Dirty = false;
                    dirty = true;
                }
            }
            if (dirty)
            {
                if (allowLineSort)
                {
                    Lines.Sort();
                }
                UpdateRenderers();
            }
        }
        public void UpdateRenderers()
        {
            indexTypeCount = new int[3];
            foreach (Line line in Lines)
            {
                indexTypeCount[(int)line.Type] += 2;
            }
            for (int i = 0; i < 3; i++)
            {
                Renderer r = Renderers[i];
                int indexCount = indexTypeCount[(int)r.Type];
                if (indexCount <= 0)
                {
                    r.Visible = false;
                }
                else
                {
                    r.Visible = true;
                    if (r.Indices == null || r.Indices.Length != indexCount)
                    {
                        r.Indices = new int[indexCount];
                    }
                    r.Primitives = 0;
                    int index = 0;
                    foreach (Line line in Lines)
                    {
                        if (line.Type == r.Type)
                        {
                            int next = line.UpdateIndices(r.Indices, index);
                            index += next;
                            if (next > 0)
                            {
                                r.Primitives++;
                            }
                        }
                    }
                }
            }

        }
        public void RecalculatePoints(float radius, float height, int revolutions, float rotationalOffset)
        {
            Points = GetPoints(radius, height, height / revolutions, rotationalOffset);
            CreateVertices(Column.Radius, Radius, 3);
        }
        public void RecalculatePoints() => RecalculatePoints(Radius, Height, Revolutions, 0);

        public void Enable()
        {
            XMult = 1;
            PlatformYOffset = 0;
            Enabled = true;
        }
        public void Disable()
        {
            PlatformYOffset = 0;
            Enabled = false;
            RidingPlatform = false;
            LastRiddenFloor = null;
            XMult = 1;
            LastSign = Vector2.Zero;
            PrevPosition = Vector2.Zero;
            LastFloor = 0;
            Platform.Collidable = false;
            TopPlatform.Collidable = true;
            forceAscentTimer = 0;
            AtTop = WasAtTop = false;
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
        public Vector3 GetPoint(float radius, float y, float halfWavelength, float angleOffset = 0)
        {
            return new Vector3(GetX(radius, y, halfWavelength, angleOffset), y, GetZ(radius, y, halfWavelength, angleOffset));
        }
        public Vector3 GetPoint(float y) => GetPoint(Radius, y, HalfWave, 0);
        public float GetX(float radius, float y, float halfWavelength, float angleOffset = 0)
        {
            return (float)(double)(radius * Math.Sin(angleOffset + y * Math.PI / halfWavelength)) * XScale;
        }
        public float GetX(float y) => GetX(Radius, y, HalfWave, 0);
        public float GetY(float y, float x, float radius, float halfWave)
        {
            float xPosInCollider = Calc.Clamp(x - CenterX, -radius, radius) * XScale;
            float yPosInCollider = Calc.Clamp(y - Top, 0, Height);
            float halfWavesFromTop = (yPosInCollider + halfWave / 2) / halfWave;
            if ((int)halfWavesFromTop % 2 != 0) xPosInCollider *= -1;
            y = halfWave * (float)Math.Asin(xPosInCollider / radius) / (float)Math.PI;
            y += (int)halfWavesFromTop * halfWave;
            y = Calc.Clamp(y, 0, Height);
            return y;
        }
        public float GetZ(float radius, float y, float halfWavelength, float angleOffset = 0)
        {
            return (float)(double)(radius * Math.Cos(angleOffset + y * Math.PI / halfWavelength)) * ZScale;
        }
        public float GetZ(float y) => GetZ(Radius, y, HalfWave, 0);

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