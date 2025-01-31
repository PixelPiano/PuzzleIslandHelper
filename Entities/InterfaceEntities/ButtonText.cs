
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public class ButtonText : Entity
    {
        public string Text;
        public Vector2 Scale;
        public float Size;
        public Vector2 RenderPosition;
        public float TextWidth;
        public Vector2 TextOffset;
        public float Alpha = 1;
        public Button ParentButton;
        public Window Window;
        public Color Color = Color.Black;
        public ButtonText(Button parent, string text, float textSize, Vector2 Scale, Vector2 offset)
        {
            Size = textSize;
            Tag = TagsExt.SubHUD;
            Text = text;
            this.Scale = Scale;
            TextOffset = offset;
            ParentButton = parent;
            Window = parent.Window;
        }
        public override void Render()
        {
            base.Render();
            
            if(Window is null)
            {
                Window = ParentButton.Window;
                if(Window is null) return;
            }
            if (!Window.Drawing || Window.Interface.ForceHide || Alpha <= 0)
            {
                return;
            }

            ActiveFont.Font.Draw(Size, Text, RenderPosition + TextOffset, Vector2.Zero, Scale,
                Color.Lerp(Color, ParentButton.Disabled ? Color.LightGray : Color, 0.5f) * Alpha);

        }
    }
}