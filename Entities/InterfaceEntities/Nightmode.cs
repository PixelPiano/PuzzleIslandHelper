using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [TrackedAs(typeof(DesktopEntity))]
    public class Nightmode : DesktopEntity
    {
        public Sprite sprite;
        public Nightmode(Interface inter) : base(inter,(int)Interface.Priority.Nightmode, 10, true)
        {
            Depth = Interface.BaseDepth - 1;
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/"));
            sprite.AddLoop("sun", "sun", 0.1f);
            sprite.AddLoop("moon", "moon", 0.1f);
            Collider = new Hitbox(sprite.Width, sprite.Height);
            sprite.Play(Parent.NightMode ? "moon" : "sun");
        }
        public override void OnClick()
        {
            base.OnClick();
            bool prev = Parent.NightMode;
            Parent.NightMode = !prev;
            sprite.Play(!prev ? "moon" : "sun");
        }
        public override void Begin(Scene scene)
        {
            base.Begin(scene);
            sprite.Play(Parent.NightMode ? "moon" : "sun");
        }
        public override void Update()
        {
            base.Update();
            if (Parent.Power != null)
            {
                Position = Parent.Power.BottomRight + new Vector2(8, -Height);
            }
        }
    }
}