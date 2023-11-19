using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class SurfaceSoundBlock :Solid 
    {
        public SurfaceSoundBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, data.Bool("safe"))
        {
            SurfaceSoundIndex = data.Int("surfaceSoundIndex");
        }

    }
}