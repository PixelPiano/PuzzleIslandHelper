using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
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
    [CalidusCutscene("First")]
    public class FirstMeeting : CalidusCutscene
    {
        public FirstMeeting(Player player = null, Calidus calidus = null, Arguments start = null, Arguments end = null) : base(player, calidus, start, end)
        {

        }
        public override void OnBegin(Level level)
        {
            PianoModule.Session.TimesMetWithCalidus = 1;
            CalCut.FirstIntro.Register();
            base.OnBegin(level);
        }
        public override void OnEnd(Level level)
        {
            level.ResetZoom();
            Player player = Level.GetPlayer();
            /*            Calidus calidus = Level.Tracker.GetEntity<Calidus>();
                        if (calidus != null)
                        {
                            switch (Cutscene)
                            {
                                case CalCut.Second or CalCut.SecondTalkAboutWarp:
                                    calidus.Emotion(Calidus.Mood.Normal);
                                    calidus.StartFollowing(Calidus.Looking.Player);
                                    calidus.Talkable = false;
                                    calidus.RemoveTag(Tags.Global);
                                    break;
                                case CalCut.TalkAboutNote:
                                    calidus.Emotion(Calidus.Mood.Normal);
                                    calidus.Look(Calidus.Looking.DownRight);
                                    break;
                            }
                        }*/
            /*            if (CalCut.Second.GetCutsceneFlag())
                        {
                            SceneAs<Level>().Session.SetFlag("DigitalBetaWarpEnabled");
                        }*/
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StNormal;
            }
            if (WasSkipped)
            {
                Audio.PauseMusic = false;
                level.Session.SetFlag("blockGlitch", false);
                Glitch.Value = 0;
                Level.StopShake();
                if (player != null)
                {
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
/*                    switch (Cutscene)
                    {
                        case CalCut.Second:
                            if (Calidus != null)
                            {
                                Calidus.Look(Calidus.Looking.DownRight);
                                Calidus.CanFloat = true;
                                Calidus.Position = Calidus.OrigPosition;
                            }
                            break;
                    }*/
                }
            }
            base.OnEnd(level);
        }
        private IEnumerator First(Player player, Level level)
        {
            Coroutine lookAroundCoroutine = new Coroutine(false);
            Coroutine screenZoomAcrossRoutine = new Coroutine(false);
            bool lookingAround = false;
            Add(lookAroundCoroutine);
            level.InCutscene = true;
            while (!player.OnGround())
            {
                yield return null;
            }
            player.ForceCameraUpdate = false;
            Vector2 zoomPosition = new Vector2(145, player.Position.Y - level.LevelOffset.Y - 40);
            zoomPosition.X = Math.Max(0, zoomPosition.X);
            Coroutine zoomIn = new Coroutine(level.ZoomTo(new Vector2(145, player.Position.Y - level.LevelOffset.Y - 40), 1.5f, 2));
            Coroutine cameraTo = new Coroutine(CameraTo(level.LevelOffset + Vector2.UnitY * 8, 2, Ease.SineInOut));
            Coroutine walk = new Coroutine(player.DummyWalkTo(145 + level.Bounds.X));
            Add(zoomIn, cameraTo, walk);
            while (zoomIn.Active || cameraTo.Active || walk.Active)
            {
                yield return null;
            }
            yield return 0.2f;
            yield return player.DummyWalkTo(player.X + 16);
            yield return 1;
            yield return player.DummyWalkTo(player.X - 16);
            yield return 1;
            Vector2 focusPoint = Vector2.Zero;
            IEnumerator lookSideToSide(Player player)
            {
                lookingAround = true;
                while (lookingAround)
                {
                    player.Facing = Facings.Right;
                    yield return 0.5f;
                    player.Facing = Facings.Left;
                    yield return 0.5f;
                }
            }
            IEnumerator zoomAcross()
            {
                if (Level.Tracker.GetEntity<WarpCapsuleBeta>() is var machine)
                {
                    focusPoint = new Vector2(machine.CenterX - Level.Camera.X + 16, zoomPosition.Y);
                    screenZoomAcrossRoutine = new Coroutine(Level.ZoomAcross(focusPoint, 1.5f, 7));
                    Add(screenZoomAcrossRoutine);
                }
                yield return null;
            }
            yield return Textbox.Say("Calidus1", maddyStartle, zoomAcross, stopLookingSideToSide, waitZoom, calidusFix, wait1, wait3, quickLookRight, maddyWalkUp, glitch, warp, waithalfsecond);
            IEnumerator maddyStartle()
            {
                Audio.PauseMusic = true;
                yield return 0.1f;
                player.Jump();
                Add(new Coroutine(lookSideToSide(player)));
                yield return null;
            }
            IEnumerator stopLookingSideToSide()
            {
                lookingAround = false;
                yield return null;
            }
            IEnumerator waitZoom()
            {
                yield return 0.2f;
                if (screenZoomAcrossRoutine.Active)
                {
                    screenZoomAcrossRoutine.Cancel();
                    Remove(screenZoomAcrossRoutine);
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

        public override IEnumerator Cutscene(Level level)
        {
            yield return new SwapImmediately(First(Player, Level));
            EndCutscene(level);
        }
    }
}
