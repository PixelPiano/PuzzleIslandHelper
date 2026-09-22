using Celeste.Mod.Entities;
using Iced.Intel;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static Celeste.Mod.PuzzleIslandHelper.Entities.InvertAuth;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/ElderTrapdoor")]
    public class ElderTrapdoor : JumpthruPlatform
    {
        private class TileGridWithOnePixelTakenFromTop : TileGrid
        {
            public int ClipWidth;
            public int ClipX;
            public TileGridWithOnePixelTakenFromTop(int tilesX, int tilesY) : base(8, 8, tilesX, tilesY)
            {
            }
            public override void Render()
            {
                newRenderAt(Entity.Position + Position);
            }

            public void newRenderAt(Vector2 position)
            {
                if (Alpha <= 0f)
                {
                    return;
                }

                if (ClipCamera == null && base.Scene is Level level)
                {
                    ClipCamera = level.Camera;
                }

                Rectangle clippedRenderTiles = GetClippedRenderTiles();
                int tileWidth = TileWidth;
                int tileHeight = TileHeight;
                Color color = Color * Alpha;
                if (clippedRenderTiles.Y == 0)
                {
                    for (int i = clippedRenderTiles.Left; i < clippedRenderTiles.Right; i++)
                    {
                        Vector2 position3 = new Vector2(position.X + (float)(clippedRenderTiles.Left * tileWidth), position.Y);
                        MTexture mTexture = Tiles[i, 0];
                        if (mTexture != null)
                        {
                            if (clippedRenderTiles.X < ClipX || clippedRenderTiles.X > ClipX + ClipWidth)
                            {
                                Draw.SpriteBatch.Draw(mTexture.Texture.Texture_Safe, position3 + Vector2.UnitY, mTexture.ClipRect, color);
                            }
                            else
                            {
                                Rectangle clip = mTexture.ClipRect;
                                clip.Y++;
                                clip.Height--;
                                Draw.SpriteBatch.Draw(mTexture.Texture.Texture_Safe, position3 + Vector2.UnitY, clip, color);
                            }
                        }
                        position3.X += tileWidth;
                    }
                    clippedRenderTiles.Y++;
                }
                Vector2 position2 = new Vector2(position.X + (float)(clippedRenderTiles.Left * tileWidth), position.Y + (float)(clippedRenderTiles.Top * tileHeight));
                for (int i = clippedRenderTiles.Left; i < clippedRenderTiles.Right; i++)
                {
                    for (int j = clippedRenderTiles.Top; j < clippedRenderTiles.Bottom; j++)
                    {
                        MTexture mTexture = Tiles[i, j];
                        if (mTexture != null)
                        {
                            Draw.SpriteBatch.Draw(mTexture.Texture.Texture_Safe, position2, mTexture.ClipRect, color);
                        }

                        position2.Y += tileHeight;
                    }

                    position2.X += tileWidth;
                    position2.Y = position.Y + (float)(clippedRenderTiles.Top * tileHeight);
                }
            }

            public void neworig_RenderAt(Vector2 position)
            {
                if (Alpha <= 0f)
                {
                    return;
                }

                Rectangle clippedRenderTiles = GetClippedRenderTiles();
                Color color = Color * Alpha;
                for (int i = clippedRenderTiles.Left; i < clippedRenderTiles.Right; i++)
                {
                    for (int j = clippedRenderTiles.Top; j < clippedRenderTiles.Bottom; j++)
                    {
                        Tiles[i, j]?.Draw(position + new Vector2(i * TileWidth, j * TileHeight), Vector2.Zero, color);
                    }
                }
            }
        }
        private JumpThru HelperPlatform;
        public Vector2 Orig;
        private float amount;
        private TileGrid Tiles;
        private char tiletype;
        private int tileHeight, tileWidth;
        private Entity tilesEntity;
        private FlagList fadeFlag;
        private int trapdoorX, trapdoorWidth;
        public FlagList CompletedFlag;
        private float completeSpeedLerp;
        public ElderTrapdoor(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), data.FlagList("tilesFadeFlag"), data.FlagList("completedFlag"), data.Int("trapdoorX"), data.Int("trapdoorWidth"))
        {
        }
        public ElderTrapdoor(Vector2 position, int tileWidth, int tileHeight, char tiletype, FlagList fadeFlag, FlagList completedFlag, int trapdoorX, int trapdoorWidth) : base(position + Vector2.UnitX * trapdoorX, trapdoorWidth, "")
        {
            Orig = position;
            Tag |= Tags.TransitionUpdate;
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.tiletype = tiletype;
            this.fadeFlag = fadeFlag;
            this.trapdoorX = trapdoorX;
            this.trapdoorWidth = trapdoorWidth;
            CompletedFlag = completedFlag;

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (PianoModule.Session.LeaderTimer is TrapdoorChandelier timer)
            {
                Draw.Line(Position, timer.Position, Color.Yellow, 2);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(HelperPlatform = new JumpThru(Position, 10, true));
            tilesEntity = new Entity(Position - Vector2.UnitX * trapdoorX)
            {
                Depth = -10000,
                Collider = new Hitbox(tileWidth, tileHeight)
            };
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int)(tilesEntity.X / 8f) - tileBounds.Left;
            int y = (int)(tilesEntity.Y / 8f) - tileBounds.Top;
            int tilesX = tileWidth / 8;
            int tilesY = tileHeight / 8;
            Tiles = GFX.FGAutotiler.GenerateOverlay(tiletype, x, y, tilesX, tilesY, solidsData).TileGrid;
            tilesEntity.Add(new EffectCutout());
            tilesEntity.Add(Tiles);
            scene.Add(tilesEntity);
            /*            Tiles.ClipWidth = trapdoorWidth / 8;
                        Tiles.ClipX = trapdoorX / 8;*/
        }

        public override void Update()
        {
            base.Update();
            if (Scene.GetPlayer() is not Player player) return;
            if (fadeFlag)
            {
                Tiles.Alpha = Calc.Approach(Tiles.Alpha, 0.3f, Engine.DeltaTime);
            }
            else if (player.Bottom <= Y)
            {
                Tiles.Alpha = Calc.Approach(Tiles.Alpha, 1, Engine.DeltaTime);
            }
            bool completed = CompletedFlag;
            if (!completed)
            {
                if (PianoModule.Session.LeaderTimer is TrapdoorChandelier timer)
                {
                    SetProgress(player, timer.Amount);
                }
            }
            else if (amount < 1)
            {
                SetProgress(player, Calc.Approach(amount, 1, Engine.DeltaTime * 15 * completeSpeedLerp));
                completeSpeedLerp = Calc.Approach(completeSpeedLerp, 1, Engine.DeltaTime);
            }
            HelperPlatform.Collidable = amount != 1;
        }
        public void SetProgress(Player player, float amount)
        {
            this.amount = amount;
            Vector2 platformPosition = Orig + Calc.AngleToVector(-MathHelper.PiOver2 * amount, Math.Clamp(player.X - Orig.X, 0, 48));
            HelperPlatform.MoveToY(platformPosition.Y, 0);
            HelperPlatform.CenterX = platformPosition.X;
            Collidable = amount < 1 && player.Bottom < HelperPlatform.Bottom;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Components.RemoveAll<Image>();
            if (CompletedFlag)
            {
                SetProgress(Scene.GetPlayer(), 1);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            HelperPlatform.RemoveSelf();
            tilesEntity.RemoveSelf();
        }
        public override void Render()
        {
            base.Render();
            Draw.LineAngle(Orig, amount * -MathHelper.PiOver2, trapdoorWidth, Color.Purple);
        }
    }
    [CustomEntity("PuzzleIslandHelper/TrapdoorChandelier")]
    [Tracked]
    public class TrapdoorChandelier : Entity
    {
        public int SpinDirection;
        public float Amount;
        private bool slowDown;
        private float closeSpeedMult;
        private float openTime, openTimer;
        private float waitTimer;
        private float prevEased;
        private bool skipOpenAnimation;
        public float FrameValue;
        public float FrameSpeed;
        public MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/elderTrapdoor/chandelier0" + Frame];
        public int Frame;
        private int maxFrames = 8;
        public EntityID ID;
        private bool canDashAttack = true;
        public bool Spinning => TagCheck(Tags.Persistent);
        private string origLevelName;
        private float singleWaitTime = 4;
        private float leaderWaitTime = 10;
        private bool extendedTime;
        public bool InPlay;
        public TrapdoorChandelier(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            ID = id;
            Collider = new Hitbox(Texture.Width, Texture.Height);
            Position -= Collider.HalfSize;
            VertexLight light;
            Add(light = new VertexLight(Color.White, 0.5f, (int)Width, (int)Width * 2));
            light.Position = Collider.HalfSize;
            BloomPoint bloom;
            Add(bloom = new BloomPoint(0.5f, (int)Width));
            bloom.Position = Collider.HalfSize;
            Tag |= Tags.TransitionUpdate;
            Add(new PlayerCollider((v) =>
            {
                if (!v.StartedDashing && canDashAttack && v.DashAttacking && v.DashDir.X != 0)
                {
                    Tag |= Tags.Persistent | Tags.Global;
                    SceneAs<Level>().Session.DoNotLoad.Add(ID);
                    InPlay = true;
                    canDashAttack = false;
                    SpinDirection = Math.Sign(v.DashDir.X);
                    Start(0.8f, false);
                }
            }));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            origLevelName = (scene as Level).Session.Level;
            Frame = (scene as Level).Session.GetCounter("TrapdoorChandelierLastFrame{" + ID.Key.ToString() + '}');
            FrameValue = Frame;
            Rectangle bounds = (scene as Level).Bounds;
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/elderTrapdoor/rod"];
            float offset = -tex.Height;
            Vector2 p = TopCenter;
            while (!Scene.CollideCheck<Solid>(p) && bounds.Contains(p))
            {
                Add(new Image(tex)
                {
                    Position = TopCenter - Position + new Vector2(-tex.Width / 2, offset)
                });
                offset -= 8;
                p.Y -= 8;
            }
        }
        private float inchValue;
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;

            if (!player.DashAttacking)
            {
                canDashAttack = true;
            }

            if (!(player.StateMachine.State == Player.StDummy || player.Dead || level.Transitioning))
            {
                float prevAmount = Amount;
                if (openTimer > 0 && !skipOpenAnimation)
                {
                    openTimer = Math.Max(openTimer - Engine.DeltaTime, 0);
                    float eased = Ease.SineIn(1 - (openTimer / openTime));
                    Amount = Math.Min(1, Amount + eased - prevEased);
                    FrameSpeed = 30f * SpinDirection;
                    prevEased = eased;
                }
                else if (waitTimer > 0)
                {
                    closeSpeedMult = 0;
                    FrameSpeed = 30f * SpinDirection;
                    Amount = 1;
                    waitTimer -= Engine.DeltaTime;
                    if (waitTimer <= 0)
                    {
                        waitTimer = 0;
                        slowDown = true;
                    }
                }
                else if (slowDown)
                {
                    closeSpeedMult = Calc.Approach(closeSpeedMult, 1, Engine.DeltaTime);
                    Amount = Calc.Approach(Amount, 0, closeSpeedMult * Engine.DeltaTime / 2);
                    FrameSpeed = 30f * Amount * SpinDirection;
                }
                if (slowDown && FrameSpeed == 0 && Amount == 0)
                {
                    if (InPlay)
                    {
                        RemoveTag(Tags.Persistent);
                        RemoveTag(Tags.Global);
                        SceneAs<Level>().Session.DoNotLoad.Remove(ID);
                        InPlay = false;
                    }
                    if (player.SceneAs<Level>().Session.Level == origLevelName)
                    {
                        if (!(Frame == 0 || Frame == 4))
                        {
                            inchValue += SpinDirection * Engine.DeltaTime * 2;
                            if (inchValue < 0)
                            {
                                inchValue = (maxFrames - 1) + inchValue;
                            }
                            if (inchValue >= maxFrames)
                            {
                                inchValue = inchValue % maxFrames;
                            }
                        }
                        else if (PianoModule.Session.LeaderTimer == this)
                        {
                            PianoModule.Session.LeaderTimer = null;
                        }
                    }
                    else
                    {
                        RemoveSelf();
                    }
                }
                if (FrameSpeed != 0)
                {
                    FrameValue = (FrameValue + FrameSpeed * Engine.DeltaTime);
                    if (FrameValue >= maxFrames)
                    {
                        FrameValue %= maxFrames;
                    }
                    else if (FrameValue < 0)
                    {
                        FrameValue = (maxFrames - 1) + (FrameValue % maxFrames);
                    }
                }
                if (SpinDirection < 0)
                {
                    Frame = (maxFrames - 1) - (int)((FrameValue + inchValue) % maxFrames);
                }
                else if (SpinDirection > 0)
                {
                    Frame = (int)((FrameValue + inchValue) % maxFrames);
                }
            }
        }

        public void Start(float openTime, bool skipOpen = false)
        {
            bool isLeader = false;
            foreach (TrapdoorChandelier c in Scene.Tracker.GetEntities<TrapdoorChandelier>())
            {
                if (c != this && c.InPlay)
                {
                    PianoModule.Session.LeaderTimer = this;
                    isLeader = true;
                    break;
                }
            }
            this.openTime = openTimer = openTime;
            closeSpeedMult = 0;
            waitTimer = isLeader ? leaderWaitTime : singleWaitTime;
            if (isLeader)
            {
                foreach (TrapdoorChandelier c in Scene.Tracker.GetEntities<TrapdoorChandelier>())
                {
                    if (c != this && c.InPlay)
                    {
                        if (c.openTimer > 0 || c.waitTimer <= 0 || c.slowDown) c.MimicStart(openTime, leaderWaitTime + openTime, true);
                        else c.waitTimer = leaderWaitTime;
                    }
                }
            }
            FrameValue = (FrameValue + inchValue) % maxFrames;
            inchValue = 0;
            prevEased = 0;
            skipOpenAnimation = skipOpen;
            slowDown = false;
        }
        public void MimicStart(float openTime, float waitTime, bool skipOpen = false)
        {
            this.openTime = openTimer = openTime;
            closeSpeedMult = 0;
            waitTimer = waitTime;
            FrameValue = (FrameValue + inchValue) % maxFrames;
            inchValue = 0;
            prevEased = 0;
            skipOpenAnimation = skipOpen;
            slowDown = false;
        }
        public override void Removed(Scene scene)
        {
            (scene as Level).Session.SetCounter("TrapdoorChandelierLastFrame{" + ID.Key.ToString() + '}', Frame);
            base.Removed(scene);
            (scene as Level).Session.DoNotLoad.Remove(ID);
            InPlay = false;
            if (PianoModule.Session.LeaderTimer == this)
            {
                PianoModule.Session.LeaderTimer = null;
            }
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position, Color.White);
        }
    }

    [Tracked]
    [Obsolete("Use ElderTrapdoor.cs")]
    public class BetaElderTrapdoor : Solid
    {
        public static MTexture TextureFG => GFX.Game["objects/PuzzleIslandHelper/elderTrapdoor/textureFG"];
        public static MTexture TextureBG => GFX.Game["objects/PuzzleIslandHelper/elderTrapdoor/textureBG"];
        public static MTexture TextureBlock => GFX.Game["objects/PuzzleIslandHelper/elderTrapdoor/block"];
        private Vector2 from;
        private Vector2 to;
        private Vector2 prev;
        private Solid Floor;
        private Solid Ceiling;
        private Image Image;
        public BetaElderTrapdoor(EntityData data, Vector2 offset) : base(data.Position + offset, TextureFG.Width, 8, true)
        {
            Tag |= Tags.TransitionUpdate;
            Image = new Image(TextureBG);
            Add(Image);
            Depth = 1;
            Position -= new Vector2(TextureFG.Width, TextureFG.Height) / 2;
            from = Position;
            to = data.NodesOffset(offset)[0] - new Vector2(TextureFG.Width, TextureFG.Height) / 2;
            Floor = new Solid(Position + Vector2.UnitY * (TextureFG.Height - 8), Width, Height, true)
            {
                new Image(TextureBlock),
                new LightOcclude()
            };
            Floor.Tag = Tags.TransitionUpdate;

            Ceiling = new Solid(Position, Width, Height, true)
            {
                new Image(TextureFG),
                new Image(TextureBlock),
                new LightOcclude()
            };
            Ceiling.Tag = Tags.TransitionUpdate;

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Floor);
            scene.Add(Ceiling);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(Floor);
            scene.Remove(Ceiling);
        }
        public override void Update()
        {
            base.Update();
            float max = 0;
            /*            foreach (ChandelierTimer timer in Scene.Tracker.GetEntities<ChandelierTimer>())
                        {
                            max = Math.Max(max, timer.Amount);
                        }*/
            SetProgress(max);
        }
        public void SetProgress(float percent)
        {
            prev = Position;
            Vector2 target = Vector2.Lerp(from, to, percent);
            Vector2 amount = target - prev;
            int x = (int)Math.Round(amount.X);
            int y = (int)Math.Round(amount.Y);
            MoveH(x);
            MoveV(y);
            Floor.MoveH(x);
            Floor.MoveV(y);
            Ceiling.MoveH(x);
            Ceiling.MoveV(y);
        }
    }
}