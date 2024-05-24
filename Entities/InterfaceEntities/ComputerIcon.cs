using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{

    public class ComputerIcon : Entity
    {
        public string Name;
        public string TextID; //ID for text stored in Dialog.txt
        public string TabText; //Standard string to be drawn in the Window tab if this icon is open
        public Interface Interface; //Reference to the Interface this icon is in

        public MTexture Texture; //The icon's desktop texture
        public bool JustClicked;
        private float clickedTimer;
        private Vector2? clickedPosition;
        public bool Dragging;
        public ComputerIcon(Interface inter, string name, string textID, string tabText = "")
        {
            Interface = inter;
            Name = name;
            TextID = textID;
            TabText = tabText;
            Texture = GFX.Game["objects/PuzzleIslandHelper/interface/icons/" + Name.ToLower()];
            Depth = Interface.BaseDepth - 1;
            Collider = new Hitbox(Texture.Width, Texture.Height);
            Visible = false; //native to Entity.cs. An Entity only renders if Visible is true
        }
        public override void Update()
        {
            base.Update();
            if (clickedPosition.HasValue && Vector2.Distance(clickedPosition.Value, Interface.MousePosition) >= 1)
            {
                Dragging = true;
            }
            if (JustClicked)
            {
                clickedTimer += Engine.DeltaTime;
                if (clickedTimer > Engine.DeltaTime * 5f)
                {
                    clickedTimer = 0;
                    JustClicked = false;
                }
            }
            else
            {
                clickedPosition = null;
                if (!Interface.LeftPressed) Dragging = false;
            }
            if (Dragging)
            {
                Center = Interface.MouseWorldPosition.Floor();
            }
            Left = Calc.Max(Interface.IconBounds.Left, Left);
            Right = Calc.Min(Interface.IconBounds.Right, Right);
            Top = Calc.Max(Interface.IconBounds.Top, Top);
            Bottom = Calc.Min(Interface.IconBounds.Bottom, Bottom);

        }
        public void OnClick()
        {
            JustClicked = true;
            clickedTimer = 0;
            clickedPosition = Interface.MousePosition;
        }
        public bool DoubleClicked()
        {
            if (!JustClicked)
            {
                OnClick();
                return false;
            }
            return true;
        }
        public override void Render() //(Only called if base.Visible is true
        {
            base.Render();
            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position, Color.White);
        }
    }
}
