using Microsoft.Xna.Framework;
using Monocle;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [TrackedAs(typeof(DesktopClickable))]
    public class ComputerIcon : DesktopClickable
    {
        public string Name;
        public string TextID; //ID for text stored in Dialog.txt
        public string TabText; //Standard string to be drawn in the Window tab if this icon is open
        public float Alpha = 1;
        public MTexture Texture; //The icon's desktop image
        public bool JustClicked;
        private float clickedTimer;
        private Vector2? clickedPosition;
        public bool Dragging;
        public ComputerIcon(Interface inter, string name, string textID, string tabText = "") : base(inter)
        {
            Name = name;
            TextID = textID;
            TabText = tabText;
            Texture = GFX.Game["objects/PuzzleIslandHelper/interface/icons/" + Name.ToLower()];
            Collider = new Hitbox(Texture.Width, Texture.Height);
            Visible = true;
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
            if (Dragging && !(Interface.Window is not null && Interface.Window.Drawing))
            {
                Center = Interface.Collider.AbsolutePosition;
            }
            else
            {
                Entity m = Interface.monitor;
                if (X < m.X) X = m.X;
                if (Y < m.Y) Y = m.Y;
                if (X + Width > m.X + m.Width) X = m.X + m.Width - Width;
                if (Y + Height > m.Y + m.Height) Y = m.Y + m.Height - Height;
            }


            Position = Position.Floor();
        }
        public override void OnClick()
        {
            base.OnClick();
            if (JustClicked)
            {
                Interface.OpenIcon(this);
            }
            JustClicked = true;
            clickedTimer = 0;
            clickedPosition = Interface.MousePosition;
        }
        public override void Render() //(Only called if base.Visible is true
        {
            if(Interface.ForceHide) return;
            base.Render();

            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position, Color.White * Alpha);


        }
    }
}
