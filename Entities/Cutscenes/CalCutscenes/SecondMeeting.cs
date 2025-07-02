using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Calidus;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [CalidusCutscene("Second")]
    public class SecondMeeting : CalidusCutscene
    {
        public SecondMeeting(Player player = null, Calidus calidus = null, Arguments start = null, Arguments end = null) : base(player, calidus, start, end)
        {

        }
        public override void OnBegin(Level level)
        {
            PianoModule.Session.TimesMetWithCalidus = 2;
            base.OnBegin(level);
        }
        public override void OnEnd(Level level)
        {
            level.Session.SetFlag("DigitalBetaWarpEnabled");
            base.OnEnd(level);
            Calidus?.RemoveTag(Tags.Global);
            if (WasSkipped)
            {
                if (Calidus != null)
                {
                    Calidus.Look(Calidus.Looking.DownRight);
                    Calidus.CanFloat = true;
                    Calidus.Position = Calidus.OrigPosition;
                }
            }
        }
        public override IEnumerator Cutscene(Level level)
        {
            Add(new Coroutine(Level.ZoomTo(level.Marker("camera1", true), 1.4f, 1)));
            yield return Player.DummyWalkTo(level.Marker("player1").X);
            yield return Textbox.Say("Calidus2",
                Normal, Happy, Stern, Eugh, Surprised,//0 - 4
                RollEye, LookLeft, LookRight, LookUp, LookDown, //5 - 9
                LookUpLeft, LookDownLeft, LookDownRight, LookUpRight, LookCenter, //10 - 14
                LookPlayer, WaitForOne, WaitForTwo, CaliDramaticFloatAway, CaliStopBeingDramatic, //15 - 19
                CaliTurnRight, CaliMoveCloser, CaliMoveBack, Closed, Nodders, //20 - 24
                PlayerPoke, Reassemble, PlayerToMarker2, PlayerToMarker3, PlayerToMarker4,//25 - 29
                PlayerToMarker5, PlayerToMarkerCalidus, PanToCalidus, PlayerToMarker6, PlayerLookLeft, PlayerLookRight, askIfHasntDied); //30 - 36
            yield return Level.ZoomBack(1);
            EndCutscene(Level);
        }
        //currently unused
        private IEnumerator askIfHasntDied()
        {
            if (!Level.Session.GetFlag("HasDiedInTransitLab"))
            {
                yield return new SwapImmediately(Textbox.Say("howToLeaveTransitLab", WaitForOne));
            }
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
        //unused
        private IEnumerator CaliMoveBack()
        {
            yield return null;
        }

    }
}
