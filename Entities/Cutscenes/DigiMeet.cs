using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{

    [Tracked]
    public class DigiMeet : CutsceneEntity
    {
        private bool Started;
        private bool Completed;
        private Calidus Calidus;
        public DigiMeet()
            : base()
        {
        }

        public override void OnBegin(Level level)
        {
            Calidus = level.Tracker.GetEntity<Calidus>();
            if (Started || Completed)
            {
                return;
            }
            Player player = level.GetPlayer();
            Add(new Coroutine(Cutscene(player, level)));
            Started = true;
        }
        public override void OnEnd(Level level)
        {

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
        private IEnumerator Cutscene(Player player, Level level)
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
            level.Session.SetFlag("blockGlitch");
            yield return 0.1f;
            Calidus.Surprised(true);
            Calidus.LookDir = Calidus.Looking.Up;
            yield return 0.4f;
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
            while (text.InCutscene)
            {
                yield return null;
            }
            yield return new SwapImmediately(End(player, level));
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


        private IEnumerator End(Player player, Level level)
        {
            if (!TagCheck(Tags.Global))
            {
                AddTag(Tags.Global);
            }
            level.Flash(Color.White, true);
            level.Remove(Calidus);
            PianoUtils.InstantRelativeTeleport(level, "0-lcomp", true);
            //InstantTeleport(level, player, "0-lcomp", 254, 383);
            yield return null;
            player.Speed.X = -64;
            player.StateMachine.State = Player.StDummy;
            yield return 0.3f;
            yield return Textbox.Say("Ca21");
            yield return 0.2f;
            yield return Textbox.Say("Ca22");
            yield return 1;
            yield return Textbox.Say("Ca23");
            level.InCutscene = false;
            player.StateMachine.State = Player.StNormal;
            RemoveTag(Tags.Global);

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
                level.Camera.Position = level.LevelOffset + new Vector2(0, 260);
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
