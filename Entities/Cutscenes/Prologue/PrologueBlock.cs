using Celeste.Mod.Entities;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static FrostHelper.Entities.GrowBlock;

// PuzzleIslandHelper.DecalEffects
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [CustomEntity("PuzzleIslandHelper/PrologueBlock")]
    [Tracked]
    public class PrologueBlock : Solid
    {
        public TileGrid Tiles;
        private char tileType = 'w';
        public EffectCutout Cutout;
        public int Order;
        public float Delay;
        public bool Fg;
        public string Flag;
        public bool ForceUncollidable;
        public float Amplitude;
        public bool Appeared;
        public bool WaitForController;
        public bool ControllerResponded;

        public bool UsingEffect => !Tiles.Visible && !Instant && Amplitude < 1 && ControllerState;
        public bool Instant => Order < 0;
        public bool TilesVisible => Tiles is not null && Tiles.Visible;
        public bool ControllerState => (WaitForController && ControllerResponded) || !WaitForController;

        public TransitionListener Listener;
        public VirtualRenderTarget Target;
        private static VirtualRenderTarget _Mask;
        public bool GondolaBlock;
        public static VirtualRenderTarget Mask => _Mask ??= VirtualContent.CreateRenderTarget("PrologueBlockMask", 320, 180);
        public PrologueBlock(Vector2 position, float width, float height, int order, float delay, bool fg, bool waitForController) : base(position, width, height, false)
        {
            Depth = fg ? -10501 : 5000;
            Order = order;
            Delay = delay;
            Fg = fg;
            WaitForController = waitForController;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            Target = VirtualContent.CreateRenderTarget("PrologueBlockTarget", (int)Width, (int)Height);
            Add(new PlayerCollider(OnPlayer));
            Add(Cutout = new EffectCutout());
            Tag |= Tags.TransitionUpdate;
            Add(Listener = new TransitionListener());
            Listener.OnOutBegin = () =>
            {
                ForceUncollidable = true;
            };
        }
        public PrologueBlock(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Int("order"), data.Float("delay"), data.Bool("fg"), data.Bool("waitForController"))
        {

        }
        public void OnPlayer(Player player)
        {
            if (!ControllerState) return;
            if (!player.Dead && Amplitude > 0.5f)
            {
                PianoUtils.InstantRelativeTeleport(Scene, "transfer", true);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int)(base.X / 8f) - tileBounds.Left;
            int y = (int)(base.Y / 8f) - tileBounds.Top;
            int tilesX = (int)base.Width / 8;
            int tilesY = (int)base.Height / 8;
            Tiles = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
            Add(Tiles);
            Add(new TileInterceptor(Tiles, highPriority: false));
            Add(new BeforeRenderHook(BeforeRender));
            Appeared = Tiles.Visible = Instant;
            Collidable = Amplitude > 0.5f || Instant;
            DisableLightsInside = false;
        }
        public void Appear()
        {
            if (!ControllerState) return;
            Visible = true;

            Add(new Coroutine(FuzzIn()));
        }

        public IEnumerator FuzzIn()
        {
            if (!Instant)
            {
                yield return Delay;
                for (float i = 0; i < 1; i += Engine.DeltaTime * 1.5f)
                {
                    Amplitude = Ease.CubeIn(i);
                    yield return null;
                }
            }
            Tiles.Visible = true;
            Amplitude = 1;
            Appeared = true;
            yield return null;
        }
        public void BeforeRender()
        {

            #region SetRenderTarget
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Mask);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            #endregion
            if (!UsingEffect || Scene is not Level level) return;
            ShaderFX.FuzzyAppear.ApplyScreenSpaceParameters(level);
            ShaderFX.FuzzyAppear.Parameters["Amplitude"]?.SetValue(Amplitude);

            #region DrawToObject
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,
                null, Matrix.Identity);
            Tiles.RenderAt(Vector2.Zero);
            Draw.SpriteBatch.End();
            #endregion


            #region MaskToObject
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                EasyRendering.AlphaMaskBlendState,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                ShaderFX.FuzzyAppear, level.Camera.Matrix);
            Draw.SpriteBatch.Draw(Mask, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            #endregion
        }
        public override void Render()
        {
            base.Render();
            if (!UsingEffect || Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Target, Position, Color.White);
        }

        public override void Update()
        {
            base.Update();
            Collidable = !ForceUncollidable && ControllerState && (Amplitude > 0.5f || Instant);

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
        }
        public static void Unload()
        {
            _Mask?.Dispose();
            _Mask = null;
        }

    }
    [CustomEntity("PuzzleIslandHelper/PrologueBlockController")]
    [Tracked]
    public class PrologueBlockManager : Entity
    {
        public string Flag;
        public bool State
        {
            get
            {
                if (Scene is not Level level) return false;
                return string.IsNullOrEmpty(Flag) || level.Session.GetFlag(Flag);
            }
        }
        public bool Started;
        public bool SpawnKillblock;

        public PrologueBlockManager(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SpawnKillblock = data.Bool("spawnKillblockOnEnd");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            TryStart();
        }
        public override void Update()
        {
            base.Update();
            TryStart();
        }
        public void TryStart()
        {
            if (!Started && State)
            {
                Add(new Coroutine(Sequence(Scene as Level)));
            }
        }
        public IEnumerator BlocksAppear(List<Entity> blocks)
        {
            float time = 0;
            foreach (PrologueBlock block in blocks)
            {
                block.Appear();
                if (block.Delay > time)
                {
                    time = block.Delay;
                }
            }
            yield return time;
        }
        public IEnumerator Sequence(Level level)
        {
            if (level is null || Started) yield break;
            Started = true;
            int currentOrder = 0;
            bool allAdded;
            List<Entity> blocks = new();
            List<Entity> blocksInLevel = level.Tracker.GetEntities<PrologueBlock>();
            while (true)
            {
                yield return null;
                blocks.Clear();
                allAdded = true;
                for (int i = 0; i < blocksInLevel.Count; i++)
                {

                    PrologueBlock block = blocksInLevel[i] as PrologueBlock;
                    block.ControllerResponded = true;
                    if (!block.Appeared)
                    {
                        allAdded = false;
                    }
                    if (block.Order != currentOrder) continue;
                    blocks.Add(blocksInLevel[i]);
                }
                if (blocks.Count > 0)
                {
                    yield return BlocksAppear(blocks);
                }
                currentOrder++;
                if (allAdded || currentOrder > 100) break;
            }
            if (SpawnKillblock)
            {
                yield return 1f;
                PrologueKillBlock killBlock = new PrologueKillBlock();
                level.Add(killBlock);
            }
        }
    }

    [Tracked]
    public class PrologueKillBlock : Entity
    {
        public PrologueKillBlock() : base(Vector2.Zero)
        {

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            if (scene.GetPlayer() is not Player player || player.Dead) return;
            PrologueBlock killBlock = new PrologueBlock(level.LevelOffset, level.Bounds.Width, level.Bounds.Height, 0, 0, true, false);
            scene.Add(killBlock);
            killBlock.Appear();
        }
    }

}
