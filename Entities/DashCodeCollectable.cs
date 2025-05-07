using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// PuzzleIslandHelper.DashCodeCollectable
//Code is a modified combination of FrostHelper's "Dash Code Trigger" and XaphanHelper's "Custom Collectable Entity"
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DashCodeCollectable")]
    [TrackedAs(typeof(HeartGem))]
    public class DashCodeCollectable : Entity
    {
        private readonly List<string> currentInputs = [];
        private Wiggler scaleWiggler;
        private Wiggler moveWiggler;
        private Sprite sprite;
        private Entity heart;
        private Vector2 heartOffset;
        private Vector2 moveWiggleDir;
        public EntityID ID;
        private readonly string[] code;
        private readonly string collectSound = "event:/game/07_summit/gem_get";
        private readonly string spriteName;
        private readonly string audio;
        public string CollectableCollectedFlag;
        public string CollectableSpawnedFlag;
        public string CustomFlagOnCollected;
        public bool Collected { get; private set; }
        public bool Spawned { get; private set; }
        public bool InArea { get; private set; }
        private float bounceSfxDelay;
        private bool gotCode;
        private readonly bool onlyFlag;
        private readonly bool canRespawn;
        private readonly bool resetFlagOnAdded;
        private readonly bool usesBounds;
        private readonly bool visibleBounds;
        public readonly bool IsHeart;

        public DashCodeCollectable(Vector2 position, bool isHeart) :
            base(position)
        {
            IsHeart = isHeart;
        }
        public DashCodeCollectable(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset, data.Attr("Texture", "objects/PuzzleIslandHelper/dashCodeCollectable/miniHeart").Contains("miniHeart"))
        {
            onlyFlag = data.Bool("noCollectable");
            resetFlagOnAdded = data.Bool("flagDebug", true);
            canRespawn = data.Bool("canRespawn");
            usesBounds = data.Bool("usesBounds");
            visibleBounds = data.Bool("visibleBounds");
            CustomFlagOnCollected = data.Attr("flagOnCollected");
            code = data.Attr("code").Replace(" ", "").Split(',');
            audio = data.Attr("event", "event:/new_content/game/10_farewell/glitch_short");
            spriteName = data.Attr("Texture", "objects/PuzzleIslandHelper/dashCodeCollectable/miniHeart");
            heartOffset = data.Nodes[0];
            Collider = new Hitbox(data.Width, data.Height);
            Tag = Tags.TransitionUpdate;
            ID = id;
            CollectableSpawnedFlag = "DashCodeCollectableSpawned:" + id.ToString();
            CollectableCollectedFlag = "DashCodeCollectableCollected:" + id.ToString();
            Add(new DashListener()
            {
                OnDash = dir =>
                {
                    string text = "";
                    dir = dir.CorrectJoystickPrecision();
                    text = dir.Y < 0f ? "U" : dir.Y > 0f ? "D" : "";
                    text += dir.X < 0f ? "L" : dir.X > 0f ? "R" : "";
                    if (!InArea && usesBounds)
                    {
                        text = "";
                    }
                    currentInputs.Add(text);
                    if (!gotCode && !Spawned && !Collected)
                    {
                        if (currentInputs.Count > code.Length)
                        {
                            currentInputs.RemoveAt(0);
                        }
                        if (currentInputs.Count == code.Length)
                        {
                            for (int i = 0; i < code.Length; i++)
                            {
                                if (!currentInputs[i].Equals(code[i]))
                                {
                                    return;
                                }
                            }
                            gotCode = true;
                            SpawnCollectable(true);
                        }
                    }
                }
            });
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            Spawned = level.Session.GetFlag(CollectableSpawnedFlag);
            Collected = level.Session.GetFlag(CollectableCollectedFlag);
            if (resetFlagOnAdded)
            {
                level.Session.SetFlag(CustomFlagOnCollected, false);
            }
            if (Spawned && !Collected)
            {
                SpawnCollectable(false);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            InArea = !usesBounds || CollideCheck<Player>();
        }
        public override void Render()
        {
            if (visibleBounds)
            {
                Draw.Rect(Collider, InArea ? Color.Blue : Color.LightGreen);
            }
            base.Render();
        }
        private void OnPlayer(Player player)
        {
            Level level = Scene as Level;
            if (player.DashAttacking)
            {
                Add(new Coroutine(Collect(player, heart, level)));
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
        private IEnumerator Collect(Player player, Entity collectable, Level level)
        {
            Visible = false;
            collectable.Visible = false;
            collectable.Collidable = false;
            level.Shake();
            SoundEmitter.Play(collectSound, collectable);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            for (int i = 0; i < 10; i++)
            {
                Scene.Add(new AbsorbOrb(collectable.Center, player));
            }
            level.Flash(Color.White, drawPlayerOver: true);
            level.Session.SetFlag(CollectableCollectedFlag);
            if (!canRespawn)
            {
                level.Session.DoNotLoad.Add(ID);
            }
            if (!string.IsNullOrEmpty(CustomFlagOnCollected))
            {
                level.Session.SetFlag(CustomFlagOnCollected, true);
            }
            PianoModule.Session.CollectedIDs.TryAdd(this);
            heart.RemoveSelf();

            yield return null;
        }
        private void SpawnCollectable(bool effectsAdded)
        {
            if (Scene is not Level level) return;
            if (onlyFlag)
            {
                level.Session.SetFlag(CustomFlagOnCollected);
                return;
            }
            level.Add(heart = new Entity(level.LevelOffset + heartOffset));
            if (effectsAdded)
            {
                Audio.Play(audio, heart.Position);
                level.Flash(Color.White, drawPlayerOver: true);
            }
            heart.Add(sprite = new Sprite(GFX.Game, spriteName));
            sprite.AddLoop("idle", "", 0.08f);
            sprite.AddLoop("static", "", 1f, 0);
            sprite.Play("idle");
            heart.Collider = new Hitbox(12f, 12f, 4f, 4f);
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                sprite.Scale = Vector2.One * (1f + f * 0.3f);
            }));
            heart.Add(new PlayerCollider(OnPlayer));
            heart.Add(moveWiggler = Wiggler.Create(0.8f, 2f));
            moveWiggler.StartZero = true;
            level.Session.SetFlag(CollectableSpawnedFlag);
        }
        public override void Update()
        {
            base.Update();
            Spawned = SceneAs<Level>().Session.GetFlag(CollectableSpawnedFlag);
            Collected = SceneAs<Level>().Session.GetFlag(CollectableCollectedFlag);
            InArea = !usesBounds || CollideCheck<Player>();
            if (Spawned && !Collected)
            {
                bounceSfxDelay -= Engine.DeltaTime;
                //SPRITECHECK
                if (sprite != null && moveWiggler != null)
                {
                    sprite.Position = moveWiggleDir * moveWiggler.Value * -8f;
                }
            }

        }
    }
}