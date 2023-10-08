using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/DigiMeet")]
    [Tracked]
    public class DigiMeet : Trigger
    {
        private bool CameFromLeft;
        private bool SawBothSides;
        private Level level;
        private bool Started;
        private bool Completed;
        private Coroutine Main;
        private Calidus Calidus;
        private CutsceneEntity CutsceneEntity;
        public DigiMeet(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = scene as Level;
            //scene.Add(CutsceneEntity = new CutsceneEntity());
            SawBothSides = level.Session.GetFlag("digiRuinsLabRight") && level.Session.GetFlag("digiRuinsLabLeft");
            Calidus = level.Tracker.GetEntity<Calidus>();
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
        private IEnumerator LookSideToSide(Player player)
        {
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
        private IEnumerator ScreenZoomAcross(Vector2 screenPosition, float amount, float time)
        {
            Level level = SceneAs<Level>();

            yield return level.ZoomAcross(screenPosition, amount, time);
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
        private IEnumerator Cutscene(Player player, Camera cam, Level level)
        {
            level.InCutscene = true;
/*            if (true)
            {
                yield return new SwapImmediately(End(player, level));
                yield break;
            }*/
            //level.SkipCutscene();
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
            player.Jump();
            Add(new Coroutine(LookSideToSide(player)));
            yield return Textbox.Say("Ca2");
            Coroutine ScreenZoomAcrossRoutine = new Coroutine(ScreenZoomAcross(ZoomPosition + (Vector2.UnitX * 32), 1.5f, 7));
            Add(ScreenZoomAcrossRoutine);
            yield return Textbox.Say("Ca3");
            yield return Textbox.Say("Ca4");
            LookingSideToSide = false;
            yield return SayAndWait("Ca5", 0.2f);
            if (ScreenZoomAcrossRoutine.Active)
            {
                ScreenZoomAcrossRoutine.Cancel();
                Remove(ScreenZoomAcrossRoutine);
                level.ZoomSnap(ZoomPosition + (Vector2.UnitX * 32), 1.5f);
            }
            Add(new Coroutine(PlayerZoomAcross(player, 2f, 2, 32, -32)));
            yield return Textbox.Say("Ca6");
            Calidus.Parts.Play("jitter");
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
            yield return Textbox.Say("Ca7");
            yield return Textbox.Say("Ca8");
            Calidus.LookSpeed *= 5;
            Calidus.LookDir = Calidus.Looking.Right;
            yield return Calidus.Say("Ca9", "stern");

            Add(new Coroutine(Walk(16, false, 2)));
            Calidus.LookDir = Calidus.Looking.Left;
            Calidus.Surprised(false);
            yield return Textbox.Say("Ca10");

            Calidus.LookDir = Calidus.Looking.UpRight;
            yield return Textbox.Say("Ca11");

            Calidus.LookDir = Calidus.Looking.Left;
            Add(new Coroutine(Events(Wait(1), Walk(-16, true, 0.5f))));
            yield return Calidus.Say("Ca12", "stern");

            yield return Calidus.Say("Ca13", "normal");
            yield return Textbox.Say("Ca14");
            yield return 0.5f;
            yield return Calidus.Say("Ca15", "surprised");

            Calidus.LookDir = Calidus.Looking.Right;
            yield return Textbox.Say("Ca16");

            Calidus.LookDir = Calidus.Looking.Left;
            yield return Calidus.Say("Ca17", "normal");

            yield return Textbox.Say("Ca18");

            Calidus.LookDir = Calidus.Looking.Right;
            yield return Textbox.Say("Ca19");
            Calidus.LookDir = Calidus.Looking.Left;
            Add(new Coroutine(EmotionThenNormal("surprised", 0.6f)));
            Add(new Coroutine(Walk(8)));
            yield return Textbox.Say("Ca20");
            yield return 0.5f;
            Calidus.Emotion("happy");
            yield return Textbox.Say("Ca21");
            yield return Textbox.Say("Ca22");

            yield return Calidus.Say("Ca23", "happy");
            Calidus.LookDir = Calidus.Looking.Right;
            yield return Calidus.Say("Ca24", "normal");
            Add(new Coroutine(LookBackAndForth()));
            yield return Calidus.Say("Ca25", "eugh");
            CalidusLookAround = false;
            yield return Textbox.Say("Ca26");

            yield return 1;
            Calidus.Surprised(true);
            yield return 0.2f;
            Calidus.LookAtPlayer = true;
            yield return Calidus.Say("Ca27", "stern");


            yield return Textbox.Say("Ca28"); //Wha-
            Vector2 pos = Calidus.Position;
            AddTag(Tags.Global);
            for (float i = 0; i < 1; i += Engine.DeltaTime * 2)
            {
                Calidus.Position = Vector2.Lerp(pos, player.TopCenter, Ease.BackIn(i));
                yield return null;
            }
            yield return new SwapImmediately(End(player, level));


            //Calidus runs into Maddy, just as the screen begins to corrupt.
            //In an instant, Maddy is "ejected" from the digital world, and is panting. +Anxiety effect
            //M: Gah! That hurt! What'd you do that-
            //M:....for?
            //M: Calidus?
            //Camera zooms out, cutscene ends


            yield return null;
        }
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
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            /*            if (!CheckConditions() || Started || Completed)
                        {
                            return;
                        }*/
            Add(Main = new Coroutine(Cutscene(player, level.Camera, level)));
            Started = true;

        }
        
       
        private IEnumerator End(Player player, Level level)
        {
            if (!TagCheck(Tags.Global))
            {
                AddTag(Tags.Global);
            }
            level.Flash(Color.White, true);
            level.Remove(Calidus);
            InstantTeleport(level, player, "ruinsLab1", 254, 383);
            yield return null;
            player.Speed.X = -64;
            player.StateMachine.State = Player.StDummy;
            yield return 0.3f;
            yield return Textbox.Say("Ca29");
            yield return 0.2f;
            yield return Textbox.Say("Ca30");
            yield return 1;
            yield return Textbox.Say("Ca31");
            level.InCutscene = false;
            player.StateMachine.State = Player.StNormal;
            RemoveTag(Tags.Global);

        }
        private bool CheckConditions()
        {
            if (!SawBothSides)
            {
                return false;
            }
            if (level.Session.GetFlag("cameFromLeft"))
            {
                CameFromLeft = true;
            }
            else if (level.Session.GetFlag("cameFromRight"))
            {
                CameFromLeft = false;
            }
            else
            {
                return false;
            }
            return true;
        }
        public override void OnLeave(Player player)
        {
        }
        public static void InstantTeleport(Scene scene, Player player, string room, Vector2 nearestSpawn)
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
                session.RespawnPoint = level2.GetSpawnPoint(nearestSpawn);
                level.Session.FirstLevel = false;
                level.LoadLevel(Player.IntroTypes.Transition);

                Vector2 val4 = level.Session.RespawnPoint.Value - val2;
                level.Camera.Position = level.LevelOffset + val3 + val4;
                level.Add(player);
                player.Position = level.Session.RespawnPoint.Value;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset + val4);

                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }
        public static void InstantTeleport(Scene scene, Player player, string room, float positionX, float positionY)
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

                level.Add(player);
                player.Position = level.LevelOffset + new Vector2(positionX, positionY);
                level.Camera.Position = level.LevelOffset + new Vector2(0,260);
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                //return;

                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }

        public static void TeleportTo(Scene scene, Player player, string room, Player.IntroTypes introType = Player.IntroTypes.Transition, Vector2? nearestSpawn = null)
        {
            Level level = scene as Level;
            if (level != null)
            {
                level.OnEndOfFrame += delegate
                {
                    level.TeleportTo(player, room, introType, nearestSpawn);
                };
            }
        }
    }
}
