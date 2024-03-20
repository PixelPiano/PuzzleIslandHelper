using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.FancyTileEntities;
using System;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Celeste.Mod.PandorasBox;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Configuration;
using FrostHelper.Helpers;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.ShiftAreaEntities
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
        public TileGrid Tiles;
        public bool FG;
        public ShiftAreaRenderer(bool fg, TileGrid tiles) : base(Vector2.Zero)
        {
            FG = fg;
            Depth = fg ? -10001 : 9999;
            Tag |= Tags.TransitionUpdate;
            Add(new BeforeRenderHook(BeforeRender));
            Tiles = tiles;
        }
        public void RenderAt(Vector2 offset)
        {
            if (Tiles.Alpha <= 0f) return;
            Rectangle clippedRenderTiles = GetClippedRenderTiles(Tiles, offset);
            int tileWidth = Tiles.TileWidth;
            int tileHeight = Tiles.TileHeight;
            Color color = Tiles.Color * Tiles.Alpha;
            Vector2 position2 = new Vector2(offset.X + clippedRenderTiles.Left * tileWidth, offset.Y + clippedRenderTiles.Top * tileHeight);
            position2 = position2.Round();
            for (int i = clippedRenderTiles.Left; i < clippedRenderTiles.Right; i++)
            {
                for (int j = clippedRenderTiles.Top; j < clippedRenderTiles.Bottom; j++)
                {
                    MTexture mTexture = Tiles.Tiles[i, j];
                    if (mTexture != null)
                    {
                        Draw.SpriteBatch.Draw(mTexture.Texture.Texture_Safe, position2, mTexture.ClipRect, color);
                    }

                    position2.Y += tileHeight;
                }

                position2.X += tileWidth;
                position2.Y = offset.Y + clippedRenderTiles.Top * tileHeight;
            }

        }
        public Rectangle GetClippedRenderTiles(TileGrid grid, Vector2 offset)
        {
            int left, top, bottom, right;

            Level level = SceneAs<Level>();
            Camera camera = level.Camera;
            Vector2 vector = camera.Position + offset;
            int extend = 0;//Tiles.VisualExtend;
            left = (int)Math.Max(0.0, Math.Floor((camera.Left - vector.X) / grid.TileWidth) - extend);
            top = (int)Math.Max(0.0, Math.Floor((camera.Top - vector.Y) / grid.TileHeight) - extend);
            right = (int)Math.Min(grid.TilesX, Math.Ceiling((camera.Right - vector.X) / grid.TileWidth) + extend);
            bottom = (int)Math.Min(grid.TilesY, Math.Ceiling((camera.Bottom - vector.Y) / grid.TileHeight) + extend);

            left = Math.Max(left, 0);
            top = Math.Max(top, 0);
            right = Math.Min(right, grid.TilesX);
            bottom = Math.Min(bottom, grid.TilesY);

            return new Rectangle(left, top, right - left, bottom - top);
        }
        public static void Unload()
        {
            _FGTarget?.Dispose();
            _BGTarget?.Dispose();
            _FGTarget = null;
            _BGTarget = null;
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            UsedTemp = false;
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
        }
        public void BeforeRender()
        {
            if (Scene is not Level level) return;
            Matrix matrix = Matrix.Identity;

            if (!UsedTemp)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
                    DepthStencilState.None, RasterizerState.CullNone, null, matrix);

                foreach (ShiftArea area in level.Tracker.GetEntities<ShiftArea>())
                {
                    area.DrawMask();
                }
                Draw.SpriteBatch.End();
                UsedTemp = true;
            }


            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, matrix);

            Vector2 offset = level.LevelOffset - level.Camera.Position;
            RenderAt(offset - Tiles.VisualExtend * Vector2.One * 8);

            Draw.SpriteBatch.End();

            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, EasyRendering.AlphaMaskBlendState, SamplerState.PointClamp,
                DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}
