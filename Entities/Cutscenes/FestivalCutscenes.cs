using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Structs;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [CustomEvent("PuzzleIslandHelper/RivalsIntroduction")]
    [Tracked]
    public class FestivalCutscenes : CutsceneEntity
    {
        private enum cleanupStates
        {
            None,
            Competition,
            Disaster
        }
        public enum Types
        {
            RivalsIntro,
            Jaques1,
            JaquesMentionRandy,
            Randy1,
            Jaques2,
            JaquesIntoCompetition,
            None
        }
        public class Darkness : Entity
        {
            public Vector2 MonsterPosition = new Vector2(0, 148);
            public Darkness() : base()
            {
                Depth = -10000000;
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Position = (scene as Level).Camera.Position;
            }
            public IEnumerator SlideToX(float x, float time)
            {
                float from = MonsterPosition.X;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    MonsterPosition.X = Calc.LerpClamp(from, x, i);
                    yield return null;
                }
                MonsterPosition.X = x;
            }
            public override void Render()
            {
                base.Render();
                Draw.Rect(Position, 320, 180, Color.Black);
                Draw.Rect(Position + MonsterPosition, 16, 16, Color.Red);
            }
        }
        public Player Player;
        public FormativeRival Jaques;
        public PrimitiveRival Randy;
        public FestivalFloat Float;
        public Calidus Calidus;
        public FestivalJudge Judge;
        public FestivalTrailer Trailer;
        public Ghost Ghost;
        public Darkness Dark;
        public Types CutsceneType = Types.RivalsIntro;
        private cleanupStates cleanupState = cleanupStates.None;
        private bool jaquesLookAround;
        private bool randyLookAround;
        private bool incrementSkipCheckJaques;
        private bool incrementSkipCheckRandy;
        public Vector2 JaquesOrig, RandyOrig;
        public override void Update()
        {
            base.Update();
            Position = Level.Camera.Position;
        }
        public FestivalCutscenes() : this(Types.RivalsIntro)
        {

        }
        public FestivalCutscenes(Types type) : base()
        {
            CutsceneType = type;
            Collider = new Hitbox(8, 8);

            Add(new DebugComponent(Microsoft.Xna.Framework.Input.Keys.H, delegate { Calidus.Drunk = true; }, true));
            Add(new DebugComponent(Microsoft.Xna.Framework.Input.Keys.Y, delegate { Calidus.Drunk = false; }, true));
        }
        public void GetEntities(Level level)
        {
            Player = level.GetPlayer();
            Jaques = level.Tracker.GetEntity<FormativeRival>();
            Randy = level.Tracker.GetEntity<PrimitiveRival>();
            Float = level.Tracker.GetEntity<FestivalFloat>();
            Calidus = level.Tracker.GetEntity<Calidus>();
            Judge = level.Tracker.GetEntity<FestivalJudge>();
            Trailer = level.Tracker.GetEntity<FestivalTrailer>();
            Ghost = level.Tracker.GetEntity<Ghost>();
        }
        public void DisableMovement(Player player)
        {
            player.StateMachine.State = Player.StDummy;
        }
        public void EnableMovement(Player player)
        {
            player.StateMachine.State = Player.StNormal;
        }
        public override void OnBegin(Level level)
        {
            bool rivalsMet = Level.Session.GetFlag("RivalsHaveEntered");
            GetEntities(level);
            if (CutsceneType is Types.RivalsIntro && !rivalsMet)
            {
                DisableMovement(Player);
                Player.ForceCameraUpdate = true;
                if (Jaques != null && Randy != null)
                {
                    RandyOrig = Randy.Position;
                    JaquesOrig = Jaques.Position;
                    Randy.TimesTalkedTo = 0;
                    Jaques.TimesTalkedTo = 0;
                    Add(new Coroutine(RivalsIntro()));
                }
            }
            else if (rivalsMet)
            {
                switch (CutsceneType)
                {
                    case Types.Jaques1:
                        DisableMovement(Player);
                        Add(new Coroutine(Jaques1()));
                        break;
                    case Types.JaquesMentionRandy:
                        DisableMovement(Player);
                        Add(new Coroutine(JaquesMentionRandy()));
                        break;
                    case Types.Randy1:
                        DisableMovement(Player);
                        Add(new Coroutine(Randy1()));
                        break;
                    case Types.Jaques2:
                        DisableMovement(Player);
                        Add(new Coroutine(Jaques2()));
                        break;
                    case Types.JaquesIntoCompetition:
                        DisableMovement(Player);
                        Add(new Coroutine(JaquesCompConfirm()));
                        break;
                }
            }
        }
        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                if (incrementSkipCheckJaques && Jaques != null)
                {
                    Jaques.TimesTalkedTo++;
                }
                if (incrementSkipCheckRandy && Randy != null)
                {
                    Randy.TimesTalkedTo++;
                }
            }
            if (Randy.TimesTalkedTo >= 1)
            {
                Randy.Talk.Enabled = false;
            }
            switch (CutsceneType)
            {
                case Types.RivalsIntro:
                    if (Jaques != null && Randy != null)
                    {
                        Level.Session.SetFlag("RivalsHaveEntered");
                        Jaques.JumpLoop = false;
                        Randy.JumpLoop = false;
                        Jaques.Position = JaquesOrig;
                        Randy.Position = RandyOrig;
                        Jaques.Talk.Enabled = true;
                        Randy.Talk.Enabled = true;
                    }
                    level.ResetZoom();
                    if (level.GetPlayer() is Player player)
                    {
                        level.Camera.Position = new Vector2(player.CameraTarget.X, level.Camera.Position.Y);
                        player.StateMachine.State = Player.StNormal;
                    }
                    break;
                case Types.Jaques1:
                case Types.JaquesMentionRandy:
                case Types.Randy1:
                case Types.Jaques2:
                    EnableMovement(Player);
                    break;
                case Types.JaquesIntoCompetition:
                    if (cleanupState == cleanupStates.None)
                    {
                        EnableMovement(Player);
                    }
                    break;
                default:
                    break;
            }
            switch (cleanupState)
            {
                case cleanupStates.Competition:
                    break;
                case cleanupStates.Disaster:
                    break;
            }
        }

        public void PrepareCompetition()
        {
            DisableMovement(Player);
            Player.ForceCameraUpdate = false;
            cleanupState = cleanupStates.Competition;
            Level.Camera.ToMarker("judgeCam");
            Judge.ToMarker("judgePosition");
            Judge.SetUp(Player);
            Jaques.ToMarkerX("jaquesCompStart");
            Randy.ToMarkerX("randyCompStart");
            Jaques.Ground();
            Randy.Ground();
            Trailer.Reset();
            //Float.Cover();
            if (Calidus != null)
            {
                Calidus.Position = Player.TopRight - new Vector2(Calidus.Width + 8, 16);
                Calidus.Look(Calidus.Looking.Right);
            }
        }
        private IEnumerator RivalsIntro()
        {
            yield return new SwapImmediately(Competition());
            EndCutscene(Level);
            yield break;
            yield return new SwapImmediately(Textbox.Say("FESTIVAL_RIVALS_00", zoom, zoomAcross, wait, approachRandy, bounce));
            yield return Level.ZoomBack(1);
            yield return null;
            EndCutscene(Level);
        }
        private IEnumerator Jaques1()
        {
            incrementSkipCheckJaques = true;
            yield return new SwapImmediately(Jaques.PlayerStepBack(Player));
            Jaques.FacePlayer(Player);
            yield return new SwapImmediately(Textbox.Say("FESTIVAL_RIVALS_01a"));
            Jaques.TimesTalkedTo++;
            incrementSkipCheckJaques = false;
            EndCutscene(Level);
        }
        private IEnumerator JaquesMentionRandy()
        {
            yield return new SwapImmediately(Jaques.PlayerStepBack(Player));
            Jaques.FacePlayer(Player);
            yield return new SwapImmediately(Textbox.Say("FESTIVAL_RIVALS_02"));
            EndCutscene(Level);
        }
        private IEnumerator Jaques2()
        {
            incrementSkipCheckJaques = true;
            yield return new SwapImmediately(Jaques.PlayerStepBack(Player));
            Jaques.FacePlayer(Player);
            yield return new SwapImmediately(Textbox.Say("FESTIVAL_RIVALS_03"));
            Jaques.TimesTalkedTo++;
            incrementSkipCheckJaques = false;
            EndCutscene(Level);
        }
        private IEnumerator Randy1()
        {
            incrementSkipCheckRandy = true;
            yield return new SwapImmediately(Randy.PlayerStepBack(Player));
            Randy.FacePlayer(Player);
            yield return new SwapImmediately(Textbox.Say("FESTIVAL_RIVALS_01b"));
            Randy.TimesTalkedTo++;
            incrementSkipCheckRandy = false;
            EndCutscene(Level);
        }
        private IEnumerator JaquesCompConfirm()
        {
            yield return new SwapImmediately(Jaques.PlayerStepBack(Player));
            Jaques.FacePlayer(Player);
            yield return new SwapImmediately(Textbox.Say("FESTIVAL_RIVALS_04"));
            yield return new SwapImmediately(ChoicePrompt.Prompt("FESTIVAL_RIVALS_04c2", "FESTIVAL_RIVALS_04c1"));
            if (ChoicePrompt.Choice > 0)
            {
                incrementSkipCheckJaques = true;
                yield return new SwapImmediately(Textbox.Say("FESTIVAL_RIVALS_04aa"));
                Jaques.TimesTalkedTo++;
                incrementSkipCheckJaques = false;
                yield return new SwapImmediately(Competition());
            }
            else
            {
                EndCutscene(Level);
            }
        }

        public IEnumerator Competition()
        {
            yield return new SwapImmediately(Fader.FadeInOut(Color.Transparent, Color.Black, 1, 0.6f, PrepareCompetition));
            /* yield return new SwapImmediately(Textbox.Say("FESTIVAL_RIVALS_05a", PlayerLookLeft, CalidusShock, CalidusStern, CalidusNormal, JaquesRunToMaddy, JaquesRunBack, JaquesFloatLeft, JaquesFloatCenter, JaquesFloatRight, JaquesFloatReveal, Rave, JaquesStepOnPlatform));

             yield return FadeToRandy();
             yield return Textbox.Say("FESTIVAL_RIVALS_05b");
             List<string> choicesRemaining = new();
             for (int i = 1; i < 5; i++)
             {
                 choicesRemaining.Add("FESTIVAL_RIVALS_05c" + i + 'q');
             }
             while (choicesRemaining.Count > 0)
             {
                 yield return new SwapImmediately(ChoicePrompt.Prompt(choicesRemaining.ToArray()));
                 int choice = ChoicePrompt.Choice;
                 yield return new SwapImmediately(Textbox.Say(choicesRemaining[choice].TrimEnd('q')));
                 choicesRemaining.RemoveAt(choice);
             }*/
            yield return Disaster();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dark?.RemoveSelf();
        }
        public IEnumerator Disaster()
        {
            //yield return new FadeInOut(2, OnComplete);
            yield return new SwapImmediately(Textbox.Say("FESTIVAL_RIVALS_06"));
            yield return new SwapImmediately(ChoicePrompt.Prompt("FESTIVAL_RIVALS_06c1", "FESTIVAL_RIVALS_06c2"));
            /* Calidus wakes Maddy up NOT DONE
             * Check on Jaques NOT DONE
             * Jaques wakes up NOT DONE
             * Immediately goes to check on Randy NOT DONE
             * Opens to door NOT DONE
             * Randy gone NOT DONE
             * Tries to speak NOT DONE
             * Finds out he's like Randy now NOT DONE
             * Kinda fucked up ngl NOT DONE
             */
            //rumble, jaques calm, creature appears,
            //crowd scatters, jaques/randy run, jaques/randy turn
            //jaques/randy run to trailer, get in trailer

            /*rumble
             * 0 rumble
             * 1 jaques calm
             * 2 creature appears
             * 3 randy panic
             * 4 crowd scatter
             * 5 maddy to jaques
             * 6 creature materializes
             * 7 jaques to front of trailer
             * 8 creature approaches first
             * 9 calidus approach creature
             * 10 calidus glitch
             * 11 everyone back up
             * 12 creature advances
             * 13 look at jaques
             * 14 move behind jaques
             * 15 light on ghost
             * 16 run to trailer
             * 17 jump in trailer
             */


            yield return new SwapImmediately(Textbox.Say("FESTIVAL_RIVALS_07", rumble, jaquesCalmDown, ghostAppears, randyPanic, crowdScatter, MaddyRunToJaques, ghostMaterialize, jaquesToFrontOfTrailer, ghostFirstApproach, calidusApproachGhost, calidusGlitch, calidusDizzy, everyoneBackUp, ghostAdvance, allLookAtJaques, moveBehindJaques, lightOnGhost, runToTrailerBack, getInTrailer));

            //ghost walks to right, ghost roars, ghost walks to left, ghost roars, ghost leaves
        }
        public IEnumerator Trapped()
        {
            yield return null;
        }

        private IEnumerator everyoneBackUp()
        {
            Add(new Coroutine(Jaques.WalkToX(Jaques.X + 16, 1, true)));
            Add(new Coroutine(Randy.WalkToX(Randy.X + 16, 1, true)));
            Add(new Coroutine(Player.DummyWalkTo(Player.X + 16, true)));
            if (Calidus != null)
            {
                Add(new Coroutine(Calidus.FloatToX(Calidus.X + 16)));
            }
            yield return null;
        }
        private IEnumerator moveToXThen(Actor actor, float x, float speedMult = 1, bool walkBackwards = false, Action onEnd = null)
        {
            x = (int)Math.Round(x);
            if (actor.Position.X == x) yield break;
            int dir = Math.Sign(x - actor.Position.X);
            if (walkBackwards) dir *= -1;
            if (actor is Player)
            {
                (actor as Player).Facing = (Facings)dir;
                yield return (actor as Player).DummyWalkTo(x, walkBackwards);
            }
            else
            {
                if (actor is VertexPassenger)
                {
                    (actor as VertexPassenger).Facing = (Facings)dir;
                }
                while (Math.Abs(actor.Position.X - x) > 2)
                {
                    actor.MoveTowardsX(x, 90f * speedMult * Engine.DeltaTime);
                    yield return null;
                }
            }
            actor.Position.X = x;
            onEnd?.Invoke();
        }
        private IEnumerator moveBehindJaques()
        {
            yield return new SwapImmediately(moveGroup(Jaques.Right, 6, Facings.Left, Player, Randy, Calidus));
        }
        private IEnumerator moveGroup(float to, float spacing, Facings facing, params Actor[] actors)
        {
            Actor first = actors[0];
            List<Coroutine> routines = new();
            actors.OrderBy(item => MathHelper.Distance(item.CenterX, to));

            float space = 0;
            foreach (Actor actor in actors)
            {
                routines.Add(new Coroutine(moveToXThen(actor, to + space)));
                space += actor.Width + spacing;
                yield return null;
            }
            foreach (Coroutine r in routines)
            {
                Add(r);
            }
            for (int i = 0; i < routines.Count; i++)
            {
                while (!routines[i].Finished) yield return null;
                if (actors[i] is Player player) player.Facing = facing;
                if (actors[i] is VertexPassenger p) p.Facing = facing;
                if (actors[i] is Calidus c) c.Look(facing is Facings.Left ? Calidus.Looking.Left : Calidus.Looking.Right);
            }
            yield return null;
        }
        private IEnumerator lightOnGhost()
        {
            yield return null;
        }
        private IEnumerator ghostAdvance()
        {
            Ghost.Speed.X = 85f;
            yield return null;
        }
        private IEnumerator ghostFirstApproach()
        {
            Add(new Coroutine(delayedRetreat()));
            Ghost.Speed.X = 145f;
            yield return null;
        }

        private IEnumerator delayedRetreat()
        {
            yield return null;
            Add(new Coroutine(Randy.WalkX(8, 1.4f, true)));
            yield return 0.5f;
            Add(new Coroutine(Player.DummyWalkTo(Player.X + 20, true)));
            yield return 0.1f;
            Add(new Coroutine(Jaques.WalkX(16, 1.2f, true)));
        }
        private IEnumerator calidusApproachGhost()
        {
            Calidus.StopFollowing();
            yield return 0.4f;
            float from = Calidus.X;
            yield return PianoUtils.Lerp(Ease.SineInOut, 1.2f, f => Calidus.X = Calc.LerpClamp(from, Ghost.Right + 20, f));
        }
        private IEnumerator burstCalidusWarp(Vector2 to)
        {
            void addShiz()
            {
                Calidus.AddAfterImage(0.7f, 0.5f);
                Level.Displacement.AddBurst(Calidus.Center, 0.4f, 0, Calidus.Width, 1);
                QuickGlitch.Create(Calidus, new Range2(2, 5), Vector2.UnitX, Engine.DeltaTime, 3, 0.2f);
            }
            Vector2 scale = Calidus.Scale;
            addShiz();
            yield return Engine.DeltaTime * 4;
            Calidus.Center = to;
            addShiz();
            yield return Engine.DeltaTime * 3;
            Calidus.Scale = Calidus.EyeScale = scale;
        }
        private IEnumerator calidusDizzy()
        {
            calidusGlitching = false;
            yield return 0.6f;
            yield return Calidus?.FloatToX(Player.Left - 8, 0.3f);
            yield return 0.1f;
            Calidus.Drunk = false;
            Calidus.Stern();

            yield return null;
        }
        private bool calidusGlitching;
        private IEnumerator calidusGlitchCoroutine()
        {
            Calidus.Dizzy();
            calidusGlitching = true;
            Vector2 start = Calidus.Center;
            while (calidusGlitching)
            {
                Vector2 target = start + new Vector2(Calc.Random.Range(-32, 16), Calc.Random.Range(-16, 8));
                yield return new SwapImmediately(burstCalidusWarp(target));
            }
            yield return new SwapImmediately(burstCalidusWarp(start));
            Calidus.Drunk = true;
        }
        private IEnumerator calidusGlitch()
        {
            Add(new Coroutine(calidusGlitchCoroutine()));
            Ghost.GlitchCalidus();
            yield return null;
        }
        private IEnumerator MaddyRunToJaques()
        {
            Calidus.StartFollowing(Calidus.Looking.Left);
            Player.DummyGravity = true;
            yield return Player.DummyWalkTo(Jaques.Left - 16, false, 2);
            yield return null;
        }
        private IEnumerator ghostMaterialize()
        {
            if (Marker.TryFind("ghostCam", out Vector2 pos))
            {
                Player.Facing = Facings.Left;
                Jaques.Facing = Facings.Left;
                randyLookAround = false;
                float from = Level.Camera.X;
                yield return PianoUtils.Lerp(Ease.SineInOut, 1, f => Level.Camera.X = Calc.LerpClamp(from, pos.X, f));
                yield return Ghost.Materialize();
                yield return PianoUtils.Lerp(Ease.SineInOut, 1, f => Level.Camera.X = Calc.LerpClamp(pos.X, from, f));
            }
            yield return null;
        }
        private IEnumerator jaquesToFrontOfTrailer()
        {
            yield return Jaques.WalkToX(Trailer.Left + 16, 2);
            yield return null;
        }
        private IEnumerator allLookAtJaques()
        {
            yield return 0.2f;
            Player.Facing = Randy.Facing = Facings.Right;
            Calidus?.Look(Calidus.Looking.Right);
            yield return null;
        }
        private IEnumerator runToTrailerBack()
        {
            float x = Level.Camera.X;
            Add(new Coroutine(PianoUtils.Lerp(Ease.SineInOut, 1.5f, f => Level.Camera.X = Calc.LerpClamp(x, x + 80, f))));
            yield return new SwapImmediately(moveGroup(Trailer.Right + 8, 4, Facings.Left, Jaques, Randy, Player, Calidus));
        }
        private IEnumerator crowdScatter()
        {
            yield return null;
        }
        private IEnumerator MoveActorToXAndJump(Actor actor, float x, float jumpAtDist, float speedMult = 1)
        {
            bool jumped = false;
            float dist = MathHelper.Distance(actor.Position.X, x);
            if (actor is Player) (actor as Player).Facing = (Facings)Math.Sign(x - actor.X);
            if (actor is VertexPassenger) (actor as VertexPassenger).Facing = (Facings)Math.Sign(x - actor.X);
            while (dist > 2)
            {
                actor.MoveTowardsX(x, 90f * speedMult * Engine.DeltaTime);
                dist = MathHelper.Distance(actor.Position.X, x);
                if (!jumped && dist <= jumpAtDist)
                {
                    if (actor is Player) (actor as Player).Jump();
                    if (actor is VertexPassenger) (actor as VertexPassenger).Jump();
                    jumped = true;
                }
                yield return null;
            }
            actor.Position.X = x;
        }
        private IEnumerator getInTrailer()
        {
            Trailer.PrepareForGettingIn();
            float x = Trailer.Left + 8;
            Coroutine[] routines = new Coroutine[]
            {
                new(MoveActorToXAndJump(Jaques,x,Trailer.Width)),
                new(MoveActorToXAndJump(Player,x + 16, Trailer.Width - 20)),
                new(MoveActorToXAndJump(Randy, x + 32, Trailer.Width - 32)),
                new(Calidus.FloatToX(x + 48))
            };
            Add(routines);
            foreach (var r in routines)
            {
                while (!r.Finished) yield return null;
            }
            Trailer.ShutDoor();
            Dark = new Darkness();
            Scene.Add(Dark);
            yield return null;
        }
        private IEnumerator wait()
        {
            yield return 1;
        }
        private IEnumerator bounce()
        {
            Jaques.JumpLoop = true;
            Randy.JumpLoop = true;
            Randy.CannotJumpTimer = 0.1f;
            yield return null;
        }
        private IEnumerator approachRandy()
        {
            yield return Jaques.MoveXNaive(48, 1.2f, Ease.SineOut);
        }
        private IEnumerator zoom()
        {
            float fromX = Level.Camera.Position.X;
            float toX = Level.MarkerCentered("meetZoom").X;

            Level.GetPlayer().ForceCameraUpdate = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1f)
            {
                Level.Camera.Position = new Vector2(Calc.LerpClamp(fromX, toX, Ease.CubeInOut(i)), Level.Camera.Position.Y);
                yield return null;
            }
            yield return 0.2f;
            yield return Level.ZoomTo(new Vector2(160, 120), 1.5f, 1);
            yield return null;
        }
        private IEnumerator zoomAcross()
        {
            float fromX = Level.Camera.Position.X;
            Player player = Level.GetPlayer();
            float toX = player.CameraTarget.X;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1f)
            {
                Level.Camera.Position = new Vector2(Calc.LerpClamp(fromX, toX, Ease.CubeInOut(i)), Level.Camera.Position.Y);
                yield return null;
            }
        }
        private IEnumerator PlayerLookLeft()
        {
            Player.Facing = Facings.Left;
            yield return null;
        }
        private IEnumerator PlayerLookRight()
        {
            Player.Facing = Facings.Right;
            yield return null;
        }
        private IEnumerator JaquesRunToMaddy()
        {
            yield return Jaques.WalkToX(Player.Right + 8, 2);
            Player.Facing = Facings.Right;
            yield return null;
        }
        private IEnumerator JaquesRunBack()
        {
            if (Marker.TryFind("jaquesCompStart", out Vector2 position))
            {
                yield return Jaques.WalkToX(position.X, 2);
            }
            yield return null;
        }
        private IEnumerator JaquesStepOnPlatform()
        {
            Jaques.Facing = Facings.Right;
            float from = Jaques.Position.X;
            float x = Float.CenterX - Jaques.Width / 2;
            bool jumped = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime * 1.3f)
            {
                Jaques.Position.X = Calc.LerpClamp(from, x, i);
                if (i > 0.6f && !jumped)
                {
                    Jaques.Jump();
                    jumped = true;
                }
                yield return null;
            }
            Position.X = x;
            yield return 0.1f;
            Jaques.Facing = Facings.Left;
        }
        private IEnumerator JaquesFloatCenter()
        {
            yield return new SwapImmediately(Jaques.WalkToX(Float.CenterX - Jaques.Width / 2));
        }
        private IEnumerator JaquesFloatLeft()
        {
            jaquesLookAround = false;
            yield return new SwapImmediately(Jaques.WalkToX(Float.Left + Jaques.Width / 2 + 4));
        }
        private IEnumerator JaquesFloatRight()
        {
            yield return new SwapImmediately(Jaques.WalkToX(Float.Right - Jaques.Width * 1.5f - 4));
        }
        private IEnumerator JaquesFloatReveal()
        {
            //Float.State = FestivalFloat.States.Jaques;
            //Float.Hide();
            //yield return new SwapImmediately(Float.Reveal());
            yield return null;
        }
        private IEnumerator Rave()
        {
            //Float.Party();
            yield return null;
        }
        public void PrepareStageForRandy()
        {
            Jaques.ToMarker("jaquesCompStart");
            Randy.CenterX = Float.CenterX + Float.Width / 2;
            Randy.Bottom = Float.Top;

            //Float.Presentor = Presentors.Randy;
            Randy.Facing = Facings.Left;
            Jaques.Facing = Facings.Right;
        }
        private IEnumerator FadeToRandy()
        {
            yield return new SwapImmediately(Fader.FadeInOut(Color.Transparent, Color.Black, 1.2f, 1f, PrepareStageForRandy));
        }
        private IEnumerator RandyFloatRight()
        {
            yield return new SwapImmediately(Randy.WalkToX(Float.Right - Jaques.Width / 2 - 4));
        }
        private IEnumerator CalidusShock()
        {
            Calidus?.Emotion(Calidus.Mood.Surprised);
            yield return null;
        }
        private IEnumerator CalidusStern()
        {
            Calidus?.Emotion(Calidus.Mood.Stern);
            yield return null;
        }
        private IEnumerator CalidusNormal()
        {
            Calidus?.Emotion(Calidus.Mood.Normal);
            yield return null;
        }
        private bool calidusLookingLeftAndRight;
        private bool playerLookingLeftAndRight;
        private IEnumerator calidusLookLeftRight()
        {
            calidusLookingLeftAndRight = true;
            while (calidusLookingLeftAndRight)
            {
                Calidus.Look(Calidus.Looking.Left);
                for (float i = 0; i < 0.4f && calidusLookingLeftAndRight; i += Engine.DeltaTime) yield return null;
                if (!calidusLookingLeftAndRight) break;
                Calidus.Look(Calidus.Looking.Right);
                for (float i = 0; i < 0.4f && calidusLookingLeftAndRight; i += Engine.DeltaTime) yield return null;
                if (!calidusLookingLeftAndRight) break;
            }
            Calidus.Look(Calidus.Looking.Left);
        }
        private IEnumerator playerLookLeftRight()
        {
            playerLookingLeftAndRight = true;
            while (playerLookingLeftAndRight)
            {
                Player.Facing = Facings.Left;
                for (float i = 0; i < 0.45f && playerLookingLeftAndRight; i += Engine.DeltaTime) yield return null;
                if (!playerLookingLeftAndRight) break;
                Player.Facing = Facings.Right;
                for (float i = 0; i < 0.45f && playerLookingLeftAndRight; i += Engine.DeltaTime) yield return null;
                if (!playerLookingLeftAndRight) break;
            }
            Player.Facing = Facings.Left;
        }
        private IEnumerator rumble()
        {
            Add(new Coroutine(playerLookLeftRight()));
            Add(new Coroutine(calidusLookLeftRight()));
            LevelShaker.Intensity = 0.5f;
            yield return null;
        }
        private IEnumerator reactToGhostAppear()
        {
            yield return null;
            jaquesLookAround = false;
            Jaques.Facing = Facings.Left;
            yield return 0.1f;
            Judge.Chair.GetUp(Player, Player.StDummy);
            Player.DummyGravity = true;
            playerLookingLeftAndRight = false;
            Player.Facing = Facings.Left;

            yield return null;
            Add(new Coroutine(Jaques.WalkX(16, 2, true)));
            yield return 0.6f;
            Add(new Coroutine(Player.DummyWalkTo(Player.X + 24, true, 1.4f)));
            calidusLookingLeftAndRight = false;
            Calidus.LookSpeed = 0.5f;
            Calidus.Look(Calidus.Looking.Left);
            yield return 0.3f;
            Calidus.LookAt(Ghost);
            yield return 0.1f;
            Calidus.LookSpeed = 1f;
            yield return Calidus.FloatToXLerp(Calidus.X + 16, 1.4f, Ease.SineInOut);
            yield return null;

        }
        private IEnumerator randyToJaques(Facings endFacing)
        {
            yield return Randy.WalkToX(Jaques.Right + 2, 1.5f);
            Randy.Facing = endFacing;
        }
        private IEnumerator playerToJaques(Facings endFacing)
        {
            yield return Player.DummyWalkTo(Jaques.Right + 3 + Randy.Width + 2, false, 2);
            Player.Facing = endFacing;
        }
        private IEnumerator calidusToJaques(Calidus.Looking endLook)
        {
            yield return Calidus.FloatToX(Jaques.Right + 3, 2);
            Calidus.Look(endLook);
        }
        private IEnumerator getBehindJaques()
        {
            Add(new Coroutine(randyToJaques(Facings.Left)));
            yield return 0.5f;
            Add(new Coroutine(playerToJaques(Facings.Left)));
            yield return 0.1f;
            Add(new Coroutine(calidusToJaques(Calidus.Looking.Left)));
        }
        private IEnumerator jaquesRunToFrontOfTrailer()
        {
            yield return Jaques.WalkToX(Trailer.Left, 2);
            Jaques.Facing = Facings.Left;
        }
        private IEnumerator ghostAppears()
        {
            float x = Level.Camera.X;
            float to = Marker.Find("ghostCam").X;
            yield return PianoUtils.Lerp(Ease.SineInOut, 1, f => Level.Camera.X = Calc.LerpClamp(x, to, f));
            if (Marker.TryFind("ghostAppear", out Vector2 pos))
            {
                Ghost = new Ghost(pos) { StartEmpty = true };
                Level.Add(Ghost);
                Add(new Coroutine(reactToGhostAppear()));
            }
            yield return 1.2f;
            Add(new Coroutine(PianoUtils.Lerp(Ease.SineInOut, 1, f => Level.Camera.X = Calc.LerpClamp(to, x, f))));
        }

        private IEnumerator jaquesCalmDown()
        {
            yield return MoveActorToXAndJump(Jaques, Float.CenterX - Jaques.Width / 2, 8, 2);
            Add(new Coroutine(jaquesLookAroundLoop()));
            yield return null;
        }
        private IEnumerator jaquesLookAroundLoop()
        {
            jaquesLookAround = true;
            while (jaquesLookAround)
            {
                Jaques.Facing = (Facings)(-(int)Jaques.Facing);
                yield return 0.5f;
            }
        }
        private IEnumerator randyLookAroundLoop()
        {
            randyLookAround = true;
            float start = Randy.Position.X;
            while (randyLookAround)
            {
                yield return new SwapImmediately(Randy.WalkToX(start - 16, 2));
                yield return new SwapImmediately(Randy.WalkToX(start + 16, 2));
            }
            Randy.Facing = Facings.Left;
            yield return null;
        }
        private IEnumerator randyPanic()
        {
            Add(new Coroutine(randyLookAroundLoop()));
            yield return null;
        }
        private IEnumerator JaquesLookLeft()
        {
            Jaques.Facing = Facings.Left;
            yield return null;
        }
        private IEnumerator JaquesLookRight()
        {
            Jaques.Facing = Facings.Right;
            yield return null;
        }
        private IEnumerator jaquesRunToRandy()
        {
            Add(new Coroutine(Jaques.WalkToX(Randy.Position.X - Jaques.Width, 3)));
            yield return null;
        }
        private IEnumerator calidusMoveLeft()
        {
            if (Calidus != null)
            {
                float from = Calidus.Position.X;
                yield return PianoUtils.Lerp(Ease.Linear, 1, f => Calidus.Position.X = Calc.LerpClamp(from, from - 32, f));
                yield return 0.5f;
                Calidus.Look(Calidus.Looking.Left);
            }
            yield return null;
        }
        private IEnumerator maddyLookLeft()
        {
            Player.Facing = Facings.Left;
            yield return null;
        }
        private IEnumerator calidusLookRight()
        {
            Calidus?.Look(Calidus.Looking.Right);
            yield return null;
        }
        private IEnumerator maddyRunOff()
        {
            Player.Facing = Facings.Right;
            yield return Player.DummyWalkTo(Player.X + 50, false, 3); //change to position later
            yield return null;
        }
        private IEnumerator lightFlash()
        {
            Level.Flash(Color.White);
            yield return null;
        }
        private IEnumerator headache()
        {
            //headache effects
            yield return null;
        }
        private IEnumerator rushToMaddy()
        {
            if (Calidus != null)
            {
                float from = Calidus.Position.X;
                yield return PianoUtils.Lerp(Ease.Linear, 1, f => Calidus.Position.X = Calc.LerpClamp(from, Player.Position.X, f));
            }
            yield return null;
        }
        private IEnumerator randyDisappears()
        {
            yield return null;
        }
        private IEnumerator cameraToPlayer()
        {
            Vector2 from = Level.Camera.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
            {
                Level.Camera.Position = Vector2.Lerp(from, Player.CameraTarget, Ease.CubeInOut(i));
            }
            yield return null;
        }
        private IEnumerator ghostHitsTrailer()
        {
            yield return null;
        }
        private IEnumerator ghostRoar()
        {
            yield return null;
        }
    }
}
