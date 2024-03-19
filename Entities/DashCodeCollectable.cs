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
        private bool OnlyFlag;

        public EntityID ID;

        private Sprite sprite;

        private string collectSound = "event:/game/07_summit/gem_get";

        private Vector2 moveWiggleDir;

        private string spriteName;

        private float bounceSfxDelay;

        private readonly string[] code;

        private Wiggler scaleWiggler;

        private Wiggler moveWiggler;

        private bool gotCode = false;

        private EventInstance sfx;

        private List<string> currentInputs = new List<string>();

        private string audio = "";

        private DashListener dashListener;

        private bool canRespawn;

        private bool spawned;

        private bool collected;

        public string flag;

        private bool flagDebug;

        private string spawnedFlag = "";

        public string collectedFlag = "";

        private bool inBounds = false;

        private bool usesBounds;

        private Player player;

        public bool isHeart;

        private float xBound;

        private float yBound;

        private Vector2 dataNode;
        private Rectangle bounds;
        private Vector2 levelPosition;
        private Entity heart;
        private bool visibleBounds;
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            levelPosition = new Vector2(level.Bounds.Left, level.Bounds.Top);

            spawned = SceneAs<Level>().Session.GetFlag(spawnedFlag);
            collected = SceneAs<Level>().Session.GetFlag(collectedFlag);

            scene.Add(heart = new Entity(levelPosition+dataNode));

            float newX = Position.X;
            float newY = Position.Y;
            if (!usesBounds)
            {
                newX =(scene as Level).Bounds.Left;
                xBound = (scene as Level).Bounds.Width;
                newY = (scene as Level).Bounds.Top;
                yBound = (scene as Level).Bounds.Height;
            }
            bounds = new Rectangle((int)newX,(int)newY, (int)xBound, (int)yBound);
            if (flagDebug)
            {
                SceneAs<Level>().Session.SetFlag(flag, false);
            }
            if(!collected && spawned)
            {
                SpawnCollectable(false);
            }

        }
        public DashCodeCollectable(Vector2 position, bool isHeart):
            base(position)
        {
            this.isHeart = isHeart;
        }
        public DashCodeCollectable(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset, data.Attr("Texture", "objects/PuzzleIslandHelper/dashCodeCollectable/miniHeart").Contains("miniHeart"))
        {
            OnlyFlag        = data.Bool("noCollectable");
            flagDebug       = data.Bool("flagDebug", true);
            canRespawn      = data.Bool("canRespawn");
            usesBounds      = data.Bool("usesBounds");
            visibleBounds   = data.Bool("visibleBounds");
            flag            = data.Attr("flagOnCollected");
            code            = data.Attr("code").Split(',').Select(Convert.ToString).ToArray();
            audio           = data.Attr("event", "event:/new_content/game/10_farewell/glitch_short");
            spriteName      = data.Attr("Texture", "objects/PuzzleIslandHelper/dashCodeCollectable/miniHeart");
            yBound          = data.Height;
            xBound          = data.Width;
            dataNode        = data.Nodes[0];


            Collider = new Hitbox
            (data.Width, data.Height);

            spawned = false;
            collected = false;
            gotCode = false;
            Add(dashListener = new DashListener());
            dashListener.OnDash = delegate (Vector2 dir)
            {
                string text = "";

                text = dir.Y < 0f ? "U" : dir.Y > 0f ? "D" : "";
                text += dir.X < 0f ? "L" : dir.X > 0f ? "R" : "";
                if (!inBounds && usesBounds)
                {
                    text = "";
                }
                currentInputs.Add(text);
                if (!gotCode && !spawned && !collected)
                {
                    if (currentInputs.Count > code.Length)
                    {
                        currentInputs.RemoveAt(0);
                    }
                    if (currentInputs.Count == code.Length)
                    {
                        bool foo = true;
                        for (int i = 0; i < code.Length; i++)
                        {
                            foo = !currentInputs[i].Equals(code[i]) ? false : foo;
                        }

                        if (foo)
                        {
                            gotCode = foo;
                            SpawnCollectable(true);
                        }
                        
                    }
                }
            };
            Tag = Tags.TransitionUpdate;
            ID = id;
            spawnedFlag = "DashCodeCollectableHeartSpawn" + id.ToString();
            collectedFlag = "DashCodeCollectableHeartCollected" + id.ToString();
        }
        public override void Render()
        {
            player = Scene.Tracker.GetEntity<Player>();
            if (visibleBounds)
            {
                if (inBounds)
                {
                    Draw.Rect(bounds, Color.Blue);
                }
                else
                {
                    Draw.Rect(bounds, Color.LightGreen);
                }
            }
            base.Render();
        }
        private void OnPlayer(Player player)
        {
            Level level = Scene as Level;
            if (player.DashAttacking)
            {
                Add(new Coroutine(Collect(player, level)));
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
            Visible = false;
            heart.Visible = false;
            heart.Collidable = false;
            Session session = SceneAs<Level>().Session;
            level.Shake();
            SoundEmitter.Play(collectSound, heart);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

            for (int i = 0; i < 10; i++)
            {
                Scene.Add(new AbsorbOrb(levelPosition + dataNode + new Vector2(8f, 8f), player));
            }


            level.Flash(Color.White, drawPlayerOver: true);
            SceneAs<Level>().Session.SetFlag(collectedFlag);
            if (!canRespawn)
            {
                session.DoNotLoad.Add(ID);
            }
            if(flag != "")
            { 
                SceneAs<Level>().Session.SetFlag(flag, true);
            }
            if (!PianoModule.Session.CollectedIDs.Contains(this))
            {
                PianoModule.Session.CollectedIDs.Add(this);
            }
            heart.RemoveSelf();

            yield return null;
        }
        public IEnumerator WaitBeforeRemoveRoutine() 
        {
            float timer = 0.02f;
            while (timer > 0)
            {
                timer -= Engine.DeltaTime;
                yield return null;
            }
            RemoveSelf();
        }
        private void SpawnCollectable(bool effectsAdded)
        {
            if (effectsAdded)
            {
                sfx = Audio.Play(audio,heart.Position);
                (Scene as Level).Flash(Color.White, drawPlayerOver: true);
            }
            if (OnlyFlag)
            {
                SceneAs<Level>().Session.SetFlag(flag);
                return;
            }

            heart.Add(sprite = new Sprite(GFX.Game, spriteName));
            sprite.AddLoop("idle", "", 0.08f);
            sprite.AddLoop("static", "", 1f, 0);
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
            SceneAs<Level>().Session.SetFlag(spawnedFlag);
        }
        public override void Update()
        {
            base.Update();
            player = Scene.Tracker.GetEntity<Player>();
            if(player == null)
            {
                return;
            }
            spawned = SceneAs<Level>().Session.GetFlag(spawnedFlag);
            collected = SceneAs<Level>().Session.GetFlag(collectedFlag);
            if (spawned && !collected)
            {
                bounceSfxDelay -= Engine.DeltaTime;
                //SPRITECHECK
                sprite.Position = moveWiggleDir * moveWiggler.Value * -8f;
            }
            inBounds = player.CollideRect(bounds);

        }
    }
}