using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [CustomProgram("mail")]
    [TrackedAs(typeof(WindowContent))]
    public class MailProgram : WindowContent
    {
        public BetterButton LaunchButton;
        public MailProgram(BetterWindow window) : base(window)
        {
            Name = "mail";
            DraggingEnabled = false;
            ClosingEnabled = false;
        }

        public override void OnOpened(BetterWindow window)
        {
            base.OnOpened(window);
            Circle circle = new Circle(27 / 2f);
            ProgramComponents.Add(LaunchButton = new BetterButton(Window, circle, "greenCircle", OnClicked));
            LaunchButton.Position = new Vector2(Window.WindowWidth, Window.WindowHeight) - new Vector2(LaunchButton.Width + 4, LaunchButton.Height + 4);
            LaunchButton.Visible = true;
        }
        private void OnClicked()
        {
            Scene.Add(new WarpToCalidus(1));
            LaunchButton.Disabled = true;
        }

    }
}