using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class GrassShift : CutsceneEntity
    {
        public static ShaderOverlay Shader;
        private float max = 0.05f;
        public bool Shaking;
        private int part;
        private string room = "r-2c";
        private static bool SkippedPart1;
        private static Vector2 RelativeTeleportPosition;
        public GrassShift(int part) : base()
        {
            this.part = part;
            AddTag(Tags.TransitionUpdate);
        }

        public override void OnBegin(Level level)
        {
            if (level.Session.GetFlag("GrassShiftCutsceneWatched"))
            {
                return;
            }
            if (part == 1)
            {
                SkippedPart1 = false;
            }

            if (Shader is null)
            {
                Shader = new ShaderOverlay(ShaderFX.FuzzyNoise);
                Shader.AddTag(Tags.Global);
                level.Add(Shader);
            }

            if (part == 1)
            {
                Add(new Coroutine(CutscenePart1(level)));
            }
            else if (part == 2)
            {
                Add(new Coroutine(CutscenePart2(level)));
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public IEnumerator CutscenePart1(Level level)
        {
            Shader.ForceLevelRender = false;
            Shader.Amplitude = 0;
            Player player = level.GetPlayer();
            player.StateMachine.State = 11;
            yield return player.DummyWalkTo(Level.Marker("moveTo").X);
            yield return Textbox.Say("grassShift", ZoomIn, FaceLeft, TempleShake, FallDown, WorldDistort);
            TeleportToRuinsBeforePipes();
        }
        public IEnumerator CutscenePart2(Level level)
        {
            Player player = level.GetPlayer();
            if (SkippedPart1)
            {
                player.StateMachine.State = 0;
                player.Position = level.LevelOffset + RelativeTeleportPosition;
                EndCutscene(level);
                yield break;
            }
            Add(new Coroutine(TempleShake()));
            Shader.ForceLevelRender = true;
            Shader.Amplitude = max;
            player.StateMachine.State = 11;
            player.X = Level.Marker("moveTo").X;
            OnTeleport(Level);
            player.Facing = Facings.Left;
            yield return Textbox.Say("grassShift2", Wait, WaitAgain, Settle, GetBackUp);
            yield return Level.ZoomBack(1);
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            Player player = level.GetPlayer();
            if (player is not null)
            {
                player.StateMachine.State = Player.StNormal;
            }
            if (WasSkipped)
            {
                if (part == 1)
                {
                    SkippedPart1 = true;
                }
                if (level.Session.Level != room)
                {
                    TeleportToRuinsBeforePipes();
                }
                if (part == 2 && Shader != null)
                {
                    Shader.Amplitude = 0;
                    level.Remove(Shader);
                }
            }
            if(WasSkipped || part == 2)
            {
                level.Session.SetFlag("GrassShiftCutsceneWatched");
            }
        }

        public void TeleportToRuinsBeforePipes()
        {
            Player player = Level.GetPlayer();
            InstantTeleport(Level, player, room, null);
        }
        public void OnSkipTeleport(Level level)
        {
            Level = level;
            level.ZoomTarget = 1;
            level.Zoom = 1;
            level.SkippingCutscene = false;
        }
        public IEnumerator FaceLeft()
        {
            Level.GetPlayer().Facing = Facings.Left;
            yield return null;
        }
        public IEnumerator ZoomIn()
        {
            Player player = Level.GetPlayer();
            player.Facing = Facings.Right;
            Coroutine walk = new Coroutine(player.DummyWalkTo(Level.Marker("moveTo").X));
            Add(walk);
            yield return Level.ZoomTo(Level.Marker("moveTo", true), 1.8f, 1);
            while (!walk.Finished)
            {
                yield return null;
            }
            yield return 0.5f;
        }
        public IEnumerator TempleShake()
        {
            Shaking = true;
            Add(new Coroutine(ShakeRoutine()));
            yield return null;
        }
        public IEnumerator ShakeRoutine()
        {
            while (Shaking)
            {
                Level.shakeTimer = Math.Max(Level.shakeTimer, Engine.DeltaTime);
                Level.shakeDirection = Vector2.UnitX;
                yield return null;
            }
            yield return null;
        }
        public IEnumerator FallDown()
        {
            Coroutine ah = new Coroutine(Textbox.Say("grassShiftAh"));
            Add(ah);
            while (!ah.Finished)
            {
                yield return null;
            }
            yield return null;
        }
        public IEnumerator WorldDistort()
        {
            float duration = 4;
            Shader.ForceLevelRender = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                Shader.Amplitude = Calc.LerpClamp(0, max, i);
                yield return null;
            }
            Shader.Amplitude = max;
            yield return 1;
        }
        public void OnTeleport(Level level)
        {
            Level = level;
            level.ZoomFocusPoint = Level.Marker("moveTo", true);
            level.ZoomTarget = 1.8f;
            level.Zoom = 1.8f;
        }
        public IEnumerator Wait()
        {
            yield return 2f;
        }
        public IEnumerator WaitAgain()
        {
            yield return 1f;
        }
        public IEnumerator Settle()
        {
            Shader.ForceLevelRender = true;
            Shader.Amplitude = max;
            float duration = 4;
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                Shader.Amplitude = Calc.LerpClamp(max, 0, i);
                yield return null;
            }
            Shader.ForceLevelRender = false;
            Shader.Amplitude = 0;
            Shaking = false;
            if (Shader is not null)
            {
                Level.Remove(Shader);
            }
            Shader = null;
            yield return 1;
            yield return null;
        }
        public IEnumerator GetBackUp()
        {
            yield return null;
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
                Vector2 val2 = player.Position - level.LevelOffset;
                RelativeTeleportPosition = val2;
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
