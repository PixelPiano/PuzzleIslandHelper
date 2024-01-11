using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{

    [TrackedAs(typeof(BetterWindowButton))]
    public class BetterWindowButton : BetterButton
    {
        private Interface entityInterface;
        public BetterWindowButton(Interface entityInterface, string path = null, Action OnClicked = null, IEnumerator Routine = null)
            : base(entityInterface, path, OnClicked, Routine)
        {
            this.entityInterface = entityInterface;
            this.OnClicked = OnClicked;
            this.Routine = Routine;
            ButtonCollider = new Hitbox(Texture.Width, Texture.Height);
        }
        public override void Update()
        {
            base.Update();
            Color = Color.Lerp(Color.White, Disabled ? Color.LightGray : Color.White, 0.5f);
            ButtonCollider.Position = RenderPosition;
            if (Scene is Level level && TextRenderer is not null)
            {
                TextRenderer.RenderPosition = (level.Camera.CameraToScreen(RenderPosition + new Vector2(2, 1))).ToInt() * 6;
            }
            if (Disabled)
            {
                Pressing = false;
                Texture = GFX.Game[Path + "button"];
                return;
            }
            bool collidingWithMouse = ButtonCollider.Collide(entityInterface) && !Interface.Buffering;
            if (collidingWithMouse && Interface.LeftClicked)
            {
                Pressing = true;
                Texture = GFX.Game[Path + "buttonPressed"];
            }
            else
            {
                if (Pressing && collidingWithMouse)
                {
                    RunActions();
                }
                Pressing = false;
                Texture = GFX.Game[Path + "button"];
            }
        }
    }
}