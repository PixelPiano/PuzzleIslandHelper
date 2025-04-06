using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.FadeWarp
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FadeWarp")]
    [Tracked]
    public class FadeWarp : Entity
    {
        private readonly string Room;
        private readonly bool usesWipe;
        private readonly Color FadeColor;
        private static float opacity;
        private readonly float FadeSpeed = 0.5f;
        private readonly float FadeSpeedMultiplier = 1;
        private Level l;
        private readonly Coroutine routine;
        private Player player;
        private readonly Sprite sprite;
        private readonly string path;
        private Entity entity;
        private readonly int SpriteDepth;
        private readonly bool usesTarget;
        private readonly string targetId;
        private readonly bool usesSprite;
        private readonly string flag;
        private readonly bool usesFlag;
        public bool isDoor;
        public int keyId;
        private bool Unlocked;
        private string dialogue;
        private EntityID id;
        public static bool Transitioning;
        private static Holdable Held;
        private readonly TalkComponent talk;
        public void ResetDoor()
        {
            Unlocked = false;
            PianoModule.Session.DoorIds.Remove(id);
        }
        public FadeWarp(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            this.id = id;
            keyId = data.Int("keyId", -1);
            isDoor = data.Bool("actsLikeDoor", false);
            dialogue = data.Attr("dialog", "noDialogueEntered");

            targetId = data.Attr("targetId");
            flag = data.Attr("flag");
            usesFlag = data.Bool("usesFlag");
            path = data.Attr("doorType");
            usesSprite = data.Bool("usesSprite");
            usesTarget = data.Bool("usesTarget");
            SpriteDepth = data.Int("spriteDepth");

            sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/fadeWarp/");
            sprite.AddLoop("idle", "ruinsHouseDoor" + path, 1f);
            sprite.FlipX = data.Bool("flipX");

            Collider = new Hitbox(sprite.Width, sprite.Height);
            usesWipe = data.Bool("useWipeInstead", false);
            FadeColor = data.HexColor("color");
            FadeSpeedMultiplier = data.Float("fadeSpeed");
            Room = data.Attr("roomName");
            Depth = -1000000;
            routine = new Coroutine(Sequence());
            Add(talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), Vector2.UnitX * Width / 2, Interact));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (/*ValidPath && */usesSprite)
            {
                scene.Add(entity = new Entity(Position));
                entity.Add(sprite);
                entity.Depth = SpriteDepth;
                sprite.Play("idle");
            }
        }
        public override void Update()
        {
            base.Update();
            if (usesFlag && !isDoor)
            {
                if (Scene is not null)
                {
                    talk.Enabled = SceneAs<Level>().Session.GetFlag(flag);
                }
            }


            if (routine.Finished && !usesWipe)
            {
                RemoveTag(Tags.Global);
                RemoveSelf();
            }
        }
        private IEnumerator Cutscene(Player player)
        {
            AddTag(Tags.Global);
            talk.Enabled = false;
            if (!usesWipe)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime * FadeSpeed * FadeSpeedMultiplier)
                {
                    opacity = i;
                    yield return null;
                }
                TeleportTo(SceneAs<Level>(), player, Room);
                yield return 0.3f;
                for (float i = 1; i > 0; i -= Engine.DeltaTime * FadeSpeed * FadeSpeedMultiplier)
                {
                    opacity = i;
                    yield return null;
                }
                yield return null;
            }
            else
            {
                this.player = player;
                player.StateMachine.State = 11;
                Transitioning = true;
                new FallWipe(SceneAs<Level>(), false, OnComplete)
                {
                    Duration = 0.6f,
                    EndTimer = 0.7f
                };
            }
            yield return null;
        }
        private IEnumerator DialogCutscene(Player player, int keyId, string dialogue)
        {
            player.StateMachine.State = 11;
            bool unlock = false;
            bool onlyDialog = keyId == -2;
            bool condition1 = keyId == -1;
            bool condition2 = false;
            int counter = 0;
            if (onlyDialog)
            {
                yield return Textbox.Say(dialogue);
                player.StateMachine.State = 0;
                yield break;
            }
            if (PianoModule.Session.DoorIds.Contains(id))
            {
                Add(new Coroutine(Cutscene(player)));
                yield break;
            }
            if (!condition1)
            {
                foreach (FadeWarpKey.KeyData data in PianoModule.Session.Keys)
                {
                    if (data.id == keyId && !Unlocked)
                    {
                        condition2 = true;
                        unlock = true;
                        break;
                    }
                    counter++;
                }
                if (counter == PianoModule.Session.Keys.Count)
                {
                    condition1 = true;
                }
            }
            if (condition1)
            {
                if (!string.IsNullOrEmpty(dialogue) && (!usesFlag || usesFlag && !SceneAs<Level>().Session.GetFlag(flag)))
                {
                    yield return Textbox.Say(dialogue);
                }
                else
                {
                    Add(new Coroutine(Cutscene(player)));
                }
            }
            if (condition2 && unlock)
            {
                if (!Unlocked)
                {
                    //todo: play unlocking sound
                    EventInstance sfx;
                    sfx = Audio.Play("event:/PianoBoy/stool_hit_ground", Position);
                    while (Audio.IsPlaying(sfx))
                    {
                        yield return null;
                    }
                    if (!PianoModule.Session.DoorIds.Contains(id) && keyId != -1 && keyId != -2)
                    {
                        PianoModule.Session.DoorIds.Add(id);
                    }
                    Unlocked = true;
                }
                Add(new Coroutine(Cutscene(player)));
            }
            player.StateMachine.State = 0;
            yield return null;

        }
        private void Interact(Player player)
        {
            if (isDoor || usesFlag && !SceneAs<Level>().Session.GetFlag(flag))
            {
                Add(new Coroutine(DialogCutscene(player, keyId, dialogue)));
            }
            else
            {
                Add(new Coroutine(Cutscene(player)));
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
                Duration = 1f,
                EndTimer = 0.5f
            };
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
                            //SetOnGround(player);
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
            if (Held != null)
            {
                player.Add(Held);
            }
            yield return null;

        }
        private void End()
        {
            Transitioning = false;
            player.StateMachine.State = 0;
            RemoveTag(Tags.Global);
            RemoveSelf();
        }
        /*        private void SetOnGround(Entity entity)
        {
            if (Scene as Level is not null)
            {
                try
                {
                    while (!entity.CollideCheck<SolidTiles>())
                    {
                        entity.Offset.Y += 1;
                    }
                }
                catch
                {
                    Console.WriteLine($"{entity} could not find any SolidTiles below it to set it's Y Offset to");
                }
                entity.Offset.Y -= 1;
            }
        }*/
        public override void Render()
        {
            base.Render();

            if (Scene as Level is null)
            {
                return;
            }
            l = Scene as Level;

            if (!usesWipe)
            {
                Draw.Rect(l.Bounds, FadeColor * opacity);
            }
        }
        private IEnumerator Sequence()
        {

            for (float i = 0; i < 1; i += Engine.DeltaTime * FadeSpeed * FadeSpeedMultiplier)
            {
                opacity = i;
                yield return null;
            }
            TeleportTo(SceneAs<Level>(), SceneAs<Level>().Tracker.GetEntity<Player>(), Room);
            yield return 0.3f;
            for (float i = 1; i > 0; i -= Engine.DeltaTime * FadeSpeed * FadeSpeedMultiplier)
            {
                opacity = i;
                yield return null;
            }
            yield return null;
        }
        public static void TeleportTo(Scene scene, Player player, string room, Player.IntroTypes introType = Player.IntroTypes.Transition, Vector2? nearestSpawn = null)
        {
            Level level = scene as Level;
            if (level != null)
            {
                if (player.Holding != null)
                {
                    Held = player.Holding;
                }
                level.OnEndOfFrame += delegate
                {
                    level.TeleportTo(player, room, introType, nearestSpawn);
                };
            }
        }
    }
}