using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Flungus;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/BoxTeleport")]
    public class BoxTeleport : Trigger
    {
        public string Marker;
        public bool SnapToGroundOnTeleport;
        public string Room;
        public enum Modes
        {
            LeftToRight,
            TopToBottom,
            None
        }
        public enum Operators
        {
            GreaterThan,
            LessThan
        }
        public Modes Mode;
        public Operators Operator;
        public float Threshold;
        public bool Teleporting;
        public bool IfEqual;
        public FlagData FlagData;

        public BoxTeleport(EntityData data, Vector2 offset) : base(data, offset)
        {
            Marker = data.Attr("targetMarkerId");
            SnapToGroundOnTeleport = data.Bool("snapToGround");
            Room = data.Attr("room");
            Mode = data.Enum<Modes>("mode");
            Operator = data.Enum<Operators>("operator");
            Threshold = data.Float("threshold");
            IfEqual = data.Bool("activateIfEqual");
            FlagData = data.Flag();
        }
        public void DoTeleport(Player player)
        {
            Teleporting = true;
            AddTag(Tags.Global);
            player.DisableMovement();
            new FallWipe(SceneAs<Level>(), false, OnComplete)
            {
                Duration = 0.6f,
                EndTimer = 0.7f
            };
            void OnComplete()
            {
                bool wasNotInvincible = false;

                if (!SaveData.Instance.Assists.Invincible)
                {
                    wasNotInvincible = true;
                    SaveData.Instance.Assists.Invincible = true;
                }
                Level level = SceneAs<Level>();
                TeleportTo(SceneAs<Level>(), player, Room);
                Add(new Coroutine(TeleportRoutine(player, wasNotInvincible, level.Camera)));
                new MountainWipe(SceneAs<Level>(), true, End)
                {
                    Duration = 1f,
                    EndTimer = 0.5f
                };
                void End()
                {
                    Teleporting = false;
                    player.StateMachine.State = 0;
                    RemoveTag(Tags.Global);
                    RemoveSelf();
                }
            }
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

        private IEnumerator TeleportRoutine(Player player, bool wasNotInvincible, Camera camera)
        {
            yield return null;
            foreach (Marker target in SceneAs<Level>().Tracker.GetEntities<Marker>())
            {
                if (Marker == target.ID)
                {
                    player.Position = target.Position + new Vector2(4, 5);
                    if (SnapToGroundOnTeleport)
                    {
                        player.Ground(true);
                    }
                    camera.Position = player.CameraTarget;
                    if (target.Args.TryGetValue("facing", out string value))
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            value = value.ToLower().Replace(" ", "");
                            switch (value)
                            {
                                case "left":
                                    player.Facing = Facings.Left;
                                    break;
                                case "right":
                                    player.Facing = Facings.Right;
                                    break;
                            }
                        }
                    }
                    break;
                }
            }
            if (wasNotInvincible)
            {
                SaveData.Instance.Assists.Invincible = false;
            }
            yield return null;

        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            TryExecute(player);
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            TryExecute(player);
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            TryExecute(player);
        }
        public void TryExecute(Player player)
        {
            if (Teleporting || !FlagData.State) return;
            if (Mode == Modes.None)
            {
                DoTeleport(player);
            }
            else
            {
                float track, side1, side2;
                if (Mode == Modes.LeftToRight)
                {
                    track = player.CenterX;
                    side1 = Left;
                    side2 = Right;
                }
                else
                {
                    track = player.CenterY;
                    side1 = Top;
                    side2 = Bottom;
                }
                float limit = side1 + (side2 - side1) * Threshold;
                bool condition = Operator switch
                {
                    Operators.GreaterThan => IfEqual ? track >= limit : track > limit,
                    Operators.LessThan => IfEqual ? track <= limit : track < limit,
                    _ => false
                };
                if (condition)
                {
                    DoTeleport(player);
                }
            }
        }
    }
}