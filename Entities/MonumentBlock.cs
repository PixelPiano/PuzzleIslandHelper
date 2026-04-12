using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MonumentBlock")]
    [Tracked]
    public class MonumentBlock : Solid
    {
        private EntityID id;
        private char tileType;
        private bool blendIn;
        private TileGrid tileGrid;
        public MonumentBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, true)
        {
            tileType = data.Char("tiletype", '3');
            blendIn = data.Bool("blendIn");
            Depth = -12999;
            this.id = id;
            Add(new MonumentComponent()
            {
                IDs = data.Attr("shiftID").Replace(" ","").Split(',',System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries),
                Width = data.Width,
                Height = data.Height,
                PadX = -8,
                PadY = -8,
                OnEnableHook = (bool instant) =>
                {
                    Visible = true;
                },
                OnDisableHook = (bool instant) =>
                {
                    Visible = false;
                }
            });

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!blendIn)
            {
                tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)Width / 8, (int)Height / 8).TileGrid;
            }
            else
            {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int)(X / 8f) - tileBounds.Left;
                int y = (int)(Y / 8f) - tileBounds.Top;
                int tilesX = (int)Width / 8;
                int tilesY = (int)Height / 8;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
                Add(new EffectCutout());
            }
            Add(new LightOcclude());
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, highPriority: true));
            if (CollideCheck<Player>())
            {
                RemoveSelf();
            }
        }
        public override void Update()
        {
            base.Update();
        }
    }
}