using Celeste.Mod.PuzzleIslandHelper.Entities.Windows;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{

    [TrackedAs(typeof(Component))]
    public class BetterWindowButton : Image
    {
        public Action OnClicked;
        public IEnumerator Routine;
        public string Text;
        public bool Waiting;
        public const string Path = "objects/PuzzleIslandHelper/interface/icons/";
        public bool InFocus;
        public BetterWindowButton(Action OnClicked = null, IEnumerator Routine = null) : base(GFX.Game[Path + "button00"])
        {
            this.OnClicked = OnClicked;
            this.Routine = Routine;
        }
        public override void Update()
        {
            base.Update();
            Position = ToInt(Position);
            if (Interface.LeftClicked && Texture.ClipRect.Contains((int)Interface.MousePosition.X, (int)Interface.MousePosition.Y) && !Waiting)
            {
                Waiting = true;
                Texture = GFX.Game[Path + "buttonPressed00"];
            }
            else if (Waiting)
            {
                Texture = GFX.Game[Path + "button00"];
                Waiting = false;
                if (OnClicked is not null)
                {
                    OnClicked();
                }
                if (Routine is not null)
                {
                    Entity.Add(new Coroutine(Routine));
                }
            }
        }

        public override void Render()
        {
            base.Render();
            ActiveFont.Font.Draw(40f, Text, RenderPosition, Vector2.Zero, Scale, Color.Black);


        }
        public Vector2 AbsoluteDrawPosition(int i)
        {
            Vector2 vec1 = ToInt((Entity.Scene as Level).Camera.CameraToScreen(BetterWindow.ButtonsUsed[i].Position)) * 6;
            Vector2 adjust = ToInt(new Vector2(Width / 2, 6));
            return vec1 + adjust;
        }
        private Vector2 ToInt(Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }
    }
}