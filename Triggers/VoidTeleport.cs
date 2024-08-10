
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/VoidTeleport")]
    [Tracked]
    public class VoidTeleport : Trigger
    {
        public string Room;
        private bool used;
        public VoidTeleport(EntityData data, Vector2 offset) : base(data, offset)
        {
            Room = data.Attr("room");
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (used) return;
            used = true;
            Scene.Add(new PersistantGlitch(Teleport));
        }
        public void Teleport()
        {
            InstantTeleport(Scene, Room);
        }
        public static void InstantTeleport(Scene scene, string room)
        {
            Level level = scene as Level;
            if (level == null)
            {
                return;
            }
            Player player = level.GetPlayer();
            if (player == null) return;
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

                player.Position = level.LevelOffset + val2;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);

                Vector2 newPos = level.LevelOffset;

                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }
        public class PersistantGlitch : Entity
        {
            public Action WhenHitMax;
            public PersistantGlitch(Action whenHitMax) : base()
            {
                WhenHitMax = whenHitMax;
                AddTag(Tags.Global);
                Add(new Coroutine(GlitchRoutine()));
            }
            private IEnumerator GlitchRoutine()
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
                {
                    Glitch.Value = i;
                    yield return null;
                }
                Glitch.Value = 1;
                Audio.Play("event:/PianoBoy/invertGlitch2");
                WhenHitMax?.Invoke();
                yield return null;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
                {
                    Glitch.Value = 1 - i;
                    yield return null;
                }
                Glitch.Value = 0;
                RemoveSelf();
            }
        }
    }
}
