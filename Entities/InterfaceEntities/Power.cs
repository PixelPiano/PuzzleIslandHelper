using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [TrackedAs(typeof(DesktopClickable))]
    public class Power : DesktopClickable
    {
        public Sprite sprite;
        public Power(Interface inter, string path = "objects/PuzzleIslandHelper/interface/") : base(inter, (int)Interface.Priority.Power)
        {
            Depth = Interface.BaseDepth - 1;
            Add(sprite = new Sprite(GFX.Game, path));
            sprite.AddLoop("idle", "power", 1f);
            Collider = new Hitbox(sprite.Width, sprite.Height);
            sprite.Play("idle");
        }
        public override void Begin(Scene scene)
        {
            base.Begin(scene);
            sprite.Play("idle");

        }
        public override void Update()
        {
            base.Update();
            if (Interface.Monitor is not null)
            {
                Position = Interface.Monitor.Position + new Vector2(8, Interface.Monitor.Height - Height - 8);
            }
        }
        public override void OnClick()
        {
            base.OnClick();
            if (!Interface.Closing)
            {
                Interface.CloseInterface(false);
            }
        }
    }
}