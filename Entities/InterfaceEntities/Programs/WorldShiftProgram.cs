using Celeste.Mod.PuzzleIslandHelper.Beta;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [TrackedAs(typeof(WindowContent))]
    [CustomProgram("WorldShift")]
    public class WorldShiftProgram : WindowContent
    {
        public Button OnButton;
        public Button ScanButton;
        public Button LaunchButton;
        public bool disabled;
        public bool Activated;
        public bool ScanCompleted;
        public bool CanLaunch;

        public enum Progress
        {
            Off,
            Activated,
            Scanned,
            Launched
        }
        public Progress ProgramProgress;
        public WorldShiftProgram(Window window) : base(window)
        {
            Name = "WorldShift";
        }

        public override void OnOpened(Window window)
        {
            base.OnOpened(window);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            ProgramComponents.Add(OnButton = new Button(Window, null, Activate) { Text = "Start"});
            ProgramComponents.Add(ScanButton = new Button(Window, null, Scan) {Text = "Scan?"});
            ProgramComponents.Add(LaunchButton = new Button(Window, null, Launch) { Text = "Launch"});

           // SetProgress(0);
        }
        public void Activate()
        {
            if (disabled) return;
            disabled = true;

            if (Interface.Machine is BetaWorldShiftMachine machine)
            {
                machine.Activate();
                Activated = true;
            }
        }
        public void Scan()
        {
            //SetProgress(Progress.);
        }
        public void Launch()
        {
            //SetProgress(3);
        }
        public void SetProgress(Progress progress)
        {
            if(OnButton == null || ScanButton == null || LaunchButton == null) return;
            switch (progress)
            {
                //hologram is invisible
                case Progress.Off:
                    OnButton.Show();
                    ScanButton.Hide();
                    LaunchButton.Hide();
                    break;
                //hologram is visible, has not scanned
                case Progress.Activated:
                    OnButton.Hide();
                    LaunchButton.Hide();
                    ScanButton.Show();
                    break;
                //has scanned, ready to launch
                case Progress.Scanned:
                    OnButton.Hide();
                    ScanButton.Hide();
                    LaunchButton.Show();
                    break;
                //has launched
                default:
                    LaunchButton.Hide();
                    OnButton.Hide();
                    ScanButton.Hide();
                    break;
            }
        }
        public override void Update()
        {
            base.Update();
        }
        public override void Render()
        {
            base.Render();
        }
    }
}