using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using static Celeste.Autotiler;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/AddAnimatedTilesTrigger")]
    [Tracked]
    public class AddAnimatedTilesTrigger : Trigger
    {
        public Vector2[] Nodes;
        public char newTileType;
        public bool newBlendIn;
        public bool linkVisible = true;
        public bool linkPositions = true;
        public int extX, extY;
        public Vector2 Offset;
        public bool addTileInterceptorIfAbsent = true;
        public AddAnimatedTilesTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Nodes = data.NodesOffset(offset);
            newTileType = data.Char("newTileType", '3');
            newBlendIn = data.Bool("newBlendIn", true);
            linkVisible = data.Bool("linkVisible", true);
            linkPositions = data.Bool("linkPositions", true);
            extX = data.Int("extendX");
            extY = data.Int("extendY");
            Offset = new Vector2(data.Float("offsetX"), data.Float("offsetY"));
            addTileInterceptorIfAbsent = data.Bool("addTileInterceptorIfAbsent", true);
            Tag |= Tags.TransitionUpdate;
        }
        [Tracked]
        private class animatedModifier : Component
        {
            public AnimatedTiles AnimatedTiles;
            public TileGrid TileGrid;
            public AddAnimatedTilesTrigger Trigger;
            public animatedModifier(TileGrid from, AddAnimatedTilesTrigger trigger) : base(true, false)
            {
                Trigger = trigger;
                TileGrid = from;
            }

            public override void Added(Entity entity)
            {
                base.Added(entity);
                Generated g;
                if (!Trigger.newBlendIn)
                {
                    g = GFX.FGAutotiler.GenerateBox(Trigger.newTileType, (int)entity.Width / 8 + Trigger.extX, (int)entity.Height / 8 + Trigger.extY);
                }
                else
                {
                    Level level = entity.SceneAs<Level>();
                    Rectangle tileBounds = level.Session.MapData.TileBounds;
                    VirtualMap<char> solidsData = level.SolidsData;
                    int x = (int)(entity.X / 8f) - tileBounds.Left;
                    int y = (int)(entity.Y / 8f) - tileBounds.Top;
                    int tilesX = (int)entity.Width / 8 + Trigger.extX;
                    int tilesY = (int)entity.Height / 8 + Trigger.extY;
                    g = GFX.FGAutotiler.GenerateOverlay(Trigger.newTileType, x, y, tilesX, tilesY, solidsData);
                }
                AnimatedTiles = g.SpriteOverlay;
                entity.Add(AnimatedTiles);
                UpdateGrids();
            }
            public void UpdateGrids()
            {
                if (AnimatedTiles != null && TileGrid != null)
                {
                    if (Trigger.linkVisible)
                    {
                        AnimatedTiles.Visible = TileGrid.Visible;
                        AnimatedTiles.Color = TileGrid.Color;
                        AnimatedTiles.Alpha = TileGrid.Alpha;
                    }

                    if (Trigger.linkPositions)
                    {
                        AnimatedTiles.Position = TileGrid.Position + Trigger.Offset;
                    }
                }
            }
            public override void Update()
            {
                base.Update();
                UpdateGrids();
            }
        }
        public override void Awake(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            foreach (Vector2 node in Nodes)
            {
                foreach (Entity e in scene.Entities)
                {
                    if (Collide.CheckPoint(e, node))
                    {
                        if (e != level.SolidTiles && e != level.BgTiles)
                        {
                            if (e.Get<TileGrid>() is TileGrid grid)
                            {
                                if (e.Get<animatedModifier>() == null &&
                                e.Get<AnimatedTiles>() == null)
                                {
                                    animatedModifier modifier = new(grid, this);
                                    e.Add(modifier);
                                }
                                if (addTileInterceptorIfAbsent && e.Get<TileInterceptor>() == null)
                                {
                                    e.Add(new TileInterceptor(grid, true));
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}