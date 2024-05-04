using Celeste.Mod.LuaCutscenes;
using static Celeste.Mod.PuzzleIslandHelper.PuzzleData.GameshowData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.PuzzleIslandHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class Gameshow : CutsceneEntity
    {
        private Player player;
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

        private string currentPath;
        private Vector2 resultOffset = new Vector2(144, 24);
        private Screen screen;
        private Background background;
        private Blackout blackout;
        public Gameshow() : base()
        {
            Depth = -10000;
            RemoveOnSkipped = false;
            Random = new();
            SnareLoop = new SoundSource();
            Add(SnareLoop);
        }
        public class Screen : Entity
        {
            private Image check;
            private Image x;
            public Screen(Vector2 position) : base(position)
            {
                Add(new Image(GFX.Game[path + "display"]));
                Add(check = new Image(GFX.Game[path + "check"]));
                Add(x = new Image(GFX.Game[path + "x"]));
                check.Visible = x.Visible = false;
                Visible = false;
                Depth = 10001;
            }
            public IEnumerator FlashResult(bool correct, float waitTime)
            {
                x.Visible = false;
                check.Visible = false;
                for (int i = 0; i < 4; i++)
                {
                    check.Visible = correct;
                    x.Visible = !correct;
                    yield return waitTime;
                    yield return waitTime;
                }
                x.Visible = false;
                check.Visible = false;
            }

        }

        public class Background : Entity
        {
            private Image IntroCurtains;
            private Image Seats;
            private Image BgCurtains;
            private bool Rising;
            public Background(Vector2 position) : base(position)
            {
                Depth = 10002;
                Add(BgCurtains = new Image(GFX.Game[path + "curtainBG"]));
                Add(Seats = new Image(GFX.Game[path + "seats"]));
                Add(IntroCurtains = new Image(GFX.Game[path + "redCurtainsStock"]));
                Seats.Visible = false;
                BgCurtains.Visible = false;
                IntroCurtains.Scale = new Vector2(320 / IntroCurtains.Width, 180 / IntroCurtains.Height);
            }
            public override void Update()
            {
                base.Update();

                if (Rising)
                {
                    if (IntroCurtains.Position.Y < -320)
                    {
                        IntroCurtains.Visible = false;
                        Rising = false;
                        return;
                    }
                    IntroCurtains.Position.Y -= 5 * Engine.DeltaTime;
                }
            }
            public void RaiseCurtain()
            {
                Rising = true;
            }
            public void Reveal()
            {
                Seats.Visible = BgCurtains.Visible = true;

            }
        }
        public class Blackout : Entity
        {
            public Blackout(Vector2 position) : base(position)
            {
                Depth = -10000;
            }

            public override void Render()
            {
                base.Render();
                Draw.Rect(Position, 320, 180, Color.Lerp(Color.Black, Color.White, 0.1f));
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level.Session.SetFlag("gameshowReveal", false);

            screen = new Screen(Level.LevelOffset);
            background = new Background(Level.LevelOffset);
            blackout = new Blackout(Level.LevelOffset);
            Level.Add(blackout, background, screen);
        }
        public IEnumerator FlashRoutine(bool correct)
        {
            float waitTime = 0.15f;
            Audio.Play("event:/PianoBoy/" + (correct ? "correct" : "incorrect"));
            yield return screen.FlashResult(correct, waitTime);
        }

        public override void OnBegin(Level level)
        {
            player = level.GetPlayer();
            player.Visible = false;
            player.StateMachine.State = Player.StDummy;
            //GameshowSpotlight.GSRenderer.BlendState = BlendState.AlphaBlend;
            foreach (Blinker blinker in Level.Tracker.GetEntities<Blinker>())
            {
                blinker.TurnOff();
                blinker.Visible = false;
            }
            foreach (AudienceMember face in Level.Tracker.GetEntities<AudienceMember>())
            {
                face.Visible = false;
            }
            foreach (LifeDisplay display in Level.Tracker.GetEntities<LifeDisplay>())
            {
                display.Visible = false;
            }
            //SavedLighting = Level.Lighting.Alpha;
            //Level.Lighting.Alpha = 1;
            PrevMusic = Audio.CurrentMusic;
            Audio.currentMusicEvent?.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            Add(new Coroutine(Cutscene()));
        }
        public void Reveal()
        {
            //Level.Lighting.Alpha = SavedLighting;
            Level.Remove(blackout);
            GameshowSpotlight.GSRenderer.BlendState = BlendState.Additive;
            foreach (Blinker blinker in Level.Tracker.GetEntities<Blinker>())
            {
                blinker.Visible = true;
                blinker.StartContinuous(0.1f);
            }
            /*            foreach (StageLight light in Level.Tracker.GetEntities<StageLight>())
                        {
                            if (!light.Activated)
                            {
                                light.Activate();
                            }
                        }*/
            foreach (AudienceMember face in Level.Tracker.GetEntities<AudienceMember>())
            {
                face.Visible = true;
            }
            foreach (LifeDisplay display in Level.Tracker.GetEntities<LifeDisplay>())
            {
                display.Visible = true;
            }
            Level.Session.SetFlag("gameshowReveal");
            if (Level.GetPlayer() is Player player)
            {
                player.Visible = true;
            }
            background.Reveal();
            screen.Visible = true;
        }
        public override void OnEnd(Level level)
        {
            if (Lost)
            {
                InstantTeleportToSpawn(level, level.GetPlayer(), "GameshowLose");
            }

            Audio.SetMusic(PrevMusic);
        }

        public IEnumerator Cutscene()
        {
            yield return 0.5f;
            yield return Intro();
            yield return PreQuestions();
            PassedIntro = true;
            yield return Questions();
            yield return null;
            EndCutscene(Level);
        }
        public IEnumerator Intro()
        {
            Player player = Level.GetPlayer();
            SnareLoop.Position = player is null ? Level.Camera.Position + new Vector2(160, 90) : player.Center;
            SnareLoop.UpdateSfxPosition();
            SnareLoop.Play(Drum);
            yield return 2;
            float[] waitTimes = { 1.5f, 1, 0.7f, 0.5f, 1.5f };
            for (int i = 0; i < 5; i++)
            {
                //SetLights(i);
                yield return Textbox.Say("gameshowStart" + (i + 1));
                //yield return waitTimes[i];
            }
            Reveal();
            SnareLoop.Stop(false);
            Audio.Play("event:/PianoBoy/crashCymbal");
            Audio.SetMusic(Song);
            Audio.SetMusicParam("fade", 1);
            yield return Textbox.Say("gameshowStart", SayTitle, WaitALittle, Cheer);
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
        public IEnumerator PreQuestions()
        {
            yield return Textbox.Say("preQuestions", LookAtLives, StopMusic, StartMusic);
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
        public IEnumerator Questions()
        {
            List<Question> questions = PianoModule.GameshowData.Questions;
            for (int i = 0; i < questions.Count; i++)
            {
                LoopQuestion = true;
                while (LoopQuestion)
                {
                    LoopQuestion = false;
                    yield return MultiChoice(questions[i]);
                }
            }
            yield return null;
        }
        public IEnumerator MultiChoice(Question question)
        {
            bool result;
            int page = 0;
            bool unique;
            int max = question.Choices;
            int pages = (int)Math.Round((double)max / question.PerPage, 0, MidpointRounding.AwayFromZero);
            yield return Textbox.Say(question.Q);
            Audio.Play("event:/PianoBoy/questionPresented");

            while (true)
            {
                page %= pages;
                List<string> options = question.GetPage(page);
                yield return ChoicePrompt.Prompt(options.ToArray());
                if (ChoicePrompt.Choice < question.PerPage)
                {
                    string id = options[ChoicePrompt.Choice];
                    unique = RandomRoutines.ValidIDs.Contains(id + "U");
                    result = PianoModule.GameshowData.Answers.Contains(id);
                    yield return Textbox.Say(id);
                    break;
                }
                else
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
                LoopQuestion = true;
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
            yield return Random.Routine($"q{question.Number}{index}U", this);
        }
        public IEnumerator IncorrectRandom(Question question)
        {
            int random = Calc.Random.Range(1, question.RandomIncorrect);
            yield return Textbox.Say($"q{question.Number}R{random}");
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
            JumpToLoop();
            LifeDisplay display = Level.Tracker.GetEntity<LifeDisplay>();
            if (display is not null)
            {
                display.TurnOn();
            }
            yield return Questions();
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
            EndCutscene(Level);
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
    }
}
