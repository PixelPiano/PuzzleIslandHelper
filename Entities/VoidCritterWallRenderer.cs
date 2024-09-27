using Celeste.Mod.CherryHelper;
using Celeste.Mod.CommunalHelper.Backdrops;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;

// PuzzleIslandHelper.VoidCritters
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class VoidCritterWallHelper : Entity
    {

        private static VirtualRenderTarget _lights;
        public static VirtualRenderTarget Lights => _lights ??= VirtualContent.CreateRenderTarget("voidCritterWallLightBuffer", 320, 180);
        public bool PlayerSafe;
        public static readonly BlendState Subtract = new()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.ReverseSubtract
        };
        public VoidCritterWallHelper() : base()
        {
            Tag |= Tags.TransitionUpdate | Tags.Global;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public static bool CollidingWithLight(Entity entity, Level level)
        {
            foreach (CritterLight light in level.Tracker.GetComponents<CritterLight>())
            {
                if (light.Colliding(entity))
                {
                    return true;
                }
            }
            return false;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            PlayerSafe = player.Dead || player.JustRespawned || (player.Holding is Holdable h && h.Entity is VoidLamp) || !player.CollideCheck<VoidCritterWall>() || CollidingWithLight(player, level);
        }
        public void BeforeRender()
        {
            Lights.SetRenderTarget(Color.Transparent);
            if (Scene is not Level level) return;
            if (level.Tracker.GetComponents<CritterLight>() is not List<Component> list2 || list2.Count == 0) return;

            Draw.SpriteBatch.StandardBegin(level.Camera.Matrix);
            {
                foreach (CritterLight light in list2)
                {
                    if (light.OnScreen && light.Enabled)
                    {
                        light.DrawGradient(Color.White, VoidCritterWall.Offset * 2);
                    }
                }
            }
            Draw.SpriteBatch.End();
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        }

        private static void LevelLoader_OnLoadingThread(Level level)
        {
            level.Add(new VoidCritterWallHelper());
        }

        [OnUnload]
        public static void Unload()
        {
            _lights?.Dispose();
            _lights = null;
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        }

    }
}
