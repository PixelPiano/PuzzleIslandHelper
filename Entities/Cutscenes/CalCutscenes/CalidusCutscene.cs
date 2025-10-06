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
using static Celeste.Mod.PuzzleIslandHelper.Entities.Calidus;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [Tracked]
    public abstract class CalidusCutscene : CutsceneEntity
    {
        public Calidus Calidus;
        public Player Player;
        public WarpCapsule Capsule;
        public Arguments StartArgs;
        public Arguments EndArgs;
        public string EndArgsData => EndArgs.ToString();
        public string CutsceneID;
        private string flagID;
        public bool LockPlayerAtEnd;

        public CalidusCutscene(Player player = null, Calidus calidus = null, Arguments startArgs = null, Arguments endArgs = null) : base()
        {
            Player = player;
            Calidus = calidus;
            StartArgs = startArgs;
            EndArgs = endArgs;
        }
        public bool GetFlag(Level level) => string.IsNullOrEmpty(CutsceneID) || level.Session.GetFlag("CalCut" + CutsceneID);
        public void Register(Level level)
        {
            if (!string.IsNullOrEmpty(CutsceneID))
            {
                level.Session.SetFlag("CalCut" + CutsceneID);
            }
        }
        public override void Update()
        {
            base.Update();
        }
        public virtual void Prepare(Level level)
        {
            Register(level);
            Calidus ??= level.Tracker.GetEntity<Calidus>();
            Player ??= level.Tracker.GetEntity<Player>();
            if (Player != null)
            {
                Player.StateMachine.State = Player.StDummy;
                Player.StateMachine.Locked = true;
            }
            foreach (WarpCapsule capsule in level.Tracker.GetEntities<WarpCapsule>())
            {
                if (capsule.JustTeleportedTo)
                {
                    Capsule = capsule;
                    break;
                }
            }
        }
        public override void OnBegin(Level level)
        {
            Prepare(level);
            if (StartArgs != null)
            {
                Calidus?.ExecuteArgs(StartArgs);
            }
            Add(new Coroutine(Cutscene(level)));
        }
        public abstract IEnumerator Cutscene(Level level);
        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                Audio.PauseMusic = false;
                level.Session.SetFlag("blockGlitch", false);
                Glitch.Value = 0;
                Level.StopShake();
            }
            level.ResetZoom();
            Player = level.GetPlayer();
            Calidus = level.Tracker.GetEntity<Calidus>();
            if (Player != null && !LockPlayerAtEnd)
            {
                Player.StateMachine.Locked = false;
                Player.StateMachine.State = Player.StNormal;
            }
            if (EndArgs != null)
            {
                Calidus?.ExecuteArgs(EndArgs);
            }
        }
        public IEnumerator CapsuleIntro(Player player, Level level, WarpCapsule machine, float zoom, Facings facing, bool doGlitch = true)
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

        public IEnumerator PlayerLookLeft()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            player.Facing = Facings.Left;
            yield return null;
        }
        public IEnumerator PlayerLookRight()
        {
            if (Level.GetPlayer() is not Player player) yield break;
            player.Facing = Facings.Right;
            yield return null;
        }
        public IEnumerator ScreenZoom(Vector2 screenPosition, float amount, float time)
        {
            Level level = SceneAs<Level>();
            yield return level.ZoomTo(screenPosition, amount, time);
        }
        public IEnumerator PlayerZoomAcross(Player player, float amount, float time, float xOffset, float yOffset)
        {
            // position - level.Camera.Position
            Level level = SceneAs<Level>();
            yield return level.ZoomAcross(ScreenCoords(player.Position + new Vector2(xOffset, yOffset), level), amount, time);
        }
        public static Vector2 ScreenCoords(Vector2 position, Level level)
        {
            return position - level.Camera.Position;
        }
        public static IEnumerator Events(params IEnumerator[] events)
        {
            foreach (var e in events) yield return new SwapImmediately(e);
        }
        public static IEnumerator StutterGlitch(int frames)
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
        public static IEnumerator GlitchOut(Player player, Level level)
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
        public static IEnumerator Walk(Player player, float x, bool backwards = false, float speedmult = 1, bool intoWalls = false)
        {
            float positionX = player.Position.X;
            yield return player.DummyWalkTo(positionX + x, backwards, speedmult, intoWalls);
        }
        public static IEnumerator Wait(float time)
        {
            yield return time;
        }
        public static IEnumerator WaitForOne()
        {
            yield return Wait(1);
        }
        public static IEnumerator WaitForTwo()
        {
            yield return Wait(2);
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
