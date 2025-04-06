using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using static Celeste.Autotiler;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    //todo: rework void wall rendering to use custom tileset for gradient
    public class VoidTiles : Entity
    {
        public SolidTiles tiles;
        public TileGrid Tiles;
        public Grid Grid;
        public VirtualMap<char> Data;
        public VoidTiles(Vector2 position, VirtualMap<char> data)
        {
            Position = position;
            Data = data;
            Autotiler.Generated generated = GFX.FGAutotiler.GenerateMap(data, paddingIgnoreOutOfLevel: true);
            Tiles = generated.TileGrid;
            Tiles.VisualExtend = 1;
        }

    }
    [Tracked]
    public class VoidCritterWallHelper : Entity
    {
        public static bool Simple => PianoModule.Session.DEBUGBOOL1;
        private static VirtualRenderTarget _lights;
        public static VirtualRenderTarget Lights => _lights ??= VirtualContent.CreateRenderTarget("voidCritterWallLightBuffer", 320, 180);
        private static VirtualRenderTarget _walls;
        public static VirtualRenderTarget Walls => _walls ??= VirtualContent.CreateRenderTarget("voidCritterWallWallBuffer", 320, 180);
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
            Tag |= Tags.TransitionUpdate | Tags.Persistent;
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
            if (Scene is not Level level || level.GetPlayer() is not Player player || player.Dead || player.JustRespawned
                || (player.Holding is Holdable h && h.Entity is VoidLamp))
            {
                return;
            }
            VoidCritterWall collided = player.CollideFirst<VoidCritterWall>();
            if (collided != null && collided.FlagState && !(CollidingWithLight(player, level) || VoidSafeZone.Check(player)))
            {
                player.Die(Vector2.Zero);
            }
        }
        public void BeforeRender()
        {
            if (Simple)
            {
                SimpleBeforeRender();
            }
            else
            {
                ComplexBeforeRender();
            }
        }
        public override void Render()
        {
            base.Render();
            if (Simple)
            {
                SimpleRender();
            }
        }
        public void ComplexBeforeRender()
        {
            Lights.SetAsTarget(Color.Transparent);
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
        public void SimpleBeforeRender()
        {
            if (Scene is not Level level || level.Tracker.GetEntities<VoidCritterWall>() is not List<Entity> list || list.Count == 0) return;
            if (level.Tracker.GetComponents<CritterLight>() is not List<Component> list2 || list2.Count == 0) return;

            Lights.SetAsTarget(Color.Transparent);
            Draw.SpriteBatch.StandardBegin(level.Camera.Matrix);
            {
                foreach (CritterLight light in list2)
                {
                    if (light.OnScreen && light.Enabled)
                    {
                        light.DrawLight(Color.White);
                    }
                }
            }
            Draw.SpriteBatch.End();

            Walls.SetAsTarget(Color.Transparent);
            Draw.SpriteBatch.StandardBegin(level.Camera.Matrix);
            foreach (VoidCritterWall wall in list)
            {
                if (wall.OnScreen)
                {
                    Draw.Rect(wall.Collider, Color.White);
                }
            }
            Draw.SpriteBatch.End();

            Draw.SpriteBatch.StandardBegin(level.Camera.Matrix, Subtract);
            {
                Draw.SpriteBatch.Draw((RenderTarget2D)Lights, level.Camera.Position, Color.White);
            }
            Draw.SpriteBatch.End();
        }
        public void SimpleRender()
        {
            base.Render();
            if (Scene is not Level level) return;
            Draw.SpriteBatch.Draw((RenderTarget2D)Walls, level.Camera.Position, Color.MediumPurple);
        }

        [OnUnload]
        public static void Unload()
        {
            _lights?.Dispose();
            _lights = null;
            _walls?.Dispose();
            _walls = null;
        }

    }
}