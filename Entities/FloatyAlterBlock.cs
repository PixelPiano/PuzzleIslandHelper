using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FloatyAlterBlock")]
    [Tracked]
    public class FloatyAlterBlock : FancyFloatySpaceBlock
    {
        public static readonly BlendState AlphaMaskClearBlendState = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.SourceAlpha
        };
        private static EntityData createData(Vector2 position, int seed, string tileData, float width, float height, char tileType, bool disableSpawnOffset)
        {
            string fakeTileData = "";
            for (float i = 0; i < height; i += 8)
            {
                for (float j = 0; j < width; j += 8)
                {
                    fakeTileData += tileType;
                }
                fakeTileData += '\n';
            }
            return new EntityData()
            {
                Position = position,
                Width = (int)width,
                Height = (int)height,
                Values = new()
                {
                    {"randomSeed",seed},
                    {"tileData",fakeTileData},
                    {"connectsTo",tileType },
                    {"disableSpawnOffset",disableSpawnOffset },
                    {"tiletype",tileType}
                }
            };
        }
        public FloatyAlterBlock(EntityData data, Vector2 offset) :
            base(createData(data.Position, data.Int("randomSeed"), data.Attr("tileData"), data.Width, data.Height, data.Char("tiletype", '3'), data.Bool("disableSpawnOffset")), offset)
        {

        }
        public static void RenderTiles(TileGrid grid, Vector2 offset)
        {
            if (grid is null || grid.Alpha <= 0f) return;
            offset -= grid.VisualExtend * Vector2.One * 8;
            int tileWidth = grid.TileWidth;
            int tileHeight = grid.TileHeight;
            Color color = grid.Color * grid.Alpha;
            Vector2 position2 = offset;
            position2 = position2.Round();
            for (int i = 0; i < grid.TilesX; i++)
            {
                for (int j = 0; j < grid.TilesY; j++)
                {
                    MTexture tex = grid.Tiles[i, j];
                    if (tex != null)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position2, tex.ClipRect, color);
                    }
                    position2.Y += tileHeight;
                }
                position2.X += tileWidth;
                position2.Y = offset.Y;
            }
        }
        public static void Load()
        {
            On.Celeste.FloatySpaceBlock.OnDash += FloatySpaceBlock_OnDash;
        }
        public static void Unload()
        {
            On.Celeste.FloatySpaceBlock.OnDash -= FloatySpaceBlock_OnDash;
        }
        public bool Settled;

        private void SpaceBlockUpdate()
        {
            SolidUpdate();
            if (!Settled)
            {
                if (MasterOfGroup)
                {
                    bool flag = false;
                    foreach (FloatySpaceBlock item in Group)
                    {
                        if (item.HasPlayerRider())
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        foreach (JumpThru jumpthru in Jumpthrus)
                        {
                            if (jumpthru.HasPlayerRider())
                            {
                                flag = true;
                                break;
                            }
                        }
                    }

                    if (flag)
                    {
                        sinkTimer = 0.3f;
                    }
                    else if (sinkTimer > 0f)
                    {
                        sinkTimer -= Engine.DeltaTime;
                    }

                    if (sinkTimer > 0f)
                    {
                        yLerp = Calc.Approach(yLerp, 1f, 1f * Engine.DeltaTime);
                    }
                    else
                    {
                        yLerp = Calc.Approach(yLerp, 0f, 1f * Engine.DeltaTime);
                    }

                    sineWave += Engine.DeltaTime;
                    dashEase = Calc.Approach(dashEase, 0f, Engine.DeltaTime * 1.5f);
                }
                else
                {
                    sinkTimer = yLerp = sineWave = dashEase = 0;
                }
                modifiedMoveToTarget();
            }

            LiftSpeed = Vector2.Zero;
        }
        public void modifiedMoveToTarget()
        {
            float num = (float)Math.Sin(sineWave) * 2f;
            Vector2 vector = Calc.YoYo(Ease.QuadIn(dashEase)) * dashDirection * 4f;
            for (int i = 0; i < 2; i++)
            {
                foreach (KeyValuePair<Platform, Vector2> move in Moves)
                {
                    Platform key = move.Key;
                    bool flag = false;
                    JumpThru jumpThru = key as JumpThru;
                    Solid solid = key as Solid;
                    if ((jumpThru != null && jumpThru.HasRider()) || (solid != null && solid.HasRider()))
                    {
                        flag = true;
                    }

                    if ((flag || i != 0) && (!flag || i != 1))
                    {
                        Vector2 value = move.Value;
                        float num2 = MathHelper.Lerp(value.Y, value.Y + 6f, Ease.SineInOut(yLerp)) + num;
                        key.MoveToY(num2 + vector.Y);
                    }
                }
            }
        }
        private void SolidUpdate()
        {
            PlatformUpdate();
            MoveH(Speed.X * Engine.DeltaTime);
            MoveV(Speed.Y * Engine.DeltaTime);
            if (!EnableAssistModeChecks || SaveData.Instance == null || !SaveData.Instance.Assists.Invincible || base.Components.Get<SolidOnInvinciblePlayer>() != null || !Collidable)
            {
                return;
            }

            Player player = CollideFirst<Player>();
            Level level = base.Scene as Level;
            if (player == null && base.Bottom > (float)level.Bounds.Bottom)
            {
                player = CollideFirst<Player>(Position + Vector2.UnitY);
            }

            if (player != null && player.StateMachine.State != 9 && player.StateMachine.State != 21)
            {
                Add(new SolidOnInvinciblePlayer());
                return;
            }

            TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
            if (theoCrystal != null && !theoCrystal.Hold.IsHeld)
            {
                Add(new SolidOnInvinciblePlayer());
            }
        }
        private void PlatformUpdate()
        {
            EntityUpdate();
            LiftSpeed = Vector2.Zero;
            if (!shaking)
            {
                return;
            }

            if (base.Scene.OnInterval(0.04f))
            {
                Vector2 vector = shakeAmount;
                shakeAmount = Calc.Random.ShakeVector();
                OnShake(shakeAmount - vector);
            }

            if (shakeTimer > 0f)
            {
                shakeTimer -= Engine.DeltaTime;
                if (shakeTimer <= 0f)
                {
                    shaking = false;
                    StopShaking();
                }
            }
        }
        private void EntityUpdate()
        {
            Components.Update();
        }
        public override void Update()
        {
            SpaceBlockUpdate();
        }
        public void SetUnsettledCollision(bool state)
        {
            if (Scene is Level level)
            {
                foreach (FloatyAlterBlock block in level.Tracker.GetEntities<FloatyAlterBlock>())
                {
                    if (block != this && !block.Settled)
                    {
                        block.Collidable = state;
                    }
                }
            }
        }
        public bool TrySnapToBelow(Level level)
        {
            while (!CollideCheck<BlockAlter>() && !CollideCheck<FloatyAlterBlock>())
            {
                if (Position.Y > level.Bounds.Bottom) return false;
                Position.Y += 8;
            }
            while (CollideCheck<BlockAlter>() || CollideCheck<FloatyAlterBlock>())
            {
                Position.Y -= 1;
                if (Position.Y + Height < level.Bounds.Top) return false;
            }
            if (level.GetPlayer() is Player player)
            {
                Vector2 orig;
                Vector2 playerBottom = orig = player.BottomCenter - Vector2.UnitY;
                while (player.CollideCheck<FloatyAlterBlock>(playerBottom))
                {
                    playerBottom.Y--;
                }
                player.Position.Y += playerBottom.Y - orig.Y;
            }
            return true;

        }
        public void RushDownwards()
        {
            if (Scene is not Level level || Settled) return;
            SetUnsettledCollision(false);
            Vector2 orig = Position;
            if (TrySnapToBelow(level))
            {
                Settled = true;
                AddEffects(orig);
            }
            else
            {
                RemoveSelf();
            }
            SetUnsettledCollision(true);
        }
        private void AddEffects(Vector2 from)
        {
            Scene.Add(new AfterImage(from, this));
        }
        private static DashCollisionResults FloatySpaceBlock_OnDash(On.Celeste.FloatySpaceBlock.orig_OnDash orig, FloatySpaceBlock self, Player player, Vector2 direction)
        {
            if (self is FloatyAlterBlock block && direction.Y > 0 && !block.Settled)
            {
                block.RushDownwards();
                return DashCollisionResults.Rebound;
            }
            return orig(self, player, direction);
        }

        [Tracked]
        public class AfterImage : Entity
        {
            private VirtualRenderTarget target, white;
            private TileGrid tiles;
            private float scale = 1;
            private float alpha = 0.5f;
            private bool renderedToTarget;
            private FloatyAlterBlock block;
            public AfterImage(Vector2 position, FloatyAlterBlock block) : base(position)
            {
                Depth = block.Depth - 1;
                Collider = new Hitbox(block.Width, block.Height);
                this.block = block;

                target = VirtualContent.CreateRenderTarget("FAB AfterImage", (int)Width, (int)Height);
                white = VirtualContent.CreateRenderTarget("FAB whiteout", (int)Width, (int)Height);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 1, true);
                tween.OnUpdate = t =>
                {
                    alpha = Calc.LerpClamp(0.5f, 0, t.Eased);
                    scale = Calc.LerpClamp(1, 1.5f, t.Eased);
                };
                tween.OnComplete = t =>
                {
                    RemoveSelf();
                };
                Add(tween);
                Add(new BeforeRenderHook(BeforeRender));

            }
            public void BeforeRender()
            {
                if (renderedToTarget || Scene is not Level level) return;
                Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
                RenderTiles(block.tiles, Vector2.Zero);
                Draw.SpriteBatch.End();


                Engine.Graphics.GraphicsDevice.SetRenderTarget(white);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, AlphaMaskClearBlendState, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
                Draw.SpriteBatch.Draw((Texture2D)target, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
     

                renderedToTarget = true;
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw((Texture2D)target, Position + Collider.HalfSize, null, Color.White * alpha, 0, Collider.HalfSize, scale, SpriteEffects.None, 0);
                Draw.SpriteBatch.Draw((Texture2D)white, block.Position, Color.White * alpha);

            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                target?.Dispose();
                target = null;
                white?.Dispose();
                white = null;
            }
        }
    }

}