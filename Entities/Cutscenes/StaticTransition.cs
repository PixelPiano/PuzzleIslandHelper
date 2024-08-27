using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class StaticTransition : CutsceneEntity
    {
        public ShaderOverlay Shader;
        private string room;
        public StaticTransition(string room) : base()
        {
            this.room = room;
            Tag |= Tags.TransitionUpdate | Tags.Persistent;
        }

        public override void OnBegin(Level level)
        {
            if (Shader is null)
            {
                Shader = new ShaderOverlay(ShaderFX.FuzzyNoise);
                Shader.AddTag(Tags.Global);
                level.Add(Shader);
                Add(new Coroutine(Cutscene(level)));
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Shader.RemoveSelf();
        }
        public IEnumerator Cutscene(Level level)
        {
            Player player = level.GetPlayer();
            player.StateMachine.State = 11;
            Shader.ForceLevelRender = true;
            Shader.Amplitude = 1;
            yield return 3f;
            TeleportToRoom();
            yield return null;
            level = Scene as Level;
            EndCutscene(level);
            yield return null;
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
                if (level.Session.Level != room)
                {
                    TeleportToRoom();
                }
                if (Shader != null)
                {
                    Shader.RemoveSelf();
                }
            }
        }

        public void TeleportToRoom()
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
      
        public void OnTeleport(Level level)
        {
            Level = level;
            level.ZoomFocusPoint = Level.Marker("moveTo", true);
            level.ZoomTarget = 1.8f;
            level.Zoom = 1.8f;
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
