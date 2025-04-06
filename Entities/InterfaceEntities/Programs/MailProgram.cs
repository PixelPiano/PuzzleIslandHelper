using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [CustomProgram("mail")]
    [TrackedAs(typeof(WindowContent))]
    public class MailProgram : WindowContent
    {
        public Button LaunchButton;
        public MailProgram(Window window) : base(window)
        {
            Name = "mail";
            DraggingEnabled = false;
            ClosingEnabled = true;
        }

        public override void OnOpened(Window window)
        {
            base.OnOpened(window);
            //Circle circle = new Circle(27 / 2f);
            //ProgramComponents.Add(LaunchButton = new Button(Window, circle, "greenCircle", OnClicked));
            //LaunchButton.Position = new Vector2(Window.WindowWidth, Window.WindowHeight) - new Vector2(LaunchButton.Width + 4, LaunchButton.Height + 4);
            //LaunchButton.Visible = true;
        }
        private void OnClicked()
        {
            //Scene.Add(new WarpToCalidus()); //todo: replace with ip address puzzle thing
            //LaunchButton.Disabled = true;
        }

    }
}