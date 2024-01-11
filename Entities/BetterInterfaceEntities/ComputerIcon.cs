using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{

    public class ComputerIcon : Entity
    {
        public Sprite Sprite;
        public string Name = "";
        public string Text = "";
        public string TabText = "";
        public Interface Interface;
        public static readonly List<string> dictionary = new List<string>
            {
                "unknown",
                "text",
                "chatlog",
                "folder",
                "access",
                "ram",
                "pico",
                "sus",
                "info",
                "destruct",
                "Freq",
                "pipe",
                "life"
            };
        public static readonly List<string> TextDictionary = new List<string>
            {
                "text",
                "info",
                "invalid",
                "unknown",
            };
        public static readonly List<string> StaticText = new List<string>
            {
                "invalid",
                "unknown",
                "ram",
                "access",
                "destruct",
                "pipe",
                "life"
            };
        public bool Open = false;
        private Color color = Color.White;
        public ComputerIcon(Interface inter, string name, string textID, [Optional] string lowerText)
        {
            Interface = inter;
            Depth = Interface.BaseDepth - 1;
            Name = dictionary.Contains(name) ? name : "invalid";
            Text = SetID(textID);
            if (!string.IsNullOrEmpty(lowerText))
            {
                TabText = lowerText;
            }
            Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/icons/"));
            Sprite.AddLoop("idle", Name, 0.1f);
            Sprite.SetColor(color);
            Collider = new Hitbox(Sprite.Width, Sprite.Height);
        }
        public ComputerIcon(string textId)
        {

        }
        public bool IsDynamicText()
        {
            return !StaticText.Contains(Name) && TextDictionary.Contains(Name);
        }
        public bool IsStaticText()
        {
            return StaticText.Contains(Name);
        }
        public bool IsText()
        {
            return IsDynamicText() || IsStaticText();
        }
        public string GetID()
        {
            string result = "";
            if (IsStaticText())
            {
                result = Name;
            }
            else if (IsDynamicText() && !string.IsNullOrEmpty(Text))
            {
                result = Text;
            }
            return result;
        }
        public string SetID(string ID)
        {
            //string result = "";
            /*            if (IsStaticText())
                        {
                            result = Name;
                        }
                        else if (IsDynamicText() && !string.IsNullOrEmpty(ID))
                        {
                            result = ID;
                        }*/
            return ID;
        }
    }
}