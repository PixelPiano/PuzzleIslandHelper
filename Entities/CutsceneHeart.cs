using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.CutsceneHeart
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CutsceneHeart")]
    [TrackedAs(typeof(HeartGem))]
    public class CutsceneHeart : Entity
    {
        public EntityID ID;
        public Sprite Sprite;
        private string collectSound = "event:/game/07_summit/gem_get";
        private Vector2 moveWiggleDir;
        private string spriteName;
        private float bounceSfxDelay;
        private Wiggler scaleWiggler;
        private Wiggler moveWiggler;
        private EventInstance sfx;
        private Player player;
        public string flag;
        public Entity heart;
        private bool Collected;
        private string room;
        private string returnRoom;
        private bool TeleportsPlayer;
        private Cutscene cutscene;
        public CutsceneHeart(Vector2 position, string sprite, string flag, bool teleportsPlayer, string room, string returnRoom, bool flipped, EntityID id) : base(position)
        {
            spriteName = sprite;
            TeleportsPlayer = teleportsPlayer;
            this.room = room;
            this.returnRoom = returnRoom;
            ID = id;
            this.flag = flag;
            Tag = Tags.TransitionUpdate;
            heart = new Entity(Position)
            {
                Collider = new Hitbox(12f, 12f, 4f, 4f)
            };
            Collider = new Hitbox(12f, 12f, 4f, 4f);
            heart.Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/cutsceneHeart/"));
            Sprite.AddLoop("idle", spriteName, 0.08f);
            Sprite.AddLoop("static", spriteName, 1f, 0);
            Sprite.X = -(Sprite.Width - 12);
            if (flipped)
            {
                Sprite.CenterOrigin();
                Sprite.Position += new Vector2(Sprite.Width / 2, Sprite.Height / 2);
                Sprite.Scale = -Vector2.One;
            }
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                Sprite.Scale = Vector2.One * (1f + f * 0.3f);
            }));
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            heart.Add(moveWiggler);
            heart.Add(new PlayerCollider(OnPlayer));
            Sprite.Play("idle");
        }
        public CutsceneHeart(Vector2 position, string sprite, string flag, EntityID id) : base(position)
        {
            spriteName = sprite;
            ID = id;
            Tag = Tags.TransitionUpdate;
            this.flag = flag;
            heart = new Entity(Position)
            {
                Collider = new Hitbox(12f, 12f, 4f, 4f)
            };
            Collider = new Hitbox(12f, 12f, 4f, 4f);
            Collidable = false;
            heart.Collidable = false;
            heart.Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/cutsceneHeart/"));
            Sprite.AddLoop("idle", spriteName, 0.08f);
            Sprite.AddLoop("static", spriteName, 1f, 0);
            Sprite.X = -(Sprite.Width - 12);
            Sprite.Play("idle");
        }
        public CutsceneHeart(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset, data.Attr("sprite"), data.Attr("flag"), data.Bool("teleportsPlayer"), data.Attr("room"), data.Attr("returnRoom"), data.Bool("flipped"), id)
        {
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = scene.GetPlayer();

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(heart);
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
        public class Cutscene : MemoryTextscene
        {
            public Action onEnd;
            public Cutscene(string dialog) : base(dialog)
            {
            }
            public override void OnEnd(Level level)
            {
                base.OnEnd(level);
                onEnd?.Invoke();
            }

        }
        public void SetRate(float value)
        {
            InvertOverlay.UseNormalTimeRate = false;
            InvertOverlay.ForceState(true);
            InvertOverlay.playerTimeRate = value;
        }
        private IEnumerator Collect(Player player, Level level)
        {
            var hinventory = PianoModule.Session.HeartInventory;
            if (!hinventory.Collected.ContainsKey(spriteName))
            {
                hinventory.Collected.Add(spriteName, flag);
            }
            else
            {
                hinventory.Collected[spriteName] = flag;
            }
            AddTag(Tags.Global);
            level.Session.DoNotLoad.Add(ID);
            level.Session.SetFlag("InCutsceneHeartCutscene");
            level.Session.SetFlag(flag);
            Visible = false;
            heart.Visible = false;
            heart.Collidable = false;
            Collidable = false;
            level.Shake();
            SoundEmitter.Play(collectSound, heart);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

            level.Flash(Color.White, drawPlayerOver: true);
            yield return null;
            string thisRoom = level.Session.Level;
            Vector2 SavedPosition = Position + new Vector2(8f, 8f);
            level = SceneAs<Level>();
            Glitch.Value = 0.8f;
            SetRate(0.4f);
            player.DummyGravity = false;
            yield return 0.1f;
            for (float i = 0.4f; i > 0; i -= Engine.RawDeltaTime * 2)
            {
                SetRate(i);
                yield return null;
            }
            yield return 0.3f;
            Audio.Play("event:/PianoBoy/invertGlitch2");
            yield return null;
            yield return null;
            if (TeleportsPlayer)
            {
                InstantTeleport(SceneAs<Level>(), player, room, true);
                yield return null;
                player = level.Tracker.GetEntity<Player>();
            }
            cutscene = spriteName switch
            {
                "green" => new Cutscene("GREENHEART"),
                "blue" => new Cutscene("BLUEHEART"),
                "red" => new Cutscene("REDHEART"),
                _ => new Cutscene("invalid"),
            };
            bool wasSkipped = false;
            cutscene.onEnd = delegate { wasSkipped = cutscene.WasSkipped; };
            SceneAs<Level>().Add(cutscene);
            Glitch.Value = 0.5f;
            SetRate(0f);
            player.DummyGravity = false;
            float mult = TeleportsPlayer ? 1 : 5;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime * mult)
            {
                Glitch.Value = Calc.Random.Range(0.5f, 1f);
                yield return null;
            }
            if (TeleportsPlayer)
            {
                while (cutscene.InCutscene)
                {
                    yield return null;
                }
            }
            else
            {
                yield return 1;
                if (cutscene != null)
                {
                    level.Remove(cutscene);
                }
            }
            yield return null;
            if (!wasSkipped)
            {
                for (float i = 0; i < 2; i += Engine.RawDeltaTime * mult)
                {
                    float value = Ease.QuintIn(i / 2);
                    if (value > 0.5f)
                    {
                        Glitch.Value = Calc.Random.Range(value, 1);
                    }
                    if (i > 1)
                    {
                        SetRate(Ease.QuintIn(i - 1));
                    }
                    yield return null;
                }
            }
            Glitch.Value = 0;
            SetRate(1);
            player.DummyGravity = true;
            Audio.Play("event:/PianoBoy/invertGlitch2");
            yield return null;
            yield return null;
            if (TeleportsPlayer)
            {
                string room = string.IsNullOrEmpty(returnRoom) ? thisRoom : returnRoom;
                InstantTeleport(SceneAs<Level>(), player, room, spriteName != "green");
                yield return null;
                level = SceneAs<Level>();
                player = level.Tracker.GetEntity<Player>();
                yield return null;
            }
            SoundEmitter.Play(collectSound, player);
            for (int i = 0; i < 10; i++)
            {
                Scene.Add(new AbsorbOrb(Position + new Vector2(8f, 8f), player));
            }
            SetRate(1);
            InvertOverlay.ResetState();
            level.Session.SetFlag("InCutsceneHeartCutscene", false);
            player.StateMachine.State = Player.StNormal;
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
                if (moveWiggler != null)
                {
                    Sprite.Position = moveWiggleDir * moveWiggler.Value * -8f - (Vector2.UnitX * 4);
                }
            }
        }
        public static void InstantTeleport(Scene scene, Player player, string room, bool same)
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
                //return;

                level.Camera.Position = level.LevelOffset + val3;
                level.Add(player);
                if (same)
                {
                    player.Position = level.LevelOffset + val2;
                }
                else if (session.RespawnPoint.HasValue)
                {
                    player.Position = session.RespawnPoint.Value;
                    level.Camera.Position = level.LevelOffset;
                }
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);

                Vector2 newPos = level.LevelOffset;

                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }
    }
}