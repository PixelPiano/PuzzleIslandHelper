using Celeste.Mod.LuaCutscenes;
using static Celeste.Mod.PuzzleIslandHelper.PuzzleData.GameshowData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using System.Linq;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using System.Reflection;
using Celeste.Mod.PuzzleIslandHelper.Components;
using ExtendedVariants.Module;
using VivHelper.Triggers;
using ExtendedVariants.Variants;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class Gameshow : CutsceneEntity
    {
        public int WrongAnswers;
        public bool Lost;
        public RandomRoutines Random;
        public bool LoopQuestion;
        public bool PassedIntro;
        public bool StartedMusic;
        public SoundSource SnareLoop;
        private const string Song = "event:/PianoBoy/ThatsMyMerleeIntro";
        private const string Drum = "event:/PianoBoy/drumroll";
        private const string path = "objects/PuzzleIslandHelper/gameshow/";
        public const float HiddenLighting = 0.7f;
        public const float RevealLighting = 0.2f;

        private Screen screen;
        private Background background;
        private Foreground foreground;
        private BgCurtains curtains;
        private LightPath[] Paths;
        private BOOM Boom;
        private Host Host;
        private float HostZoomAmount;
        private bool zoomInHost;
        private GameshowSpotlight playerLight, hostLight;
        private Coroutine cutscene;
        public static List<string> RoomOrder = new();
        public int CorrectAnswers;
        public static int RoomsVisited => RoomOrder.Count;
        private int part;

        public Gameshow(int part) : base()
        {
            this.part = part;
            Depth = -10000;
            RemoveOnSkipped = false;
            Random = new();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            SetLightColor(Color.White);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (part == 0)
            {
                SetLightColor(Color.White);
                Host = Level.Tracker.GetEntity<Host>();
                playerLight = GameshowSpotlight.GetSpotlight(scene, "player");
                hostLight = GameshowSpotlight.GetSpotlight(scene, "host");
                Level.Session.SetFlag("gameshowReveal", RoomsVisited != 0);
                Vector2 topLeft = Level.LevelOffset;
                Vector2 center = topLeft + new Vector2(160, 90);
                screen = new Screen(topLeft);
                background = new Background(topLeft);
                Level.Add(background, screen);
                Level.Camera.Position = Level.LevelOffset;
                if (!string.IsNullOrEmpty(GameshowPassage.LastRoomVisited) && !RoomOrder.Contains(GameshowPassage.LastRoomVisited))
                {
                    RoomOrder.Add(GameshowPassage.LastRoomVisited);
                }
                foreach (Passage passage in Level.Tracker.GetEntities<Passage>())
                {
                    if (RoomOrder.Contains(passage.TeleportTo))
                    {
                        passage.Deactivate();
                    }
                }
                Paths = new LightPath[2];
                Paths[0] = new LightPath(center - Vector2.UnitX * 20, 40, 0);
                Paths[1] = new LightPath(center + Vector2.UnitX * 20, 40, 180);
                Level.Add(Paths);
                hostLight.StartFollowing(Paths[0]);
                playerLight.StartFollowing(Paths[1]);
                SnareLoop = new SoundSource();
                Add(SnareLoop);
                if (RoomsVisited == 0)
                {
                    Boom = new BOOM(topLeft);
                    foreground = new Foreground(topLeft);
                    curtains = new BgCurtains(topLeft, 11);
                    Level.Add(foreground, curtains, Boom);
                }
            }
        }
        public override void Render()
        {
            base.Render();
            if (Host != null)
            {
                Host.DrawBeam();
            }
        }
        public override void OnBegin(Level level)
        {
            Level.Session.SetFlag("GameshowWinTeleport", false);
            PianoModule.Session.AltCalidusSceneState = AltCalidus.AltCalidusScene.States.BeforeGameshow;
            switch (part)
            {
                case 1:
                    Glitch.Value = 0.7f;
                    Add(cutscene = new Coroutine(LoseCutscene()));
                    break;
                case 2:
                    Add(cutscene = new Coroutine(HallCutscene()));
                    break;
                case 3:
                    Glitch.Value = 0;
                    Add(cutscene = new Coroutine(RewindCutscene()));
                    break;
                case 0:
                    Add(cutscene = new Coroutine(MainCutscene()));
                    break;
            }
        }


        public override void OnEnd(Level level)
        {
            if (level.GetPlayer() is not Player player) return;
            if (part == 0)
            {
                if (WasSkipped)
                {
                    Reveal(true);
                    if (Host is not null)
                    {
                        Host.Visible = false;
                    }
                    AllFocusPaths();
                    cutscene.Cancel();
                    SnareLoop.Stop(false);
                    FadeInMusic();
                }
            }
            Brighten();
            ReleasePlayer(player);
        }
        public override void Update()
        {
            base.Update();
            if (zoomInHost)
            {
                Vector2 target = Host.Center - Level.LevelOffset;
                Vector2 adjust = (new Vector2(160, 90) - target) / HostZoomAmount;
                Level.ZoomSnap(target + adjust, HostZoomAmount);
            }
        }

        #region Routines
        private IEnumerator HallCutscene()
        {
            Player player = Level.GetPlayer();
            player.Facing = Facings.Right;
            player.Light.Alpha = 0;
            yield return 0.2f;
            Level.Add(new MiniTextbox("inTheDarkHall"));
            yield return null;
        }
        private float anxiety;
        private IEnumerator AnxietyWobble()
        {
            float time, anx;
            while (true)
            {
                time = Calc.Random.Range(1, 4f);
                anx = anxiety;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    Distort.Anxiety = (anx = Calc.Approach(anx, anxiety, Engine.DeltaTime * 3)) + Ease.SineIn(i) * 0.2f;
                    yield return null;
                }

                time = Calc.Random.Range(1, 4f);
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    Distort.Anxiety = (anx = Calc.Approach(anx, anxiety, Engine.DeltaTime * 3)) + 1 - Ease.SineOut(i) * 0.2f;
                    yield return null;
                }

                yield return null;
            }
        }
        private IEnumerator RewindCutscene()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            player.Light.Alpha = 1;
            player.Light.Visible = true;
            Level.Session.SetFlag("RewindTeleport", false);
            Level.Session.SetFlag("faceEncounterFlag", false);
            MakeUnskippable();
            player.StateMachine.State = Player.StDummy;
            anxiety = 0;

            Add(new Coroutine(Level.ZoomTo(new Vector2(220, 120), 1.6f, 0.7f)));
            yield return player.DummyWalkTo(player.X + 64);
            yield return 0.1f;
            yield return Textbox.Say("preRewind");
            Coroutine anx = new Coroutine(AnxietyWobble());
            Add(anx);
            Level.Flash(Color.White, false);

            Audio.Play("event:/PianoBoy/invertGlitch");
            for (float i = 0; i < 4; i++)
            {
                Glitch.Value = Calc.Random.Range(0.6f, 1);
                Celeste.Freeze(Engine.DeltaTime * (i + 2));
                yield return Engine.DeltaTime * 2;
            }
            Glitch.Value = 0;

            anxiety = 1;
            yield return 0.2f;
            yield return Textbox.Say("rewindB");
            yield return 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)
            {
                anxiety = Calc.LerpClamp(1, 0.5f, Ease.SineInOut(i));
                yield return null;
            }
            anxiety = 0.5f;
            yield return Textbox.Say("rewindBreath");
            yield return 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                anxiety = Calc.LerpClamp(0.5f, 0f, Ease.SineInOut(i));
                yield return null;
            }
            anxiety = 0;
            anx.Cancel();
            anx.RemoveSelf();
            while (Distort.Anxiety != 0)
            {
                Distort.Anxiety = Calc.Approach(Distort.Anxiety, 0, Engine.DeltaTime);
                yield return null;
            }
            yield return 0.6f;
            yield return Textbox.Say("rewindC");
            yield return Level.ZoomBack(0.7f);
            player.StateMachine.State = Player.StNormal;
            EndCutscene(Level);
            yield return null;
        }
        private IEnumerator LoseCutscene()
        {
            MakeUnskippable();
            Player player = Level.GetPlayer();
            player.StateMachine.State = Player.StDummy;
            player.Facing = Facings.Left;

            Audio.SetMusic("event:/PianoBoy/invertAmbience");
            yield return null;
            Level.Session.SetFlag("GameshowLoseTeleport", false);
            yield return Textbox.Say("rewindA");
            yield return 0.5f;
            Level.Session.SetFlag("RewindTeleport");
            yield return null;
        }

        public IEnumerator MainCutscene()
        {
            int timesReturned = 0;
            Player player = Level.GetPlayer();
            player.Light.Visible = false;
            if (Level.Camera.Position != Level.LevelOffset)
            {
                Level.Camera.Position = Level.LevelOffset;
            }
            player.Facing = Facings.Left;
            player.StateMachine.State = Player.StDummy;
            if (RoomsVisited == 0)
            {
                Hide();
                playerLight.On = hostLight.On = false;
                Audio.SetMusic("null");
                yield return 2f;
                yield return Intro();
                PassedIntro = true;
            }
            else
            {
                player.Light.Visible = true;
                player.Facing = Facings.Left;
                player.StateMachine.State = Player.StDummy;
                MakeUnskippable();
                Reveal(true);
                yield return 0.8f;
                StartCheering();
                List<Entity> transitions = Level.Tracker.GetEntities<PassageTransition>();
                while (transitions is not null && transitions.Count > 0)
                {
                    yield return null;
                }
                StopCheering();

                int index = RoomsVisited - 1;
                if (!string.IsNullOrEmpty(RoomOrder[index]) && RoomOrder[index].Contains("GameshowRoom"))
                {
                    if (int.TryParse(RoomOrder[index][^1].ToString(), out int roomNum))
                    {
                        timesReturned = roomNum;
                        yield return AskQuestion(roomNum);
                    }
                }
            }
            if (RoomsVisited >= 4)
            {
                yield return BigWin();
            }
            else
            {
                yield return Textbox.Say("rfree" + timesReturned, HostLeave);
            }
            AllFocusPaths();
            EndCutscene(Level);
        }
        public IEnumerator Intro()
        {
            Player player = Level.GetPlayer();
            player.Facing = Facings.Left;
            hostLight.On = playerLight.On = true;
            SnareLoop.Play(Drum);
            yield return 2;
            float[] waitTimes = { 1.5f, 1, 0.7f, 0.5f, 1.5f };
            for (int i = 0; i < 5; i++)
            {
                yield return Textbox.Say("gameshowStart" + (i + 1));
                yield return null;
            }
            yield return 1f;
            Reveal();
            SnareLoop.Stop(false);
            FadeInMusic();
            yield return SayTitle();
            yield return CutAtEnd("gsBanter1");
            yield return Textbox.Say("gsBanter2");
            yield return CutAtEnd("gsBanter3");
            yield return CutAtEnd("gsBanter4");
            Add(new Coroutine(DigiFlashAndShakeScreen()));
            Add(new Coroutine(StopMusic()));
            yield return Textbox.Say("gsBanter5");
            yield return Textbox.Say("gsBanter6", SpotlightToHost, SpotlightToPlayer, SpotlightSlideDown, WaitForOne, WaitForTwo);
            AllFocusHost();
            yield return Textbox.Say("gsBanter7", RAISE, THAT, CURTAIN, ResetZoom, RaiseCurtainNormal, Host.RunOff, Host.Return, RaiseCurtainFast, CurtainBarrage, other, WaitForOne);
            yield return CutAtEnd("gsBanter8");
            yield return Textbox.Say("gsBanter9", Cheer, PanCamera, PanCameraBack, LookAtLives, StopMusic, StartMusic);
        }
        public IEnumerator FlashAnswer(bool correct)
        {
            AllFocusScreen();
            SetLightColor(correct ? Color.Green : Color.Red);
            float waitTime = 0.15f;
            Audio.Play("event:/PianoBoy/" + (correct ? "correct" : "incorrect"));
            for (int i = 0; i < 4; i++)
            {
                screen.SetState(correct);
                SetLightState(true);
                yield return waitTime;
                screen.HideState();
                SetLightState(false);
                yield return waitTime;
            }
            SetLightState(true);
        }
        private IEnumerator HostLeave()
        {
            yield return Host.Skedaddle();
        }
        private IEnumerator RAISE()
        {
            Audio.Play("event:/PianoBoy/crashCymbal");
            zoomInHost = true;
            HostZoomAmount = 1.4f;
            yield return null;
        }
        private IEnumerator THAT()
        {
            Audio.Play("event:/PianoBoy/crashCymbal");
            zoomInHost = true;
            HostZoomAmount = 1.8f;
            yield return null;
        }
        private IEnumerator other()
        {
            zoomInHost = true;
            HostZoomAmount = 1.2f;
            yield return null;
        }
        private IEnumerator CURTAIN()
        {
            Audio.Play("event:/PianoBoy/crashCymbal");
            zoomInHost = true;
            HostZoomAmount = 2.2f;
            yield return null;
        }
        private IEnumerator ResetZoom()
        {
            zoomInHost = false;
            yield return Level.ZoomBack(1);
            HostZoomAmount = 1;
            Level.ResetZoom();
            yield return null;
        }
        public IEnumerator SpotlightToPlayer()
        {
            AllFocusPlayer();
            yield return null;
        }
        public IEnumerator SpotlightToHost()
        {
            AllFocusHost();
            yield return null;
        }
        public IEnumerator SpotlightSlideDown()
        {
            hostLight.Track = playerLight.Track = null;
            Vector2 prev = hostLight.Target;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                hostLight.Target = playerLight.Target = Vector2.Lerp(prev, prev + Vector2.UnitY * 8, Ease.QuintIn(i));
                yield return null;
            }
            AllFocusHost();
        }
        public IEnumerator LightsToScreen()
        {
            playerLight.Track = null;
            hostLight.Track = null;
            Vector2 pPrev = playerLight.Target;
            Vector2 hPrev = hostLight.Target;
            Vector2 target = Level.MarkerCentered("screen");
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                playerLight.Target = Vector2.Lerp(pPrev, target, i);
                hostLight.Target = Vector2.Lerp(hPrev, target, i);
                yield return null;
            }
            playerLight.Target = hostLight.Target = target;
        }

        public IEnumerator PanCamera()
        {
            Vector2 prev = Level.Camera.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Level.Camera.Position = Vector2.Lerp(prev, prev + Vector2.UnitX * 320, Ease.CubeIn(i));
                yield return null;
            }
            Level.Camera.Position = prev + Vector2.UnitX * 320;
        }
        public IEnumerator PanCameraBack()
        {
            Vector2 prev = Level.Camera.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Level.Camera.Position = Vector2.Lerp(prev, prev - Vector2.UnitX * 320, Ease.CubeIn(i));
                yield return null;
            }
            Level.Camera.Position = prev - Vector2.UnitX * 320;
        }
        public IEnumerator DigiFlashAndShakeScreen()
        {
            Level.Shake(0.5f);
            SpotlightsFreakout(Level);
            foreach (Blinker b in Level.Tracker.GetEntities<Blinker>())
            {
                b.Freakout();
            }
            bool state = true;
            for (float i = 0; i < 0.5f; i += Engine.DeltaTime * 2)
            {
                Level.Session.SetFlag("gameshowDigiFlash", state);
                state = !state;
                yield return null;
            }
            Level.Session.SetFlag("gameshowDigiFlash", false);
            yield return null;
        }
        public IEnumerator ShakeScreen()
        {
            Level.Shake(0.3f);
            yield return null;
        }
        public IEnumerator WaitForOne()
        {
            yield return 1f;
        }
        public IEnumerator WaitForTwo()
        {
            yield return 2f;
        }
        public IEnumerator RaiseCurtainNormal()
        {
            yield return curtains.RaiseCurtain(-1);
        }
        public IEnumerator RaiseCurtainFast()
        {
            Audio.Play("event:/PianoBoy/crashCymbal");
            yield return curtains.RaiseCurtain(-5);
        }

        public IEnumerator CurtainBarrage()
        {
            int loops = 0;
            while (!Host.DestroyedCurtains)
            {
                if (loops == 3)
                {
                    Host.Steam();
                }
                if (loops == 6)
                {
                    Host.ThrowFlammable();
                }
                Audio.Play("event:/PianoBoy/crashCymbal");
                Add(new Coroutine(curtains.RaiseCurtain(-8)));
                for (float i = 0; i < 0.5f; i += Engine.DeltaTime)
                {
                    if (Host.DestroyedCurtains) break;
                    yield return null;
                }
                loops++;
            }
            yield return Explode();
            //todo: play explosion sound
            yield return null;
        }
        public IEnumerator CutAtEnd(string dialog)
        {
            bool close = false;
            IEnumerator stop() { close = true; yield break; }
            Textbox textbox = new Textbox(dialog, stop);
            Engine.Scene.Add(textbox);
            while (!close)
            {
                yield return null;
            }
            yield return textbox.EaseClose(true);
            textbox.Close();
        }
        public IEnumerator InstantTextbox(string dialog)
        {
            Textbox textbox = new Textbox(dialog);
            Engine.Scene.Add(textbox);
            textbox.ease = textbox.gradientFade = 1;
            while (textbox.Opened)
            {
                textbox.ease = textbox.gradientFade = 1;
                yield return null;
            }
            yield return textbox.EaseClose(true);
            textbox.Close();
        }
        public IEnumerator SayTitle()
        {
            yield return Textbox.Say("gameshowTitle");
        }
        public IEnumerator WaitALittle()
        {
            yield return 0.1f;
        }
        public IEnumerator Cheer()
        {
            StartCheering();
            yield return 0.5f;
            Level.GetPlayer()?.Jump();
            yield return 3.4f;
            StopCheering();
        }
        public IEnumerator LookAtLives()
        {
            LifeDisplay display = Level.Tracker.GetEntity<LifeDisplay>();
            if (display is null)
            {
                yield break;
            }
            yield return Level.ZoomTo(new Vector2(264, 100), 3, 1);
            for (int i = 0; i < 5; i++)
            {
                display.TurnOn();
                yield return 0.2f;
                display.TurnOff();
                yield return 0.2f;
            }
            display.TurnOn();
            yield return 0.3f;
            yield return Level.ZoomBack(1);
        }
        public IEnumerator AskQuestion(int index)
        {
            Question question = PianoModule.GameshowData.QuestionSets[index - 1].GetRandom();
            if (question is null) yield return Incorrect(null, 0);
            yield return Textbox.Say($"r{index}intro", WaitForOne, SayTitle); //ask prompt

            yield return MultiChoice(question);
            yield return null;
        }
        private IEnumerator Explode()
        {
            EmitBagParticles(Host.LastBagPosition, Host.Bag.Width);
            Level.Shake(0.3f);
            Boom.Visible = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
            {
                Boom.Scale = Vector2.One * i;
                yield return null;
            }
            Add(new Coroutine(curtains.Burn(3)));
            for (int i = 0; i < 3; i++)
            {
                for (float j = 0; j < 1; j += Engine.DeltaTime / 0.2f)
                {
                    Boom.Scale = Vector2.Lerp(Vector2.One, Vector2.One * 0.8f, j);
                    yield return null;
                }
                for (float j = 0; j < 1; j += Engine.DeltaTime / 0.2f)
                {
                    Boom.Scale = Vector2.Lerp(Vector2.One * 0.8f, Vector2.One, j);
                    yield return null;
                }
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
            {
                Boom.Scale = Vector2.Lerp(Vector2.One, Vector2.Zero, i);
                yield return null;
            }
            Boom.Visible = false;
            yield return null;
        }
        public IEnumerator MultiChoice(Question question)
        {
            bool result;
            int page = 0;

            //bool unique;
            int max = question.Choices;
            int pages = (int)Math.Round((double)max / question.PerPage, 0, MidpointRounding.AwayFromZero);
            AllFocusRespective();
            yield return Textbox.Say(question.Q); //ask prompt
            Audio.Play("event:/PianoBoy/questionPresented");

            while (true)
            {
                page %= pages; //cycle back to first page if player continues after last page
                List<string> options = question.GetPage(page); //get page of choices
                if (pages > 1)
                {
                    options.Add(page == pages - 1 ? "qStart" : "qNext");
                }
                yield return ChoicePrompt.Prompt(options.ToArray()); //let player pick a choice
                if (ChoicePrompt.Choice < question.PerPage) //if player chose a valid answer
                {
                    string id = options[ChoicePrompt.Choice];
                    //unique = RandomRoutines.ValidIDs.Contains(id + "U");
                    result = PianoModule.GameshowData.IsAnswer(question, ChoicePrompt.Choice);
                    yield return Textbox.Say(id);
                    break;
                }
                else //if player chose to switch pages
                {
                    page++;
                }
            }
            FadeOutMusic();
            SetSpinTime(0.9f);
            AllFocusPaths();
            yield return Drumroll();
            SetLightState(false);
            SetSpinTime(2);
            yield return 0.9f;
            if (result)
            {
                CorrectAnswers++;
                //StartCheering();
                LaunchConfetti();
                yield return FlashAnswer(true);
                yield return 0.5f;
                //StopCheering();
                yield return Correct(RoomsVisited);
            }
            else
            {
                yield return FlashAnswer(false);
                yield return 0.5f;
                yield return Incorrect();
                /*                if (unique)
                                {
                                    yield return Incorrect(question, page * question.PerPage + ChoicePrompt.Choice);
                                }
                                else
                                {
                                    yield return IncorrectRandom(question);
                                }*/
                WrongAnswers++;
                yield return LoseLife();
            }
            ResetLightColor();
            AllFocusPaths();
            FadeInMusic();
        }
        private void LaunchConfetti()
        {
            Add(new Coroutine(confettiRoutine()));
        }
        private IEnumerator confettiRoutine()
        {
            Level.Session.SetFlag("confetti");
            yield return null;
            Level.Session.SetFlag("confetti", false);
        }
        public IEnumerator Incorrect()
        {
            yield return Textbox.Say("incorrect");
        }
        public IEnumerator Incorrect(Question question, int index)
        {
            if (index > question.Choices)
            {
                yield return IncorrectRandom(question);
                yield break;
            }
            yield return Random.Routine($"q{question.QuestionNumber}{index}U", this);
        }
        public IEnumerator IncorrectRandom(Question question)
        {
            if (question.RandomIncorrect <= 1) yield break;
            int random = Calc.Random.Range(1, question.RandomIncorrect);
            yield return Textbox.Say($"q{question.QuestionNumber}R{random}");
        }
        public IEnumerator Correct(int question)
        {
            yield return Textbox.Say("question" + question + "W");
        }
        public IEnumerator StopMusic()
        {
            FadeOutMusic();
            yield return null;
        }
        public IEnumerator StartMusic()
        {
            FadeInMusic();
            yield return null;
        }
        public void test()
        {
            if (Level.Tracker.GetEntity<LifeDisplay>() is not LifeDisplay display) return;
            display.ConsumeLife();
        }
        public IEnumerator LoseLife()
        {
            yield return null;
            LifeDisplay display = Level.Tracker.GetEntity<LifeDisplay>();
            if (display is null)
            {
                yield break;
            }
            display.ConsumeLife();
            yield return 1.5f;
            if (display.GameOver)
            {
                yield return BigLose();
            }
        }
        public IEnumerator Drumroll()
        {
            SnareLoop.Play(Drum);
            yield return 2;
            SnareLoop.Stop(false);
            yield return 0.2f;
        }
        public IEnumerator BigLose()
        {
            yield return null;
            FadeOutMusic();
            yield return Textbox.Say("bigLose");
            Level.Session.SetFlag("GameshowLoseTeleport");
        }
        public IEnumerator BigWin()
        {
            PianoModule.Session.AltCalidusSceneState = AltCalidus.AltCalidusScene.States.AfterGameshow;
            Add(new Coroutine(Cheer()));
            yield return 0.5f;
            yield return Textbox.Say("bigWin");
            if (Level.GetPlayer() is Player player)
            {
                player.StateMachine.State = Player.StNormal;
            }
            Level.Session.SetFlag("GameshowWinTeleport");
            EndCutscene(Level);
        }
        #endregion
        #region Helper Functions
        public void SetLightColor(Color color)
        {
            GSRenderer.Color = color;
        }
        public void LightApproachColor(Color color)
        {
            Add(new Coroutine(LerpColor(color)));
        }
        private IEnumerator LerpColor(Color color)
        {
            Color orig = GSRenderer.Color;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                GSRenderer.Color = Color.Lerp(orig, color, i);
                yield return null;
            }
            GSRenderer.Color = color;
        }
        public void ResetLightColor()
        {
            LightApproachColor(Color.White);
        }
        public void SetLightState(bool on)
        {
            playerLight.On = hostLight.On = on;
        }
        public void Reveal(bool skipped = false)
        {
            Brighten();
            background.Visible = true;
            screen.Visible = true;
            if (curtains != null)
            {
                curtains.Visible = !skipped;
            }
            if (foreground != null)
            {
                foreground.Visible = false;
            }

            GSRenderer.Invert = false;
            foreach (Blinker blinker in Level.Tracker.GetEntities<Blinker>())
            {
                blinker.StartContinuous(0.1f);
            }
            foreach (AudienceMember face in Level.Tracker.GetEntities<AudienceMember>())
            {
                face.Visible = true;
            }
            foreach (LifeDisplay display in Level.Tracker.GetEntities<LifeDisplay>())
            {
                display.Visible = true;
                if (skipped)
                {
                    display.TurnOn();
                }
            }
            Level.Session.SetFlag("gameshowReveal");
            if (Level.GetPlayer() is Player player)
            {
                player.Visible = true;
                player.Light.Visible = true;
                player.Light.Alpha = 1;
            }
            Host.Reveal();
            if (!skipped)
            {
                AllFocusHost();
                Audio.Play("event:/PianoBoy/crashCymbal");
            }
        }
        public void Hide()
        {
            if (Level.GetPlayer() is not Player player) return;
            player.Visible = false;
            player.StateMachine.State = Player.StDummy;
            player.Light.Visible = false;

            GSRenderer.Invert = true;
            foreach (Blinker blinker in Level.Tracker.GetEntities<Blinker>())
            {
                blinker.TurnOff();
            }
            foreach (AudienceMember face in Level.Tracker.GetEntities<AudienceMember>())
            {
                face.Visible = false;
            }
            foreach (LifeDisplay display in Level.Tracker.GetEntities<LifeDisplay>())
            {
                display.Visible = false;
            }
            Dim();
        }
        public void ReleasePlayer(Player player)
        {
            player.StateMachine.State = Player.StNormal;
            Passage.AllTeleportsActive = true;
        }
        private void Dim()
        {
            Level.Lighting.Alpha = HiddenLighting;
        }
        private void Brighten()
        {
            Level.Lighting.Alpha = RevealLighting;
        }

        public void MakeUnskippable()
        {
            (Scene as Level).InCutscene = false;
            (Scene as Level).CancelCutscene();
        }

        public void LightToPlayer()
        {
            playerLight.TargetEntity(Level.GetPlayer(), 0.2f, Vector2.Zero, true);
        }
        public void LightToHost()
        {
            hostLight.TargetEntity(Host, 0.2f, Vector2.Zero, false);
        }
        public void AllFocusPlayer()
        {
            Player player = Level.GetPlayer();
            playerLight.TargetEntity(player, 0.2f, Vector2.Zero, true);
            hostLight.TargetEntity(player, 0.2f, Vector2.Zero, true);
        }
        public void AllFocusHost()
        {
            playerLight.TargetEntity(Host, 0.2f, Vector2.Zero, false);
            hostLight.TargetEntity(Host, 0.2f, Vector2.Zero, false);
        }
        public void AllFocusRespective()
        {
            Player player = Level.GetPlayer();
            playerLight.TargetEntity(player, 0.2f, Vector2.Zero, true);
            hostLight.TargetEntity(Host, 0.2f, Vector2.Zero, false);
        }
        public void AllFocusScreen()
        {
            Vector2 position = Level.Marker("screen");
            playerLight.Track = hostLight.Track = null;
            playerLight.Target = hostLight.Target = position;
        }
        private void SetSpinTime(float time)
        {
            Paths[0].FullSpinTime = Paths[1].FullSpinTime = time;
        }
        public void AllFocusPaths()
        {
            hostLight.TargetEntity(Paths[0], 0.2f, Vector2.Zero, true);
            playerLight.TargetEntity(Paths[1], 0.2f, Vector2.Zero, true);
        }
        private void SpotlightsFreakout(Level level)
        {
            foreach (GameshowSpotlight light in level.Tracker.GetEntities<GameshowSpotlight>())
            {
                light.Freakout(level.Camera.Position + new Vector2(160, 90), 120, 65, level.GetPlayer());
            }
        }
        public void EmitBagParticles(Vector2 at, float offset)
        {
            for (float i = -135; i < 135; i += Calc.Random.Range(0.3f, 1.3f))
            {
                float angle = i.ToRad();
                Vector2 from = at + Calc.AngleToVector(angle, offset);
                int loops = Calc.Random.Range(1, 3);
                for (int j = 0; j < loops; j++)
                {
                    GravityParticle particle = new GravityParticle(from, Calc.AngleToVector(angle, Calc.Random.Range(200f, 300f)), Color.Cyan, Calc.Random.Choose(0.6f, 1));
                    particle.CanFadeAway = false;
                    Level.Add(particle);
                }
            }
        }
        public void FadeOutMusic()
        {
            Audio.SetMusicParam("fade", 0);
        }
        public void FadeInMusic()
        {
            Audio.SetMusic(Song);
            Audio.SetMusicParam("Jump", 1);
            Audio.SetMusicParam("fade", 1);
        }
        private void StartCheering()
        {
            Audio.Play("event:/PianoBoy/smallCrowdCheer");
            foreach (AudienceMember member in Level.Tracker.GetEntities<AudienceMember>())
            {
                member.Cheer();
            }
        }
        private void StopCheering()
        {
            foreach (AudienceMember member in Level.Tracker.GetEntities<AudienceMember>())
            {
                member.StopCheering();
            }
        }
        public void SetLights(int index)
        {
            List<Entity> Lights = Level.Tracker.GetEntities<StageLight>();
            bool found = false;
            foreach (StageLight light in Lights)
            {
                if (light.Index == index && !light.Activated)
                {
                    light.Activate();
                    found = true;
                }
            }
            if (found) Audio.Play("event:/PianoBoy/spotlight");
        }
        public static void InstantTeleportToSpawn(Scene scene, Player player, string room)
        {
            Level level = scene as Level;
            if (level == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(room))
            {
                return;
            }
            level.OnEndOfFrame += delegate
            {
                Vector2 levelOffset = level.LevelOffset;
                Vector2 val2 = player.Position - level.LevelOffset;
                Vector2 val3 = level.Camera.Position - level.LevelOffset;
                Facings facing = player.Facing;
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Level = room;
                Session session = level.Session;
                Level level2 = level;
                Rectangle bounds = level.Bounds;
                float num = bounds.Left;
                bounds = level.Bounds;
                session.RespawnPoint = level2.GetSpawnPoint(new Vector2(num, bounds.Top));
                level.Session.FirstLevel = false;
                level.LoadLevel(Player.IntroTypes.Transition);
                Vector2 val4 = level.DefaultSpawnPoint - level.LevelOffset - val2;
                level.Camera.Position = level.LevelOffset + val3 + val4;
                level.Add(player);
                player.Position = session.RespawnPoint.HasValue ? session.RespawnPoint.Value : level.DefaultSpawnPoint;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset + val4);
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }
        #endregion
        #region Helper Entities
        public class Screen : Entity
        {
            private Image check;
            private Image x;
            private Vector2 resultOffset = new Vector2(144, 24);
            public Screen(Vector2 position) : base(position)
            {
                Add(new Image(GFX.Game[path + "display"]));
                Add(check = new Image(GFX.Game[path + "check"]));
                Add(x = new Image(GFX.Game[path + "x"]));
                check.Position = x.Position = resultOffset;
                check.Visible = x.Visible = false;
                Visible = false;
                Depth = 10001;
            }
            public IEnumerator FlashResult(bool correct, float waitTime)
            {
                for (int i = 0; i < 4; i++)
                {
                    check.Visible = correct;
                    x.Visible = !correct;
                    yield return waitTime;
                    check.Visible = x.Visible = false;
                    yield return waitTime;
                }
            }
            public void HideState()
            {
                check.Visible = x.Visible = false;
            }
            public void SetState(bool correct)
            {
                check.Visible = correct;
                x.Visible = !correct;
            }

        }
        public class Foreground : Entity
        {
            private Curtain curtain;
            public Foreground(Vector2 position) : base(position)
            {
                Depth = -10000;
                Add(curtain = new Curtain(GFX.Game[path + "redCurtains"]));
            }
            public void RaiseCurtain()
            {
                curtain.Rising = true;
            }
        }
        public class Curtain : Image
        {
            public bool Rising;
            public bool Raised;
            public float Alpha
            {
                set
                {
                    Color = Color.White * value;
                }
            }
            public Curtain(MTexture texture) : base(texture)
            {

            }
            public void Reset()
            {
                Rising = false;
                Position.Y = 0;
                Visible = true;
            }
            public IEnumerator Raise(float speed)
            {
                Rising = true;
                while (Position.Y > -320)
                {
                    Position.Y += speed;
                    yield return null;
                }
                Raised = true;
                Visible = false;
                Rising = false;
            }
        }
        public class BOOM : Entity
        {
            public Image Boom;
            public Vector2 Scale
            {
                get
                {
                    return Boom.Scale;
                }
                set
                {
                    Boom.Scale = value;
                }
            }
            public BOOM(Vector2 position) : base(position)
            {
                Depth = 2;
                Add(Boom = new Image(GFX.Game["objects/PuzzleIslandHelper/gameshow/host/BOOM"]));
                Boom.CenterOrigin();
                Boom.Position += new Vector2(Boom.Width / 2, Boom.Height / 2);
                Boom.Scale = Vector2.Zero;
                Visible = false;
            }
        }
        public class BgCurtains : Entity
        {
            public Curtain Raiser;
            public Curtain Dummy;
            public bool Raising;
            public int Count;
            public bool Leaving;
            public BgCurtains(Vector2 position, int count) : base(position)
            {
                Depth = 3;
                Count = count;
                Dummy = new Curtain(GFX.Game[path + "redCurtains"]);
                Raiser = new Curtain(GFX.Game[path + "redCurtains"]);
                Raiser.Scale = Dummy.Scale = new Vector2(320 / Dummy.Width, 180 / Dummy.Height);
                Add(Dummy, Raiser);
                Visible = false;
            }
            public IEnumerator Burn(float wait)
            {
                Leaving = true;
                Raiser.Texture = GFX.Game[path + "redCurtains(burnt)"];
                Raiser.Alpha = 0;
                Raiser.Reset();
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
                {
                    Raiser.Alpha = i;
                    Dummy.Alpha = 1 - i;
                    yield return null;
                }

                Raiser.Alpha = 1;
                Dummy.Alpha = 0;
                Dummy.Visible = false;
                yield return wait;
                yield return RaiseCurtain(-1);
                Leaving = false;
            }
            public override void Update()
            {
                if (Scene is not Level level) return;
                Position = level.LevelOffset;
                base.Update();
            }
            public override void Render()
            {
                base.Render();
                if (Dummy.Visible)
                {
                    Dummy.Render();
                }
                if (Raiser.Visible)
                {
                    Raiser.Render();
                }
            }

            public IEnumerator RaiseCurtain(float speed)
            {
                Raiser.Reset();
                Raising = true;
                yield return Raiser.Raise(speed);
                Raising = false;
            }

        }
        public class Background : Entity
        {
            private TiledImage Seats;
            private TiledImage BgCurtains;

            public Background(Vector2 position) : base(position)
            {
                Depth = 10002;
                Add(BgCurtains = new TiledImage(GFX.Game[path + "curtainBG"], false, false, false, true, 2));
                Add(Seats = new TiledImage(GFX.Game[path + "seats"], false, false, false, true, 2));
                Visible = false;
            }
            public override void Update()
            {
                if (Scene is not Level level) return;
                Position = level.LevelOffset;
                base.Update();
            }
        }
        public class LightPath : Entity
        {
            public Vector2 origPosition;
            public float FullSpinTime = 2;
            private float startAngle;
            private float radius;
            public LightPath(Vector2 center, float radius, float startAngle) : base(center)
            {
                origPosition = center;
                this.radius = radius;
                this.startAngle = startAngle.ToRad();
                Add(new Coroutine(Spin()));
            }
            private IEnumerator Spin()
            {
                float angle = startAngle;
                while (true)
                {
                    for (float i = 0; i < 1; i += Engine.DeltaTime / FullSpinTime)
                    {
                        Position = origPosition + Calc.AngleToVector(angle, radius);
                        angle = (startAngle + MathHelper.TwoPi * i) % MathHelper.TwoPi;
                        yield return null;
                    }
                }
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Draw.Point(Position, Color.White);
            }
        }
        #endregion
    }
}
