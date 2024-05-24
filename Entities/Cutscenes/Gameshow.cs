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

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class Gameshow : CutsceneEntity
    {
        public int WrongAnswers;
        public bool Lost;
        public RandomRoutines Random;
        public float SavedLighting;
        public bool LoopQuestion;
        public bool PassedIntro;
        public bool StartedMusic;
        public SoundSource SnareLoop;
        public string PrevMusic;
        private const string Song = "event:/PianoBoy/ThatsMyMerleeIntro";
        private const string Drum = "event:/PianoBoy/drumroll";
        private const string path = "objects/PuzzleIslandHelper/gameshow/";


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
        public static int RoomsVisited => RoomOrder.Count;

        public Gameshow() : base()
        {
            Depth = -10000;
            RemoveOnSkipped = false;
            Random = new();
        }


        public override void Render()
        {
            base.Render();
            if (Host != null)
            {
                Host.DrawBeam();
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Host = Level.Tracker.GetEntity<Host>();
            playerLight = GameshowSpotlight.GetSpotlight(scene, "player");
            hostLight = GameshowSpotlight.GetSpotlight(scene, "host");
            Level.Session.SetFlag("gameshowReveal", RoomsVisited != 0);
            Vector2 topLeft = Level.LevelOffset;
            Vector2 center = topLeft + new Vector2(160, 90);
            screen = new Screen(topLeft);
            background = new Background(topLeft);
            Level.Add(background, screen);
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
        public IEnumerator FlashRoutine(bool correct)
        {
            float waitTime = 0.15f;
            Audio.Play("event:/PianoBoy/" + (correct ? "correct" : "incorrect"));
            yield return screen.FlashResult(correct, waitTime);
        }

        public override void OnBegin(Level level)
        {
            if (level.GetPlayer() is Player player)
            {
                player.Light.Alpha = 1f;
            }

            Add(cutscene = new Coroutine(Cutscene()));
        }
        public void Hide()
        {
            if (Level.GetPlayer() is Player player)
            {
                player.Visible = false;
                player.StateMachine.State = Player.StDummy;
            }
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
            SavedLighting = Level.Lighting.Alpha;
            Level.Lighting.Alpha = 0.6f;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Level.Lighting.Alpha = SavedLighting;
        }
        public void Reveal(bool skipped = false)
        {
            Level.Lighting.Alpha = 0.2f;
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
            }
            Host.Reveal();
        }
        public void FreePlayer(Player player)
        {
            player.StateMachine.State = Player.StNormal;
            Passage.AllTeleportsActive = true;
        }
        public override void OnEnd(Level level)
        {
            if (level.GetPlayer() is not Player player) return;
            if (Lost)
            {
                InstantTeleportToSpawn(level, player, "GameshowLose");
            }
            if (WasSkipped)
            {
                //Level.Lighting.Alpha = SavedLighting;
                Reveal(true);
                JumpToLoop();
                cutscene.Cancel();
            }
            Level.Lighting.Alpha = 0.3f;
            FreePlayer(player);
            Audio.SetMusic(PrevMusic);
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
        public IEnumerator Cutscene()
        {
            int timesReturned = 0;
            if (RoomsVisited == 0)
            {
                Hide();
                PrevMusic = Audio.CurrentMusic;
                Audio.currentMusicEvent?.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                yield return 0.5f;
                yield return Intro();
                PassedIntro = true;
            }
            else
            {
                if (Level.Camera.Position != Level.LevelOffset)
                {
                    Level.Camera.Position = Level.LevelOffset;
                }
                Player player = Level.GetPlayer();
                player.Facing = Facings.Left;
                player.StateMachine.State = Player.StDummy;
                MakeUnskippable();
                Reveal(true);
                yield return 0.8f;
                StartCheering();
                while (PassageTransition.Transitioning)
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
                LifeDisplay display = Level.Tracker.GetEntity<LifeDisplay>();
                if (display is not null && display.GameOver)
                {
                    yield return BigLose();
                    //todo: link to lose sequence
                }
                else
                {
                    yield return BigWin();
                    //todo: teleport player
                }
            }
            yield return Textbox.Say("rfree" + timesReturned, HostLeave);
            yield return null;
            EndCutscene(Level);
        }

        private IEnumerator HostLeave()
        {
            yield return Host.Skedaddle();
        }
        public IEnumerator Intro()
        {
            Player player = Level.GetPlayer();
            player.Facing = Facings.Left;
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
            AllFocusHost();
            SnareLoop.Stop(false);
            Audio.Play("event:/PianoBoy/crashCymbal");
            Audio.SetMusic(Song);
            Audio.SetMusicParam("fade", 1);
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
        public void MakeUnskippable()
        {
            (Scene as Level).InCutscene = false;
            (Scene as Level).CancelCutscene();
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
        private void SpotlightsFreakout(Level level)
        {
            foreach (GameshowSpotlight light in level.Tracker.GetEntities<GameshowSpotlight>())
            {
                light.Freakout(level.Camera.Position + new Vector2(160, 90), 120, 65, level.GetPlayer());
            }
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
            Question question = PianoModule.GameshowData.QuestionSets[index].GetRandom();
            if (question is null) yield return Incorrect(null, 0);
            yield return Textbox.Say($"r{index}intro", WaitForOne, SayTitle); //ask prompt
            yield return MultiChoice(question);
            yield return null;
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

            bool unique;
            int max = question.Choices;
            int pages = (int)Math.Round((double)max / question.PerPage, 0, MidpointRounding.AwayFromZero);
            yield return Textbox.Say(question.Q); //ask prompt
            Audio.Play("event:/PianoBoy/questionPresented");

            while (true)
            {
                page %= pages; //cycle back to first page if player continues after last page
                List<string> options = question.GetPage(page); //get page of choices
                yield return ChoicePrompt.Prompt(options.ToArray()); //let player pick a choice
                if (ChoicePrompt.Choice < question.PerPage) //if player chose a valid answer
                {
                    string id = options[ChoicePrompt.Choice];
                    unique = RandomRoutines.ValidIDs.Contains(id + "U");
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
            yield return Drumroll();

            if (result)
            {
                yield return 0.1f;
                StartCheering();
                yield return FlashRoutine(true);
                yield return 0.5f;
                StopCheering();
                yield return Correct(page * question.PerPage + ChoicePrompt.Choice);
            }
            else
            {
                yield return 0.3f;
                yield return FlashRoutine(false);
                yield return 0.5f;
                if (unique)
                {
                    yield return Incorrect(question, page * question.PerPage + ChoicePrompt.Choice);
                }
                else
                {
                    yield return IncorrectRandom(question);
                }
                WrongAnswers++;
                yield return LoseLife();
            }
            FadeInMusic();
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
            yield return Textbox.Say("q" + question + "W");
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
        public void FadeOutMusic()
        {
            Audio.SetMusicParam("fade", 0);
        }
        public void FadeInMusic()
        {
            Audio.SetMusicParam("Jump", 1);
            Audio.SetMusicParam("fade", 0);
            Audio.SetMusicParam("fade", 1);
        }
        public void JumpToLoop()
        {
            SnareLoop.Stop();
            Level.Session.Audio.Music.Event = Song;
            Level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            FadeInMusic();
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
        public IEnumerator Skipped()
        {
            Level.Lighting.Alpha = SavedLighting;
            Reveal(true);
            JumpToLoop();
            yield return null;
            EndCutscene(Level);
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
            Lost = true;
        }
        public IEnumerator BigWin()
        {
            yield return Textbox.Say("q3Wa");
            yield return Cheer();
            yield return Textbox.Say("q3Wb");
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
            public LightPath(Vector2 center, float radius, float startAngle) : base(center)
            {
                origPosition = center;
                float angle = startAngle.ToRad();
                Tween t = Tween.Create(Tween.TweenMode.Looping, Ease.Linear, 2, true);
                t.OnUpdate = (Tween t) =>
                {
                    Position = center + Calc.AngleToVector(angle, radius);
                    angle = (startAngle + MathHelper.TwoPi * t.Eased) % MathHelper.TwoPi;
                };
                Add(t);
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Draw.Point(Position, Color.White);
            }
        }
    }
}
