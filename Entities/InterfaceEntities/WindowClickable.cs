using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class WindowClickable : WindowComponent
    {
        public Collider Collider;
        public WindowClickable(Window window, float width, float height) : base(window)
        {
            Window = window;
            Collider = new Hitbox(width, height);
        }
        public virtual void OnClick()
        {

        }
        public override void Update()
        {
            base.Update();
            Collider.Position = RenderPosition;
        }
    }
}