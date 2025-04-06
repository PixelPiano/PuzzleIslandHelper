using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Grate")]
    [Tracked]
    public class Grate : Solid
    {
        private MTexture texture => GFX.Game[path];
        private string path;

        public Grate(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, 8, true)
        {
            path = data.Attr("path");
            Tag |= Tags.TransitionUpdate;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            int cols = (int)(Width / 8);

            Image left = new Image(texture.GetSubtexture(0, Calc.Random.Range(0, 2) * 8, 8, 8));
            Add(left);
            for (int x = 1; x < cols; x++)
            {
                Image middle = new Image(texture.GetSubtexture(8, Calc.Random.Range(0, 2) * 8, 8, 8));
                Add(middle);
                middle.Position.X = x * 8;
            }
            Image right = new Image(texture.GetSubtexture(16, Calc.Random.Range(0, 2) * 8, 8, 8));
            Add(right);
            right.Position.X = Width - 8;

        }
    }
}
