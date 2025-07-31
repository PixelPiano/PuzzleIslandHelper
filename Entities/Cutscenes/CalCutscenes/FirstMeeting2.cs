using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Calidus;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [CalidusCutscene("First2")]
    public class FirstMeeting2 : CalidusCutscene
    {
        public FirstMeeting2(Player player = null, Calidus calidus = null, Arguments start = null, Arguments end = null) : base(player, calidus, start, end)
        {

        }
        public override void OnBegin(Level level)
        {
            Calidus = level.Tracker.GetEntity<Calidus>();

            PianoModule.Session.TimesMetWithCalidus = 2;
            base.OnBegin(level);
        }
        public override void OnEnd(Level level)
        {
            level.Session.SetFlag("DigitalBetaWarpEnabled");
            base.OnEnd(level);
            leftWobbleRoutine?.Cancel();
            rightWobbleRoutine?.Cancel();
            Calidus.Arms[0].Offset.X = prevLeftArmOffset;
            Calidus.Arms[1].Offset.X = prevRightArmOffset;
            Calidus.StartFollowing();
            Calidus.Emotion(Mood.Normal);
            if (WasSkipped)
            {
                if (Calidus != null)
                {
                    Calidus.CanFloat = true;
                    Calidus.Position = Calidus.OrigPosition;
                }
            }
        }
        private float camSaveX;
        public IEnumerator CutAtEnd(string dialog, params Func<IEnumerator>[] events)
        {
            bool close = false;
            IEnumerator stop() { close = true; yield break; }
            Textbox textbox = new Textbox(dialog, [.. events.Prepend(stop)]);
            Engine.Scene.Add(textbox);
            while (!close)
            {
                yield return null;
            }
            yield return textbox.EaseClose(true);
            textbox.Close();
        }
        public override IEnumerator Cutscene(Level level)
        {
            IEnumerator zoomRoutine()
            {
                camSaveX = level.Camera.Position.X;
                Player.ForceCameraUpdate = true;
                yield return Level.ZoomTo(level.Marker("camera2", true), 1.4f, 1);
                Player.ForceCameraUpdate = false;
            }
            Add(new Coroutine(zoomRoutine()));
            yield return Player.DummyWalkTo(Level.Marker("player3").X);
            Player.Face(Calidus);
            //yield return Player.DummyWalkTo(level.Marker("player1").X);
            yield return CutAtEnd("CalidusPowerA", walkToMarker2, revealCalidus);
            yield return CutAtEnd("CalidusPowerB");
            yield return CutAtEnd("CalidusPowerC");
            Calidus.ReturnParts(2);
            Calidus.CanFloat = true;
            Calidus.FadeLight(1, 1);
            yield return CutAtEnd("CalidusPowerD", calidusLookRight, calidusLookPlayer, calidusLookUpRight, startMovingHands, increaseHandWobble, calidusPanic, stopHandWobble);

            lookCoroutine?.Cancel();
            Calidus.Look(Looking.Player);
            yield return Textbox.Say("CalidusPowerE", calidusLookRight, calidusLookPlayer, calidusLookDownLeft, calidusFly);
            yield return Level.ZoomBack(1);
            EndCutscene(Level);
        }
        private IEnumerator calidusFly()
        {
            Calidus.FixSequence();
            yield return 1;
            Calidus.CanFloat = true;
            Calidus.FloatToYNaive(-30);
        }
        private IEnumerator startMovingHands()
        {
            prevLeftArmOffset = Calidus.Arms[0].Offset.X;
            Add(leftWobbleRoutine = new Coroutine(wobbleArm(Calidus.Arms[0], 1)));
            yield return armWobbleDelay / 2f;
            prevRightArmOffset = Calidus.Arms[1].Offset.X;
            Add(rightWobbleRoutine = new Coroutine(wobbleArm(Calidus.Arms[1], 1)));
            yield return null;

        }
        private IEnumerator increaseHandWobble()
        {
            armWobbleDelay -= 0.2f;
            armWobbleAmount += 0.4f;
            yield return null;
        }
        private IEnumerator stopHandWobble()
        {
            leftWobbleRoutine?.Cancel();
            rightWobbleRoutine?.Cancel();
            Calidus.Arms[0].Offset.X = prevLeftArmOffset;
            Calidus.Arms[1].Offset.X = prevRightArmOffset;
            yield return null;
        }
        private Coroutine lookCoroutine, leftWobbleRoutine, rightWobbleRoutine;
        private float armWobbleDelay = 1;
        private float armWobbleAmount = 1;
        private float prevLeftArmOffset, prevRightArmOffset;
        private IEnumerator wobbleArm(Part part, int dir)
        {
            while (true)
            {
                yield return armWobbleDelay;
                part.Offset.X += dir * armWobbleAmount;
                yield return null;
                part.Offset.X -= dir * armWobbleAmount;
                yield return null;
            }

        }
        private IEnumerator calidusPanic()
        {
            Add(lookCoroutine = new Coroutine(lookRoutine()));
            yield return null;
        }
        private IEnumerator lookRoutine()
        {
            while (true)
            {
                Calidus.Look(Looking.Right);
                yield return 0.4f;
                Calidus.Look(Looking.Left);
                yield return 0.4f;
            }
        }
        private IEnumerator calidusLookRight()
        {
            Calidus.Look(Looking.Right);
            yield return null;
        }
        private IEnumerator calidusLookPlayer()
        {
            Calidus.Look(Looking.Player);
            yield return null;
        }
        private IEnumerator calidusLookUpRight()
        {
            Calidus.Look(Looking.UpRight);
            yield return null;
        }
        private IEnumerator calidusLookDownLeft()
        {
            Calidus.Look(Looking.DownLeft);
            yield return null;
        }
        private IEnumerator revealCalidus()
        {
            yield return 1f;
            yield return Player.Boop(1, 4);
            yield return 1f;
            Calidus.LookAt(Player);
            yield return 0.8f;
            Calidus.Emotion(Mood.Surprised);
            Add(new Coroutine(Player.DummyWalkTo(Level.Marker("player4").X, true, 3)));
        }
        private IEnumerator walkToMarker2()
        {
            yield return Player.DummyWalkTo(Level.Marker("player2").X);
        }
        private IEnumerator walkToMarker3()
        {
            yield return Player.DummyWalkTo(Level.Marker("player3").X);
        }
        //currently unused
        private IEnumerator PanToCalidus()
        {
            float from = Level.Camera.X;
            float to = camSaveX + (Level.Marker("camera2") - Level.Marker("camera1")).X;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                Level.Camera.Position = new Vector2(Calc.LerpClamp(from, to, Ease.SineInOut(i)), Level.Camera.Position.Y);
                yield return null;
            }
            yield return 0.8f;
        }
        private IEnumerator Reassemble()
        {
            yield return Calidus.ReassembleRoutine();
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


    }
}
