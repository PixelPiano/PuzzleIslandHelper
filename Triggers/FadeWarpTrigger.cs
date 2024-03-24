using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/FadeWarpTrigger")]
    public class FadeWarpTrigger : Trigger
    {
        private string Room;
        private bool usesTarget;
        private string targetId;
        private Player player;

        public FadeWarpTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Room = data.Attr("roomName");
            usesTarget = data.Bool("usesTarget");
            targetId = data.Attr("targetId");
        }
        private void End()
        {
            player.StateMachine.State = 0;
            RemoveTag(Tags.Global);
            RemoveSelf();
        }
        private void SetOnGround(Entity entity)
        {

            if (Scene as Level is not null)
            {
                try
                {
                    while (!entity.CollideCheck<SolidTiles>())
                    {
                        entity.Position.Y += 1;
                    }
                }
                catch
                {
                    Console.WriteLine($"{entity} could not find any SolidTiles below it to set it's Y Position to");
                }
                entity.Position.Y -= 1;
            }
        }
        private IEnumerator TeleportPlayer(Player player, bool wasNotInvincible, Camera camera)
        {
            yield return null;
            if (usesTarget)
            {
                foreach (FadeWarpTarget target in SceneAs<Level>().Tracker.GetEntities<FadeWarpTarget>())
                {
                    if (targetId == target.id && !string.IsNullOrEmpty(target.id))
                    {
                        player.Position = target.Position + new Vector2(15, 18);

                        if (target.onGround)
                        {
                            SetOnGround(player);
                        }
                        camera.Position = player.CameraTarget;
                        break;
                    }
                }
            }
            if (wasNotInvincible)
            {
                SaveData.Instance.Assists.Invincible = false;
            }
            yield return null;

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
        private void OnComplete()
        {
            bool wasNotInvincible = false;

            if (!SaveData.Instance.Assists.Invincible)
            {
                wasNotInvincible = true;
                SaveData.Instance.Assists.Invincible = true;
            }
            Level level = SceneAs<Level>();
            TeleportTo(SceneAs<Level>(), player, Room);
            Add(new Coroutine(TeleportPlayer(player, wasNotInvincible, level.Camera)));
            new MountainWipe(SceneAs<Level>(), true, End)
            {
                Duration = 0.4f,
                EndTimer = 1f
            };
        }
        private IEnumerator Cutscene(Player player)
        {
            AddTag(Tags.Global);

            this.player = player;
            player.StateMachine.State = 11;
            new FallWipe(SceneAs<Level>(), false, OnComplete)
            {
                Duration = 0.6f,
                EndTimer = 0.7f
            };
            yield return null;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnEnter(Player player)
        {
            Add(new Coroutine(Cutscene(player)));
        }

        public override void OnLeave(Player player)
        {
        }
    }
}
