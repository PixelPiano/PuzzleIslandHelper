using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PopBlock")]
    [Tracked]
    public class PopBlock : Solid
    {
        public const float ScaleAdd = 0.5f;
        public const float ScaleTime = 0.15f;
        public float Padding;
        public Vector2 PaddingOffset => new Vector2(Padding);
        private EntityID id;
        public Vector2 Scale = Vector2.One;
        private VirtualRenderTarget Buffer;
        private VirtualRenderTarget EmptyBuffer;
        private bool drawnOnce;
        private List<Image> images = [];
        private List<Image> emptyImages = [];
        public MTexture MainTexture;
        public MTexture EmptyTexture;
        public bool OnlyOnce;
        public bool CanRespawn = true;
        public bool Popped;
        public bool InRoutine;
        private LightOcclude occlude;
        public ParticleType PopParticles = new()
        {
            Color = Color.Pink,
            Color2 = Color.HotPink,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Linear,
            LifeMin = 0.5f,
            LifeMax = 1.1f,
            SpeedMin = 20f,
            SpeedMax = 40f,
            Friction = 1f,
            Size = 1
        };
        public PopBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, true)
        {
            Padding = (Calc.Max(Width, Height) * (1f + ScaleAdd)) / 2;
            Depth = -12999;
            this.id = id;
            Buffer = VirtualContent.CreateRenderTarget("pop-block-" + id.ToString(), data.Width + (int)Padding * 2, data.Height + (int)Padding * 2);
            EmptyBuffer = VirtualContent.CreateRenderTarget("empty-pop-block-" + id.ToString(), data.Width + (int)Padding * 2, data.Height + (int)Padding * 2);
            Add(new BeforeRenderHook(BeforeRender));
            MainTexture = GFX.Game["objects/PuzzleIslandHelper/popBlock/sprite"];
            EmptyTexture = GFX.Game["objects/PuzzleIslandHelper/popBlock/empty"];
            CanRespawn = data.Bool("respawns", true);
            OnlyOnce = data.Bool("onlyOnce");
            OnDashCollide = DashCollide;
        }

        private DashCollisionResults DashCollide(Player player, Vector2 direction)
        {
            DashLaunch(player, direction);
            return DashCollisionResults.NormalOverride;
        }
        public void DashLaunch(Player player, Vector2 dir)
        {
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            Celeste.Freeze(Engine.DeltaTime);
            Collidable = false;
            PassivePop();

            float speed = 240f;
            player.launched = true;
            if (player.Right <= Left)
            {
                player.Speed.X = -speed * 0.9f;
            }
            if (player.Left >= Right)
            {
                player.Speed.X = speed * 0.9f;
            }
            if (player.Top >= Bottom)
            {
                player.Speed.Y = speed * 3f;
            }
            if (player.Bottom <= Top)
            {
                player.Speed.Y = -speed * 1.1f;
            }
            if (player.Speed.Y <= 50f && dir.Y == 0)
            {
                player.Speed.Y = Math.Min(-130f, player.Speed.Y);
                player.AutoJump = true;
            }
            if (player.Speed.X != 0f)
            {
                if (Input.MoveX.Value == Math.Sign(player.Speed.X))
                {
                    player.explodeLaunchBoostTimer = 0f;
                    player.Speed.X *= 1.2f;
                }
                else
                {
                    player.explodeLaunchBoostTimer = 0.01f;
                    player.explodeLaunchBoostSpeed = player.Speed.X * 1.2f;
                }
            }

            if (!player.Inventory.NoRefills)
            {
                player.RefillDash();
            }

            player.RefillStamina();
            player.dashCooldownTimer = 0.2f;
            player.StateMachine.State = 7;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Point(Position - PaddingOffset, Color.Cyan);
        }
        public override void Update()
        {
            base.Update();
            occlude.Visible = !Popped;
        }
        public void JumpLaunch(Player player)
        {
            Pop(player, 45f, 3);
        }
        public void SuperJumpLaunch(Player player)
        {
            Pop(player, 1f, 1);
        }
        public void WallJumpLaunch(Player player)
        {
            Pop(player, 45f, 0);
        }
        public void SuperWallJumpLaunch(Player player)
        {
            Pop(player, 100f, 0.2f);
        }
        public void WallKickLaunch(Player player)
        {
            Pop(player, 45f, 0.1f);
        }

        public void Launch(Player player, float speed, float multY)
        {
            if (player == null) return;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            Celeste.Freeze(Engine.DeltaTime);

            player.launchApproachX = null;
            Vector2 vector = (player.Center - Center).SafeNormalize(-Vector2.UnitY);
            float num = Vector2.Dot(vector, Vector2.UnitY);
            if (num <= 0.65f && num >= -0.55f)
            {
                vector.Y = 0f;
                vector.X = Math.Sign(vector.X);
            }

            if (multY == 0 && vector.X != 0f)
            {
                vector.Y = 0f;
                vector.X = Math.Sign(vector.X);
            }

            player.Speed += speed * vector * new Vector2(1, multY);

            if (player.Speed.Y <= 50f)
            {
                player.Speed.Y = Math.Min(-130f, player.Speed.Y);
                player.AutoJump = true;
            }

            if (player.Speed.X != 0f)
            {
                if (Input.MoveX.Value == Math.Sign(player.Speed.X))
                {
                    player.explodeLaunchBoostTimer = 0f;
                    player.Speed.X *= 1.2f;
                }
                else
                {
                    player.explodeLaunchBoostTimer = 0.01f;
                    player.explodeLaunchBoostSpeed = player.Speed.X * 1.2f;
                }
            }

            SlashFx.Burst(player.Center, player.Speed.Angle());
            if (!player.Inventory.NoRefills)
            {
                player.RefillDash();
            }

            player.RefillStamina();
            player.dashCooldownTimer = 0.2f;
            player.StateMachine.State = 7;
        }

        private IEnumerator popRoutine(Player player, float speed, float multY, bool launchPlayer = true)
        {
            InRoutine = true;
            bool exploded = false;
            for (float i = 0.3f; i < 1; i += Engine.DeltaTime / ScaleTime)
            {
                Scale = Vector2.One * (1 + Ease.CubeOut(i) * ScaleAdd);
                if (launchPlayer && i > 0.5f && !exploded)
                {
                    Launch(player, speed, multY);

                    exploded = true;
                }
                yield return null;
            }
            Scale = Vector2.One * (1 + ScaleAdd);
            OnPop();

            if (CanRespawn)
            {
                yield return 1;
                while (CollideCheck<Player>())
                {
                    yield return null;
                }
                Tween.Set(this, Tween.TweenMode.Oneshot, ScaleTime, Ease.CubeOut, t => Scale = Vector2.One * (1 + t.Eased * ScaleAdd),
                t =>
                {
                    Collidable = true;
                    Popped = false;
                    Depth = -12999;
                    Tween.Set(this, Tween.TweenMode.Oneshot, ScaleTime, Ease.CubeIn, t => Scale = Vector2.Lerp(Vector2.One * (1 + ScaleAdd), Vector2.One, t.Eased));
                });

            }
            InRoutine = false;
        }
        public void PassivePop()
        {
            Pop(null, 0, 0, false);
        }
        public void Pop(Player player, float speed, float multY, bool launchPlayer = true)
        {
            if (!Popped && !InRoutine)
            {
                Add(new Coroutine(popRoutine(player, speed, multY, launchPlayer)));
            }
        }
        public void OnPop()
        {
            Popped = true;
            Collidable = false;
            Depth = 1;
            Tween.Set(this, Tween.TweenMode.Oneshot, ScaleTime, Ease.CubeOut, t =>
            {
                Scale = Vector2.Lerp(Vector2.One * (1 + ScaleAdd), Vector2.One, t.Eased);
            });
            Level level = Scene as Level;
            if (!CanRespawn && OnlyOnce)
            {
                level.Session.DoNotLoad.Add(id);
            }
            for (int i = 0; i < 8; i++)
            {
                level.ParticlesFG.Emit(PopParticles, Center + GetAngleOffset(i, 8, Calc.Random.Range(5, 9)), GetAngle(i, 8));
            }
        }
        public float GetAngle(int index, int count)
        {
            return index * MathHelper.TwoPi / count;
        }
        public Vector2 GetAngleOffset(int index, int count, float length)
        {
            float theta = GetAngle(index, count);
            return new Vector2(x: (float)Math.Cos(theta) * length, y: (float)Math.Sin(theta) * length);
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Player.Jump += Player_Jump;
            On.Celeste.Player.WallJump += Player_WallJump;
            On.Celeste.Player.SuperJump += Player_SuperJump;
            On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
        }

        private static void Player_SuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
        {
            //if beside block, hard launch sideways
            if (self.CollideFirst<PopBlock>(self.Position + Vector2.UnitX * -dir * 4) is PopBlock block)
            {
                block.SuperWallJumpLaunch(self);
            }
            orig(self, dir);
        }

        private static void Player_SuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
        {
            //if on top of block, hard launch anywhere
            //100f
            if (self.onGround && self.CollideFirst<PopBlock>(self.Position + Vector2.UnitY) is PopBlock block)
            {
                block.SuperJumpLaunch(self);
            }
            orig(self);
        }

        private static void Player_WallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
        {
            //if beside block, light launch sideways
            if (self.CollideFirst<PopBlock>(self.Position + Vector2.UnitX * -dir * 2) is PopBlock block)
            {
                block.WallJumpLaunch(self);
            }
            orig(self, dir);
        }
        private static void Player_Jump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
        {

            //if on top of block, light launch anywhere
            if (self.onGround && self.CollideFirst<PopBlock>(self.Position + Vector2.UnitY) is PopBlock block)
            {
                block.JumpLaunch(self);
            }
            //else, if beside a block, light launch sideways
            else if (!self.onGround && self.CollideFirst<PopBlock>(self.Position + Vector2.UnitX * (int)self.Facing) is PopBlock block2)
            {
                block2.WallKickLaunch(self);
            }
            orig(self, particles, playSfx);
        }

        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Player.Jump -= Player_Jump;
            On.Celeste.Player.WallJump -= Player_WallJump;
            On.Celeste.Player.SuperJump -= Player_SuperJump;
            On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
        }

        public void BeforeRender()
        {
            if (drawnOnce) return;
            Buffer.SetAsTarget(true);
            Draw.SpriteBatch.StandardBegin(Matrix.Identity);
            DrawBlockOutline();
            DrawBlock();
            Draw.SpriteBatch.End();
            EmptyBuffer.SetAsTarget(true);
            Draw.SpriteBatch.StandardBegin(Matrix.Identity);
            DrawEmptyBlock();
            Draw.SpriteBatch.End();
            drawnOnce = true;
        }
        public void DrawBlock()
        {
            foreach (Image image in images)
            {
                image.Render();
            }
        }
        public void DrawBlockOutline()
        {
            foreach (Image image in images)
            {
                image.DrawSimpleOutline();
            }
        }
        public void DrawEmptyBlock()
        {
            foreach (Image image in emptyImages)
            {
                image.Render();
            }
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(Popped ? EmptyBuffer : Buffer, Position + Collider.HalfSize, null, Color.White, 0, Collider.HalfSize + PaddingOffset, Scale, SpriteEffects.None, 0);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Buffer.Dispose();
            EmptyBuffer.Dispose();
        }
        private void addImage(int x, int y, int offsetx, int offsety)
        {
            Image image = new Image(MainTexture.GetSubtexture(x, y, 8, 8));
            Image image2 = new Image(EmptyTexture.GetSubtexture(x, y, 8, 8));
            image.Position = image2.Position = new Vector2(offsetx, offsety) + PaddingOffset;
            images.Add(image);
            emptyImages.Add(image2);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            int w = (int)Width;
            int h = (int)Height;

            //add corner textures
            addImage(0, 0, 0, 0);
            addImage(16, 0, w - 8, 0);
            addImage(0, 16, 0, h - 8);
            addImage(16, 16, w - 8, h - 8);

            //add top + bottom textures
            for (int x = 8; x < w - 8; x += 8)
            {
                addImage(8, 0, x, 0);
                addImage(8, 16, x, h - 8);
            }
            //add left + right textures
            for (int y = 8; y < h - 8; y += 8)
            {
                addImage(0, 8, 0, y);
                addImage(16, 8, w - 8, y);
            }
            //add middle textures
            for (int x = 8; x < w - 8; x += 8)
            {
                for (int y = 8; y < h - 8; y += 8)
                {
                    addImage(8, 8, x, y);
                }
            }
            Add(occlude = new LightOcclude());
            if (CollideCheck<Player>())
            {
                RemoveSelf();
            }
        }
    }
}