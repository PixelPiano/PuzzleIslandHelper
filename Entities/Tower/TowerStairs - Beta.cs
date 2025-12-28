using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    public class BetaTowerStairs : WaveSlope
    {
        public class Railing : Entity
        {
            public VirtualRenderTarget Target;
            public BetaTowerStairs Parent;
            private bool rendered;
            public Vector2 Offset;
            public Vector2[][] Points => Parent.StairPoints;
            public int Skip => 1;
            public Railing(BetaTowerStairs parent, float offset, bool background) : base(parent.Position - offset * Vector2.UnitX)
            {
                Offset = offset * Vector2.UnitX;
                Target = VirtualContent.CreateRenderTarget("TowerRailing", (int)(parent.Width + offset * 2), (int)parent.Height);
                Parent = parent;
                Depth = background ? 4 : -1;
                Add(new BeforeRenderHook(() =>
                {
                    if (rendered) return;
                    rendered = true;
                    Target.SetAsTarget(true);
                    Vector2 barOffset = Vector2.UnitY * 12;
                    Vector2 sO = Vector2.UnitX * 12;
                    void drawStairs(int i, bool useLerp)
                    {
                        Vector2[] points = Parent.GetLinePoints(i, 30);
                        for (int j = 0; j < Points[i].Length; j += Skip)
                        {
                            Vector2 p = Points[i][j].Round() - Parent.Position;
                            float target = Parent.Width / 2;
                            float lerp = MathHelper.Distance(p.X, target) / target;
                            if (p.X < target)
                            {
                                lerp *= -1;
                            }
                            Vector2 a = (p - sO / 2 + Offset);
                            Vector2 b = (p + sO / 2 + Offset);
                            Draw.Line(a, b, Color.Lerp(Color.White, Color.Black, useLerp ? 1 - Math.Abs(lerp) : 0));

                        }
                    }
                    Draw.SpriteBatch.Begin();
                    for (int i = 0; i < Points.Length; i++)
                    {
                        Vector2 left = Points[i][0];
                        Vector2 right = Points[i][^1];
                        //drawStairs(i, true);
                        continue;
                        if (left.Y < right.Y)
                        {
                            if (background)
                            {
                                //drawBars(i, true, Parent.Col.X - Parent.X, int.MaxValue);
                                //drawSlope(i, true, Parent.Col.X - Parent.X, int.MaxValue);
                                drawStairs(i, true);
                            }
                            else
                            {
                                //drawBars(i, false, int.MinValue, Parent.Col.X - Parent.X);
                                //drawSlope(i, false, int.MinValue, Parent.Col.X - Parent.X);
                            }
                        }
                        else
                        {
                            if (background)
                            {
                                //drawStairs(i, false);
                                //drawBars(i, false, int.MinValue, Parent.Col.Left - Parent.X);
                                //drawSlope(i, false, int.MinValue, Parent.Col.Left - Parent.X);
                            }
                            else
                            {
                                //drawBars(i, false, Parent.Col.Left - Parent.X, int.MaxValue);
                                //drawSlope(i, false, Parent.Col.Left - Parent.X, int.MaxValue);
                            }
                        }
                    }
                    Draw.SpriteBatch.End();
                }));
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(Target, Parent.Position - Offset, Color.White);
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Target?.Dispose();
            }
        }

        public static bool DebugFlashRender = false;
        public (int lvl, int pnt) Flash = (-1, -1), prevFlash = (-1, -1);
        public Vector2[][] StairPoints;
        public TowerFloor[] Floors;
        public InvisibleBarrier[] Safeguards = new InvisibleBarrier[2];
        public Column Col;
        public PlayerShade PlayerShade;
        public int Levels;
        public bool RidingPlatform;
        public bool PlayerFading;
        public JumpThru TopPlatform;
        public float TopPlatformCollisionTimer;
        public float RailingHeight = 16;
        public Railing FGRailing, BGRailing;
        public int Skip = 10;
        public BetaTowerStairs(EntityData data, Vector2 offset) : base(data, offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Levels = data.Int("levels", 3);
            Points = new SlopePoint[Levels + 1];
            Points[0] = new SlopePoint(BottomCenter, CircleIn);
            float yInc = Height / Levels;

            float xOffset = Width / 2;
            float yOffset = -yInc;
            for (int i = 1; i < Levels + 1; i++)
            {
                Points[i] = new SlopePoint(BottomCenter + new Vector2(xOffset, -yInc * i), CircleOutIn);
                xOffset *= -1;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Vector3 getPoint(float r, float c, float h)
            {
                return new Vector3(r * (float)Math.Sin(c * h), h, r * (float)Math.Cos(c * h));
            }
            float r = Width / 2;
            float c = 0.05f;
            float h = Height;
            Vector3 from = getPoint(r, c, 0);
            for (int i = 0; i < h; i += 3)
            {
                Vector3 next = getPoint(r, c, i);
                Draw.Line(from.XY() + TopCenter, next.XY() + TopCenter, Color.Lerp(Color.White, Color.Black, Math.Abs(next.Z / r)));
                from = next;
            }
            StairPoints = GetStairPoints((int)Width);
            Level level = scene as Level;
            float bottom = level.Bounds.Bottom;
            //Col = new Column(this, Position + Vector2.UnitX * ((Width / 2) - 24), 48, bottom - Top);
            scene.Add(Col);
            scene.Add(PlayerShade = new PlayerShade(0));
            scene.Add(TopPlatform = new JumpThru(Position, (int)Width, true));
            Safeguards[0] = new InvisibleBarrier(TopLeft - Vector2.UnitX * (Platform.Width / 2f + 8), 8, Height);
            Safeguards[1] = new InvisibleBarrier(TopRight + Vector2.UnitX * (Platform.Width / 2f), 8, Height);
            scene.Add(Safeguards);
            SetSafeguards(false);
            Floors = new TowerFloor[Levels];
            for (int i = 0; i < Levels; i++)
            {
                Vector2[] points = StairPoints[i];
                Rectangle bounds = points.Bounds();
                //Floors[i] = new TowerFloor(this, new Vector2((int)X, bounds.Y), points, (int)Width, bounds.Height);
            }
            scene.Add(Floors);
            //FGRailing = new Railing(this, 6, false);
            //BGRailing = new Railing(this, 6, true);
            scene.Add(FGRailing, BGRailing);
        }
        public Vector2[][] GetStairPoints(int steps)
        {
            Vector2[][] array = new Vector2[Levels][];
            for (int i = 0; i < Levels; i++)
            {
                array[i] = GetLinePoints(i, i == 0 ? steps / 2 : steps);
            }
            return array;
        }
        public void SetSafeguards(bool state)
        {
            Safeguards[0].Active = state;
            Safeguards[1].Active = state;
            Safeguards[0].Collidable = state;
            Safeguards[1].Collidable = state;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Floors.RemoveSelves();
            Col.RemoveSelf();
            PlayerShade.RemoveSelf();
            TopPlatform.RemoveSelf();
            Safeguards.RemoveSelves();
            FGRailing.RemoveSelf();
            BGRailing.RemoveSelf();
        }

        public override void Update()
        {
            Vector2 prevPlatformPosition = new Vector2(Platform.CenterX, Platform.Top);
            if (Scene.GetPlayer() is not Player player) return;
            if (PlayerFading)
            {
                PlayerShade.Alpha = 0;
            }
            else if (!CollideCheck(player) || player.Bottom > Platform.Y || Col.InElevator)
            {
                PlayerShade.Alpha = Calc.Approach(PlayerShade.Alpha, 0, Engine.DeltaTime);
            }
            else
            {
                if (Floor != 0 && Floor % 2 == 0)
                {
                    float percent = 1 - (MathHelper.Distance(Percent, 0.5f) / 0.5f);
                    PlayerShade.Alpha = Calc.Approach(PlayerShade.Alpha, 0.8f * percent, Engine.DeltaTime);
                }
                else
                {
                    PlayerShade.Alpha = Calc.Approach(PlayerShade.Alpha, 0f, Engine.DeltaTime);
                }
            }
            base.Update();
            RidingPlatform = player.IsRiding(Platform);
            SetSafeguards(RidingPlatform);

            if (Safeguards[0].Active && Safeguards[0].CollideCheck(player))
            {
                player.Left = Safeguards[0].Right;
            }
            if (Safeguards[1].Active && Safeguards[1].CollideCheck(player))
            {
                player.Right = Safeguards[1].Left;
            }
            if (TopPlatformCollisionTimer > 0)
            {
                TopPlatformCollisionTimer -= Engine.DeltaTime;
                if (TopPlatformCollisionTimer < 0)
                {
                    TopPlatformCollisionTimer = 0;
                }
            }
            TopPlatform.Collidable = TopPlatformCollisionTimer == 0;
            if (TopPlatform.HasPlayerRider())
            {
                if (Platform.Y - TopPlatform.Y <= 16 && Input.MoveX == 0 && Input.MoveY == 1)
                {
                    TopPlatformCollisionTimer = 0.3f;
                }
            }
            if (DebugFlashRender)
            {
                if (RidingPlatform)
                {
                    SetFlashPointIndex(player);
                    if (Flash != prevFlash && prevFlash.lvl >= 0)
                    {
                        ReleaseAllFlashes(true);
                    }
                }
                else
                {
                    ReleaseAllFlashes(false);
                }
            }
        }

        public void ReleaseAllFlashes(bool ignoreCurrent)
        {
            for (int i = 0; i < Floors.Length; i++)
            {
                for (int j = 0; j < Floors[i].LeftFlashes.Length; j++)
                {
                    if (ignoreCurrent && i == Flash.lvl && j == Flash.pnt) continue;
                    Floors[i].LeftFlashes[j].AlphaRate = 1f;
                }
                for (int j = 0; j < Floors[i].RightFlashes.Length; j++)
                {
                    if (ignoreCurrent && i == Flash.lvl && j == Flash.pnt) continue;
                    Floors[i].RightFlashes[j].AlphaRate = 1f;
                }
            }
        }
        public void SetFlashPointIndex(Player player)
        {
            Vector2[] points = StairPoints[Index];
            int closestPointIndex = -1;
            float dist = int.MaxValue;
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 p = points[i];
                float d = Vector2.DistanceSquared(p, player.BottomCenter);
                if (d < dist)
                {
                    closestPointIndex = i;
                    dist = d;
                }
            }
            FlashOn(Index, closestPointIndex);

        }
        public void FlashOn(int index, int point)
        {
            prevFlash = Flash;
            Flash = (index, point);
            var l = Floors[index].LeftFlashes[point];
            var r = Floors[index].RightFlashes[point];
            l.Alpha = 1;
            r.Alpha = 1;
            l.AlphaRate = 0;
            r.AlphaRate = 0;
        }
        public List<Vector3> SinPoints = [];
        public override void Render()
        {
            Vector3 getPoint(float r, float c, float h)
            {
                return new Vector3(r * (float)Math.Sin(c * h), h, r * (float)Math.Cos(c * h));
            }
            float r = Width / 2;
            float c = 0.05f;
            float h = Height;
            Vector3 from = getPoint(r, c, 0);
            for (int i = 0; i < h; i += 3)
            {
                Vector3 next = getPoint(r, c, i);
                Draw.Line(from.XY() + TopCenter, next.XY() + TopCenter, Color.Lerp(Color.White, Color.Black, Math.Abs(next.Z / r)));
                from = next;
            }

            /*            float stairWidth = 12;
                        Vector2 stairOffset = Vector2.UnitX * stairWidth / 2;
                        DrawSlope(-Vector2.UnitY * RailingHeight - stairOffset, 30, Color.Lerp(Color.White, Color.Black, 0.8f));
                        foreach (var a in StairPoints)
                        {
                            foreach (var v in a)
                            {
                                Draw.Line(v - stairOffset, v - stairOffset - Vector2.UnitY * RailingHeight, Color.Gray);
                                Draw.Line(v - stairOffset, v + stairOffset, Color.White);
                                Draw.Line(v + stairOffset, v + stairOffset - Vector2.UnitY * RailingHeight, Color.Gray);
                            }
                        }
                        DrawSlope(-Vector2.UnitY * RailingHeight + stairOffset, 30, Color.Lerp(Color.White, Color.Black, 0.8f));*/
        }
    }
}
/*
 * 
                    void drawBars(int i, bool useLerp, float minX, float maxX)
                    {
                        for (int j = 0; j < Points[i].Length; j += Skip)
                        {
                            Vector2 p = Points[i][j].Round() - Parent.Position;
                            float target = Parent.Width / 2;
                            float lerp = MathHelper.Distance(p.X, target) / target;
                            if (p.X < target)
                            {
                                lerp *= -1;
                            }
                            float colorLerp = Math.Abs(lerp);
                            Vector2 nudge = lerp * sO / 2;
                            if (!(p.X + nudge.X < minX || p.X + nudge.X > maxX))
                            {
                                Vector2 a = (p + nudge + Offset);
                                Vector2 b = (p + nudge - barOffset + Offset);
                                Draw.Line(a, b, Color.Lerp(Color.White, Color.Black, useLerp ? 1 - colorLerp : 0));
                            }
                        }
                    }
                    void drawSlope(int i, bool useLerp, float minX, float maxX)
                    {
                        Vector2 from = Points[i][0].Round() - Parent.Position;
                        for (int j = 1; j < Points[i].Length; j += Skip)
                        {
                            Vector2 p = Points[i][j].Round() - Parent.Position;
                            float target = Parent.Width / 2;
                            float lerp = MathHelper.Distance(p.X, target) / target;
                            if (p.X < target)
                            {
                                lerp *= -1;
                            }
                            float colorLerp = Math.Abs(lerp);
                            Vector2 nudge = lerp * sO / 2;
                            if (!(p.X + nudge.X < minX || p.X + nudge.X > maxX))
                            {
                                Vector2 a = (from + nudge - barOffset + Offset);
                                Vector2 b = (p + nudge - barOffset + Offset);
                                Draw.Line(a, b, Color.Lerp(Color.White, Color.Black, useLerp ? 1 - colorLerp : 0), 2);
                            }
                            from = p;
                        }
                    }
 * 
 */