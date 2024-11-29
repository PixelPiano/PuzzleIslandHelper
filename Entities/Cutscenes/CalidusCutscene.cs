using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{

    [Tracked]
    public class CalidusCutscene : CutsceneEntity
    {
        public enum Cutscenes
        {
            FirstIntro,
            SecondIntro,
            First,
            Second,
            SecondA,
            Third,
            TEST
        }
        public Cutscenes Cutscene;
        private Calidus Calidus;
        public Aura aura;
        public CalidusCutscene(Cutscenes cutscene)
            : base()
        {
            Cutscene = cutscene;
        }

        [TrackedAs(typeof(ShaderOverlay))]
        public class Aura : ShaderOverlay
        {
            public bool Strong;
            public float Random;
            private Player player;
            private bool forcedVisibleState;
            public Aura() : base("PuzzleIslandHelper/Shaders/glitchAura", "", true)
            {
            }
            public override void Update()
            {
                player = Scene.GetPlayer();
                if (player != null)
                {
                    player.Visible = forcedVisibleState;
                }
                base.Update();
            }
            public override void BeforeApply()
            {
                base.BeforeApply();
                if (player is null) return;
                player.Visible = false;
            }
            public override void AfterApply()
            {
                base.AfterApply();
                if (player is null) return;
                Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
                GameplayRenderer.Begin();
                player.Render();
                Draw.SpriteBatch.End();
            }
            public override bool ShouldRender()
            {
                return Amplitude >= 0 && base.ShouldRender();
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                if (player != null)
                {
                    player.Visible = true;
                }
            }

            public override void ApplyParameters()
            {
                base.ApplyParameters();
                if (Scene is not Level level || player is null || Effect is null || Effect.Parameters is null) return;
                Effect.Parameters["Strong"]?.SetValue(Strong);
                Effect.Parameters["Center"]?.SetValue((player.Center - level.Camera.Position) / new Vector2(320, 180));
                Effect.Parameters["Size"]?.SetValue(0.01f);
                Effect.Parameters["Random"]?.SetValue(Calc.Random.Range(1, 100f));
            }

        }
        public void SetCutsceneFlag()
        {
            Level.Session.SetFlag("CalidusCutscene" + Cutscene.ToString() + "Watched");
        }
        public static bool GetCutsceneFlag(Scene scene, Cutscenes cutscene)
        {
            return (scene as Level).Session.GetFlag("CalidusCutscene" + cutscene.ToString() + "Watched");
        }
        public override void OnBegin(Level level)
        {
            Player player = level.GetPlayer();

            if (GetCutsceneFlag(level, Cutscene))
            {
                EndCutscene(Level);
                return;
            }
            switch (Cutscene)
            {
                case Cutscenes.FirstIntro:
                    Add(new Coroutine(GlitchOut(player, level)));
                    break;
                case Cutscenes.First:
                    PianoModule.Session.TimesMetWithCalidus = 1;
                    Add(new Coroutine(Cutscene1(player, level)));
                    break;
                case Cutscenes.SecondIntro:
                    Add(new Coroutine(Intro2(player, level)));
                    break;
                case Cutscenes.Second:
                    PianoModule.Session.TimesMetWithCalidus = 2;
                    Add(new Coroutine(Cutscene2(player, level)));
                    break;
                case Cutscenes.SecondA:
                    PianoModule.Session.TimesMetWithCalidus = 2;
                    Add(new Coroutine(Cutscene2B(player, level)));
                    break;
                case Cutscenes.Third:
                    PianoModule.Session.TimesMetWithCalidus = 3;
                    Add(new Coroutine(Cutscene3(player, level)) { UseRawDeltaTime = true });
                    break;
                case Cutscenes.TEST:
                    Add(new Coroutine(TestScene(player, Calidus)));
                    break;
            }
        }
        private IEnumerator TestScene(Player player, Calidus calidus)
        {
            player.StateMachine.State = Player.StDummy;
            yield return Textbox.Say("CalidusTest");
            player.StateMachine.State = Player.StNormal;

            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            if (Cutscene != Cutscenes.TEST)
            {
                SetCutsceneFlag();
            }
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
                    case Cutscenes.Third:
                        break;
                    case Cutscenes.TEST:
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
                Level.StopShake();
                if (TagCheck(Tags.Global))
                {
                    RemoveTag(Tags.Global);
                }
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
        }
        private IEnumerator Intro2(Player player, Level level)
        {
            level.InCutscene = true;
            Add(new Coroutine(GlitchOut(player, level)));
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
        private IEnumerator Intro3(Player player, Level level)
        {
            level.InCutscene = true;
            Add(new Coroutine(GlitchOut(player, level)));
            Vector2 playerMarker = level.Marker("player");
            Vector2 ZoomPosition = level.Marker("camera1") - level.LevelOffset;
            Coroutine zoomIn = new Coroutine(ScreenZoom(ZoomPosition, 1.5f, 2));
            Coroutine walkTo = new Coroutine(player.DummyWalkTo(playerMarker.X));
            Add(zoomIn);
            yield return 1.4f;
            Add(walkTo);
            while (zoomIn.Active || walkTo.Active)
            {
                yield return null;
            }
            Calidus.FloatLerp(level.Marker("calidus"), 2, Ease.SineOut);
        }
        private IEnumerator Dialogue3(Player player, Level level)
        {
            yield return Textbox.Say("CalidusCutscene3",
                Normal, Stern, Surprised, Angry, WaitForOne, AngryApproach, ApproachPlayer, TugOnBook, LetGoOfBook, WaitForTwo);
        }
        private IEnumerator AngryApproach()
        {
            Calidus?.FloatLerp(Calidus.Position - Vector2.UnitX * 16, 0.1f);
            yield return null;
        }
        private IEnumerator ApproachPlayer()
        {
            if (Level.GetPlayer() is Player player && Calidus != null)
            {
                Calidus.CanFloat = false;
                yield return Calidus.FloatLerpRoutine(player.CenterRight + new Vector2(Calidus.OrbSprite.Width, -Calidus.Height / 2), 1.5f, Ease.CubeOut);
            }
            yield return null;
        }
        private bool tuggingOnBook;
        private bool freeToLetGo;
        private IEnumerator TugOnBook()
        {
            if (Level.GetPlayer() is Player player && Calidus != null)
            {
                Add(new Coroutine(tugRoutine(player, Calidus)));
            }
            yield return null;
        }
        private IEnumerator tugRoutine(Player player, Calidus calidus)
        {
            tuggingOnBook = true;
            float tugDist = 5;
            float singleTugTime = 0.3f;
            while (tuggingOnBook)
            {
                float pFrom = player.Position.X;
                float cFrom = calidus.Position.X;
                float time = singleTugTime + Calc.Random.Range(-0.1f, 0.1f);
                Tween tug = Tween.Create(Tween.TweenMode.YoyoOneshot, Ease.Linear, time, true);
                tug.OnUpdate = t =>
                {
                    player.Position.X = Calc.LerpClamp(pFrom, pFrom + tugDist, t.Eased);
                    calidus.Position.X = Calc.LerpClamp(cFrom, cFrom + tugDist, t.Eased);
                };
                Add(tug);
                yield return time * 2;
                player.Position.X = pFrom;
                calidus.Position.X = cFrom;
                yield return Calc.Random.Choose(0.1f, 0.5f);
            }
            freeToLetGo = true;
        }
        private IEnumerator LetGoOfBook()
        {
            if (Level.GetPlayer() is Player player && Calidus != null)
            {
                tuggingOnBook = false;
                while (!freeToLetGo) yield return null;
                yield return 0.4f;
                float pFrom = player.Position.X;
                float cFrom = Calidus.Position.X;

                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
                {
                    player.Position.X = Calc.LerpClamp(pFrom, pFrom - 4, Ease.SineIn(i));
                    Calidus.Position.X = Calc.LerpClamp(cFrom, cFrom + 4, Ease.SineIn(i));
                    yield return null;
                }
                pFrom = player.Position.X;
                cFrom = Calidus.Position.X;
                Tween shake = Tween.Create(Tween.TweenMode.YoyoLooping, null, 0.2f, true);
                shake.OnUpdate = t =>
                {
                    player.Position.X = pFrom + Calc.LerpClamp(-1, 1, t.Eased);
                    Calidus.Position.X = cFrom + Calc.LerpClamp(-1, 1, t.Eased);
                };
                Add(shake);
                yield return 2;
                Remove(shake);
                Coroutine text = new Coroutine(Textbox.Say("maddyScream"));
                Add(text);
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.4f)
                {
                    player.Position.X = pFrom + Calc.LerpClamp(0, -24, Ease.CubeInOut(i));
                    Calidus.Position.X = cFrom + Calc.LerpClamp(0, 24, Ease.CubeInOut(i));
                    yield return null;
                }
                while (!text.Finished) yield return null;
                yield return 0.1f;
            }

            yield return null;
        }
        private IEnumerator Dialogue3Beta(Player player, Level level)
        {
            yield return Calidus.Say("Cc1", "normal");
            Calidus.LookDir = Calidus.Looking.Right;
            yield return Calidus.Say("Cc2", "stern");
            yield return 0.1f;
            Calidus.RollEye();
            yield return 0.6f;
            yield return Textbox.Say("Cc3");
            Calidus.LookDir = Calidus.Looking.Left;
            Calidus.Emotion(Calidus.Mood.Stern);
            List<string> choices = new() { "Ccq1", "Ccq2", "Ccq3", "Ccq4" };
            while (choices.Count > 0)
            {
                yield return ChoicePrompt.Prompt(choices.ToArray());
                yield return Textbox.Say(choices[ChoicePrompt.Choice]);
                yield return Textbox.Say(choices[ChoicePrompt.Choice] + "a");
                choices.RemoveAt(ChoicePrompt.Choice);
            }
            yield return Textbox.Say("Cc4");
            Calidus.Symbols.Visible = false;
            yield return Calidus.Say("Cc5", "eugh");
            //Calidus.Symbols.Offset.X += Calidus.OrbSprite.Width;
            yield return Calidus.Say("Cc6", "angry");
            Calidus.Symbols.Visible = false;
            //Calidus.Symbols.Offset.X -= Calidus.OrbSprite.Width / 2;
            yield return Calidus.Say("Cc7", "surprised", WaitForOne, Approach, BackOff);
            yield return Calidus.Say("Cc8", "stern", AuraSmall, Approach);
            yield return Calidus.Say("Cc9", "surprised", AuraMedium, WalkLeft);
            yield return Calidus.Say("Cc10", "stern");
            yield return Textbox.Say("Cc11", AuraLarge, WalkRight, FlashIn);
        }
        private IEnumerator FlashIn()
        {
            BlackFlash flash = new BlackFlash();
            Scene.Add(flash);
            yield return flash.FlashIn(0.5f);
        }
        private IEnumerator FlashOut()
        {
            if (Scene.Tracker.GetEntity<BlackFlash>() is BlackFlash flash)
            {
                yield return flash.FlashOut(0.5f);
            }
            yield return null;
        }
        [Tracked]
        public class BlackFlash : Entity
        {
            private float whiteAlpha;
            private float blackAlpha;
            public BlackFlash() : base()
            {
                Depth = -100000;
            }
            public override void Render()
            {
                base.Render();
                if (Scene is not Level level) return;
                if (blackAlpha > 0)
                {
                    Draw.Rect(level.Camera.Position, 320, 180, Color.Black * blackAlpha);
                }
                if (whiteAlpha > 0)
                {
                    Draw.Rect(level.Camera.Position, 320, 180, Color.White * whiteAlpha);
                }
            }
            public IEnumerator FlashIn(float whiteTime)
            {
                blackAlpha = 0;
                whiteAlpha = 0;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.07f)
                {
                    whiteAlpha = Calc.LerpClamp(0, 1f, i);
                    yield return null;
                }
                whiteAlpha = 1;
                blackAlpha = 1;
                for (float i = 0; i < 1; i += Engine.DeltaTime / whiteTime)
                {
                    whiteAlpha = Calc.LerpClamp(1f, 0, Ease.SineIn(i));
                    yield return null;
                }
                whiteAlpha = 0;
                yield return null;
            }
            public IEnumerator FlashOut(float whiteTime)
            {
                blackAlpha = 1;
                whiteAlpha = 0;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.07f)
                {
                    whiteAlpha = Calc.LerpClamp(0, 1f, i);
                    yield return null;
                }
                whiteAlpha = 1;
                blackAlpha = 0;
                for (float i = 0; i < 1; i += Engine.DeltaTime / whiteTime)
                {
                    whiteAlpha = Calc.LerpClamp(1f, 0, Ease.SineIn(i));
                    yield return null;
                }
                whiteAlpha = 0;
                RemoveSelf();
                yield return null;
            }
        }
        private IEnumerator FlashBlackIn()
        {
            yield return null;
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
        private IEnumerator GlitchCutscene(Player player, Level level)
        {

            //Calidus.MoveTo(level.Marker("calidus"));
            //todo: add custom burst effect
            //include invert effects, pixel duplication, other weird effects
            AuraAmount = 1;
            AuraStrong = true;
            ShakeLevel = true;
            //level flashes, camera snaps back to default view
            Level.Flash(Color.White);
            Level.ResetZoom();
            Calidus.StartShaking();
            Calidus.Emotion(Calidus.Mood.Surprised);
            //instantly madeline is floating in the air, mildly convulsing
            player.Position = level.Marker("playerFloating");
            player.DummyGravity = false;
            player.DummyAutoAnimate = true;

            //calidus looks around worriedly
            //displacement bursts at intervals, span length of whole screen
            yield return 0.1f;
            //Calidus.LookAtPlayer = true;
            yield return 1;
            yield return Calidus.Say("Cc12", "surprised", FlashOut);
            yield return 0.9f;
            SwapNext(Color.White * 0.3f, player);
            LargePulse();
            yield return 0.9f;
            SwapNext(Color.White * 0.3f, player);
            LargePulse();
            yield return 1.4f;
            SwapNext(Color.White * 0.3f, player);
            LargePulse();
            yield return 0.5f;
            //Calidus.LookDir = Calidus.Looking.Right;
            //Calidus.LookAtPlayer = false;
            Calidus.FloatLerp(Calidus.Position + Vector2.UnitX * 16, 5);
            float from = Engine.TimeRate;
            InvertOverlay.HoldState = true;
            //time slows down, as it gets to 0 the bursts Start reversing back to madeline
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / 1.3f)
            {
                Engine.TimeRate = Calc.LerpClamp(from, 0, Ease.SineInOut(i));
                yield return null;
            }
            yield return 0.2f;
            float maxTime = 1;
            int bursts = 12;
            float decay = 0.6f;
            for (int i = 0; i < bursts; i++)
            {
                float progress = (float)i / bursts;
                float time = (1 - progress) * (1 - (decay * progress)) * maxTime;
                ConstantTimeBurst.AddBurst(player.Position, time, 320, 0);
                yield return time;
            }
            //once they all return to her, short glitchy sequence, then a very large glitchy blast
            //madeline and calidus both sent flying in opposite directions, glitch a bit and then disappear
            ConstantTimeBurst.AddBurst(player.Center, 2, 0, 320, 1);
            float interval = 0.2f;
            for (int i = 0; i < 20; i++)
            {
                SwapNext(null, player);
                yield return interval;
                interval *= 0.9f;
                if (interval < Engine.DeltaTime) interval = Engine.DeltaTime;
            }
            player.Visible = false;
            Calidus.Visible = false;
            //QuickGlitch playerGlitch = new QuickGlitch(player, Vector2.One * 4, Engine.DeltaTime, 8, 0.8f);
            //QuickGlitch calidusGlitch = new QuickGlitch(Calidus, Vector2.One * 4, Engine.DeltaTime, 5, 0.8f);
            //Level.Add(playerGlitch, calidusGlitch);
            while (Level.Tracker.GetEntities<QuickGlitch>().Count > 0)
            {
                yield return null;
            }
            yield return null;
        }
        private IEnumerator Cutscene1(Player player, Level level)
        {
            level.InCutscene = true;
            Vector2 ZoomPosition = new Vector2(113, player.Position.Y - level.LevelOffset.Y - 40);
            Coroutine zoomIn = new Coroutine(ScreenZoom(ZoomPosition, 1.5f, 2));
            Coroutine walkTo = new Coroutine(player.DummyWalkTo(113 + level.Bounds.X));
            Add(zoomIn);
            Add(walkTo);
            while (zoomIn.Active || walkTo.Active)
            {
                yield return null;
            }
            yield return 0.2f;
            yield return Walk(16);
            yield return 1;
            yield return Walk(-16);
            yield return 1;
            yield return SayAndWait("Ca1", 0.6f);
            yield return 0.6f;
            Audio.PauseMusic = true;
            player.Jump();
            Add(new Coroutine(LookSideToSide(player)));
            yield return Textbox.Say("Ca2");
            Coroutine ScreenZoomAcrossRoutine = new Coroutine(SceneAs<Level>().ZoomAcross(ZoomPosition + Vector2.UnitX * 32, 1.5f, 7));
            Add(ScreenZoomAcrossRoutine);
            yield return Textbox.Say("Ca3");
            yield return Textbox.Say("Ca4");
            LookingSideToSide = false;
            yield return SayAndWait("Ca5", 0.2f);
            if (ScreenZoomAcrossRoutine.Active)
            {
                ScreenZoomAcrossRoutine.Cancel();
                Remove(ScreenZoomAcrossRoutine);
                level.ZoomSnap(ZoomPosition + Vector2.UnitX * 32, 1.5f);
            }
            Add(new Coroutine(PlayerZoomAcross(player, 2f, 2, 32, -32)));
            yield return Textbox.Say("Ca5a");
            Calidus.BrokenParts.Play("jitter");
            yield return 1.4f;
            Calidus.FixSequence();
            while (Calidus.Broken)
            {
                yield return null;
            }
            yield return null;
            Calidus.LookSpeed /= 5;
            Calidus.LookDir = Calidus.Looking.Left;
            player.Facing = Facings.Right;
            Add(new Coroutine(Events(Wait(0.5f), Walk(-16, true))));
            yield return Textbox.Say("Ca6");
            yield return Textbox.Say("Ca7");
            yield return Calidus.Say("Ca8", "stern");
            yield return Textbox.Say("Ca9");
            yield return Calidus.Say("Ca10", "normal");
            yield return Textbox.Say("Ca11");
            yield return 1;
            yield return Textbox.Say("Ca12");
            yield return 3;
            yield return Textbox.Say("Ca13");
            yield return Calidus.Say("Ca14", "surprised");
            yield return Calidus.Say("Ca15", "happy");
            yield return Textbox.Say("Ca16");
            Calidus.LookSpeed *= 5;
            Calidus.LookDir = Calidus.Looking.Right;
            yield return Calidus.Say("Ca16a", "stern");
            Add(new Coroutine(Walk(16, false, 2)));
            Calidus.LookDir = Calidus.Looking.Left;
            Calidus.Surprised(false);
            yield return Calidus.Say("Ca17", "surprised");
            yield return 0.5f;
            yield return Calidus.Say("Ca18", "normal");


            //Rumble, glitchy effects
            //todo: add sound
            level.Session.SetFlag("blockGlitch");
            yield return 0.1f;
            //shake level somehow
            yield return 0.1f;
            Calidus.Surprised(true);
            Calidus.LookDir = Calidus.Looking.Up;
            yield return 0.7f;
            Calidus.LookDir = Calidus.Looking.Left;
            yield return Calidus.Say("Ca19", "surprised");
            Calidus.Emotion("stern");
            yield return Textbox.Say("Ca20");

            Vector2 pos = Calidus.Position;
            AddTag(Tags.Global);
            for (float i = 0; i < 1; i += Engine.DeltaTime * 2)
            {
                Calidus.Position = Vector2.Lerp(pos, player.TopCenter, Ease.BackIn(i));
                yield return null;
            }
            level.Flash(Color.White, true);
            level.Session.SetFlag("blockGlitch", false);
            SingleTextscene text = new SingleTextscene("CaL1");
            level.Add(text);
            Level.StopShake();
            while (text.InCutscene)
            {
                yield return null;
            }
            yield return End(player, level);
        }
        private IEnumerator Cutscene2(Player player, Level level)
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
                PlayerToMarker5, PlayerToMarkerCalidus, PanToCalidus, PlayerToMarker6, PlayerFaceLeft, PlayerFaceRight); //30 - 33
            yield return Level.ZoomBack(1);
            EndCutscene(Level);
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
        private IEnumerator Cutscene2B(Player player, Level level)
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

        private IEnumerator Cutscene3Beta(Player player, Level level)
        {
            level.Add(aura = new Aura());
            player.Position = level.DefaultSpawnPoint;
            level.Camera.Position = level.LevelOffset;
            Calidus.LookDir = Calidus.Looking.Left;
            yield return Intro3(player, level);
            yield return Dialogue3Beta(player, level);
            yield return GlitchCutscene(player, level);
            EndCutscene(level);
            yield return null;
        }
        private IEnumerator Cutscene3(Player player, Level level)
        {
            player.Position = level.DefaultSpawnPoint;
            level.Camera.Position = level.LevelOffset;
            Calidus.LookDir = Calidus.Looking.Left;
            yield return Intro3(player, level);
            yield return Dialogue3Beta(player, level);
            yield return GlitchCutscene(player, level);
            EndCutscene(level);
            yield return null;
        }
        public void AddGlitches(Player player, Calidus calidus)
        {
            Level.Add(new QuickGlitch(player, new Range2(2, 5, 2, 5), Vector2.One * 8, Engine.DeltaTime, 8, 0.4f));
            Level.Add(new QuickGlitch(calidus, new Range2(2, 5, 2, 5), Vector2.Zero, Engine.DeltaTime, 8, 0.4f) { Offset = -Vector2.UnitX * 8 });
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (Cutscene == Cutscenes.Third)
            {
                if (aura is not null)
                {
                    scene.Remove(aura);
                }
            }
        }
        private IEnumerator CaliDramaticFloatAway()
        {
            Calidus.FloatLerp(Calidus.Position + Vector2.UnitX * 24, 2, Ease.SineInOut);
            yield return null;
        }
        private IEnumerator CaliDramatiLookUpRight()
        {
            Calidus.LookDir = Calidus.Looking.UpRight;
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

        /*      public enum Mood
        {
            Happy,
            Stern,
            Normal,
            RollEye,
            Laughing,
            Shakers,
            Nodders,
            Closed,
            Angry,
            Surprised,
            Wink,
            Eugh
        }*/
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
        private Vector2 caliMoveBackPosition;
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
            if (Cutscene is Cutscenes.FirstIntro)
            {
                Add(new Coroutine(StutterGlitch(20)));
                Vector2 pos = player.Center - Level.Camera.Position - Vector2.UnitY * 24;
                yield return Textbox.Say("wtc1", PanOut, WaitForPanOut);
                yield return Level.ZoomBack(0.8f);
                EndCutscene(Level);
            }
            else
            {
                yield return StutterGlitch(20);
            }
        }
        private bool panningOut;
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
        public IEnumerator SwapRoutine(int times, float interval, Color? flash, Player player)
        {
            for (int i = 0; i < times; i++)
            {
                SwapNext(flash, player);
                yield return interval;
            }
        }
        public void SwapNext(Color? flash, Player player)
        {
            SwapSection((Section % 4) + 1, Level, flash, player);
            AddGlitches(player, Calidus);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.3f, true);
            tween.OnUpdate = (Tween t) =>
            {
                Glitch.Value = 0.4f - t.Eased * 0.4f;
            };
            tween.OnComplete = delegate
            {
                Glitch.Value = 0;
            };
            Add(tween);
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
        public int Section = 1;
        public int PrevSection = 1;

        public void SwapSection(int section, Level level, Color? flashColor, Player player)
        {
            PrevSection = Section;
            Section = section;
            Vector2 moveBy = level.Marker("camera" + Section) - level.Marker("camera" + PrevSection);
            player.Position += moveBy;
            //player.Hair.MoveHairBy(moveBy);
            Calidus.Position += moveBy;
            level.Camera.Position += moveBy;

            if (flashColor.HasValue)
            {
                level.Flash(flashColor.Value);
            }
        }
        private IEnumerator SmallBurst()
        {
            Player player = Level.GetPlayer();
            Level.Displacement.AddBurst(player.Center, 0.3f, 0, 40, 1);
            Calidus.FloatLerp(Calidus.Position + Vector2.UnitX * 4, 0.3f);
            yield return null;
        }
        private IEnumerator WalkLeft()
        {
            Player player = Level.GetPlayer();
            yield return player.DummyWalkTo(player.X - 40);
        }
        private IEnumerator WalkRight()
        {
            Player player = Level.GetPlayer();
            yield return player.DummyWalkTo(player.X + 40);
        }
        private IEnumerator Approach()
        {
            Player player = Level.GetPlayer();
            Add(new Coroutine(player.DummyWalkTo(player.X + 8, false, 2)));
            yield return 0.07f;
            Calidus.FloatLerp(Calidus.Position + Vector2.UnitX * 6, 0.3f);
            yield return null;
        }
        private IEnumerator MoveCamera(Vector2 amount, float time)
        {
            Vector2 pos = Level.Camera.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Level.Camera.Position = Vector2.Lerp(pos, pos + amount, i);
                yield return null;
            }
            yield return null;
        }
        private IEnumerator BackOff()
        {
            Player player = Level.GetPlayer();
            yield return player.DummyWalkTo(player.X - 16, true, 1.4f);
            yield return 0.5f;
            Calidus.FloatLerp(Calidus.Position - Vector2.UnitX * 6, 1);
            yield return 0.2f;
            player.Facing = Facings.Left;
            yield return 1;
        }
        private float AuraAmount;
        private bool AuraStrong;
        private bool ShakeLevel;
        public override void Update()
        {
            base.Update();
            if (aura != null)
            {
                aura.Amplitude = AuraAmount;
                aura.Strong = AuraStrong;
            }
            if (ShakeLevel)
            {
                Level.shakeTimer = Engine.DeltaTime;
            }
            //AuraAmount = (float)Math.Sin(Scene.TimeActive);
        }
        private IEnumerator AuraGrowTo(float to, float time)
        {
            float from = AuraAmount;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                AuraAmount = Calc.LerpClamp(from, to, Ease.QuintInOut(i));
                yield return null;
            }
            AuraAmount = to;
        }
        private IEnumerator AuraSmall()
        {
            Add(new Coroutine(AuraGrowTo(0.1f, 0.6f)));
            yield return null;
        }
        private IEnumerator AuraMedium()
        {
            Add(new Coroutine(AuraGrowTo(0.3f, 0.6f)));
            yield return null;
        }
        private IEnumerator AuraLarge()
        {
            Add(new Coroutine(AuraGrowTo(0.5f, 0.6f)));
            yield return null;
        }
        private void LargePulse()
        {
            Player player = Level.GetPlayer();
            Level.Flash(Color.White * 0.5f);
            Level.Displacement.AddBurst(player.Center, 1f, 0, 150, 1);
            Calidus.FloatLerp(Calidus.Position + Vector2.UnitX * 6, 0.3f);
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
        private bool LookingSideToSide = true;
        private IEnumerator PlayerZoom(Player player, float amount, float time, float xOffset, float yOffset)
        {
            Level level = SceneAs<Level>();
            yield return level.ZoomTo(ScreenCoords(player.Position + new Vector2(xOffset, yOffset), level), amount, time);
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
        private bool CalidusLookAround = true;
        private float? musicVolume;
        private IEnumerator LookBackAndForth()
        {
            while (CalidusLookAround)
            {
                Calidus.LookDir = Calidus.Looking.UpRight;
                yield return 1;
                Calidus.LookDir = Calidus.Looking.UpLeft;
                yield return 1;
            }
        }
        private IEnumerator EmotionThenNormal(string emotion, float wait)
        {
            Calidus.Emotion(emotion);
            yield return wait;
            Calidus.Emotion("normal");
        }

        private IEnumerator SayAndWait(string id, float waitTime)
        {
            yield return Textbox.Say(id);
            yield return waitTime;
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
        private IEnumerator End(Player player, Level level)
        {
            if (!TagCheck(Tags.Global))
            {
                AddTag(Tags.Global);
            }
            level.Flash(Color.White, true);
            level.Remove(Calidus);
            InstantTeleport(level, player, "0-lcomp", null);
            yield return null;
            player = Engine.Scene.GetPlayer();
            player.Speed.X = -64;
            player.StateMachine.State = Player.StDummy;
            yield return 0.3f;
            yield return Textbox.Say("Ca21");
            yield return 0.2f;
            yield return Textbox.Say("Ca22");
            yield return 1;
            yield return Textbox.Say("Ca23");
            Audio.PauseMusic = false;
            level.InCutscene = false;
            player.StateMachine.State = Player.StNormal;
            RemoveTag(Tags.Global);
            EndCutscene(level);
        }
    }
}
