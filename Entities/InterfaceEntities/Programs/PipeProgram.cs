using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [TrackedAs(typeof(WindowContent))]
    [CustomProgram("PipeProgram")]
    public class PipeProgram : WindowContent
    {
        private string path => Flipped ? "01" : "00";
        private string texPath = "objects/PuzzleIslandHelper/interface/pipes/";
        public bool Flipped;
        public static bool PipeCutsceneStarted;
        private bool InRoutine;
        public Button FixButton;


        public PipeProgram(Window window) : base(window)
        {
            Name = "Pipe";
        }
        public override void OnOpened(Window window)
        {

            base.OnOpened(window);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            ProgramComponents.Add(new CustomButton(Window, "Switch", 35f, Vector2.Zero, Switch));
            Circle circle = new Circle(27 / 2f);
            ProgramComponents.Add(FixButton = new Button(Window, circle,  "greenCircle", OnFixClicked, FixPipes()));
            FixButton.Position = new Vector2(Window.WindowWidth / 2, Window.WindowHeight / 2) - new Vector2(FixButton.Width / 2, FixButton.Height / 2);
            FixButton.Visible = false;
            FixButton.Disabled = true;
        }
        public void OnFixClicked()
        {
            SetPipeState(4);
        }
        private IEnumerator FixPipes()
        {
            InRoutine = true;
            Interface.Buffering = true;
            List<string> dialogs = new();
            dialogs.Add("pipeFixer");
            dialogs.AddRange(CodeDialogStorage.PipeLoader);
            MiniLoader = null;
            Add(MiniLoader = new MiniLoader(Window, new Vector2(6, Window.WindowHeight - 8), 100, dialogs.ToArray(), 0.3f,
                Window.CaseWidth * 6f - 6, Window.CaseWidth * 3f));
            while (!MiniLoader.Finished)
            {
                yield return null;
            }

            Remove(MiniLoader);
            MiniLoader = null;
            InRoutine = false;

            Interface.Buffering = false;
        }
        private IEnumerator Routine(int i)
        {
            if (Scene is not Level level)
            {
                yield break;
            }
            InRoutine = true;
            SoundSource sfx = new SoundSource();

            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player is null)
            {
                yield break;
            }
            sfx.Position = player.Center;
            Interface.Talk.Active = false;
            Interface.Talk.Visible = false;
            Interface.Buffering = true;
            if (i == 0)
            {
                PipeScrew screw = (PipeScrew)SceneAs<Level>().Tracker.GetEntities<PipeScrew>().Find(item => (item as PipeScrew).UsedInCutscene);
                if (screw is not null)
                {
                    screw.Launched = false;
                    screw.Position = screw.originalPosition;
                    level.Session.SetFlag(screw.flag, false);
                    PianoModule.Session.PipeScrewRestingPoint = null;
                    PianoModule.Session.PipeScrewRestingFrame = 0;
                    PianoModule.Session.PipeScrewLaunched = false;
                    screw.Screw.Play("idle");
                }
                SetPipeState(1);

                player.StateMachine.State = Player.StDummy;
                sfx.Position = player.Position + Vector2.UnitX * 2;
                yield return PlayAndWait(sfx, "event:/PianoBoy/env/local/pipes/metalCreak1");
                yield return PlayAndWait(sfx, "event:/PianoBoy/env/local/pipes/metalsnap");
                Preserve = true;
                yield return Interface.ShutDown(true);
                player.StateMachine.State = Player.StDummy;
                screw?.Launch();
                SceneAs<Level>().Session.SetFlag("screwLaunch");
                yield return player.DummyWalkTo(player.Position.X + 8, true, 3);
                player.Facing = Facings.Left;
                yield return Textbox.Say("pipeAttemptZero");

                PianoModule.Session.PipeSwitchAttempts = 1;
                player.StateMachine.State = Player.StNormal;
            }
            else if (i == 1)
            {
                SetPipeState(1);
                yield return PlayAndWait(sfx, "event:/PianoBoy/env/local/pipes/metalCreak2");

                PianoModule.Session.PipeSwitchAttempts = 2;
            }
            else if (i == 2)
            {
                PianoModule.Session.HasBrokenPipes = false;
                yield return PlayAndWait(sfx, "event:/PianoBoy/env/local/pipes/metalsnap");
                yield return Interface.ShutDown(true);
                player.StateMachine.State = Player.StDummy;
                yield return null;
                player.Facing = Facings.Left;
                yield return PipeBreak();
                yield return Textbox.Say("pipeAttemptOhNo");

                player.StateMachine.State = Player.StNormal;
                SetPipeState(2);
            }


            yield return 0.3f;
            Interface.Talk.Visible = true;
            Interface.Talk.Active = true;
            Interface.Buffering = false;
            InRoutine = false;
            yield return null;
        }
        private IEnumerator PipeBreak()
        {
            PipeCutsceneStarted = true;

            Level level = Scene as Level;
            Vector2 camPosition = level.Camera.Position;
            foreach (PipeSpout spout in level.Tracker.GetEntities<PipeSpout>())
            {
                if (spout.FlagState)
                {
                    spout.GrowBreak();
                }
            }
            float duration = 5;
            for (float i = 0; i < duration; i += Engine.DeltaTime)
            {
                level.Camera.Position = Vector2.Lerp(camPosition, level.LevelOffset, i / duration);
                yield return null;
            }
            yield return 3;
            for (float i = 0; i < 1; i += 0.1f)
            {
                level.Camera.Position = Vector2.Lerp(level.LevelOffset, camPosition, i);
                yield return null;
            }
            yield return null;
        }
        private IEnumerator SwitchPipes()
        {
            InRoutine = true;
            Interface.Buffering = true;
            FixButton.Disabled = true;

            MiniLoader = null;
            Add(MiniLoader = new MiniLoader(Window, new Vector2(6, Window.WindowHeight - 8), 100, CodeDialogStorage.PipeLoader, 0.3f,
                Window.CaseWidth * 6f - 6, Window.CaseWidth * 3f));
            while (!MiniLoader.Finished)
            {
                yield return null;
            }
            Remove(MiniLoader);
            MiniLoader = null;
            Flipped = !Flipped;
            SceneAs<Level>().Session.SetFlag("pipesSwitched", Flipped);
            InRoutine = false;
            Interface.Buffering = false;
        }
        public void Switch()
        {
            if (GetPipeState() == 1)
            {
                if (!InRoutine)
                {
                    Add(new Coroutine(Routine(PianoModule.Session.PipeSwitchAttempts)));
                }
                return;
            }
            else if (GetPipeState() == 3 && !InRoutine)
            {
                Add(new Coroutine(SwitchPipes()));
            }
        }
        public int GetPipeState()
        {
            return PianoModule.Session.GetPipeState();
        }
        public void SetPipeState(int state)
        {
            PianoModule.Session.SetPipeState(state);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Flipped = SceneAs<Level>().Session.GetFlag("pipesSwitched");
            PianoModule.Session.CutsceneSpouts.Clear();
        }

        public override void Update()
        {
            if (Window is null)
            {
                base.Update();
                return;
            }
            if (GetPipeState() == 3 && Window.Drawing)
            {
                FixButton.Visible = true;
                FixButton.Disabled = false;
            }
            else
            {
                FixButton.Visible = false;
                FixButton.Disabled = true;
            }
            base.Update();
            if (Scene is not Level level)
            {
                return;
            }
            switch (GetPipeState())
            {
                case 2:
                    Window.DisableButtons();
                    Window.ChangeWindowText("PIPEWARN");
                    break;
                case 3:
                    Window.DisableButtons();
                    Window.ChangeWindowText("PIPEBEFOREFIX");
                    break;
                default:
                    Window.EnableButtons();
                    Window.ChangeWindowText("PIPE");
                    break;
            }
            if (GetPipeState() == 2)
            {
                bool allOn = true;
                for (int i = 0; i < 5; i++)
                {
                    if (!level.Session.GetFlag("valve" + (i + 1)))
                    {
                        allOn = false;
                        break;
                    }
                }
                if (allOn)
                {
                    SetPipeState(3);
                }
            }
        }
        public override void Render()
        {
            if (Window is null || !Window.Drawing)
            {
                base.Render();
                return;
            }
            Vector2 drawPosition = Window.DrawPosition.Floor() + Vector2.UnitX;
            GFX.Game[texPath + "windowContent" + path].Draw(drawPosition);
            int state = GetPipeState();
            if (state is 2 or 3)
            {
                GFX.Game[texPath + "warningBack"].Draw(drawPosition, Vector2.Zero, state is 2 ? Color.Red : Color.Green);
                if (state is 2)
                {
                    GFX.Game[texPath + "warningSign"].Draw(drawPosition);
                }
                for (int i = 0; i < 5; i++)
                {
                    if (Scene is not Level level)
                    {
                        continue;
                    }
                    bool flag = level.Session.GetFlag("valve" + (i + 1));
                    Vector2 offset = new Vector2(63, 80) + i * (Vector2.UnitX * (2 + 14));
                    GFX.Game[texPath + "node" + (flag ? "Full" : "")].Draw(drawPosition + offset);
                }
            }
            base.Render();
        }
    }
}