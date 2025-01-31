using Microsoft.Xna.Framework;
using Monocle;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [TrackedAs(typeof(DesktopEntity))]
    public class Icon : DesktopEntity
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
        public Icon(Interface inter, string name, string textID, string tabText = "") : base(inter, false, 2)
        {
            Name = name;
            TextID = textID;
            TabText = tabText;
            Texture = GFX.Game["objects/PuzzleIslandHelper/interface/icons/" + Name.ToLower()];
            Collider = new Hitbox(Texture.Width, Texture.Height);
            Visible = true;
        }
        public Icon(Interface inter, InterfaceData.Preset.IconData data) : base(inter, false, 2)
        {
            Name = data.ID;
            TextID = data.Window;
            TabText = data.Tab;
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
            if (!Parent.LeftPressed) Dragging = false;
            if (Dragging && !(Parent.Window is not null && Parent.Window.Drawing))
            {
                Center = Parent.Collider.AbsolutePosition;
            }
            else
            {
                Entity m = Parent.Monitor;
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
                Parent.OpenIcon(this);
            }
            JustClicked = true;
            clickedTimer = 0;
            clickedPosition = Parent.MousePosition;
        }
        public void InterfaceRender()
        {
            if (Parent.ForceHide || !Visible) return;
            base.Render();
            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position, Color.White * Alpha);
        }
    }
}
