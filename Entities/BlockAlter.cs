using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BlockAlter")]
    [Tracked]
    public class BlockAlter : Solid
    {
        private EntityID id;
        private char tileType;
        public BlockAlter(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, true)
        {
            tileType = data.Char("tiletype", '3');
            Depth = -12999;
            this.id = id;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            TileGrid tileGrid;
            tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)Width / 8, (int)Height / 8).TileGrid;
            Add(new LightOcclude());
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, highPriority: true));
            if (CollideCheck<Player>())
            {
                RemoveSelf();
            }
        }
    }
}