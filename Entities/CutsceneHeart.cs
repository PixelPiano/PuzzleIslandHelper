using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// PuzzleIslandHelper.CutsceneHeart
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CutsceneHeart")]
    [TrackedAs(typeof(HeartGem))]
    public class CutsceneHeart : Entity
    {
        public EntityID ID;
        private Sprite sprite;
        private string collectSound = "event:/game/07_summit/gem_get";
        private Vector2 moveWiggleDir;
        private string spriteName;
        private float bounceSfxDelay;
        private Wiggler scaleWiggler;
        private Wiggler moveWiggler;
        private EventInstance sfx;
        private Player player;
        private string flag;
        private Entity heart;
        private bool Collected;
        private string room;
        private bool TeleportsPlayer;
        private MemoryTextscene Cutscene;
        private Vector2 HoldPosition;

        public CutsceneHeart(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            spriteName = data.Attr("sprite");
            TeleportsPlayer = data.Bool("teleportsPlayer");
            room = data.Attr("room");
            Collider = new Hitbox(data.Width, data.Height);
            Tag = Tags.TransitionUpdate;
            ID = id;
            flag = data.Attr("flag");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = Scene.Tracker.GetEntity<Player>();

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            switch (spriteName)
            {
                case "green":
                    Cutscene = new MemoryTextscene("GREENHEART");
                    break;
                case "blue":
                    Cutscene = new MemoryTextscene("BLUEHEART");
                    break;
                case "red":
                    Cutscene = new MemoryTextscene("REDHEART");
                    break;
                default: Cutscene = new MemoryTextscene("invalid"); break;
            }
            scene.Add(heart = new Entity(Position));
            heart.Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/cutsceneHeart/"));
            sprite.AddLoop("idle", spriteName, 0.08f);
            sprite.AddLoop("static", spriteName, 1f, 0);
            heart.Collider = new Hitbox(12f, 12f, 4f, 4f);

            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                sprite.Scale = Vector2.One * (1f + f * 0.3f);
            }));
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            heart.Add(moveWiggler);
            heart.Add(new PlayerCollider(OnPlayer));
            sprite.Play("idle");
        }
        public override void Render()
        {
            base.Render();
        }
        private void OnPlayer(Player player)
        {
            Level level = Scene as Level;

            if (player.DashAttacking)
            {
                Add(new Coroutine(Collect(player, level)));
                Collected = true;
                return;
            }
            player.PointBounce(heart.Center);
            moveWiggler.Start();
            scaleWiggler.Start();
            moveWiggleDir = (heart.Center - player.Center).SafeNormalize(Vector2.UnitY);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            if (bounceSfxDelay <= 0f)
            {
                Audio.Play("event:/game/general/crystalheart_bounce", heart.Position);
                bounceSfxDelay = 0.1f;
            }
        }
        private IEnumerator Collect(Player player, Level level)
        {
            AddTag(Tags.Global);
            level.Session.DoNotLoad.Add(ID);
            level.Session.SetFlag("InCutsceneHeartCutscene");
            level.Session.SetFlag(flag);
            Visible = false;
            heart.Visible = false;
            heart.Collidable = false;
            level.Shake();
            SoundEmitter.Play(collectSound, heart);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

            level.Flash(Color.White, drawPlayerOver: true);
            yield return null;
            string thisRoom = level.Session.Level;
            Vector2 SavedPosition = Position + new Vector2(8f, 8f);
            level = SceneAs<Level>();
            Glitch.Value = 0.8f;
            InvertOverlay.playerTimeRate = 0.4f;
            player.DummyGravity = false;
            yield return 0.1f;
            for (float i = 0.4f; i > 0; i -= Engine.RawDeltaTime * 2)
            {
                InvertOverlay.playerTimeRate = i;
                yield return null;
            }
            yield return 0.3f;
            Audio.Play("event:/PianoBoy/invertGlitch2");
            yield return null;
            yield return null;
            if (TeleportsPlayer)
            {
                InstantTeleport(SceneAs<Level>(), player, room, true, 0, 0);
                yield return null;
                player = level.Tracker.GetEntity<Player>();
            }
            if (Cutscene != null)
            {
                SceneAs<Level>().Add(Cutscene);
            }
            Glitch.Value = 0.5f;
            InvertOverlay.playerTimeRate = 0f;
            player.DummyGravity = false;
            float mult = TeleportsPlayer ? 1 : 5;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime * mult)
            {
                Glitch.Value = Calc.Random.Range(0.5f, 1f);
                yield return null;
            }
            if (TeleportsPlayer)
            {
                while (Cutscene.InCutscene)
                {
                    yield return null;
                }
            }
            else
            {
                yield return 1;
                if (Cutscene != null)
                {
                    level.Remove(Cutscene);
                }
            }
            for (float i = 0; i < 2; i += Engine.RawDeltaTime * mult)
            {
                float value = Ease.QuintIn(i / 2);
                if (value > 0.5f)
                {
                    Glitch.Value = Calc.Random.Range(value, 1);
                }
                if (i > 1)
                {
                    InvertOverlay.playerTimeRate = Ease.QuintIn(i - 1);
                }
                yield return null;
            }
            Glitch.Value = 0;
            InvertOverlay.playerTimeRate = 1;
            player.DummyGravity = true;
            Audio.Play("event:/PianoBoy/invertGlitch2");
            yield return null;
            yield return null;
            if (TeleportsPlayer)
            {
                InstantTeleport(SceneAs<Level>(), player, thisRoom, true, 0, 0);
                yield return null;
                level = SceneAs<Level>();
                player = level.Tracker.GetEntity<Player>();
                yield return null;
            }
            SoundEmitter.Play(collectSound, heart);
            for (int i = 0; i < 10; i++)
            {
                Scene.Add(new AbsorbOrb(Position + new Vector2(8f, 8f), player));
            }
            InvertOverlay.playerTimeRate = 1;
            Engine.TimeRate = 1;
            level.Session.SetFlag("InCutsceneHeartCutscene", false);
            RemoveTag(Tags.Global);
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
        public override void Update()
        {
            base.Update();
            if (player == null)
            {
                return;
            }
            if (!Collected)
            {
                bounceSfxDelay -= Engine.DeltaTime;
                sprite.Position = moveWiggleDir * moveWiggler.Value * -8f;
            }
        }
        public static void InstantTeleport(Scene scene, Player player, string room, bool sameRelativePosition, float positionX, float positionY)
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
                if (sameRelativePosition)
                {
                    level.Camera.Position = level.LevelOffset + val3;
                    level.Add(player);
                    player.Position = level.LevelOffset + val2;
                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                    //return;
                }
                else
                {
                    Vector2 val4 = new Vector2(positionX, positionY) - level.LevelOffset - val2;
                    level.Camera.Position = level.LevelOffset + val3 + val4;
                    level.Add(player);
                    player.Position = new Vector2(positionX, positionY);
                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset + val4);
                }
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }
    }
}