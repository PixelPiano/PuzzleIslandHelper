using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/RuinsDoor")]
    [Tracked]
    public class RuinsDoor : Entity
    {
        private readonly string room;
        private Player player;
        private readonly Sprite sprite;
        private readonly string path;
        private readonly string targetDoorId;
        private readonly string doorId;
        private readonly string flag;
        public int KeyId;
        private bool unlocked;
        private string dialogue;
        private EntityID id;
        public static bool Transitioning;
        public static string TargetID;
        private readonly TalkComponent talk;
        public bool FlagEmpty => string.IsNullOrEmpty(flag);
        public void ResetDoor()
        {
            unlocked = false;
            PianoModule.Session.DoorIds.Remove(id);
        }
        public RuinsDoor(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            this.id = id;
            KeyId = data.Int("keyId", -1);
            dialogue = data.Attr("dialog", "noDialogueEntered");
            targetDoorId = data.Attr("targetDoorId");
            doorId = data.Attr("doorId");
            flag = data.Attr("flag");
            path = data.Attr("doorType");
            sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/fadeWarp/");
            sprite.AddLoop("idle", "ruinsHouseDoor" + path, 1f);
            sprite.FlipX = data.Bool("flipX");
            Add(sprite);
            sprite.Play("idle");

            Collider = new Hitbox(sprite.Width, sprite.Height);
            room = data.Attr("roomName");
            Depth = data.Int("depth");
            Add(talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), Vector2.UnitX * Width / 2, Interact));
        }
        private void Transition(Player player)
        {
            AddTag(Tags.Global);
            talk.Enabled = false;
            this.player = player;
            player.StateMachine.State = 11;
            Transitioning = true;
            new FallWipe(SceneAs<Level>(), false, OnComplete)
            {
                Duration = 0.6f,
                EndTimer = 0.7f
            };
        }
        private IEnumerator DialogCutscene(Player player, int keyId, string dialogue)
        {
            player.StateMachine.State = 11;
            bool unlock = false;
            bool onlyDialog = keyId == -2;
            bool noKey = keyId == -1;
            bool hasCorrectKey = false;
            if (onlyDialog)
            {
                yield return Textbox.Say(dialogue);
                player.StateMachine.State = 0;
                yield break;
            }
            if (PianoModule.Session.DoorIds.Contains(id))
            {
                Transition(player);
                yield break;
            }
            if (!noKey)
            {
                foreach (FadeWarpKey.KeyData data in PianoModule.Session.Keys)
                {
                    if (data.id == keyId && !unlocked)
                    {
                        hasCorrectKey = true;
                        unlock = true;
                        break;
                    }
                }
                if (!unlock)
                {
                    noKey = true;
                }
            }
            if (noKey)
            {
                if (!string.IsNullOrEmpty(dialogue) && (FlagEmpty || (!FlagEmpty && !SceneAs<Level>().Session.GetFlag(flag))))
                {
                    yield return Textbox.Say(dialogue);
                }
                else
                {
                    Transition(player);
                }
            }
            else if (hasCorrectKey && unlock)
            {
                if (!unlocked)
                {
                    //todo: play unlocking sound
                    EventInstance sfx = Audio.Play("event:/PianoBoy/stool_hit_ground", Position);
                    while (Audio.IsPlaying(sfx))
                    {
                        yield return null;
                    }
                    if (!PianoModule.Session.DoorIds.Contains(id) && keyId != -1 && keyId != -2)
                    {
                        PianoModule.Session.DoorIds.Add(id);
                    }
                    unlocked = true;
                }
                Transition(player);
            }
            player.StateMachine.State = 0;
            yield return null;

        }
        private void Interact(Player player)
        {
            if (!string.IsNullOrEmpty(dialogue) || KeyId != -1)
            {
                Add(new Coroutine(DialogCutscene(player, KeyId, dialogue)));
            }
            else
            {
                Transition(player);
            }

        }
        private void OnComplete()
        {
            Level level = SceneAs<Level>();
            TeleportTo(SceneAs<Level>(), player, room, this);
            new MountainWipe(SceneAs<Level>(), true, End)
            {
                Duration = 1f,
                EndTimer = 0.5f
            };
        }
        private void End()
        {
            Transitioning = false;
            player.StateMachine.State = 0;
            RemoveTag(Tags.Global);
            RemoveSelf();
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
        }
        [OnUnload]
        public static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= Level_OnLoadLevel;
        }

        private static void Level_OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (Transitioning)
            {
                if (level.GetPlayer() is not Player player) return;
                foreach (RuinsDoor door in level.Tracker.GetEntities<RuinsDoor>())
                {
                    if (door.doorId == TargetID)
                    {
                        player.Position = level.GetSpawnPoint(door.Position);
                        level.Camera.Position = player.CameraTarget;
                        break;
                    }
                }
            }
            TargetID = "";
            Transitioning = false;
        }
        public static void TeleportTo(Scene scene, Player player, string room, RuinsDoor from)
        {
            Level level = scene as Level;
            if (level != null)
            {
                Transitioning = true;
                TargetID = from.targetDoorId;
                level.OnEndOfFrame += delegate
                {
                    level.TeleportTo(player, room, Player.IntroTypes.Transition);
                };
            }
        }
    }
}