using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
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
        public bool Stored => FirfilStorage.Stored.Contains(this);
        public bool FollowingPlayer => Follower != null && Follower.HasLeader && Follower.Leader.Entity is Player;
        public bool CanFollow = true;
        public bool AtNest;
        public Follower Follower;
        public Vector2 Offset;
        public Vector2 OffsetTarget;
        public const float FlySpeed = 20f;
        public const int MaxFollowing = 10;
        public float SpeedMult = 1;
        public EntityID ID;
        private float offsetTimer;
        private float colorLerp;
        public Color Color;
        public Vector2 CirclePos;
        public bool InView;
        private float afterImageTimer = 0.1f;
        public float Alpha = 1;
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
            Tween.Set(this, Tween.TweenMode.Looping, 0.23f, Ease.SineInOut, t => colorLerp = t.Eased);
            Tag |= Tags.TransitionUpdate;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            afterImageTimer += Calc.Random.Range(0f, 0.3f);
        }
        public void FadeIn(float duration = 0.8f)
        {
            Alpha = 0;
            Tween.Set(this, Tween.TweenMode.Oneshot, duration, Ease.SineIn, t => Alpha = t.Eased);
        }

        public void DrawWithAlpha(Vector2 position, float alphaMult)
        {
            if (InView)
            {
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
            base.Render();
            DrawWithAlpha(Position + Offset, 1);
        }
        public void OnPlayer(Player player)
        {
            if (!Stored && !AtNest && !FollowingPlayer && CanFollow)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(ID);
                Pulse.Circle(this, Pulse.Fade.Linear, Pulse.Mode.Oneshot, Offset, 0, 8, 0.8f, true, Color, Color.White, Ease.CubeIn, Ease.CubeIn);
                if (FirfilsFollowing >= MaxFollowing)
                {
                    Follower.PersistentFollow = false;
                    FirfilStorage.Store(this);
                }
                else
                {
                    Follower.PersistentFollow = true;
                    player.Leader.GainFollower(Follower);
                }
            }
        }
        public void Reset()
        {
            Alpha = 1;
            Collidable = true;
            Follower.Leader?.LoseFollower(Follower);
            SceneAs<Level>().Session.DoNotLoad.Remove(ID);
            if (TagCheck(Tags.Persistent))
            {
                RemoveTag(Tags.Persistent);
            }
        }
        public void OnArriveAtNest()
        {
            Collidable = false;
            AtNest = true;
            Follower.Leader?.LoseFollower(Follower);
        }
    }
    [CustomEntity("PuzzleIslandHelper/FirfilNest")]
    [Tracked]
    public class FirfilNest : Entity
    {
        public static List<EntityID> CollectedFirfilIDs => PianoModule.Session.CollectedFirfilIDs;
        public static MTexture Texture => GFX.Game["PuzzleIslandHelper/firfil/nest"];
        public List<Firfil> Firfils = new();
        public Image Nest;
        public Collider CollectCollider;
        public float CircleRotation;
        public FirfilNest(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Nest = new Image(Texture));
            CollectCollider = new Hitbox(Nest.Width + 24, Nest.Height + 40, -12, -32);
            Collider = new Hitbox(Nest.Width, Nest.Height);
            Add(new PlayerCollider(OnPlayer, CollectCollider));
        }
        public override void Update()
        {
            base.Update();
            HandlePositions(false);
            CircleRotation = (CircleRotation + 1) % 360;
        }
        public void HandlePositions(bool instant)
        {
            for (int i = 0; i < Firfils.Count; i++)
            {
                SetFirfilPosition(i, Firfils[i], instant);
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
                f.Position += (vector2 - f.Position) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            foreach (Firfil f in scene.Tracker.GetEntities<Firfil>())
            {
                if (CollectedFirfilIDs.Contains(f.ID))
                {
                    CollectFirfil(f);
                }
            }
            HandlePositions(true);

        }
        public void CollectFirfil(Firfil firfil)
        {
            if (Firfils.TryAdd(firfil))
            {
                CollectedFirfilIDs.TryAdd(firfil.ID);
                firfil.OnArriveAtNest();
            }
        }

        public void OnPlayer(Player player)
        {
            FirfilStorage.ReleaseToNest(this, player.Center);
            foreach (Firfil f in player.Leader.GetFollowerEntities<Firfil>())
            {
                CollectFirfil(f);
            }
        }
    }

    [ConstantEntity("PuzzleIslandHelper/FirfilStorage")]
    [Tracked]
    public class FirfilStorage : Entity
    {
        public static List<Firfil> Stored = new();
        public FirfilStorage() : base()
        {

        }
        public static void Store(Firfil firfil)
        {
            if (Stored.TryAdd(firfil))
            {
                if (!firfil.TagCheck(Tags.Persistent))
                {
                    firfil.AddTag(Tags.Persistent);
                }
            }
        }
        public static void ReleaseToNest(FirfilNest nest, Vector2 position)
        {
            foreach (Firfil f in Stored)
            {
                f.Position = position;
                f.FadeIn();
                nest.CollectFirfil(f);
            }
            Stored.Clear();
        }
        public static void Release(bool fade)
        {
            if (Engine.Scene is not null)
            {
                foreach (Firfil f in Stored)
                {
                    f.Reset();
                    if (fade)
                    {
                        f.FadeIn();
                    }
                }
                Stored.Clear();

            }
        }
    }
}
