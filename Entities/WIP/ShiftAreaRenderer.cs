using Microsoft.Xna.Framework;
using Monocle;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

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
        public List<ShiftArea> Areas = new();
        public Vector2 Scale = Vector2.One;
        public Vector3 Offset;
        public Matrix Matrix = Matrix.Identity;
        public Vector3 RotationOrigin;
        public Vector2 ScaleOrigin;
        public float XRotation;
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
            Scale = Vector2.One;
            FG = fg;
            Depth = fg ? -10001 : 9999;
            Tag |= Tags.TransitionUpdate | Tags.Global;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            Matrix = Matrix.CreateFromAxisAngle(new Vector3(1f, 0, 0), XRotation);
            Vector3 origin = Vector3.Transform(RotationOrigin, Matrix);
            Matrix *= Matrix.CreateTranslation(MathHelper.Distance(origin.X, RotationOrigin.X) + Offset.X, MathHelper.Distance(origin.Y, RotationOrigin.Y) + Offset.Y, MathHelper.Distance(origin.Z, RotationOrigin.Z) + Offset.Z);
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || doNotRender) return;
            Draw.SpriteBatch.Draw(Target, level.Camera.Position + ScaleOrigin, null, Color.White, 0, ScaleOrigin, Scale, SpriteEffects.None, 0);
        }
        public void BeforeRender()
        {
            if (Scene is not Level level) return;
            doNotRender = !LevelHasAreas(level);
            Position = level.Camera.Position;
            if (doNotRender) return;
            GetAllAreas(level);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            foreach (ShiftArea area in Areas)
            {
                RenderArea(area);
            }
        }
        public void RenderArea(ShiftArea area)
        {
            Level level = Scene as Level;
            if (!area.Visible || !area.State) return;
            Vector2 add = area.Position + Offset.XY() - level.Camera.Position + (area.FollowCamera ? level.Camera.Position - level.LevelOffset : Vector2.Zero);
            area.BeforeRender(FG, !FG);
            Matrix matrix = Matrix * Matrix.CreateScale(new Vector3(1, 1, 0));
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            if (area.OnScreen)
            {
                Draw.SpriteBatch.Draw(FG ? area.FGTarget : area.BGTarget, add, null, area.AreaColor * area.Alpha, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            }
            area.AfterRender(level);
            Draw.SpriteBatch.End();
        }
        public bool LevelHasAreas(Level level)
        {
            return level.Tracker.GetEntities<ShiftArea>() is List<Entity> list && list.Count > 0;
        }
        private void GetAllAreas(Level level)
        {
            Areas.Clear();
            foreach (ShiftArea area in level.Tracker.GetEntities<ShiftArea>())
            {
                if ((FG && area.HasFG) || (!FG && area.HasBG))
                {
                    Areas.Add(area);
                }
            }
            Areas = Areas.OrderByDescending(item => item.AreaDepth).ToList();
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
