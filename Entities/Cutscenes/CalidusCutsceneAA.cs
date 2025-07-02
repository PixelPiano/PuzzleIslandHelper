using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    public static class CalScene
    {
        public static bool GetCutsceneFlag(this CalCut cutscene)
        {
            if (Engine.Scene is not Level level) return false;
            return level.Session.GetFlag("CalCut" + cutscene.ToString());
        }
        public static void Register(this CalCut cutscene)
        {
            if (Engine.Scene is Level level)
            {
                level.Session.SetFlag("CalCut" + cutscene.ToString());
            }
        }
    }
    public enum CalCut
    {
        FirstIntro,
        First,
        SecondIntro,
        Second,
        SecondTryWarp,
        SecondTalkAboutWarp,
        TalkAboutNote
    }
    [Tracked]
    public class CalidusCutsceneAA : CutsceneEntity
    {
        private bool keepPlayerDummy;
        public CalCut Cutscene;
        private Calidus Calidus;
        private Vector2 caliMoveBackPosition;
        private bool panningOut;
        private bool ShakeLevel;
        private bool LookingSideToSide = true;
        public Coroutine ScreenZoomAcrossRoutine;
        public CalidusCutsceneAA(CalCut cutscene)
            : base()
        {
            Cutscene = cutscene;
        }
        public override void OnBegin(Level level)
        {
            Player player = level.GetPlayer();
            Coroutine coroutine = null;
            switch (Cutscene)
            {
                case CalCut.FirstIntro:
                    player.StateMachine.State = Player.StDummy;
                    player.StateMachine.Locked = true;
                    coroutine = new Coroutine(FirstIntro(player, level));
                    break;
                case CalCut.First:
                    PianoModule.Session.TimesMetWithCalidus = 1;
                    CalCut.FirstIntro.Register();
                    coroutine = new Coroutine(First(player, level));
                    break;
                case CalCut.SecondIntro:
                    player.StateMachine.State = Player.StDummy;
                    player.StateMachine.Locked = true;
                    CalCut.FirstIntro.Register();
                    CalCut.First.Register();
                    if (!CalCut.Second.GetCutsceneFlag()) coroutine = new Coroutine(SecondIntro(player, level));
                    break;
                case CalCut.Second:
                    PianoModule.Session.TimesMetWithCalidus = 2;
                    CalCut.FirstIntro.Register();
                    CalCut.First.Register();
                    CalCut.SecondIntro.Register();
                    coroutine = new Coroutine(Second(player, level));
                    break;
                case CalCut.SecondTalkAboutWarp:
                    PianoModule.Session.TimesMetWithCalidus = 2;
                    break;
                case CalCut.TalkAboutNote:
                    PianoModule.Session.TimesMetWithCalidus = 2;
                    coroutine = new Coroutine(SecondOptional(player, level));
                    break;
            }
            if (coroutine != null)
            {
                Add(coroutine);
            }
        }
        public IEnumerator TalkAboutWarp(Player player, Level level)
        {
            yield return Textbox.Say("CalidusLeftBehindB");
            EndCutscene(level);
        }
        public override void OnEnd(Level level)
        {
            Cutscene.Register();
            level.ResetZoom();
            Player player = Level.GetPlayer();
            Calidus calidus = Level.Tracker.GetEntity<Calidus>();
            if (calidus != null)
            {
                switch (Cutscene)
                {
                    case CalCut.Second or CalCut.SecondTalkAboutWarp:
                        calidus.Emotion(Calidus.Mood.Normal);
                        calidus.StartFollowing(Calidus.Looking.Player);
                        calidus.RemoveTag(Tags.Global);
                        break;
                    case CalCut.TalkAboutNote:
                        calidus.Emotion(Calidus.Mood.Normal);
                        calidus.Look(Calidus.Looking.DownRight);
                        break;
                }
            }
            if (CalCut.Second.GetCutsceneFlag())
            {
                SceneAs<Level>().Session.SetFlag("DigitalBetaWarpEnabled");
            }
            if (player != null)
            {
                player.StateMachine.Locked = false;
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
                        case CalCut.First:
                            if (level.Session.Level != "0-lcomp")
                            {
                                InstantTeleport(level, player, "0-lcomp", null);
                                Running = false;
                                level.InCutscene = false;
                                WarpCapsuleBeta machine = Scene.Tracker.GetEntity<WarpCapsuleBeta>();
                                if (machine != null && PianoModule.Session.PowerState != LabPowerState.Barely)
                                {
                                    PianoModule.Session.PowerState = LabPowerState.Barely;
                                    machine.InCutscene = false;
                                    machine.TimesTypeUsed++;
                                }
                            }
                            break;
                        case CalCut.Second:
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

        }
        private IEnumerator Events(params IEnumerator[] routines)
        {
            foreach (IEnumerator o in routines)
            {
                yield return o;
            }
        }
        private IEnumerator CapsuleIntro(Player player, Level level, WarpCapsuleBeta machine, float zoom, Facings facing, bool doGlitch = true)
        {
            if (machine != null)
            {
                if (doGlitch) Add(new Coroutine(Events(GlitchOut(player, level), StutterGlitch(20))));
                Vector2 zoomPosition = (machine.Center - level.Camera.Position) + new Vector2(1.5f, 3);
                level.ZoomSnap(zoomPosition, zoom);
                player.BottomCenter = machine.Floor.TopCenter;
                player.Facing = facing;
                if (!machine.InCutscene)
                {
                    machine.Add(new Coroutine(machine.ReceivePlayerRoutine(player, false)));
                }
                while (machine.DoorState == WarpCapsule.DoorStates.Opening)
                {
                    player.StateMachine.State = Player.StDummy;
                    yield return null;
                }
                yield return 0.1f;
            }
        }
        private IEnumerator FirstIntro(Player player, Level level)
        {
            yield return CapsuleIntro(player, level, level.Tracker.GetEntity<WarpCapsuleBeta>(), 7.4f, Facings.Right);
            yield return Textbox.Say("wtc1", PanOut, WaitForPanOut);
            yield return Level.ZoomBack(0.8f);
            EndCutscene(Level);
        }
        private IEnumerator First(Player player, Level level)
        {
            Coroutine lookAroundCoroutine = new Coroutine(false);
            Add(lookAroundCoroutine);
            level.InCutscene = true;
            while (!player.OnGround())
            {
                yield return null;
            }
            player.ForceCameraUpdate = false;
            Vector2 zoomPosition = new Vector2(145, player.Position.Y - level.LevelOffset.Y - 40);
            zoomPosition.X = Math.Max(0, zoomPosition.X);
            Coroutine zoomIn = new Coroutine(ScreenZoom(new Vector2(145, player.Position.Y - level.LevelOffset.Y - 40), 1.5f, 2));
            Coroutine cameraTo = new Coroutine(CameraTo(level.LevelOffset + Vector2.UnitY * 8, 2, Ease.SineInOut));
            Coroutine walk = new Coroutine(player.DummyWalkTo(145 + level.Bounds.X));
            Add(zoomIn, cameraTo,walk);
            while (zoomIn.Active || cameraTo.Active || walk.Active)
            {
                yield return null;
            }
            yield return 0.2f;
            yield return Walk(player, 16);
            yield return 1;
            yield return Walk(player, -16);
            yield return 1;
            Vector2 focusPoint = Vector2.Zero;
            IEnumerator zoomAcross()
            {
                if (Level.Tracker.GetEntity<WarpCapsuleBeta>() is var machine)
                {
                    focusPoint = new Vector2(machine.CenterX - Level.Camera.X + 16, zoomPosition.Y);
                    ScreenZoomAcrossRoutine = new Coroutine(Level.ZoomAcross(focusPoint, 1.5f, 7));
                    Add(ScreenZoomAcrossRoutine);
                }
                yield return null;
            }
            yield return Textbox.Say("Calidus1", maddyStartle, zoomAcross, stopLookingSideToSide, waitZoom, calidusFix, wait1, wait3, quickLookRight, maddyWalkUp, glitch, warp, waithalfsecond);
            IEnumerator maddyStartle()
            {
                Audio.PauseMusic = true;
                yield return 0.1f;
                player.Jump();
                Add(new Coroutine(LookSideToSide(player)));
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
                    level.ZoomSnap(focusPoint, 1.5f);
                }
                Add(new Coroutine(PlayerZoomAcross(player, 2f, 2, 40, -32)));
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
                Add(new Coroutine(Events(Wait(0.5f), Walk(player, -16, true))));
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
                Calidus.Surprised(true);
                Add(new Coroutine(Level.ZoomBack(1.2f)));
                IEnumerator routine()
                {
                    while (true)
                    {
                        Calidus.LookDir = Calidus.Looking.UpLeft;
                        yield return 0.7f;
                        Calidus.LookDir = Calidus.Looking.UpRight;
                        yield return 0.7f;
                    }
                }
                lookAroundCoroutine.Replace(routine());
            }
            IEnumerator warp()
            {
                lookAroundCoroutine.Cancel();
                Calidus.Look(Calidus.Looking.Left);
                WarpCapsuleBeta machine = level.Tracker.GetEntity<WarpCapsuleBeta>();
                if (machine != null)
                {
                    void onEnd()
                    {
                        level.Remove(Calidus);
                        PianoModule.Session.PowerState = LabPowerState.Barely;
                        machine.InCutscene = false;
                        machine.TimesTypeUsed++;
                    }
                    machine.Teleport(player, WarpCapsuleBeta.LabID, true, onEnd);
                    yield return 0.1f;
                }
                level.Session.SetFlag("blockGlitch", false);
                yield return null;
            }
        }
        private IEnumerator SecondIntro(Player player, Level level)
        {
            player.StateMachine.State = Player.StDummy;
            level.InCutscene = true;
            yield return CapsuleIntro(player, level, level.Tracker.GetEntity<WarpCapsuleBeta>(), 2, Facings.Right);
            yield return 1.3f;
            player.ForceCameraUpdate = false;
            if (Marker.TryFind("player", out Vector2 playerMarker))
            {
                yield return player.DummyWalkTo(playerMarker.X);
            }
            yield return 1f;
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
            if (Cutscene == CalCut.Second)
            {
                Calidus.Stern();
            }
        }
        //currently unused
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
            Calidus.FloatToLerp(Calidus.Position + Vector2.UnitX * 24, 2, Ease.SineInOut);
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
            Calidus.FloatToLerp(Calidus.Position + Vector2.UnitX * -24, 1, Ease.SineInOut);
            Calidus.LookDir = Calidus.Looking.Left;
            yield return null;
        }
        private IEnumerator CaliTurnRight()
        {
            Calidus.LookDir = Calidus.Looking.Right;
            Calidus.FloatToLerp(Calidus.Position + Vector2.UnitX * 16, 1, Ease.SineInOut);
            yield return null;
        }
        private IEnumerator CaliMoveCloser()
        {
            if (Level.GetPlayer() is Player player)
            {
                Calidus.FloatToLerp(player.Center + (Calidus.Position - player.Center) * 0.7f, 0.5f, null);
            }
            yield return null;
        }
        private IEnumerator CaliMoveBack()
        {
            if (caliMoveBackPosition != Vector2.Zero)
            {
                Calidus.FloatToLerp(Calidus.OrigPosition, 1, Ease.SineOut);
            }
            yield return null;
        }
        private IEnumerator GlitchOut(Player player, Level level)
        {
            player.StateMachine.State = Player.StDummy;
            level.ZoomSnap(player.Center - level.Camera.Position - Vector2.UnitY * 24, 1.7f);
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

        private IEnumerator Walk(Player player, float x, bool backwards = false, float speedmult = 1, bool intoWalls = false)
        {
            float positionX = player.Position.X;
            yield return player.DummyWalkTo(positionX + x, backwards, speedmult, intoWalls);
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
            // position - level.Camera.Position
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
        public override void Update()
        {
            base.Update();
            if (ShakeLevel)
            {
                Level.shakeTimer = Engine.DeltaTime;
            }
        }
    }
}
