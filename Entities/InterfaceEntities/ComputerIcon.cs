using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        public override void Render() //(Only called if base.Visible is true
        {
            base.Render();
            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position, Color.White);
        }
    }
}
