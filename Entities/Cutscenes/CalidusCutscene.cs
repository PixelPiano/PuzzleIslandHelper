using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class CalidusCutscene : CutsceneEntity
    {
        public static void MarkCutsceneAsWatched(Scene scene, Cutscenes cutscene)
        {
            (scene as Level).Session.SetFlag("CalCut" + cutscene.ToString());
        }
        public static bool CutsceneWatched(Scene scene, Cutscenes cutscene)
        {
            return (scene as Level).Session.GetFlag("CalCut" + cutscene.ToString());
        }
        public enum Cutscenes
        {
            FirstIntro,
            First,
            FirstOutro,
            SecondIntro,
            Second,
            SecondA
        }

        public Cutscenes Cutscene;
        private Calidus Calidus;
        private Vector2 caliMoveBackPosition;
        private bool panningOut;
        private bool ShakeLevel;
        private bool LookingSideToSide = true;
        public Coroutine ScreenZoomAcrossRoutine;
        public CalidusCutscene(Cutscenes cutscene)
            : base()
        {
            Cutscene = cutscene;
        }

        public override void OnBegin(Level level)
        {
            Player player = level.GetPlayer();
            switch (Cutscene)
            {
                case Cutscenes.FirstIntro:
                    Add(new Coroutine(FirstIntro(player, level)));
                    break;
                case Cutscenes.First:
                    PianoModule.Session.TimesMetWithCalidus = 1;
                    Add(new Coroutine(First(player, level)));
                    break;
                case Cutscenes.FirstOutro:
                    PianoModule.Session.TimesMetWithCalidus = 1;
                    Add(new Coroutine(FirstOutro(player, level)));
                    break;
                case Cutscenes.SecondIntro:
                    Add(new Coroutine(SecondIntro(player, level)));
                    break;
                case Cutscenes.Second:
                    PianoModule.Session.TimesMetWithCalidus = 2;
                    Add(new Coroutine(Second(player, level)));
                    break;
                case Cutscenes.SecondA:
                    PianoModule.Session.TimesMetWithCalidus = 2;
                    Add(new Coroutine(SecondOptional(player, level)));
                    break;
            }
        }
        public override void OnEnd(Level level)
        {
            level.ResetZoom();
            Player player = Level.GetPlayer();
            Calidus calidus = Level.Tracker.GetEntity<Calidus>();
            if (calidus != null)
            {
                switch (Cutscene)
                {
                    case Cutscenes.Second:
                        calidus.Emotion(Calidus.Mood.Normal);
                        break;
                    case Cutscenes.SecondA:
                        calidus.Emotion(Calidus.Mood.Normal);
                        calidus.Look(Calidus.Looking.DownRight);
                        break;
                }

            }
            if (player != null && (WasSkipped || (int)Cutscene < 5))
            {
                player.StateMachine.State = Player.StNormal;
            }
            if (WasSkipped)
            {
                Audio.PauseMusic = false;
                level.Session.SetFlag("blockGlitch", false);
                ShakeLevel = false;
                Glitch.Value = 0;
                Level.StopShake();

                if (player != null)
                {
                    switch (Cutscene)
                    {
                        case Cutscenes.First:
                            if (level.Session.Level != "0-lcomp")
                            {
                                InstantTeleport(level, player, "0-lcomp", null);
                                level.InCutscene = false;
                            }
                            break;
                        case Cutscenes.Second:
                            if (Calidus != null)
                            {
                                Calidus.Look(Calidus.Looking.DownRight);
                                Calidus.CanFloat = true;
                                Calidus.Position = Calidus.OrigPosition;
                            }
                            break;
                    }
                }
            }
            PianoModule.Session.CalidusCutscene = Cutscene switch
            {
                Cutscenes.FirstIntro => Cutscenes.First,
                Cutscenes.First => WasSkipped ? Cutscenes.SecondIntro : Cutscenes.FirstOutro,
                Cutscenes.FirstOutro => Cutscenes.SecondIntro,
                Cutscenes.SecondIntro => Cutscenes.Second,
                Cutscenes.Second => Cutscenes.SecondA,
                _ => PianoModule.Session.CalidusCutscene
            };
        }
        private IEnumerator FirstIntro(Player player, Level level)
        {
            yield return GlitchOut(player, level);
            Add(new Coroutine(StutterGlitch(20)));
            yield return Textbox.Say("wtc1", PanOut, WaitForPanOut);
            yield return Level.ZoomBack(0.8f);
            EndCutscene(Level);
        }
        private IEnumerator First(Player player, Level level)
        {
            level.InCutscene = true;
            Vector2 zoomPosition = new Vector2(113, player.Position.Y - level.LevelOffset.Y - 40);
            Coroutine zoomIn = new Coroutine(ScreenZoom(new Vector2(113, player.Position.Y - level.LevelOffset.Y - 40), 1.5f, 2));
            Coroutine walkTo = new Coroutine(player.DummyWalkTo(113 + level.Bounds.X));
            Coroutine cameraTo = new Coroutine(CameraTo(level.LevelOffset + Vector2.UnitY * 8, 2, Ease.SineInOut));
            Add(zoomIn, walkTo, cameraTo);
            while (zoomIn.Active || walkTo.Active || cameraTo.Active)
            {
                yield return null;
            }
            yield return 0.2f;
            yield return Walk(16);
            yield return 1;
            yield return Walk(-16);
            yield return 1;
            yield return Textbox.Say("Calidus1", maddyStartle, zoomAcross, stopLookingSideToSide, waitZoom, calidusFix, wait1, wait3, quickLookRight, maddyWalkUp, glitch, warp, waithalfsecond);

            IEnumerator maddyStartle()
            {
                Audio.PauseMusic = true;
                yield return 0.1f;
                player.Jump();
                Add(new Coroutine(LookSideToSide(player)));
                yield return null;
            }
            IEnumerator zoomAcross()
            {
                ScreenZoomAcrossRoutine = new Coroutine(SceneAs<Level>().ZoomAcross(zoomPosition + Vector2.UnitX * 32, 1.5f, 7));
                Add(ScreenZoomAcrossRoutine);
                yield return null;
            }
            IEnumerator stopLookingSideToSide()
            {
                LookingSideToSide = false;
                yield return null;
            }
            IEnumerator waitZoom()
            {
                yield return 0.2f;
                if (ScreenZoomAcrossRoutine.Active)
                {
                    ScreenZoomAcrossRoutine.Cancel();
                    Remove(ScreenZoomAcrossRoutine);
                    level.ZoomSnap(zoomPosition + Vector2.UnitX * 32, 1.5f);
                }
                Add(new Coroutine(PlayerZoomAcross(player, 2f, 2, 32, -32)));
            }
            IEnumerator calidusFix()
            {
                Calidus.LookSpeed /= 5;
                Calidus.LookDir = Calidus.Looking.Left;
                Calidus.BrokenParts.Play("jitter");
                yield return 1.4f;
                Calidus.FixSequence();
                while (Calidus.Broken)
                {
                    yield return null;
                }
                yield return null;
                player.Facing = Facings.Right;
                Add(new Coroutine(Events(Wait(0.5f), Walk(-16, true))));
            }
            IEnumerator wait1()
            {
                yield return 1;
            }
            IEnumerator wait3()
            {
                yield return 3;

            }
            IEnumerator quickLookRight()
            {
                Calidus.LookSpeed *= 5;
                Calidus.LookDir = Calidus.Looking.Right;
                yield return null;
            }
            IEnumerator maddyWalkUp()
            {
                IEnumerator routine()
                {
                    yield return 0.3f;
                    Calidus.Surprised(false);
                    yield return 0.5f;
                    Calidus.LookDir = Calidus.Looking.Left;
                }
                Add(new Coroutine(routine()));
                //Add(new Coroutine(Walk(16, false, 2)));
                yield return null;
            }
            IEnumerator waithalfsecond()
            {
                yield return 0.5f;
                yield return null;
            }

            IEnumerator glitch()
            {

                //Rumble, glitchy effects
                //todo: add sound
                level.Session.SetFlag("blockGlitch");
                yield return 0.1f;
                IEnumerator routine()
                {
                    Calidus.Surprised(true);
                    yield return 0.1f;
                    Calidus.LookDir = Calidus.Looking.Up;
                    yield return 0.7f;
                    Calidus.LookDir = Calidus.Looking.Left;
                }
                Add(new Coroutine(routine()));
            }
            IEnumerator warp()
            {
                level.Flash(Color.White, true);
                level.Session.SetFlag("blockGlitch", false);
                //todo: Move this textscene over to something on the Lab Computer. Maybe another message.
                /* SingleTextscene text = new SingleTextscene("CaL1");
                level.Add(text);
                Level.StopShake();
                while (text.InCutscene)
                {
                    yield return null;
                }*/

                level.Remove(Calidus);
                InstantTeleport(level, player, "0-lcomp");
                yield return null;
            }
        }
        private IEnumerator FirstOutro(Player player, Level level)
        {
            player.StateMachine.State = Player.StDummy;
            IEnumerator waithalfsecond()
            {
                yield return 0.5f;
            }
            IEnumerator wait1()
            {
                yield return 1;
            }
            yield return Textbox.Say("Calidus1a", waithalfsecond, wait1);
            yield return level.ZoomBack(1);
            Audio.PauseMusic = false;
            level.InCutscene = false;
            player.StateMachine.State = Player.StNormal;
            EndCutscene(level);
        }
        private IEnumerator Cutscene1NoWarp(Player player, Level level)
        {
            yield return null;
            yield return Textbox.Say("Calidus1noWarp");
            EndCutscene(level);
        }
        private IEnumerator SecondIntro(Player player, Level level)
        {
            level.InCutscene = true;
            Add(new Coroutine(Events(GlitchOut(player, level), StutterGlitch(20))));
            Vector2 playerMarker = level.Marker("player");
            Vector2 ZoomPosition = level.Marker("camera1") - level.LevelOffset;
            Coroutine zoomIn = new Coroutine(ScreenZoom(ZoomPosition, 1.5f, 2));
            Coroutine walkTo = new Coroutine(player.DummyWalkTo(playerMarker.X));
            Add(zoomIn);
            yield return 1.3f;
            Add(walkTo);
            while (zoomIn.Active || walkTo.Active)
            {
                yield return null;
            }
            yield return 0.3f;
            yield return Textbox.Say("Cb0", PlayerLookLeft, PlayerLookRight);
            yield return Level.ZoomBack(1.5f);
            EndCutscene(Level);
        }
        private IEnumerator Second(Player player, Level level)
        {

            Add(new Coroutine(Level.ZoomTo(level.Marker("camera1", true), 1.4f, 1)));
            yield return player.DummyWalkTo(level.Marker("player1").X);
            yield return Textbox.Say("Calidus2",
                Normal, Happy, Stern, Eugh, Surprised,//0 - 4
                RollEye, LookLeft, LookRight, LookUp, LookDown, //5 - 9
                LookUpLeft, LookDownLeft, LookDownRight, LookUpRight, LookCenter, //10 - 14
                LookPlayer, WaitForOne, WaitForTwo, CaliDramaticFloatAway, CaliStopBeingDramatic, //15 - 19
                CaliTurnRight, CaliMoveCloser, CaliMoveBack, Closed, Nodders, //20 - 24
                PlayerPoke, Reassemble, PlayerToMarker2, PlayerToMarker3, PlayerToMarker4,//25 - 29
                PlayerToMarker5, PlayerToMarkerCalidus, PanToCalidus, PlayerToMarker6, PlayerFaceLeft, PlayerFaceRight, askIfHasntDied); //30 - 36
            yield return Level.ZoomBack(1);
            EndCutscene(Level);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Calidus = scene.Tracker.GetEntity<Calidus>();
            if (Cutscene == Cutscenes.Second)
            {
                Calidus.Stern();
            }
        }
        private IEnumerator askIfHasntDied()
        {
            if (!Level.Session.GetFlag("HasDiedInTransitLab"))
            {
                yield return new SwapImmediately(Textbox.Say("howToLeaveTransitLab", WaitForOne));
            }
        }
        private IEnumerator PlayerFaceLeft()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            player.Facing = Facings.Left;
            yield return null;
        }
        private IEnumerator PlayerFaceRight()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            player.Facing = Facings.Right;
            yield return null;
        }
        private IEnumerator SecondOptional(Player player, Level level)
        {
            player.StateMachine.State = Player.StDummy;
            yield return player.DummyWalkTo(Level.Marker("player5").X);
            player.Facing = Facings.Right;
            yield return Level.ZoomTo(level.Marker("camera2", true) + Vector2.UnitX * 32, 1.4f, 1);
            yield return Textbox.Say("Calidus2b",
                Normal, Stern, LookPlayer, LookDownRight, LookRight, WaitForOne);
            yield return Level.ZoomBack(1);
            player.StateMachine.State = Player.StNormal;
            EndCutscene(Level);
        }
        private IEnumerator PanToCalidus()
        {
            float from = Level.Camera.Position.X;
            float to = Level.Camera.Position.X + (Level.Marker("camera2") - Level.Marker("camera1")).X;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                Level.Camera.Position = new Vector2(Calc.LerpClamp(from, to, Ease.SineInOut(i)), Level.Camera.Position.Y);
                yield return null;
            }
            yield return 0.8f;
        }
        private IEnumerator PlayerToMarker1()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            yield return player.DummyWalkTo(Level.Marker("player1").X);
        }
        private IEnumerator PlayerToMarker2()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            yield return player.DummyWalkTo(Level.Marker("player2").X);
        }
        private IEnumerator PlayerToMarker3()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            yield return player.DummyWalkTo(Level.Marker("player3").X);
        }
        private IEnumerator PlayerToMarker4()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            yield return player.DummyWalkTo(Level.Marker("player4").X);
        }
        private IEnumerator PlayerToMarker5()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            yield return player.DummyWalkTo(Level.Marker("player5").X);
        }
        private IEnumerator PlayerToMarker6()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            yield return player.DummyWalkTo(Level.Marker("player6").X);
        }
        private IEnumerator PlayerToMarkerCalidus()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            yield return player.DummyWalkTo(Level.Marker("playerCalidus").X, false, 0.5f);
        }
        private IEnumerator Reassemble()
        {
            yield return Calidus.ReassembleRoutine();
        }
        private IEnumerator PlayerPoke()
        {
            if (Level.GetPlayer() is not Player player || Calidus == null) yield break;
            yield return Level.ZoomAcross(Level.Marker("cameraZoom", true), 1.7f, 1.5f);

            Add(new Coroutine(WaitThenFallApart(0.5f)));
            Add(new Coroutine(WaitThenBackUp(0.5f, player)));
            yield return DoubleSay("HEYCALIDUS", "CalidusAHHH");
            yield return Calidus.WaitForFallenApart();
            yield return 0.2f;
            Calidus.LookSpeed = 1;
        }

        private IEnumerator WaitThenFallApart(float time)
        {
            yield return time;
            Add(new Coroutine(ZoomWobble(1.9f, 3, 0.1f)));
            Calidus.Surprised(true);
            Calidus.Look(Calidus.Looking.Right);
            Calidus.LookSpeed = 2;
            yield return Calidus.FallApartRoutine();
        }
        private IEnumerator ZoomWobble(float to, int loops, float interval)
        {
            float from = Level.Zoom;
            Vector2 point = Level.Marker("cameraZoom", true);
            for (int i = 0; i < loops; i++)
            {
                yield return Level.ZoomAcross(point, to, interval);
                yield return Level.ZoomAcross(point, from, interval);
            }
        }
        private IEnumerator WaitThenBackUp(float time, Player player)
        {
            yield return time;
            player.Jump();
            yield return null;
            while (!player.OnGround())
            {
                yield return null;
            }
            yield return 0.1f;
            yield return player.DummyWalkTo(Level.Marker("playerIdle").X, true);
        }
        private IEnumerator DoubleSay(string dialogA, string dialogB)
        {
            Coroutine routineA = new(Textbox.Say(dialogA)), routineB = new(Textbox.Say(dialogB));
            Add(routineA, routineB);
            while (!(routineA.Finished && routineB.Finished))
            {
                yield return null;
            }
        }
        public void AddGlitches(Player player, Calidus calidus)
        {
            Level.Add(new QuickGlitch(player, new NumRange2(2, 5, 2, 5), Vector2.One * 8, Engine.DeltaTime, 8, 0.4f));
            Level.Add(new QuickGlitch(calidus, new NumRange2(2, 5, 2, 5), Vector2.Zero, Engine.DeltaTime, 8, 0.4f) { Offset = -Vector2.UnitX * 8 });
        }
        private IEnumerator CaliDramaticFloatAway()
        {
            Calidus.FloatLerp(Calidus.Position + Vector2.UnitX * 24, 2, Ease.SineInOut);
            yield return null;
        }
        private IEnumerator Happy()
        {
            Calidus.Emotion(Calidus.Mood.Happy);
            yield return null;
        }
        private IEnumerator Stern()
        {
            Calidus.Emotion(Calidus.Mood.Stern);
            yield return null;
        }
        private IEnumerator Normal()
        {
            Calidus.Emotion(Calidus.Mood.Normal);
            yield return null;
        }
        private IEnumerator RollEye()
        {
            Calidus.Emotion(Calidus.Mood.RollEye);
            yield return null;
        }
        private IEnumerator Laughing()
        {
            Calidus.Emotion(Calidus.Mood.Laughing);
            yield return null;
        }
        private IEnumerator Shakers()
        {
            Calidus.Emotion(Calidus.Mood.Shakers);
            yield return null;
        }
        private IEnumerator Nodders()
        {
            Calidus.Emotion(Calidus.Mood.Nodders);
            yield return null;
        }
        private IEnumerator Closed()
        {
            Calidus.Emotion(Calidus.Mood.Closed);
            yield return null;
        }
        private IEnumerator Angry()
        {
            Calidus.Emotion(Calidus.Mood.Angry);
            yield return null;
        }
        private IEnumerator Surprised()
        {
            Calidus.Emotion(Calidus.Mood.Surprised);
            yield return null;
        }
        private IEnumerator Wink()
        {
            Calidus.Emotion(Calidus.Mood.Wink);
            yield return null;
        }
        private IEnumerator Eugh()
        {
            Calidus.Emotion(Calidus.Mood.Eugh);
            yield return null;
        }
        private IEnumerator LookLeft()
        {
            Calidus.Look(Calidus.Looking.Left);
            yield return null;
        }
        private IEnumerator LookRight()
        {
            Calidus.Look(Calidus.Looking.Right);
            yield return null;
        }
        private IEnumerator LookUp()
        {
            Calidus.Look(Calidus.Looking.Up);
            yield return null;
        }
        private IEnumerator LookDown()
        {
            Calidus.Look(Calidus.Looking.Down);
            yield return null;
        }
        private IEnumerator LookUpRight()
        {
            Calidus.Look(Calidus.Looking.UpRight);
            yield return null;
        }
        private IEnumerator LookDownRight()
        {
            Calidus.Look(Calidus.Looking.DownRight);
            yield return null;
        }
        private IEnumerator LookDownLeft()
        {
            Calidus.Look(Calidus.Looking.DownLeft);
            yield return null;
        }
        private IEnumerator LookUpLeft()
        {
            Calidus.Look(Calidus.Looking.UpLeft);
            yield return null;
        }
        private IEnumerator LookCenter()
        {
            Calidus.Look(Calidus.Looking.Center);
            yield return null;
        }
        private IEnumerator LookPlayer()
        {
            Calidus.Look(Calidus.Looking.Player);
            yield return null;
        }
        private IEnumerator CaliStopBeingDramatic()
        {
            Calidus.FloatLerp(Calidus.Position + Vector2.UnitX * -24, 1, Ease.SineInOut);
            Calidus.LookDir = Calidus.Looking.Left;
            yield return null;
        }
        private IEnumerator CaliTurnRight()
        {
            Calidus.LookDir = Calidus.Looking.Right;
            Calidus.FloatLerp(Calidus.Position + Vector2.UnitX * 16, 1, Ease.SineInOut);
            yield return null;
        }
        private IEnumerator CaliMoveCloser()
        {
            if (Level.GetPlayer() is Player player)
            {
                Calidus.FloatLerp(player.Center + (Calidus.Position - player.Center) * 0.7f, 0.5f);
            }
            yield return null;
        }
        private IEnumerator CaliMoveBack()
        {
            if (caliMoveBackPosition != Vector2.Zero)
            {
                Calidus.FloatLerp(Calidus.OrigPosition, 1, Ease.SineOut);
            }
            yield return null;
        }
        private IEnumerator GlitchOut(Player player, Level level)
        {
            player.StateMachine.State = Player.StDummy;
            level.ZoomSnap(player.Center - Level.Camera.Position - Vector2.UnitY * 24, 1.7f);
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                Glitch.Value = 1 - i;
                yield return null;
            }
            Glitch.Value = 0;
            yield return 0.5f;
        }
        private IEnumerator WaitForPanOut()
        {
            while (panningOut)
            {
                yield return null;
            }
        }
        private IEnumerator PanOut()
        {
            Add(new Coroutine(ActuallyPanOut()));
            yield return null;
        }
        private IEnumerator ActuallyPanOut()
        {
            panningOut = true;
            yield return Level.ZoomBack(4.3f);
            panningOut = false;
            Level.ResetZoom();
        }
        private IEnumerator StutterGlitch(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                if (Calc.Random.Chance(0.1f))
                {
                    int addframes = Calc.Random.Range(1, 4);
                    Glitch.Value = Calc.Random.Range(0.08f, 0.4f);
                    yield return Engine.DeltaTime * addframes;
                    i += addframes;
                }
                Glitch.Value = 0;
            }
        }
        private IEnumerator PlayerLookLeft()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            player.Facing = Facings.Left;
            yield return null;
        }
        private IEnumerator PlayerLookRight()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            player.Facing = Facings.Right;
            yield return null;
        }

        public override void Update()
        {
            base.Update();
            if (ShakeLevel)
            {
                Level.shakeTimer = Engine.DeltaTime;
            }
            //AuraAmount = (float)Math.Sin(Scene.TimeActive);
        }
        private IEnumerator Walk(float x, bool backwards = false, float speedmult = 1, bool intoWalls = false)
        {
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null)
            {
                float positionX = player.Position.X;
                yield return player.DummyWalkTo(positionX + x, backwards, speedmult, intoWalls);
            }
            else
            {
                yield break;
            }
        }
        private IEnumerator Events(params IEnumerator[] functions)
        {
            foreach (IEnumerator o in functions)
            {
                yield return o;
            }
        }
        private IEnumerator Wait(float time)
        {
            yield return time;
        }
        private IEnumerator WaitForOne()
        {
            yield return Wait(1);
        }
        private IEnumerator WaitForTwo()
        {
            yield return Wait(2);
        }
        private IEnumerator LookSideToSide(Player player)
        {
            LookingSideToSide = true;
            while (LookingSideToSide)
            {
                player.Facing = Facings.Right;
                yield return 0.5f;
                player.Facing = Facings.Left;
                yield return 0.5f;
            }
        }
        private IEnumerator ScreenZoom(Vector2 screenPosition, float amount, float time)
        {
            Level level = SceneAs<Level>();
            yield return level.ZoomTo(screenPosition, amount, time);
        }
        private IEnumerator PlayerZoomAcross(Player player, float amount, float time, float xOffset, float yOffset)
        {
            Level level = SceneAs<Level>();
            yield return level.ZoomAcross(ScreenCoords(player.Position + new Vector2(xOffset, yOffset), level), amount, time);
        }
        private Vector2 ScreenCoords(Vector2 position, Level level)
        {
            return position - level.Camera.Position;
        }
        public static void InstantRelativeTeleport(Scene scene, string room, bool snapToSpawnPoint, int positionX = 0, int positionY = 0)
        {
            Level level = scene as Level;
            Player player = level.GetPlayer();
            if (level == null || player == null)
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
                Vector2 val2 = player.Position - levelOffset;
                Vector2 val3 = level.Camera.Position - levelOffset;
                Vector2 offset = new Vector2(positionY, positionX);
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

                level.Camera.Position = level.LevelOffset + val3 + offset.Floor();
                level.Add(player);
                if (snapToSpawnPoint && session.RespawnPoint.HasValue)
                {
                    player.Position = session.RespawnPoint.Value + offset.Floor();
                }
                else
                {
                    player.Position = level.LevelOffset + val2 + offset.Floor();
                }

                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset + offset.Floor());
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }
        public static void InstantTeleport(Scene scene, Player player, string room, Action<Level> onEnd = null)
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
                Vector2 val2 = player.Position - level.LevelOffset; ;
                Vector2 val3 = level.Camera.Position - level.LevelOffset;
                float zoom = level.Zoom;
                float zoomTarget = level.ZoomTarget;
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
                level.LoadLevel(Player.IntroTypes.None);
                level.Camera.Position = level.LevelOffset + val3;
                level.Zoom = zoom;
                level.ZoomTarget = zoomTarget;
                player.Position = level.LevelOffset + val2;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }

                onEnd?.Invoke(level);
            };
        }
    }
}
