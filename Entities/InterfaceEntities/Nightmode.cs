using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [TrackedAs(typeof(DesktopClickable))]
    public class Nightmode : DesktopClickable
    {
        public Sprite sprite;
        public Nightmode(Interface inter) : base(inter,(int)Interface.Priority.Nightmode, 10, true)
        {
            Depth = Interface.BaseDepth - 1;
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/"));
            sprite.AddLoop("sun", "sun", 0.1f);
            sprite.AddLoop("moon", "moon", 0.1f);
            Collider = new Hitbox(sprite.Width, sprite.Height);
            sprite.Play(Interface.NightMode ? "moon" : "sun");
        }
        public override void OnClick()
        {
            base.OnClick();
            bool prev = Interface.NightMode;
            Interface.NightMode = !prev;
            sprite.Play(!prev ? "moon" : "sun");
        }
        public override void Begin(Scene scene)
        {
            base.Begin(scene);
            sprite.Play(Interface.NightMode ? "moon" : "sun");
        }
        public override void Update()
        {
            base.Update();
            if (Interface.Power != null)
            {
                Position = Interface.Power.BottomRight + new Vector2(8, -Height);
            }
        }
    }
}