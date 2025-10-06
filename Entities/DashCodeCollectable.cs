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
            if (effectsAdded)
            {
                Add(new Coroutine(routine(level)) { UseRawDeltaTime = true });
            }
            else
            {
                AddHeart(level);
            }
        }
        private float timeRate = 1;
        private bool resetTimeRate;
        private static float tiny = 0.00016666697f;
        private bool holdTimeRate;
        private class flash : Entity
        {
            public float alpha = 1;
            public flash(float time) : base()
            {
                Depth = int.MinValue;
                Tween t = Tween.Set(this, Tween.TweenMode.Oneshot, time, Ease.SineInOut, t => alpha = 1 - t.Eased, t => RemoveSelf());
                t.UseRawDeltaTime = true;
                Tag |= Tags.Persistent;
            }
            public override void Render()
            {
                base.Render();
                Draw.Rect(SceneAs<Level>().Camera.Position, 320, 180, Color.Black * alpha);
            }
        }
        private IEnumerator routine(Level level)
        {
            Player player = level.GetPlayer();
            if (player == null) yield break;
            player.DisableMovement();
            AddHeart(level);
            heart.Visible = false;
            heart.Active = false;

            ShaderOverlay overlay = new ShaderOverlay("collectableGet", "", false, 1, false);
            overlay.UseRawDeltaTime = true;
            overlay.Amplitude = 1;
            level.Add(overlay);
            flash f = new flash(1);
            yield return null;
            resetTimeRate = true;
            timeRate = Engine.TimeRateB;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime * 1.2f)
            {
                Engine.TimeRateB = Calc.LerpClamp(timeRate, 0, Ease.SineInOut(i));
                yield return null;
            }
            holdTimeRate = true;

            yield return 0.5f;
            Vector2 from = level.Camera.Position;
            bool bringCameraBack = false;
            if (!heart.Center.OnScreen())
            {
                bringCameraBack = true;
                Vector2 to = heart.Center.Clamp(level.Bounds.Left, level.Bounds.Top, level.Bounds.Right - 320, level.Bounds.Bottom - 180);
                for (float i = 0; i < 1; i += Engine.RawDeltaTime)
                {
                    level.Camera.Position = Vector2.Lerp(from, to, Ease.CubeOut(i));
                    overlay.ParamVector2("Position", (heart.Center - level.Camera.Position) / new Vector2(320, 180));
                    yield return null;
                }
                level.Camera.Position = to;
                overlay.ParamVector2("Position", (heart.Center - level.Camera.Position) / new Vector2(320, 180));

            }
            yield return 0.5f;
            overlay.ParamVector2("Position", (heart.Center - level.Camera.Position) / new Vector2(320, 180));
            for (float i = 0; i < 1; i += Engine.RawDeltaTime)
            {
                overlay.Amplitude = Calc.Approach(overlay.Amplitude, tiny, Engine.RawDeltaTime);
                yield return null;
            }
            overlay.Amplitude = tiny;
            bool on = true;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime)
            {
                if (on)
                {
                    overlay.ParamFloat("Modifier", 1);
                }
                else
                {
                    overlay.ParamFloat("Modifier", 0);
                }
                on = !on;
                yield return null;
            }
            level.Remove(overlay);
            level.Flash(Color.White);
            for (int i = 0; i < 6; i++)
            {
                GravityParticle p = new GravityParticle(heart.Center + new Vector2(Calc.Random.Range(-3f, 3f), 0),
                    new Vector2(Calc.Random.Range(-20f, 20f), Calc.Random.Range(-50f, -10f)), Color.White);
                level.Add(p);
            }
            heart.Visible = true;
            heart.Active = true;
            playSound(heart.Center);
            if (bringCameraBack)
            {
                yield return 1f;
                Vector2 from2 = level.Camera.Position;
                for(float i = 0; i<1; i += Engine.RawDeltaTime)
                {
                    level.Camera.Position = Vector2.Lerp(from2, from, Ease.CubeOut(i));
                    yield return null;
                }
            }
            holdTimeRate = false;
            Engine.TimeRateB = timeRate;
            timeRate = 1;
            resetTimeRate = false;
            player.EnableMovement();

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (resetTimeRate)
            {
                Engine.TimeRateB = timeRate;
                scene.GetPlayer()?.EnableMovement();
            }
        }
        private void playSound(Vector2 position)
        {
            Audio.Play(audio, position);
        }
        private void AddHeart(Level level)
        {
            level.Add(heart = new Entity(level.LevelOffset + heartOffset));
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
            if (holdTimeRate)
            {
                Engine.TimeRateB = 0;
            }
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