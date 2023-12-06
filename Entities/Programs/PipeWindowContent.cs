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

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{



    public class PipeWindowContent : WindowContent
    {
        private string path => Flipped ? "01" : "00";
        public bool Flipped;
        public static bool PipeCutsceneStarted;
        private bool InRoutine;
        public BetterButton FixButton;

        public PipeWindowContent(BetterWindow window) : base(window)
        {
            Name = "pipe";
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
            for (int i = 1; i < 7; i++)
            {
                dialogs.Add("pipeLoader" + i);
            }
            MiniLoader = null;
            Add(MiniLoader = new MiniLoader(new Vector2(6, BetterWindow.WindowHeight - 8), 100, dialogs.ToArray(), 0.3f,
                BetterWindow.CaseWidth * 6f - 6, BetterWindow.CaseWidth * 3f));
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
                    PianoModule.SaveData.PipeScrewRestingPoint = null;
                    PianoModule.SaveData.PipeScrewRestingFrame = 0;
                    PianoModule.SaveData.PipeScrewLaunched = false;
                    screw.Screw.Play("idle");
                }
                SetPipeState(1);

                player.StateMachine.State = Player.StDummy;
                sfx.Position = player.Position + Vector2.UnitX * 2;
                yield return PlayAndWait(sfx, "event:/PianoBoy/env/local/pipes/metalCreak1");
                yield return PlayAndWait(sfx, "event:/PianoBoy/env/local/pipes/metalsnap");
                yield return Interface.InstantShutdown();
                player.StateMachine.State = Player.StDummy;
                SceneAs<Level>().Session.SetFlag("screwLaunch");
                yield return player.DummyWalkTo(player.Position.X + 8, true, 3);
                player.Facing = Facings.Left;
                yield return Textbox.Say("pipeAttemptZero");

                PianoModule.SaveData.PipeSwitchAttempts++;
                player.StateMachine.State = Player.StNormal;
            }
            else if (i == 1)
            {
                SetPipeState(1);
                yield return PlayAndWait(sfx, "event:/PianoBoy/env/local/pipes/metalCreak2");

                PianoModule.SaveData.PipeSwitchAttempts++;
            }
            else if (i == 2)
            {
                PianoModule.SaveData.HasBrokenPipes = false;
                yield return PlayAndWait(sfx, "event:/PianoBoy/env/local/pipes/metalsnap");
                yield return Interface.CloseInterface(true);
                player.StateMachine.State = Player.StDummy;
                yield return null;
                player.Facing = Facings.Left;
                yield return PipeBreak(player);
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
        private IEnumerator PlayAndWait(SoundSource source, string audio)
        {
            source.Play(audio);
            while (source.InstancePlaying)
            {
                yield return null;
            }
        }
        private IEnumerator PipeBreak(Player player)
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
            List<string> dialogs = new();

            for (int i = 1; i < 7; i++)
            {
                dialogs.Add("pipeLoader" + i);
            }
            MiniLoader = null;
            Add(MiniLoader = new MiniLoader(new Vector2(6, BetterWindow.WindowHeight - 8), 100, dialogs.ToArray(), 0.3f,
                BetterWindow.CaseWidth * 6f - 6, BetterWindow.CaseWidth * 3f));
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
                    Add(new Coroutine(Routine(PianoModule.SaveData.PipeSwitchAttempts)));
                }
                return;
            }
            else if (GetPipeState() == 3)
            {
                if (!InRoutine)
                {
                    Add(new Coroutine(SwitchPipes()));
                }
            }
        }
        public int GetPipeState()
        {
            return PianoModule.SaveData.GetPipeState();
        }
        public void SetPipeState(int state)
        {
            PianoModule.SaveData.SetPipeState(state);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Flipped =  SceneAs<Level>().Session.GetFlag("pipesSwitched");
            PianoModule.Session.CutsceneSpouts.Clear();
            Circle circle = new Circle(27 / 2f);
            Add(FixButton = new BetterButton(circle, "objects/PuzzleIslandHelper/interface/pipes/fixedButton/", OnFixClicked, FixPipes()));
            FixButton.Position = new Vector2(BetterWindow.WindowWidth / 2, BetterWindow.WindowHeight / 2) - new Vector2(FixButton.Width / 2, FixButton.Height / 2);
            FixButton.Visible = false;
            FixButton.Disabled = true;
        }
        public override void Update()
        {
            if (GetPipeState() == 3 && BetterWindow.Drawing)
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
            if (!BetterWindow.Drawing)
            {
                base.Render();
                return;
            }
            Vector2 drawPosition = BetterWindow.DrawPosition.ToInt() + Vector2.UnitX;
            GFX.Game["objects/PuzzleIslandHelper/interface/pipes/windowContent" + path].Draw(drawPosition);
            int state = GetPipeState();
            if (state is 2 or 3)
            {
                GFX.Game["objects/PuzzleIslandHelper/interface/pipes/warningBack"].Draw(drawPosition, Vector2.Zero, state is 2 ? Color.Red : Color.Green);
                if (state is 2)
                {
                    GFX.Game["objects/PuzzleIslandHelper/interface/pipes/warningSign"].Draw(drawPosition);
                }
                for (int i = 0; i < 5; i++)
                {
                    if (Scene is not Level level)
                    {
                        continue;
                    }
                    bool flag = level.Session.GetFlag("valve" + (i + 1));
                    Vector2 offset = new Vector2(63, 80) + (i * (Vector2.UnitX * (2 + 14)));
                    GFX.Game["objects/PuzzleIslandHelper/interface/pipes/node" + (flag ? "Full" : "")].Draw(drawPosition + offset);
                }
            }
            base.Render();
        }
    }
}