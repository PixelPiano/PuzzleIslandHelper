using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.CherryHelper;
using Celeste.Mod.PuzzleIslandHelper.Effects;
// PuzzleIslandHelper.MovingPlatform
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/SolidBlock")]
    [Tracked]
    public class SolidBlock : Solid
    {
        private TileGrid tiles;
        public SolidBlock(EntityData data, Vector2 offset)
          : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            Depth = -100000;
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            Add(tiles = GFX.FGAutotiler.GenerateBox(data.Char("tiletype", '3'), data.Width / 8, data.Height / 8).TileGrid);
            Calc.PopRandom();
            Collider = new Hitbox(data.Width, data.Height);
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, false));
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[data.Char("tiletype", '3')];
        }
        public override void OnShake(Vector2 amount)
        {
            if (!InvertOverlay.State)
            {
                base.OnShake(amount);
                tiles.Position += amount;
            }
        }
    }
}