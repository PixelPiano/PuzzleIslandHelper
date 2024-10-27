using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using static Celeste.Player;
using VivHelper.Triggers;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/SeamlessTeleport")]
    [Tracked]
    public class SeamlessTeleportTrigger : Trigger
    {
        public string Room;
        public SeamlessTeleportTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Room = data.Attr("roomName");
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            InstantTeleport(player.Scene, player, Room, true);
        }
        public static void InstantTeleport(Scene scene, Player player, string nextLevel, bool relative)
        {
            Level level = scene as Level;
            level.OnEndOfFrame += delegate
            {
                LevelData nextData = level.Session.MapData.Get(nextLevel);
                if(nextData == null) return;

                Vector2 levelOffset = level.LevelOffset;
                Vector2 playerPositionInLevel = player.Position - levelOffset;
                Vector2 cameraPositionInLevel = level.Camera.Position - levelOffset;
                Facings facing = player.Facing;
                level.Remove(player);
                level.Displacement.Clear();
                level.UnloadLevel();
                level.Session.Level = nextLevel;
                Session session = level.Session;
                Level level2 = level;

                session.RespawnPoint = session.MapData.Get(nextLevel) is LevelData data && relative ? data.Position + playerPositionInLevel : level.Bounds.TopLeft();
                level.Session.FirstLevel = false;
                level.Add(player);
                level.LoadLevel(IntroTypes.Transition);
                player.Position = level.LevelOffset + playerPositionInLevel;
                level.Camera.Position = level.LevelOffset + cameraPositionInLevel;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };






            /*
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
                level.LoadLevel(Player.IntroTypes.None);

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
            };*/
        }
    }
}
