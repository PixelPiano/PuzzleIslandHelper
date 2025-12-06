using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/WaveSlope")]
    [Tracked(true)]
    public class WaveSlope : Entity
    {

        public static readonly Ease.Easer CircleOut = (float t) => (float)(double)Math.Sqrt(1 - (double)Math.Pow(t - 1, 2));
        public static readonly Ease.Easer CircleIn = (float t) => (float)(1 - (double)Math.Sqrt(1 - (double)Math.Pow(t, 2)));
        public static readonly Ease.Easer CircleInOut = Ease.Follow(CircleIn, CircleOut);//(float t) => (float)(t < 0.5f ? (1 - Math.Sqrt(1 - Math.Pow(2 * t, 2))) / 2 : (Math.Sqrt(1 - Math.Pow(-2 * t + 2, 2)) + 1) / 2);
        public static readonly Ease.Easer CircleOutIn = Ease.Follow(CircleOut, CircleIn);
        public static readonly Ease.Easer SineOutIn = Ease.Follow(Ease.SineOut, Ease.SineIn);
        internal struct SlopePoint
        {
            public bool Collidable = true;
            public Ease.Easer Easer = Ease.Linear;
            public Vector2 Position;
            public float X
            {
                get => Position.X;
                set => Position.X = value;
            }
            public float Y
            {
                get => Position.Y;
                set => Position.Y = value;
            }
            public SlopePoint(Vector2 position, Ease.Easer ease = null)
            {
                Position = position;
                Easer = ease ?? Ease.Linear;
            }
        }
        internal SlopePoint[] Points;
        public FlagList FlagOnUseSlope, CollidableFlag;
        public JumpThru Platform;
        public int Floor => Math.Max(Index, HighIndex);
        private int selectedLevelEase;
        private int customDepth;
        public int Index, HighIndex;
        public float NoCollideTimer;
        private float platformWidth;
        public bool OnlyIfHoldingUp;
        public string FlagID;
        public string FlagPrefix;
        public Vector2 PrevPosition;
        public int LastFace;
        private bool floorDisabledFlag;
        public bool DisablePlatform;
        public WaveSlope(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            FlagID = data.Attr("flagID");
            FlagPrefix = "WaveSlope:" + FlagID;
            platformWidth = data.Float("platformWidth", 16);
            customDepth = Depth = data.Int("depth");
            OnlyIfHoldingUp = data.Bool("requireUpInput");
            CollidableFlag = data.FlagList("collidableFlag");
            FlagOnUseSlope = data.FlagList("flagWhenOnSlope");
            Vector2[] points = data.NodesWithPosition(offset);
            Points = [.. points.Select(item => new SlopePoint(item, null))];
            Collider = points.Collider();
        }
        private float c = 0.05f;
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Platform = new JumpThru(Points[0].Position, (int)platformWidth, true);
            Platform.Depth = customDepth;
            scene.Add(Platform);
            Index = 0;
            HighIndex = 1;
        }
        public float Percent;
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetPlayer() is Player player)
            {
                if (player.OnGround())
                {
                    if (player != Platform.GetPlayerRider())
                    {
                        NoCollideTimer = 0.5f;
                    }
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;

            if (NoCollideTimer > 0)
            {
                NoCollideTimer -= Engine.DeltaTime;
                if (NoCollideTimer < 0)
                {
                    NoCollideTimer = 0;
                }
            }
            Platform.Collidable = Collidable && NoCollideTimer == 0 && CollidableFlag && !floorDisabledFlag && !DisablePlatform;
            bool riding = player.IsRiding(Platform);
            int prevHighIndex = HighIndex;
            int prevIndexIndex = Index;
            if (Points.Length > 2)
            {
                if (riding)
                {
                    if (PrevPosition.X < Platform.CenterX)
                    {
                        LastFace = 1;
                        //platform moving right
                    }
                    else if (PrevPosition.X > Platform.CenterX)
                    {
                        LastFace = -1;
                        //platform moving left
                    }
                    Vector2 pH = Points[HighIndex].Position;
                    Vector2 pL = Points[Index].Position;
                    float distToHighY = MathHelper.Distance(Platform.Y, pH.Y);
                    float distToLowY = MathHelper.Distance(Platform.Y, pL.Y);
                    float distToHighX = MathHelper.Distance(Platform.CenterX, pH.X);
                    float distToLowX = MathHelper.Distance(Platform.CenterX, pL.X);
                    if (distToHighY < 2 && distToHighX < 2)
                    {
                        if (Index < Points.Length - 2)
                        {
                            //if player holding up
                            //OR platform facing left at the end of a left-rising slope
                            //OR platform facing right at the end of a right-rising slope
                            if (Input.MoveY < 0 || (pH.X < pL.X && LastFace == -1) || (pH.X > pL.X && LastFace == 1))
                            {
                                //move up one level
                                Index++;
                                HighIndex++;
                                LastFace = 0;
                            }
                        }
                    }
                    else if (distToLowY < 2 && distToLowX < 2)
                    {
                        if (Index > 0)
                        {
                            //if player holding down
                            //OR platform facing left at the start of a right-rising slope
                            //OR platform facing right at the start of a left-rising slope
                            if (Input.MoveY > 0 || (pH.X > pL.X && LastFace == -1) || (pL.X > pH.X && LastFace == 1))
                            {
                                HighIndex = Index;
                                Index--;
                                LastFace = 0;
                            }
                        }
                    }
                }
                else
                {
                    (int, int) adjust = IndexAdjustment(player, riding);
                    if (adjust.Item1 > -1 && adjust.Item2 > -1)
                    {
                        if (Points[adjust.Item1].Collidable)
                        {
                            Index = adjust.Item1;
                            HighIndex = adjust.Item2;
                        }
                    }
                }
            }
            Vector2 low = Points[Index].Position;
            Vector2 high = Points[HighIndex].Position;

            if (high.X < low.X)
            {
                (low, high) = (high, low);
            }
            float farLeft = Math.Min(high.X, low.X);
            float farRight = Math.Max(high.X, low.X);
            float playerPosition = player.CenterX;
            float center = Calc.Clamp(playerPosition, farLeft, farRight);
            float percent = Calc.Clamp((center - farLeft) / (farRight - farLeft), 0, 1);
            Percent = percent;
            Vector2 point = GetPoint(low, high, percent, Index);
            PrevPosition = new Vector2(Platform.CenterX, Platform.Y);
            Platform.CenterX = center;
            if (Points.Length > 2 && (Index != prevIndexIndex || HighIndex != prevHighIndex))
            {
                bool collidable = Platform.Collidable;
                Platform.Collidable = false;
                Platform.MoveToY(point.Y, 0);
                Platform.Collidable = collidable;
            }
            else
            {
                Platform.MoveToY(point.Y, 0);
            }
            floorDisabledFlag = SceneAs<Level>().Session.GetFlag(FlagPrefix + Floor + ":Off");
        }
        public virtual (int, int) IndexAdjustment(Player player, bool riding)
        {
            //keep the platform updating to catch the player if they jump between levels
            int newNext = -1;
            int newIndex = -1;
            for (int i = 0; i < Points.Length; i++)
            {
                if (Points[i].Y > player.Bottom)
                {
                    if (i < Points.Length - 1)
                    {
                        newIndex = i;
                        newNext = i + 1;
                    }
                    else
                    {
                        newIndex = i - 1;
                        newNext = i;
                    }
                }
            }
            return (newIndex, newNext);
        }
        public override void Render()
        {
            base.Render();
            DrawSlope(Vector2.Zero, 30, Color.White);
        }
        public void DrawSlope(Vector2 offset, int steps, Color color)
        {
            for (int i = 0; i < Points.Length - 1; i++)
            {
                DrawLine(offset, i, 30, Color.White);
            }
        }
        public void DrawLine(Vector2 offset, int level, int steps, Color colorA, Color colorB)
        {
            if (level < 0 || level > Points.Length - 2) return;
            Vector2 from = Points[level].Position;
            Vector2 to = Points[level + 1].Position;
            if (from.X > to.X)
            {
                (from, to) = (to, from);
            }
            Vector2 vector = from;
            float inc = 1f / steps;

            for (float j = 0; j < 1; j += inc)
            {
                Vector2 point = GetPoint(from, to, j, level);
                Draw.Line(vector + offset, point + offset, Color.Lerp(colorA, colorB, j));
                vector = point;
            }
            Draw.Line(vector, GetPoint(from, to, 1, level), colorB);
        }
        public void DrawLine(Vector2 offset, int level, int steps, Color color)
        {
            if (level < 0 || level + 1 > Points.Length - 1) return;
            Vector2[] points = GetLinePoints(level, steps);
            Vector2 vector = points[0];
            foreach (Vector2 p in points)
            {
                Draw.Line(vector + offset, p + offset, color);
                vector = p;
            }
        }

        public Vector2[] GetLinePoints(int level, int steps, bool includeEnd = true, bool eased = true)
        {
            //(r*sin(c*h), h)
            /* r = radius
             * c = center
             * h = height
             * 
             * 
             * 
             */
            /*            Vector2 getPoint(float r, float c, float h)
                        {
                            return new Vector2(r * (float)Math.Sin(c * h), h);
                        }
                        List<Vector2> fancyPoints = [Vector2.Zero, Vector2.Zero];
                        float r = 16;
                        float c = 1; //?
                        float h = 40;
                        for (int i = 0; i < h; i += 3)
                        {
                            fancyPoints.Add(getPoint(r, c, i) + Vector2.UnitY * -(h * level));
                        }
                        return [.. fancyPoints];*/
            Vector2 from = Points[level].Position;
            Vector2 to = Points[level + 1].Position;
            if (from.X > to.X)
            {
                (from, to) = (to, from);
            }
            List<Vector2> points = [];
            Vector2 vec = from;
            float length = (to - from).Length();
            int move = (int)(to - from).Length() / steps;
            while (vec != to)
            {
                float vecLen = (vec - from).Length();
                points.Add(GetPoint(from, to, vecLen / length, level));
                vec = Calc.Approach(vec, to, move);
            }
            if (includeEnd)
            {
                points.Add(vec);
            }
            return [.. points];
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform?.RemoveSelf();
        }
        public Vector2 GetPoint(Vector2 start, Vector2 end, float percent, int level)
        {
            float x = Calc.LerpClamp(start.X, end.X, percent);
            float y = Calc.LerpClamp(start.Y, end.Y, percent);
            float easeX = Calc.LerpClamp(start.X, end.X, Points[level].Easer(percent));
            float easeY = Calc.LerpClamp(start.Y, end.Y, Points[level].Easer(percent));
            return new(x, easeY);
        }

    }
}
