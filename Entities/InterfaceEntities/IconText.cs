using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public class IconText : Entity
    {
        public static float ButtonWidth;
        public static float ButtonHeight;
        public Vector2 DrawPosition;
        public float IconWidth = 0;
        public static readonly float TextScale = 0.8f;
        public static Icon CurrentIcon;
        public Interface Interface;
        public Window Window;
        private string CurrentTabText => CurrentIcon.TabText;
        public IconText(Interface computer)
        {
            Tag = TagsExt.SubHUD;
            Interface = computer;
            Window = Interface.Window;
        }
        public override void Render()
        {
            base.Render();
            if (!Window.Drawing)
            {
                return;
            }
            if (CurrentIcon is not null)
            {
                ActiveFont.Draw(CurrentTabText, TabTextPosition(), Vector2.Zero, Vector2.One * TextScale, Color.White);
            }
        }
        public Vector2 TabTextPosition()
        {
            if (Scene is not Level level)
            {
                return Vector2.Zero;
            }
            return (level.Camera.CameraToScreen(Window.DrawPosition) + new Vector2(1, -Window.tabHeight)).Floor() * 6;
        }
    }
}