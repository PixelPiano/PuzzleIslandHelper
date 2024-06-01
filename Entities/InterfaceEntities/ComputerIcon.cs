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
        public float Alpha = 1;
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
            if (clickedPosition.HasValue && clickedTimer > 0.1f)
            {
                Dragging = true;
            }
            if (JustClicked)
            {
                clickedTimer += Engine.DeltaTime;
                if (clickedTimer > Engine.DeltaTime * 10f)
                {
                    JustClicked = false;
                }
            }
            else
            {
                clickedPosition = null;
            }
            if (!Interface.LeftPressed) Dragging = false;
            if (Dragging)
            {
                Center = Interface.Collider.AbsolutePosition;
            }
            else
            {
                Entity m = Interface.Monitor;
                if (X < m.X) X = m.X;
                if (Y < m.Y) Y = m.Y;
                if (X + Width > m.X + m.Width) X = m.X + m.Width - Width;
                if (Y + Height > m.Y + m.Height) Y = m.Y + m.Height - Height;
            }
            Position = Position.Floor();
        }
        public void OnClick()
        {
            JustClicked = true;
            clickedTimer = 0;
            clickedPosition = Interface.MousePosition;
        }
        public override void Render() //(Only called if base.Visible is true
        {
            base.Render();
            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position, Color.White * Alpha);
        }
    }
}
