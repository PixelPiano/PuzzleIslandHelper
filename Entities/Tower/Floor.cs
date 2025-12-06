using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Tower.Stairs;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [Tracked]
    public class TowerFloor : Entity
    {
        public class StairAreaRenderer : Entity
        {
            public Mask Mask;
            public Action RenderAction;
            public string Flag;
            public bool RenderOnce;
            private VirtualRenderTarget target;
            private bool renderedOnce;
            private Stairs tower;
            public StairAreaRenderer(Mask mask, Stairs tower, Action renderZero, string flag, bool renderOnce, int depth) : base(new Vector2(tower.X, mask.Position.Y))
            {
                this.tower = tower;
                Tag |= Tags.TransitionUpdate;
                Mask = mask;
                RenderAction = renderZero;
                Flag = flag;
                RenderOnce = renderOnce;
                Depth = depth;
                target = VirtualContent.CreateRenderTarget("TowerStairsFloorRenderer", (int)tower.Width, mask.target.Height);
                Add(new BeforeRenderHook(BeforeRender));
            }
            public void BeforeRender()
            {
                if (RenderAction == null || (RenderOnce && renderedOnce)) return;
                renderedOnce = true;
                target.DrawThenMask(drawMask, RenderAction, Matrix.Identity, null);
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                target?.Dispose();
            }
            private void drawMask()
            {
                Draw.SpriteBatch.Draw(Mask.target, -Vector2.UnitX * (Mask.Position.X - tower.X), Color.White);
            }
            public override void Render()
            {
                base.Render();
                if (string.IsNullOrEmpty(Flag) || SceneAs<Level>().Session.GetFlag(Flag))
                {
                    Draw.SpriteBatch.Draw(target, Position + Vector2.UnitY, Color.White);
                }
            }
        }
        public Stairs TowerStairs;
        private bool rendered;
        public Mask LeftMask;
        public Mask RightMask;
        public FlashTri[] LeftFlashes, RightFlashes;
        public TowerFloor(Stairs parent, Vector2 position, Vector2[] points, int width, int height) : base(position)
        {
            TowerStairs = parent;
            LeftMask = new Mask(points, false, width, height);
            RightMask = new Mask(points, true, width, height);
            Collider = new Hitbox(width, height);
            LeftFlashes = CreateFlashTriArray(LeftMask);
            RightFlashes = CreateFlashTriArray(RightMask);
            Add(LeftFlashes);
            Add(RightFlashes);
            Tag |= Tags.TransitionUpdate;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public FlashTri[] CreateFlashTriArray(Mask mask)
        {
            FlashTri[] Flashes = new FlashTri[mask.Points.Length - 1];
            for (int i = 1; i < mask.Points.Length; i++)
            {
                Flashes[i - 1] = new FlashTri(mask.Anchor, mask.Points[i - 1], mask.Points[i], mask.SortRight);
            }
            if (mask.SortRight)
            {
                Flashes = [.. Flashes.Reverse()];
            }
            return Flashes;
        }
        public override void DebugRender(Camera camera)
        {
            Draw.SpriteBatch.Draw(LeftMask.target, Position, Color.Yellow * 0.5f);
            Draw.SpriteBatch.Draw(RightMask.target, Position, Color.Cyan * 0.5f);
            base.DebugRender(camera);
        }
        public void BeforeRender()
        {
            if (rendered) return;
            Matrix matrix = Matrix.Identity;//level.Camera.Matrix;
            rendered = true;
            LeftMask.target.SetAsTarget(true);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            LeftMask.DrawToMask(matrix);
            Draw.SpriteBatch.End();
            RightMask.target.SetAsTarget(true);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            RightMask.DrawToMask(matrix);
            Draw.SpriteBatch.End();
        }
    }
}