using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.ClutterBlock;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/TowerColumn")]
    public class Column : Entity
    {
        private VertexPositionColor[] ColVerts = new VertexPositionColor[6];
        private static readonly int[] columnIndices = [0, 1, 3, 3, 1, 4, 1, 2, 4, 4, 2, 5];
        public Color Edge = Color.Black;
        public Color Middle = Color.White;
        public VirtualRenderTarget Target;
        private bool rendered;
        public bool RenderOnce;
        public float EdgeLerp = 0; //
        public float MiddleLerp = 0; //
        public Color Edge2; //
        public Color Middle2; //
        public float MiddleDist; //
        public float Alpha = 1;
        public float Radius;
        public LightOcclude Occlude;
        public PlayerHider PlayerHider;
        public PlayerShade PlayerShade;
        public Entity Cover;
        public float FadeAlpha;
        public bool InElevator;
        public float HeightExtend;
        public bool HidingDisabled => PlayerHider.Disabled;
        private float prevPlayerLightAlpha = 1;
        public float TrueBottom;
        public List<Stairs> Stairs = [];
        public List<StairLock> Locks = [];
        //public List<Stairs.CustomPlatform> Floors = [];
        public Vector2 OrigPosition;
        private int floors;
        private int revolutionsPerFloor;
        private float stairsRadius;
        private string[] flags;
        private string lockPrefix;
        public BetterShaker XShaker;
        private Vector2 shakeVector;
        private Vector2 dustPosition;
        public Column(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("lockPrefix"), data.Width, data.Height, data.Float("stairsRadius"), data.Int("floors"), data.Int("revolutionsPerFloor"), data.Attr("flags").Split(',')) { }
        public Column(Vector2 position, string lockPrefix, float width, float height, float stairsRadius, int floors, int revolutionsPerFloor, params string[] flags) : base(position)
        {
            this.lockPrefix = lockPrefix;
            this.stairsRadius = stairsRadius;
            this.floors = floors;
            this.revolutionsPerFloor = revolutionsPerFloor;
            AddTag(Tags.TransitionUpdate);
            Collider = new Hitbox(width, height);
            Radius = width / 2;
            Add(Occlude = new LightOcclude());
            Occlude.Alpha = 0;
            this.flags = flags;
            Depth = 2;
            Add(new PostUpdateHook(postUpdate));
            Add(XShaker = new BetterShaker(OnShake));
        }
        public void ShakeFor(float time = -1)
        {
            XShaker.ShakeFor(time);
        }
        public void StopShaking()
        {
            XShaker.StopShaking();
        }
        public void OnShake(Vector2 shake)
        {
            Vector2 vec = shake.XComp();
            shakeVector += vec;
            foreach (Stairs s in Stairs)
            {
                s.OnShake(vec);
            }
            foreach (StairLock l in Locks)
            {
                l.OnShake(vec);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Y += Height;
            FloorHeight = Height / floors;
            Vector2 p = BottomCenter - new Vector2(stairsRadius, FloorHeight);
            for (int i = 0; i < floors; i++)
            {
                Stairs stairs = new Stairs(this, lockPrefix + i, p, stairsRadius * 2, FloorHeight, revolutionsPerFloor, 1, -1, flags != null && i < flags.Length ? new FlagList(flags[i]) : default);
                scene.Add(stairs);
                Stairs.Add(stairs);
                p.Y -= FloorHeight;
            }
            bool needsTopFloor = true;
            /*            foreach (Stairs stairs in Stairs)
                        {
                            Stairs.CustomPlatform floor = new Stairs.CustomPlatform(new Vector2(X, stairs.Bottom), (int)Width);
                            scene.Add(floor);
                            if (stairs.Y == Top)
                            {
                                needsTopFloor = false;
                            }
                        }
                        if (needsTopFloor)
                        {
                            Stairs.CustomPlatform topFloor = new Stairs.CustomPlatform(new Vector2(X, Top), (int)Width);
                            scene.Add(topFloor);
                        }*/
            if (Stairs.Count > 0) ///
            {
                Stairs = [.. Stairs.OrderByDescending(item => item.Bottom)];
            }

            Add(PlayerShade = new PlayerShade(0));
            Add(PlayerHider = new PlayerHider(Vector2.Zero, (int)Width, Math.Min((int)Height, 40)));
            PlayerHider.Disabled = true;
            foreach (Stairs stairs in Stairs) ///
            {
                stairs.Initialize(scene);
            }
        }
        public float FloorHeight;
        private bool canSpawnDust;
        public int FloorsUnlocked
        {
            get
            {
                int output = 0;
                for (int i = 0; i < Stairs.Count; i++)
                {
                    if (SceneAs<Level>().Session.GetFlag("TowerLock:" + lockPrefix + i))
                    {
                        output++;
                    }
                }
                return output;
            }
        }
        public void AssignStairPriorities(int floorsUnlocked)
        {
            if (!Stairs[^1].TopStairs)
            {
                Stairs[^1].CreateAndAddTopPlatform(Scene);
            }
            foreach (Stairs s in Stairs)
            {
                s.BottomStairs = false;
            }
            Stairs[floorsUnlocked].BottomStairs = true;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Stairs[^1].CreateAndAddTopPlatform(scene);
            Collider.Height += 40;
            Y -= 40;
            OrigPosition = Position;
            for (int i = 0; i < Stairs.Count; i++)
            {
                StairLock stairLock = new StairLock(Stairs[i].TopCenter - Vector2.UnitY * 20, lockPrefix + i, "", true, StairLock.States.Decayed);
                Scene.Add(stairLock);
                Locks.Add(stairLock);
            }
            MoveToY(Y - FloorHeight * FloorsUnlocked);
            if (scene.GetPlayer() is Player player)
            {
                prevPlayerLightAlpha = player.Light.Alpha;
            }
            Level level = scene as Level;
            float levelBottom = level.Bounds.Bottom;
            float y = Top;
            Vector2 pos = TopCenter;
            while (pos.Y <= levelBottom && !scene.CollideCheck<Solid>(pos + Vector2.UnitY))
            {
                pos.Y++;
            }
            if (pos.Y <= levelBottom)
            {
                dustPosition = pos;
                canSpawnDust = true;
            }
        }
        private float shadeTarget;
        public void MoveToY(float y)
        {
            if (Y != y)
            {
                foreach (Stairs s in Stairs)
                {
                    s.MoveV(y - Y);
                }
                foreach (StairLock l in Locks)
                {
                    l.Y += y - Y;
                }
                Y = y;
            }
        }
        public IEnumerator MoveRoutine(float to, float speed, int floorsUnlocked)
        {
            while (Y != to)
            {
                float next = Calc.Approach(Y, to, speed * Engine.DeltaTime);
                MoveToY(next);
                yield return null;
            }
            foreach (StairLock l in Locks)
            {
                l.UpdateTalkBounds();
            }
            AssignStairPriorities(floorsUnlocked);
        }
        public override void Update()
        {
            base.Update();
            Stairs currentStairs = null;
            float? timeActive = null;
            foreach (Stairs s in Stairs)
            {
                if (s.CollideCheck<Player>())
                {
                    if (timeActive.HasValue && timeActive.Value > s.LastActiveTime)
                    {
                        continue;
                    }
                    currentStairs = s;
                    timeActive = s.LastActiveTime;
                    break;
                }
            }
            if (currentStairs == null || !CollideCheck<Player>())
            {
                shadeTarget = 0;
                PlayerHider.Disabled = true;
            }
            else
            {
                bool playerRiding = currentStairs.Platform.HasPlayerRider();
                if (playerRiding)
                {
                    shadeTarget = currentStairs.ShadeAmount;
                }
                PlayerHider.Disabled = !currentStairs.HidePlayer;
            }
            PlayerShade.Amount = Calc.Approach(PlayerShade.Amount, shadeTarget, Engine.DeltaTime * 3);

            if (Scene.GetPlayer() is Player player)
            {
                PlayerHider.Position.Y = Math.Max(20, player.Y - 20 - Top);
                player.Light.Alpha = Calc.Approach(player.Light.Alpha, prevPlayerLightAlpha, Engine.DeltaTime);
                Occlude.Visible = !PlayerHider.Disabled;
            }

            if (canSpawnDust && XShaker.Shaking && Scene.OnInterval(0.1f))
            {
                SceneAs<Level>().ParticlesFG.Emit(ParticleTypes.Dust, 4, dustPosition, Vector2.UnitX * (Width / 2 + 8), Color.White);
            }

        }
        public void postUpdate()
        {
            Camera camera = SceneAs<Level>().Camera;
            Vector2 p = Position + shakeVector;
            p.Y = Math.Max(camera.Y - 16, p.Y);
            float height = Math.Min(212, Bottom - Math.Max(camera.Y, Top) + 32);
            UpdateVertices(p, Width, height, Color.Black, Color.White, Alpha);
            //update vertices to only render a segment that covers the camera height + some padding
        }
        public override void Render()
        {
            base.Render();
            if (Alpha > 0)
            {
                Draw.SpriteBatch.End();
                GFX.DrawIndexedVertices(SceneAs<Level>().Camera.Matrix, ColVerts, 6, columnIndices, 4);
                GameplayRenderer.Begin();
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (scene.GetPlayer() is Player player)
            {
                player.Light.Alpha = prevPlayerLightAlpha;
            }
            Stairs.RemoveSelves();
            Locks.RemoveSelves();
        }
        public void UpdateVertices(Vector2 position, float width, float height, Color edge, Color middle, float alpha)
        {
            ColVerts[0].Position.X = ColVerts[3].Position.X = position.X;
            ColVerts[1].Position.X = ColVerts[4].Position.X = position.X + width / 2;
            ColVerts[2].Position.X = ColVerts[5].Position.X = position.X + width;
            ColVerts[0].Position.Y = ColVerts[1].Position.Y = ColVerts[2].Position.Y = position.Y;
            ColVerts[3].Position.Y = ColVerts[4].Position.Y = ColVerts[5].Position.Y = position.Y + height;
            ColVerts[0].Color = ColVerts[2].Color = ColVerts[3].Color = ColVerts[5].Color = edge * alpha;
            ColVerts[1].Color = ColVerts[4].Color = middle * alpha;
        }
    }
}