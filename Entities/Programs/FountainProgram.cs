using Celeste.Mod.PandorasBox;
using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using VivHelper;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    [TrackedAs(typeof(WindowContent))]
    public class FountainProgram : WindowContent
    {
        public bool Flipped;
        public static bool PipeCutsceneStarted;
        public BetterButton FixButton;


        public FountainProgram(BetterWindow window) : base(window)
        {
            Name = "Fountain";
        }


        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Circle circle = new Circle(27 / 2f);
            Add(FixButton = new BetterButton(Interface, circle, "objects/PuzzleIslandHelper/interface/pipes/fixedButton/", OnClicked));
            FixButton.Position = new Vector2(Window.WindowWidth / 2, Window.WindowHeight / 2) - new Vector2(FixButton.Width / 2, FixButton.Height / 2);
            FixButton.Visible = true;
            FixButton.Disabled = true;
        }
        private void OnClicked()
        {
        }
        public override void Update()
        {
            if (Window is null)
            {
                base.Update();
                return;
            }
        }
        public override void Render()
        {
            base.Render();
            if (IsActive)
            {
                Vector2 drawPosition = Window.DrawPosition.ToInt() + Vector2.UnitX;
                int count = 0;
                Vector2 pos = drawPosition + Vector2.UnitX * (Window.CaseWidth / 2 - (5 * 8));
                foreach (bool value in PianoModule.Session.MiniGenStates.Values)
                {
                    if (count > 5) break;
                    Vector2 offset = (count * (Vector2.UnitX * (8)));
                    GFX.Game["objects/PuzzleIslandHelper/interface/fountain/miniGen" + (value ? "On" : "Off")].Draw(pos + offset);
                    count++;
                }
                for (int i = count; i < 6; i++)
                {
                    Vector2 offset = i * Vector2.UnitX * 8;
                    GFX.Game["objects/PuzzleIslandHelper/interface/fountain/miniGenOff"].Draw(pos + offset);
                }
            }
        }
    }
}