using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Backdrops;
using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.PuzzleIslandHelper.Helpers;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/BookshelfRoulette")]
    public class BookshelfRoulette : Parallax
    {
        public BookshelfRoulette(MTexture texture) : base(texture)
        {
        }
    }
}