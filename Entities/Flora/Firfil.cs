using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/Firfil")]
    [Tracked]
    public class Firfil : Actor
    {
        [Command("add_firfil", "Creates x firfils and make them follow the player")]
        public static void CreateFirfils(int x)
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands.Log("Current Scene is currently not a level.");
                return;
            }
            if (level.GetPlayer() is not Player player)
            {
                Engine.Commands.Log("Current Scene does not contain a player.");
                return;
            }
            for (int i = 0; i < x; i++)
            {
                Firfil f = new Firfil(player.Position, new(Guid.NewGuid().ToString(), 0));
                level.Add(f);
            }
        }
        public static int FirfilsFollowing
        {
            get
            {
                if (Engine.Scene is not null)
                {
                    return Engine.Scene.Tracker.GetEntities<Firfil>().Where(item => (item as Firfil).FollowingPlayer).Count();
                }
                return 0;
            }
        }
        public bool Stored => FirfilStorage.Stored.Contains(ID);
        public bool FollowingPlayer => Follower != null && Follower.HasLeader && Follower.Leader.Entity is Player;
        public bool CanFollow = true;
        public bool AtNest;
        public Follower Follower;
        public Vector2 Offset;
        public Vector2 OffsetTarget;
        public const float FlySpeed = 20f;
        public const int MaxFollowing = 100;
        public float SpeedMult = 1;
        public EntityID ID;
        private float offsetTimer;
        private float colorLerp;
        public Color Color;
        public Vector2 CirclePos;
        public bool InView;
        private float afterImageTimer = 0.1f;
        public float Alpha = 1;
        public bool FollowImmediately;
        public Firfil(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, id)
        {
        }
        public Firfil(Vector2 position, EntityID id) : base(position)
        {
            ID = id;
            Collider = new Hitbox(9, 9, -4, -4);
            Add(Follower = new Follower());
            Follower.FollowDelay = Engine.DeltaTime;
            Add(new PlayerCollider(OnPlayer));
            Tween.Set(this, Tween.TweenMode.Looping, 0.3f, Ease.SineInOut, t => colorLerp = t.Eased);
            Tag |= Tags.TransitionUpdate;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (FollowImmediately)
            {
                Player player = scene.GetPlayer();
                if (player != null)
                {
                    StartFollowing(player.Leader, false);
                }
            }
            if (PianoModule.Session.CollectedFirfilIDs.Contains(ID) && !AtNest)
            {
                RemoveSelf();
            }
            if (FirfilStorage.Stored.Contains(ID))
            {
                RemoveSelf();
            }
            afterImageTimer += Calc.Random.Range(0f, 0.3f);
        }
        public void FadeIn(float duration = 0.8f)
        {
            Alpha = 0;
            FadeTo(1, duration);
        }
        public void FadeTo(float to, float time)
        {
            float a = Alpha;
            Tween.Set(this, Tween.TweenMode.Oneshot, time, Ease.SineIn, t => Alpha = Calc.LerpClamp(a, to, t.Eased));
        }
        public void DrawWithAlpha(Vector2 position, float alphaMult)
        {
            if (InView)
            {
                base.Render();
                Draw.Point(position, Color * (Alpha * alphaMult));
            }
        }
        public override void Update()
        {
            base.Update();
            if (!Stored)
            {
                InView = SceneAs<Level>().Camera.GetBounds().Colliding(Collider.Bounds, 8);
                Color = Color.Lerp(AtNest ? Color.Lime : FollowingPlayer ? Color.Cyan : Color.Magenta, Color.Yellow, colorLerp);
                if (InView && afterImageTimer <= 0)
                {
                    afterImageTimer = 0.15f;
                    AfterImage.Create(Position + Offset, DrawWithAlpha, 0.4f, 1f);
                }
                if (afterImageTimer != 0)
                {
                    afterImageTimer = Calc.Approach(afterImageTimer, 0, Engine.DeltaTime);
                }
                offsetTimer -= Engine.DeltaTime;
                if (offsetTimer <= 0)
                {
                    SpeedMult = Calc.Random.Range(0.6f, 1f);
                    offsetTimer = Calc.Random.Range(0.5f, 1.2f);
                    OffsetTarget = Calc.AngleToVector(Calc.Random.NextAngle(), Calc.Random.Range(1f, 8));
                }
                Offset += (OffsetTarget - Offset) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime)) * SpeedMult;
            }
        }
        public override void Render()
        {
            DrawWithAlpha(Position + Offset, 1);
        }
        public void OnPlayer(Player player)
        {
            if (!Stored && !AtNest && !FollowingPlayer && CanFollow && FirfilsFollowing < MaxFollowing)
            {
                StartFollowing(player.Leader, true);
            }
        }
        public void StartFollowing(Leader leader, bool pulse)
        {
            SceneAs<Level>().Session.DoNotLoad.Add(ID);
            if (pulse)
            {
                PulseEntity.Circle(Position + Offset, Depth, Pulse.Fade.Linear, Pulse.Mode.Oneshot, 0, 8, 0.8f, true, Color, Color.White, Ease.CubeIn, Ease.CubeIn);
            }
            Follower.PersistentFollow = true;
            leader.GainFollower(Follower);
        }
        public void OnArriveAtNest()
        {
            Active = true;
            Visible = true;
            Collidable = false;
            AtNest = true;
            if (Follower != null)
            {
                Follower.Leader?.LoseFollower(Follower);
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/FirfilNest")]
    [Tracked]
    public class FirfilNest : Entity
    {
        public static HashSet<EntityID> CollectedFirfilIDs => PianoModule.Session.CollectedFirfilIDs;

        public static MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/firfil/nest"];
        public HashSet<Firfil> Firfils = [];
        public Image Nest;
        public float CircleRotation;
        public TalkComponent Talk;
        public int RequiredForKey;
        public string KeyID;
        public string FullKeyID => "FirfilNestKeyCollected:" + KeyID;
        private float rotateRate = 1;
        public bool CanSpawnKey => Firfils.Count >= RequiredForKey && !KeyCollected && !KeySpawned;
        public bool KeyCollected
        {
            get => KeyID.GetFlag();
            set => KeyID.SetFlag(value);
        }
        public bool KeySpawned
        {
            get => FullKeyID.GetFlag();
            set => FullKeyID.SetFlag();
        }
        public bool InCutscene;
        public bool SnapPositions;
        public int PositionLoops = 1;
        public FirfilNest(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Add(Nest = new Image(Texture));
            Collider = new Hitbox(Nest.Width, Nest.Height);
            Rectangle r = new Rectangle(0, 0, (int)Width, (int)Height);
            Add(Talk = new TalkComponent(r, Vector2.UnitX * Width / 2, Interact));
            KeyID = data.Attr("keyID");
            Tag |= Tags.TransitionUpdate;
            RequiredForKey = data.Int("requiredFirfils", 10);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Point(TopCenter - Vector2.UnitY * 32, Color.Magenta);
        }
        public override void Update()
        {
            base.Update();
            HandlePositions(SnapPositions);
            CircleRotation = (CircleRotation + rotateRate) % 360;
            if (InCutscene)
            {
                return;
            }
            if (Scene.GetPlayer() is Player player)
            {
                Talk.Enabled = player.Leader.Followers.Find(item => item.Entity is Firfil) != null;
            }
        }
        public class FirfilKeyCutscene : CutsceneEntity
        {
            public Player Player;
            public FirfilNest Nest;
            private Coroutine zoomRoutine, moveRoutine;
            private Vector2 cameraOrig;
            public FirfilKeyCutscene(Player player, FirfilNest nest) : base()
            {
                Player = player;
                Nest = nest;
            }
            public override void OnBegin(Level level)
            {
                Player.DisableMovement();
                Add(new Coroutine(Sequence()));
            }
            public IEnumerator Sequence()
            {
                cameraOrig = Level.Camera.Position;
                Vector2 target = Nest.TopCenter - Vector2.UnitY * 32;

                Add(moveRoutine = new Coroutine(CameraTo(target - new Vector2(160, 90), 2, Ease.SineInOut)));
                Add(zoomRoutine = new Coroutine(Level.ZoomTo(new Vector2(160, 90), 2, 1.5f)));

                yield return 0.8f;
                float from = Nest.rotateRate;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
                {
                    Nest.rotateRate = Calc.LerpClamp(from, 0, Ease.CubeIn(i));
                    yield return null;
                }
                while (!zoomRoutine.Finished) yield return null;
                Nest.PositionLoops = 2;
                float speed = 0;
                float speedTimer = 0;
                while (speedTimer < 1.7f)
                {
                    Nest.rotateRate -= speed;
                    speed = Calc.Approach(speed, 200f, 20f * Engine.DeltaTime);
                    if (speed > 10)
                    {
                        speedTimer += Engine.DeltaTime;
                    }
                    yield return null;
                }
                Nest.SpawnKey(false);
                from = Nest.rotateRate;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    Nest.rotateRate = Calc.LerpClamp(from, 1, Ease.SineInOut(i));
                    yield return null;
                }
                Add(zoomRoutine = new(Level.ZoomBack(1)));
                Add(moveRoutine = new(CameraTo(cameraOrig, 1, Ease.SineInOut)));
                yield return 1;
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                zoomRoutine.RemoveSelf();
                moveRoutine.RemoveSelf();
                Level.ResetZoom();
                Level.Camera.Position = cameraOrig;
                if (!Nest.KeySpawned)
                {
                    Nest.SpawnKey(true);
                }
                Nest.PositionLoops = 1;
                Nest.rotateRate = 1;
                Nest.InCutscene = false;
                Player.EnableMovement();
            }
        }
        public void HandlePositions(bool instant)
        {
            int count = 0;
            foreach (Firfil f in Firfils)
            {
                SetFirfilPosition(count, f, instant);
                count++;
            }
        }
        public void SetFirfilPosition(int index, Firfil f, bool instant)
        {
            Vector2 position = TopCenter - Vector2.UnitY * 32;
            Vector2 offset;
            switch (index)
            {
                case < 11:
                    float space = 36;
                    float num = index * space;
                    float angle = (num + CircleRotation).ToRad();
                    offset = Calc.AngleToVector(angle, 10);
                    break;
                default:
                    offset = Vector2.Zero;
                    break;
            }

            Vector2 vector2 = position + offset;
            if (instant)
            {
                f.Position = vector2;
            }
            else
            {
                for (int i = 0; i < PositionLoops; i++)
                {
                    f.Position += (vector2 - f.Position) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                }
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetPlayer() is Player player)
            {
                Talk.Enabled = player.Leader.Followers.Find(item => item.Entity is Firfil) != null;
            }
            HashSet<EntityID> inLevel = [];
            foreach (Firfil f in scene.Tracker.GetEntities<Firfil>())
            {
                //Auto collect any firfils in the level that should be at the nest
                if (CollectedFirfilIDs.Contains(f.ID))
                {
                    CollectFirfil(f);
                    inLevel.Add(f.ID);
                }
            }
            //re-add any firfils that aren't in the level that should be at the nest
            foreach (EntityID id in CollectedFirfilIDs)
            {
                if (!inLevel.Contains(id))
                {
                    CollectFirfil(CreateNewFirfil(id));
                }
            }
            HandlePositions(true);

            if (KeySpawned && !KeyCollected)
            {
                SpawnKey(true);
            }

        }
        public void SpawnKey(bool instant)
        {
            CutsceneHeart key = new(TopCenter - Vector2.UnitY * 32 - Vector2.One * 16, new EntityID(Guid.NewGuid().ToString(), 0), "", false, "", "", KeyID, Color.White, !instant);
            Scene.Add(key);
            KeySpawned = true;
        }
        public Firfil CreateNewFirfil(EntityID id)
        {
            Firfil newFirfil = new(TopCenter - Vector2.UnitY * 32, id);
            Scene.Add(newFirfil);
            return newFirfil;
        }
        public void CollectFirfil(Firfil firfil)
        {
            if (Firfils.Add(firfil))
            {
                CollectedFirfilIDs.Add(firfil.ID);
                firfil.OnArriveAtNest();
            }
        }
        public void Interact(Player player)
        {
            Input.Dash.ConsumePress();
            //todo: add ui
            foreach (Firfil f in player.Leader.GetFollowerEntities<Firfil>())
            {
                CollectFirfil(f);
            }
            if (CanSpawnKey)
            {
                Scene.Add(new FirfilKeyCutscene(player, this));
                InCutscene = true;
            }
        }
    }

    public static class FirfilStorage
    {
        public static HashSet<EntityID> Stored = [];
        public static void Store(Firfil firfil)
        {
            if (Stored.Add(firfil.ID))
            {
                firfil.RemoveSelf();
            }
        }
        public static void Take()
        {
            if (Stored.Count > 0)
            {
                if (Engine.Scene is not null && Engine.Scene.GetPlayer() is Player player)
                {
                    Firfil firfil = new Firfil(player.Position, Stored.First());
                    Stored.Remove(Stored.First());
                    Engine.Scene.Add(firfil);
                    firfil.FollowImmediately = true;
                }
            }
        }
        public static void ReleaseAll()
        {
            if (Engine.Scene is Level level)
            {
                foreach (EntityID id in Stored)
                {
                    level.Session.DoNotLoad.Remove(id);
                }
                Stored.Clear();

            }
        }
    }
}
