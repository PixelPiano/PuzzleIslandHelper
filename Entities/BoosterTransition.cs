//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BoosterTransition")]
    [Tracked]
    public class BoosterTransition : Trigger
    {
        private string RoomName;
        private bool InRoutine;
        private string flag;
        private bool Teleported;
        private bool inverted;
        private bool LeftSide;
        private bool RightSide;
        private bool UpSide;
        private bool DownSide;
        private bool State
        {
            get
            {
                if (string.IsNullOrEmpty(flag))
                {
                    return !inverted;
                }
                bool flagState = SceneAs<Level>().Session.GetFlag(flag);
                return inverted ? !flagState : flagState;
            }
        }
        public bool PlayerIsBoosting
        {
            get
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player is null)
                {
                    return false;
                }
                int state = player.StateMachine.State;
                if (state == Player.StBoost || state == Player.StRedDash)
                {
                    return true;
                }
                return false;
            }
        }
        public Vector2 prevPosition;
        public BoosterTransition(EntityData data, Vector2 offset) : base(data, offset)
        {
            RoomName = data.Attr("roomName");
            flag = data.Attr("flag");
            inverted = data.Bool("invertFlag");
            Collider = new Hitbox(data.Width, data.Height);
            LeftSide = data.Bool("fromLeft");
            RightSide = data.Bool("fromRight");
            UpSide = data.Bool("fromTop");
            DownSide = data.Bool("fromBottom");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Vector2 direction = Vector2.Normalize(player.Speed);
            if (FromValidDirection(direction))
            {
                if (PlayerIsBoosting && State)
                {
                    Add(new Coroutine(TeleportRoutine(player.LastBooster)));
                }
            }

        }
        private bool FromValidDirection(Vector2 dir)
        {
            if (LeftSide && dir.X > 0)
            {
                return true;
            }
            if (RightSide && dir.X < 0)
            {
                return true;
            }
            if (UpSide && dir.Y > 0)
            {
                return true;
            }
            if (DownSide && dir.Y < 0)
            {
                return true;
            }
            return false;
        }
        public override void Update()
        {
            base.Update();
            /*            if (Booster is not null && !Collidable && !Booster.BoostingPlayer)
                        {
                            Alert alarm = Alert.Create(Alert.AlarmMode.Oneshot,
                                delegate
                                {
                                    if (Booster is not null && !InRoutine)
                                    {
                                        SceneAs<Level>().Remove(Booster);
                                    }
                                }
                                , 0.5f);
                        }*/
        }
        private IEnumerator TeleportRoutine(Booster booster)
        {
            if (booster is null)
            {
                yield break;
            }
            InRoutine = true;
            Collidable = false;
            Player player = Scene.Tracker.GetEntity<Player>();
            AddTag(Tags.Global);
            InstantTeleport(SceneAs<Level>(), player, RoomName, booster);
            Level level = SceneAs<Level>();
            while (booster != null && booster.BoostingPlayer)
            {
                yield return null;
            }
            yield return 0.5f;
            if (booster != null)
            {
                level.Remove(booster);
            }
            InRoutine = false;
            RemoveSelf();
        }

        public static void InstantTeleport(Scene scene, Player player, string room, Booster booster)
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
                booster.AddTag(Tags.Global | Tags.Persistent);
                Vector2 levelOffset = level.LevelOffset;
                Vector2 val2 = player.Position - level.LevelOffset;
                Vector2 val3 = level.Camera.Position - level.LevelOffset;
                Facings facing = player.Facing;
                Booster savedBooster = booster;
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
                for (int i = 0; i < level.ParticlesBG.particles.Length; i++)
                {
                    if (level.ParticlesBG.particles[i].Type == booster.particleType)
                    {
                        level.ParticlesBG.particles[i].Position += newPos - levelOffset;
                    }
                }

                booster.Position = player.Position;
                booster.BoostingPlayer = true;
                booster.Visible = true;
                booster.cannotUseTimer = 0.45f;
                booster.outline.Visible = false;
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
        }
    }
}
