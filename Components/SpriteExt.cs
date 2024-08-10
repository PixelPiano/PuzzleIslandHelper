using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [TrackedAs(typeof(Sprite))]
    public class SpriteExt : Sprite
    {
        public int SubX;
        public int SubY;
        public int SubWidth;
        public int SubHeight;
        public SpriteExt() : base() { }
        public SpriteExt(Atlas atlas, string path) : base(atlas, path)
        {

        }
        public SpriteExt(Atlas atlas, string path, int x, int y, int width, int height) : base(atlas, path)
        {

        }
    }
}