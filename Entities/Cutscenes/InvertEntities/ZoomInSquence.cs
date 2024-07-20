using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Effects;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.InvertEntities
{
    [Tracked]
    public class ZoomInSequence : Entity
    {
        private static VirtualRenderTarget _Buffer;
        public static VirtualRenderTarget Buffer => _Buffer ??= VirtualContent.CreateRenderTarget("InvertZoomIn", 320, 180);
        private bool activated;
        public int Zoom = 1;
        public ZoomInSequence(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(new BeforeRenderHook(BeforeRender));
            Visible = false;
            Depth = -105010;
        }
        public void Activate()
        {
            activated = true;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Activate();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            _Buffer?.Dispose();
            _Buffer = null;
        }
        private void BeforeRender()
        {
            if (!Visible || !activated || Scene is not Level level) return;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,
            null, level.Camera.Matrix);
            level.BgTiles.Tiles.Render();
            foreach (Entity e in level.Entities)
            {
                if (e is BackgroundTiles or Trigger or SolidTiles or ZoomInSequence or InvertAuth.InvertOrb or InvertAuth.GlassStatic or Player) continue;
                e.Render();
            }
            level.SolidTiles.Tiles.Render();
            Draw.SpriteBatch.End();
        }
        public override void Render()
        {
            base.Render();
            if (!activated || Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Buffer, level.Camera.Position, null, Color.White, 0, Vector2.One / 2f, Zoom, SpriteEffects.None, 0);
        }
    }
}
