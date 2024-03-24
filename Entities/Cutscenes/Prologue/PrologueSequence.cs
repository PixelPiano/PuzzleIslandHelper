using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Celeste.Mod.CommunalHelper;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using FMOD.Studio;
using FrostHelper;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue
{
    public class PIPrologueSequence : CutsceneEntity
    {
        private class EndingCutsceneDelay : Entity
        {
            public EndingCutsceneDelay()
            {
                Add(new Coroutine(Routine()));
            }

            private IEnumerator Routine()
            {
                yield return 3f;
                (Scene as Level).CompleteArea(spotlightWipe: false, false, false);
            }
        }
        [Tracked]
        public class DuckForcer : Entity
        {
            public DuckForcer() : base()
            {

            }
        }
        private Player player;
        private float fadeOpacity = 1;
        private bool blackScreen;
        private PIGondola gondola;
        private bool stopUsingLever;
        private int Part;
        private List<CustomFlagExitBlock> blocks = new();
        public const string glitchEvent = "event:/PianoBoy/invertGlitch2";
        public PIPrologueSequence(int part)
            : base(fadeInOnSkip: false, endingChapterAfter: part >= 4)
        {
            Tag |= Tags.TransitionUpdate;
            Part = part;
            Depth = -100001;
            if (part == 2)
            {
                blackScreen = true;
            }
        }
        public void TeleportCleanup(Level level)
        {
            Level = level;
            Player player = level.GetPlayer();
            if (player is null) return;
            player.StateMachine.State = Player.StDummy;
            blackScreen = true;
            fadeOpacity = 1;
        }
        public override void OnBegin(Level level)
        {
            Level = level;
            gondola = level.Tracker.GetEntity<PIGondola>();
            Coroutine routine = Part switch
            {
                <= 1 => new Coroutine(Cutscene1()),
                2 => new Coroutine(Cutscene2()),
                3 => new Coroutine(Cutscene3()),
                >= 4 => new Coroutine(Cutscene4())
            };
            //Add(routine);
        }
        private static IDetour hook_Player_set_Ducking;
        private delegate bool orig_Player_set_Ducking(Player self);
        public static void Load()
        {
            On.Celeste.Input.GetAimVector += Input_GetAimVector;
            hook_Player_set_Ducking = new Hook(
            typeof(Player).GetProperty("Ducking").SetMethod,
            (Action<Player, bool> duck, Player player, bool v) =>
            {
                if (player is null) return;
                if (player.SceneAs<Level>().Tracker.GetEntity<DuckForcer>() is null)
                {
                    duck(player, v);
                }
                else
                {
                    duck(player, true);
                }
            });
        }
        public static void Unload()
        {
            On.Celeste.Input.GetAimVector -= Input_GetAimVector;
            hook_Player_set_Ducking?.Dispose();
        }
        private static Vector2 Input_GetAimVector(On.Celeste.Input.orig_GetAimVector orig, Facings defaultFacing)
        {
            if (Engine.Scene is not Level level || level.Tracker.GetEntity<DuckForcer>() is null)
            {
                return orig(defaultFacing);
            }
            return Vector2.UnitY;
        }

        public IEnumerator Cutscene1()
        {
            TeleportCleanup(Level);
            yield return 2f; //todo: add sounds of door opening, mailbox, picking up letter, door closing, shuffling paper, opening letter
            yield return Textbox.Say("prologue", GetLetter, ToGondola);
            yield return null;
        }

        public IEnumerator Cutscene2()
        {
            TeleportCleanup(Level);
            yield return 1;
            yield return Textbox.Say("prologue2", ToBoost, ProbeGlue, MirrorFlash, BlocksAppear);

        }
        public IEnumerator Cutscene3()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            player.StateMachine.State = Player.StDummy;
            this.player = player;
            player.ForceCameraUpdate = true;
            Coroutine sayLine = new Coroutine(Textbox.Say("prologue3a"));
            Add(sayLine);
            Coroutine runBack = new Coroutine(RunBackToGondola());
            Add(runBack);
            while (!runBack.Finished)
            {
                yield return null;
            }
            Coroutine fiddle = new Coroutine(FiddleWithGondola());
            Add(fiddle);
            while (!sayLine.Finished)
            {
                yield return null;
            }
            yield return Textbox.Say("prologue3b");
            stopUsingLever = true;
            while (!fiddle.Finished)
            {
                yield return null;
            }
            yield return BlocksSurroundGondola();
            yield return GetBlockd();
        }
        public IEnumerator Cutscene4()
        {
            TheWorldScene worldScene = new TheWorldScene("pTA", "pTB", "pTC", "pTD", "pTE");
            Level.Add(worldScene);
            while (worldScene.InCutscene)
            {
                yield return null;
            }
            EndCutscene(Level);
            yield return null;
        }
        public IEnumerator ProbeGlue()
        {
            yield return null;
        }
        public IEnumerator RunBackToGondola()
        {
            Coroutine routine = new Coroutine(player.AutoTraverse(Level.Marker("gondola").X, false, 2));
            Add(routine);
            yield return Level.ZoomTo(new Vector2(160, 90), 1.3f, 1);
            while (!routine.Finished)
            {
                yield return null;
            }
            yield return null;
        }
        public IEnumerator FiddleWithGondola()
        {

            float angle = -5f.ToRad();
            Vector2 position = player.Position;
            player.ForceCameraUpdate = false;
            while (!stopUsingLever)
            {
                gondola.Lever.Play("pulled");
                yield return null;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.2f)
                {
                    player.MoveToX(position.X - i * 2);
                    yield return null;
                }
                player.MoveToX(position.X - 2);
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.2f)
                {
                    player.MoveToX(position.X - 2 + i * 2);
                    yield return null;
                }
                player.MoveToX(position.X);
                yield return 0.1f;
                yield return null;
            }
        }
        public IEnumerator BlocksSurroundGondola()
        {
            List<PrologueBlock> blocks = new();
            foreach (PrologueBlock block in Level.Tracker.GetEntities<PrologueBlock>())
            {
                if (block.Order > 100)
                {
                    blocks.Add(block);
                }
            }
            foreach (PrologueBlock block in blocks.OrderByDescending(item => item.X))
            {
                block.Appear();
            }
            Add(new Coroutine(LookLoop()));
            yield return Textbox.Say("prologue3c");
            yield return null;
        }
        public IEnumerator LookLoop()
        {
            while (player is not null && !player.Dead)
            {
                player.Facing = Facings.Right;
                yield return 0.7f;
                player.Facing = Facings.Left;
                yield return 0.7f;
            }
        }
        public IEnumerator GetBlockd()
        {
            Level.Add(new PrologueKillBlock());
            yield return null;
        }
        public IEnumerator GetLetter()
        {
            yield return null;
        }
        public IEnumerator ToGondola()
        {
            yield return FadeOut();
            yield return Ascend();
        }

        public IEnumerator ToStone() //feels redundant 
        {
            while (!gondola.Finished)
            {
                yield return null;
            }
            yield return 1f;
            yield return player.DummyWalkTo(Level.Bounds.Width - 8);
            yield return null;
            yield return FadeStart();
            //InstantTeleport(level, player, "temple", false, gondolaStart.X + gondolaPlayerOffset, gondolaStart.Y + 52);
            yield return FadeEnd();
            yield return null;
        }
        public IEnumerator ToBoost()
        {
            yield return FadeEnd();
            Booster booster = Level.Entities.FindFirst<PrologueBooster>();
            player = Level.GetPlayer();
            if (booster is not null)
            {
                player.Center = booster.Center;
            }

            MInput.Disabled = true;
            player.StateMachine.Locked = false;
            while (!player.OnGround())
            {
                if (player.StateMachine.State == Player.StRedDash)
                {
                    player.Speed = Vector2.UnitY * -240f;
                    Input.Aim.Value = -Vector2.UnitY;
                    Input.MoveY.Value = -1;
                    Input.MoveX.Value = 0;
                }
                yield return null;
            }
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            MInput.Disabled = false;
            yield return ExamineMirror();
        }
        public IEnumerator ExamineMirror()
        {
            yield return player.AutoTraverse(Level.Marker("mirrorLeft").X);
            yield return 0.5f;
            yield return player.AutoTraverse(Level.Marker("mirror").X);
            yield return 1.5f;
            yield return player.AutoTraverseRelative(-16);
            yield return 1.5f;
            yield return player.AutoTraverseRelative(16);
            yield return Level.ZoomTo(Level.Marker("mirrorZoom", true), 2f, 1.2f);
        }
        public IEnumerator MirrorFlash()
        {
            TempleMirrorPI mirror = Level.Tracker.GetEntity<TempleMirrorPI>();
            if (mirror is null)
            {
                yield break;
            }
            //mirror flickers with exotic color
            Coroutine flash = new Coroutine(mirror.Shimmer());
            Add(flash);
            player.Facing.Flip();
            yield return null;
            player.Jump();
            yield return null;
            while (!player.OnGround())
            {
                yield return null;
            }
            player.ForceCameraUpdate = false;
            Add(new Coroutine(Level.ZoomAcross(Level.Marker("cameraScared", true), 1.9f, 1.2f)));
            //madeline runs for cover
            yield return player.DummyWalkTo(Level.Marker("mirrorRight").X, false, 2);
            player.Facing = Facings.Left;
            //Madeline ducks beneath the bottom of the camera
            DuckForcer helper = new DuckForcer();
            Level.Add(helper);
            //nothing happens
            yield return 2f;
            //madeline pokes her head up from below the screen
            Level.Remove(helper);
            //madeline waits for a bit and then approaches the mirror again
            yield return 4f;
            yield return player.AutoTraverseRelative(-8);
            yield return 1;
            yield return player.AutoTraverse(Level.Marker("mirrorNervous").X);
            yield return 2;
            Add(new Coroutine(player.DummyWalkTo(player.X + 8, true, 2)));
            //sparks in center of mirror
            //yield return mirror.Sparks();
            //Camera zoom in slowly
            yield return 1f;
            Level.Session.SetFlag("mirrorBlockShatter");
            yield return null;
            //yield return mirror.Shatter();

            //block appears in center, inside of mirror
            //madeline exclaims
            //more blocks appear
            //madeline panics
            yield return null;
        }
        public IEnumerator BlocksAppear()
        {
            List<Entity> Blocks = Level.Tracker.GetEntities<PrologueGlitchBlock>();
            DashBlock dashBlock = Level.Tracker.GetEntity<DashBlock>();
            bool broke = false;
            foreach (Entity block in Blocks)
            {
                /*if(block.ActivationID == "first"){*/

                (block as PrologueGlitchBlock).Activate();
                yield return 0.4f;
                //}
            }
            yield return 5f;
            foreach (Entity block in Blocks)
            {
                /*if(block.ActivationID == "second"){*/

                if (dashBlock is not null && (block as PrologueGlitchBlock).CollideCheck(dashBlock))
                {
                    if (!broke)
                    {
                        dashBlock.Break(dashBlock.Center, Vector2.UnitY, true);
                        player.Facing = Facings.Right;
                        broke = true;
                    }
                }
                (block as PrologueGlitchBlock).Activate();
                yield return 0.4f;
                //}
            }
            yield return null;
        }
        public IEnumerator Abscond()
        {
            //sick reference dude
            while (player is null || player.Dead || !gondola.Collider.Bounds.Contains(player.Position.ToPoint()))
            {
                yield return null;
            }
            player.DummyWalkTo(gondola.Center.X, false, 2);
            yield return null;
            yield return null;
        }
        public IEnumerator FadeToTemple()
        {
            yield return FadeStart();
            //InstantTeleport(level, player, "temple", false, gondolaStart.X + gondolaPlayerOffset, gondolaStart.Y + 52);
            yield return FadeEnd();
            while (player is null || player.Dead || !gondola.Collider.Bounds.Contains(player.Position.ToPoint()))
            {
                yield return null;
            }
            player.DummyWalkTo(gondola.Center.X, false, 2);
            yield return null;
        }
        public static void InstantTeleport(Scene scene, Player player, string room)
        {
            if (scene is not Level level)
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
                level.Camera.Position = level.LevelOffset + val3;
                level.Add(player);
                player.Position = level.DefaultSpawnPoint;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);

                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }
        public IEnumerator FadeStart(float time = 1)
        {
            blackScreen = true;
            fadeOpacity = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                fadeOpacity = Calc.LerpClamp(0, 1, i);
                yield return null;
            }
            fadeOpacity = 1;
        }
        public IEnumerator FadeEnd(float time = 1)
        {
            blackScreen = true;
            fadeOpacity = 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                fadeOpacity = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            fadeOpacity = 0;
            blackScreen = false;
        }
        public IEnumerator FadeOut()
        {
            blackScreen = true;
            fadeOpacity = 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                fadeOpacity = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            fadeOpacity = 0;
            blackScreen = false;
        }
        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {

            }
            //Engine.TimeRate = 1f;
            level.PauseLock = true;
            level.Entities.FindFirst<SpeedrunTimerDisplay>().CompleteTimer = 10f;
            level.Add(new EndingCutsceneDelay());
        }
        public override void Render()
        {
            if (blackScreen)
            {
                Draw.Rect(Level.Camera.GetBounds(), Color.Black * fadeOpacity);
            }
            base.Render();
        }
        public enum GondolaStates
        {
            MovingToEnd,
            Stopped
        }

        public SoundSource moveLoopSfx;


        public float gondolaPercent;


        public float playerXOffset;

        public float gondolaSpeed;

        public float shakeTimer;

        public bool Finished;

        public const float gondolaMaxSpeed = 64f;

        public GondolaStates gondolaState;
        public bool InGondola;
        public IEnumerator Ascend()
        {
            if (player is null)
            {
                player = Level.GetPlayer();
            }
            InGondola = true;
            Add(moveLoopSfx = new SoundSource());
            moveLoopSfx.Play("event:/game/04_cliffside/gondola_movement_loop");
            gondolaState = GondolaStates.MovingToEnd;
            Coroutine text = new Coroutine(Textbox.Say("prologueGondolaRide"));
            Add(text);
            player.ForceCameraUpdate = true;
            while (gondolaState != GondolaStates.Stopped)
            {
                yield return null;
            }

            Level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            moveLoopSfx.Stop();
            Audio.Play("event:/game/04_cliffside/gondola_finish", gondola.Position);
            gondola.RotationSpeed = 0.5f;
            yield return 0.1f;
            while (gondola.Rotation > 0f)
            {
                yield return null;
            }

            gondola.Rotation = gondola.RotationSpeed = 0f;
            Level.Shake();
            player.StateMachine.State = 11;
            player.Position = player.Position.Floor();
            while (player.CollideCheck<Solid>())
            {
                player.Y--;
            }
            while (!text.Finished)
            {
                yield return null;
            }
            InGondola = false;
            player.DummyAutoAnimate = true;
            Coroutine dummyWalk;
            Add(dummyWalk = new Coroutine(player.DummyWalkTo(840f)));
            while (player.Position.X < 16)
            {
                yield return null;
            }
            yield return FadeStart();
            dummyWalk.Cancel();
            Remove(dummyWalk);
            InstantTeleport(Level, Level.GetPlayer(), "mirror");
            Level = SceneAs<Level>();
            player = Level.GetPlayer();
            yield return FadeEnd();
        }
        public override void Update()
        {
            base.Update();
            if (!InGondola)
            {
                return;
            }
            if (moveLoopSfx != null && gondola != null)
            {
                moveLoopSfx.Position = gondola.Position;
            }
            if (gondolaState == GondolaStates.MovingToEnd)
            {
                MoveGondolaTowards(1f);
                if (gondolaPercent >= 1f)
                {
                    gondolaState = GondolaStates.Stopped;
                }
            }
            player ??= (Scene as Level).GetPlayer();
            if (player is not null)
            {
                player.Position = gondola.GetRotatedFloorPositionAt(gondola.Width / 4);
            }
        }


        public void MoveGondolaTowards(float percent)
        {
            float num = (gondola.Start - gondola.Destination).Length();
            gondolaSpeed = Calc.Approach(gondolaSpeed, 64f, 120f * Engine.DeltaTime);
            gondolaPercent = Calc.Approach(gondolaPercent, percent, gondolaSpeed / num * Engine.DeltaTime);
            gondola.Position = (gondola.Start + (gondola.Destination - gondola.Start) * gondolaPercent).Floor();
        }
    }
}

