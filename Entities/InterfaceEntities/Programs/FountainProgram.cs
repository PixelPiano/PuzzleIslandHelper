
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [TrackedAs(typeof(WindowContent))]
    [CustomProgram("Fountain")]
    public class FountainProgram : WindowContent
    {
        public bool Flipped;
        public static bool PipeCutsceneStarted;
        public Button FixButton;


        public FountainProgram(Window window) : base(window)
        {
            Name = "Fountain";
        }
        public override void OnOpened(Window window)
        {
            base.OnOpened(window);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Circle circle = new Circle(27 / 2f);
            ProgramComponents.Add(FixButton = new Button(Window, circle, "objects/PuzzleIslandHelper/interface/pipes/fixedButton/", OnClicked));
            FixButton.Position = new Vector2(Window.WindowWidth / 2, Window.WindowHeight / 2) - new Vector2(FixButton.Width / 2, FixButton.Height / 2);
            FixButton.Visible = true;
            FixButton.Disabled = false;
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
        public override void WindowRender()
        {
            base.WindowRender();
            Vector2 drawPosition = Window.DrawPosition.Floor() + Vector2.UnitX;
            int count = 0;
            Vector2 pos = drawPosition + Vector2.UnitX * (Window.CaseWidth / 2 - 5 * 8);
            foreach (bool value in PianoModule.Session.MiniGenStates.Values)
            {
                if (count > 5) break;
                Vector2 offset = count * (Vector2.UnitX * 8);
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