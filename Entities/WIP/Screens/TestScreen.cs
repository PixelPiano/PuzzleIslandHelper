using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP.Screens
{
    [CustomEntity("PuzzleIslandHelper/TestScreen")]
    [Tracked]
    public class TestScreen : Entity
    {
        public ViewScreen View;
        public Image Image;
        public Rectangle Screen = new Rectangle(0, 0, 320, 180);
        public TestScreen(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Image = new Image(GFX.Game["objects/PuzzleIslandHelper/TEST/collider"]));

            Collider = new Hitbox(Image.Width, Image.Height);
            Add(View = new ViewScreen(Width, Height, Screen, CodeDialogStorage.PipeLoader));
        }
    }
}
