using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/SpinnerBurst")]
    [Tracked]
    public class SpinnerBurst : Entity
    {
        public FlagData Flag;
        public bool Persistent;
        public EntityID ID;
        public Color Color;
        public float Delay;
        public SpinnerBurst(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Flag = data.Flag("flag", "inverted");
            Collider = new Hitbox(16, 16);
            ID = id;
            Color = data.HexColor("color", Color.Blue);
            Persistent = data.Bool("persistent");
            Delay = data.Float("delay");
            Add(new FlagListener(Flag, b =>
            {
                if (b)
                {
                    Activate();
                }
            }));
        }
        public void Activate()
        {
            if (Delay <= 0)
            {
                Destroy();
            }
            else
            {
                Alarm.Set(this, Delay, delegate { Destroy(); });
            }
        }
        public void Destroy(bool boss = false)
        {
            if (InView())
            {
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
                CrystalDebris.Burst(Center, Color, boss, 8);
            }
            if (Persistent)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(ID);
            }
            RemoveSelf();
        }
        public bool InView()
        {
            Camera camera = (base.Scene as Level).Camera;
            if (base.X > camera.X - 16f && base.Y > camera.Y - 16f && base.X < camera.X + 320f + 16f)
            {
                return base.Y < camera.Y + 180f + 16f;
            }

            return false;
        }
    }
    [CustomEntity("PuzzleIslandHelper/FlagIceBlock")]
    [Tracked]
    public class FlagIceBlock : IceBlock
    {
        private FlagData flag;
        public FlagIceBlock(Vector2 position, float width, float height, FlagData coldModeFlag)
            : base(position, width, height)
        {
            this.flag = coldModeFlag;
            Add(new FlagListener(coldModeFlag, OnChangeMode));
            Components.RemoveAll<CoreModeListener>();
        }
        public FlagIceBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Flag("flag", "inverted"))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Collidable = solid.Collidable = flag.State;
        }
        public void OnChangeMode(bool cold)
        {
            Collidable = (solid.Collidable = cold);
            if (Collidable)
            {
                return;
            }

            Level level = SceneAs<Level>();
            Vector2 center = base.Center;
            for (int i = 0; (float)i < base.Width; i += 4)
            {
                for (int j = 0; (float)j < base.Height; j += 4)
                {
                    Vector2 vector = Position + new Vector2(i + 2, j + 2) + Calc.Random.Range(-Vector2.One * 2f, Vector2.One * 2f);
                    level.Particles.Emit(P_Deactivate, vector, (vector - center).Angle());
                }
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/FlagFireBarrier")]
    [Tracked]
    public class FlagFireBarrier : FireBarrier
    {
        private FlagData flag;
        public FlagFireBarrier(Vector2 position, float width, float height, FlagData flag)
            : base(position, width, height)
        {
            this.flag = flag;
            Components.RemoveAll<CoreModeListener>();
            Add(new FlagListener(flag, OnChangeMode));
        }
        public FlagFireBarrier(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Flag("flag", "inverted"))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Collidable = (solid.Collidable = flag.State);
            if (!Collidable)
            {
                idleSfx.Stop();
            }
        }
        public void OnChangeMode(bool hot)
        {
            Collidable = (solid.Collidable = hot);
            if (!Collidable)
            {
                Level level = SceneAs<Level>();
                Vector2 center = base.Center;
                for (int i = 0; (float)i < base.Width; i += 4)
                {
                    for (int j = 0; (float)j < base.Height; j += 4)
                    {
                        Vector2 vector = Position + new Vector2(i + 2, j + 2) + Calc.Random.Range(-Vector2.One * 2f, Vector2.One * 2f);
                        level.Particles.Emit(P_Deactivate, vector, (vector - center).Angle());
                    }
                }

                idleSfx.Stop();
            }
            else
            {
                idleSfx.Play("event:/env/local/09_core/lavagate_idle");
            }
        }

    }
    [CustomEntity("PuzzleIslandHelper/WaterfallShatterBlock")]
    [Tracked]
    public class WaterfallShatterBlock : DashBlock
    {
        private FlagData VisibleFlag;
        private FlagData ShatterFlag;
        public WaterfallShatterBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, true, true, true, id)
        {
            VisibleFlag = data.Flag("visibleFlag", "invertVisibleFlag");
            ShatterFlag = data.Flag("flagOnShatter", "invertFlagOnShatter");
            Add(new FlagListener(VisibleFlag, OnVisibilityChange));
            Depth = -12999;
            OnDashCollide = NewOnDashed;
        }
        public void OnVisibilityChange(bool visible)
        {
            if (visible)
            {
                Collidable = Visible = true;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!VisibleFlag.State)
            {
                Collidable = false;
                Visible = false;
            }
        }
        public DashCollisionResults NewOnDashed(Player player, Vector2 direction)
        {
            if (!canDash && player.StateMachine.State != 5 && player.StateMachine.State != 10)
            {
                return DashCollisionResults.NormalCollision;
            }
            NewBreak(player.Center, direction);
            return DashCollisionResults.Rebound;
        }
        public void NewBreak(Vector2 from, Vector2 direction)
        {
            Audio.Play("event:/game/general/wall_break_ice", Position);

            for (int i = 0; (float)i < base.Width / 8f; i++)
            {
                for (int j = 0; (float)j < base.Height / 8f; j++)
                {
                    base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), tileType, true).BlastFrom(from));
                }
            }

            Collidable = false;
            ShatterFlag.State = !ShatterFlag.Inverted;
            RemoveAndFlagAsGone();
        }
    }
    [CustomEntity("PuzzleIslandHelper/WaterfallToggle")]
    [Tracked]
    public class WaterfallToggle : Entity
    {
        public const float Cooldown = 1f;
        public bool iceMode;
        public float cooldownTimer;
        public bool onlyFalse;
        public bool onlyTrue;
        public bool persistent;
        public bool playSounds;
        public Sprite sprite;
        public FlagData IceFlag;
        public bool Usable
        {
            get
            {
                bool state = IceFlag.State;
                return (!onlyFalse || state) && (!onlyTrue || !state);
            }
        }
        public WaterfallToggle(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("onlyFalse"), data.Bool("onlyTrue"), data.Bool("persistent"), data.Flag("iceFlag", "inverted"))
        {

        }
        public WaterfallToggle(Vector2 position, bool onlyFalse, bool onlyTrue, bool persistent, FlagData iceFlag)
            : base(position)
        {
            IceFlag = iceFlag;
            this.onlyFalse = onlyFalse;
            this.onlyTrue = onlyTrue;
            this.persistent = persistent;
            base.Collider = new Hitbox(16f, 24f, -8f, -12f);
            Add(new FlagListener(iceFlag, OnChangeMode));
            Add(new PlayerCollider(OnPlayer));
            Add(sprite = GFX.SpriteBank.Create("coreFlipSwitch"));
            base.Depth = 2000;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            iceMode = IceFlag.State;
            SetSprite(animate: false);
        }
        public void OnChangeMode(bool cold)
        {
            iceMode = cold;
            SetSprite(animate: true);
        }
        public void SetSprite(bool animate)
        {
            if (animate)
            {
                if (playSounds)
                {
                    Audio.Play(iceMode ? "event:/game/09_core/switch_to_cold" : "event:/game/09_core/switch_to_hot", Position);
                }

                if (Usable)
                {
                    sprite.Play(iceMode ? "ice" : "hot");
                }
                else
                {
                    if (playSounds)
                    {
                        Audio.Play("event:/game/09_core/switch_dies", Position);
                    }

                    sprite.Play(iceMode ? "iceOff" : "hotOff");
                }
            }
            else if (Usable)
            {
                sprite.Play(iceMode ? "iceLoop" : "hotLoop");
            }
            else
            {
                sprite.Play(iceMode ? "iceOffLoop" : "hotOffLoop");
            }

            playSounds = false;
        }
        public void OnPlayer(Player player)
        {
            if (Usable && cooldownTimer <= 0f)
            {
                playSounds = true;
                Level level = SceneAs<Level>();
                IceFlag.State = !IceFlag.State;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                level.Flash(Color.White * 0.15f, drawPlayerOver: true);
                Celeste.Freeze(0.05f);
                cooldownTimer = 1f;
            }
        }
        public override void Update()
        {
            base.Update();
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Engine.DeltaTime;
            }
        }
    }
}