using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [Tracked]
    public class ShiftAreaRenderer : Entity
    {
        private static VirtualRenderTarget _BGTarget;
        private static VirtualRenderTarget _FGTarget;
        public static VirtualRenderTarget BGTarget => _BGTarget ??= VirtualContent.CreateRenderTarget("ShiftAreaBGTarget", 320, 180);
        public static VirtualRenderTarget FGTarget => _FGTarget ??= VirtualContent.CreateRenderTarget("ShiftAreaFGTarget", 320, 180);
        public VirtualRenderTarget Target => FG ? FGTarget : BGTarget;
        public static bool UsedTemp;
        public bool FG;
        private bool doNotRender;
        public ShiftAreaRenderer(bool fg) : base(Vector2.Zero)
        {
            FG = fg;
            Depth = fg ? -10001 : 9999;
            Tag |= Tags.TransitionUpdate | Tags.Global;
            Add(new BeforeRenderHook(BeforeRender));
            Collider = new Hitbox(320, 180);
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || doNotRender) return;
            UsedTemp = false;
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
        }
        private bool levelHasAreas(Level level)
        {
            List<Entity> list = level.Tracker.GetEntities<ShiftArea>();
            return !(list is null || list.Count <= 0);
        }
        public void BeforeRender()
        {
            if (Scene is not Level level) return;
            doNotRender = !levelHasAreas(level);
            Position = level.Camera.Position;
            if(doNotRender) return;

            Matrix matrix = Matrix.Identity;

            if (!UsedTemp) //only draw masks once, since both renderers are running at the same time
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);

                foreach (ShiftArea area in level.Tracker.GetEntities<ShiftArea>())
                {
                    area.DrawMask(matrix);
                }
                Draw.SpriteBatch.End();
                UsedTemp = true;
            }


            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);

            foreach (ShiftArea area in level.Tracker.GetEntities<ShiftArea>())
            {
                Vector2 offset = area.origLevelOffset - level.Camera.Position;
                area.RenderTiles(FG, level);
            }

            Draw.SpriteBatch.End();

            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, EasyRendering.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
        public static void Unload()
        {
            _FGTarget?.Dispose();
            _BGTarget?.Dispose();
            _FGTarget = null;
            _BGTarget = null;
            On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
        }
        public static void Load()
        {
            On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
        }

        private static void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);
            self.Level.Add(new ShiftAreaRenderer(true));
            self.Level.Add(new ShiftAreaRenderer(false));
        }
    }
}
