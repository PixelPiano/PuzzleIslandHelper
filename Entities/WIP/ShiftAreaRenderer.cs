using Microsoft.Xna.Framework;
using Monocle;
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
        public bool FG;
        private bool doNotRender;
        public static void ChangeDepth(bool fg, int depth)
        {
            if (Engine.Scene is not Level level) return;
            foreach (ShiftAreaRenderer area in level.Tracker.GetEntities<ShiftAreaRenderer>())
            {
                if (area.FG == fg)
                {
                    area.Depth = depth;
                }
            }
        }
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
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
        }
        public void BeforeRender()
        {
            if (Scene is not Level level) return;
            doNotRender = !levelHasAreas(level);
            Position = level.Camera.Position;
            if (doNotRender) return;
            Matrix matrix = Matrix.Identity;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            foreach (ShiftArea area in level.Tracker.GetEntities<ShiftArea>())
            {
                if(!area.OnScreen) continue;
                area.BeforeRender(FG, !FG);
                Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
                Draw.SpriteBatch.Draw(FG ? area.FGTarget : area.BGTarget, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }
        }
        private bool levelHasAreas(Level level)
        {
            foreach (ShiftArea area in level.Tracker.GetEntities<ShiftArea>())
            {
                if ((FG && area.HasFG) || (!FG && area.HasBG)) return true;
            }
            return false;
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Depth = FG ? -10001 : 9999;
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
