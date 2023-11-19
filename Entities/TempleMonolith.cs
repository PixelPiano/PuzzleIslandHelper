using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TempleMonolith")]
    [Tracked]
    public class TempleMonolith : Solid
    {
        public Color Color { get; set; }
        private bool Reflection;
        public int Type { get; set; }
        public class Hole : Entity
        {
            public Image Image;
            public Hole(Vector2 position, Color color, int depth) : base(position)
            {
                Image = new Image(GFX.Game["objects/PuzzleIslandHelper/templeMonolith/holeGray"]);
                Image.Color = color;
                Image.X -= 10;
                Add(Image);
                Depth = depth;
            }
        }
        private bool HasFallen;
        private Hole Ceiling;
        private Image Monolith;
        private bool Falls;
        private bool Falling;
        private bool StartedFalling;
        private float FallDelay = 0;
        private EntityID ID;
        private int TimesHit;

        private static int GetWidth()
        {
            return GFX.Game["objects/PuzzleIslandHelper/templeMonolith/bigMonolith00"].Width;
        }
        private static int GetHeight()
        {
            return GFX.Game["objects/PuzzleIslandHelper/templeMonolith/bigMonolith00"].Height;
        }
        public TempleMonolith(Vector2 position, int type, bool falls, EntityID id, bool reflection) : base(position, GetWidth(), GetHeight(), false)
        {
            ID = id;
            Depth = 2001;
            Falls = falls;
            Type = type;
            Color = Type switch
            {
                0 => Color.Lerp(Color.Gray, Color.Blue, 0.5f),
                1 => Color.Blue,
                2 => Color.Green,
                3 => Color.HotPink,
                _ => Color.White
            };
            Monolith = new Image(GFX.Game["objects/PuzzleIslandHelper/templeMonolith/bigMonolith0" + type.ToString()]);
            Ceiling = new Hole(Position, Color, Depth - 1);
            Add(Monolith);
            Collider = new Hitbox(Width, Height);
            Reflection = reflection;
            if (Reflection)
            {
                Collidable = false;
                MirrorReflection mirrorReflection = new MirrorReflection();
                mirrorReflection.IgnoreEntityVisible = true;
                Visible = false;
                Add(mirrorReflection);
            }
            else
            {
                OnDashCollide = OnDashed;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Reflection)
            {
                scene.Add(Ceiling);
            }
        }
        public TempleMonolith(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, data.Attr("type")[0] - '0', data.Bool("falls"), id, data.Bool("reflection")) { }
        private DashCollisionResults OnDashed(Player player, Vector2 dir)
        {
            if (Falls && !StartedFalling && !Falling && !HasFallen)
            {
                if (TimesHit < 3)
                {
                    Add(new Coroutine(ShakeRoutine()));
                    TimesHit++;
                }
                else
                {
                    Add(new Coroutine(FallSequence()));
                }
            }

            //flash monolith and gem
            //play bell sound based on type
            //idk

            return DashCollisionResults.Rebound;
        }
        public void ShakeSfx()
        {
            Audio.Play("event:/game/general/fallblock_shake", Center);
        }
        public IEnumerator ShakeRoutine()
        {
            ShakeSfx();
            StartShaking();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            yield return 0.2f;
            float timer = 0.4f;


            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }

            StopShaking();
        }
        private IEnumerator HoldUp(float time)
        {
            for (float i = 0; i < time; i += Engine.DeltaTime)
            {
                Input.MoveY.Value = -1;
                yield return null;
            }
        }
        public IEnumerator FallSequence()
        {
            AddTag(Tags.Global);
            AddTag(Tags.Persistent);
            Ceiling.AddTag(Tags.Global);
            Ceiling.AddTag(Tags.Persistent);
            SceneAs<Level>().Session.DoNotLoad.Add(ID);
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            while (FallDelay > 0f)
            {
                FallDelay -= Engine.DeltaTime;
                yield return null;
            }
            while (true)
            {
                ShakeSfx();
                StartShaking();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

                yield return 0.2f;
                float timer = 0.4f;


                while (timer > 0f)
                {
                    yield return null;
                    timer -= Engine.DeltaTime;
                }
                while (!player.OnGround())
                {
                    yield return null;
                }
                player.StateMachine.State = Player.StFrozen;

                yield return player.DummyWalkTo(Position.X - 24, true);

                yield return null;

                Add(new Coroutine(HoldUp(3.05f)));
                yield return 2;
                StopShaking();
                yield return 1;
                for (int i = 2; i < Width; i += 4)
                {
                    if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                    {
                        SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + i, Y), Vector2.One * 4f, (float)Math.PI / 2f);
                    }

                    SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + i, Y), Vector2.One * 4f);
                }

                float speed = 0f;
                float maxSpeed = 160f;
                float playerMoveTimer = 1;
                while (true)
                {

                    Level level = SceneAs<Level>();
                    player = level.Tracker.GetEntity<Player>();
                    if (playerMoveTimer <= 0)
                    {
                        player.StateMachine.State = Player.StNormal;
                    }
                    playerMoveTimer -= Engine.DeltaTime;
                    speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                    player.ForceCameraUpdate = true;
                    if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                    {
                        break;
                    }
                    yield return null;
                }
                HasFallen = true;
                Audio.Play("event:/game/general/fallblock_impact", BottomCenter);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.3f);

                StartShaking();
                LandParticles();
                yield return 0.2f;
                StopShaking();
                if (CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f)))
                {
                    break;
                }

                while (CollideCheck<Platform>(Position + new Vector2(0f, 1f)))
                {
                    yield return 0.1f;
                }
            }
            Collidable = true;
            Safe = true;
        }
        public void LandParticles()
        {
            for (int i = 2; i <= Width; i += 4)
            {
                if (Scene.CollideCheck<Solid>(BottomLeft + new Vector2(i, 3f)))
                {
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(X + i, Bottom), Vector2.One * 4f, -(float)Math.PI / 2f);
                    float direction = ((!(i < Width / 2f)) ? 0f : ((float)Math.PI));
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(X + i, Bottom), Vector2.One * 4f, direction);
                }
            }
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            Monolith.Position += amount;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level)
            {
                return;
            }
        }
        public override void Render()
        {
            Ceiling.Image.DrawSimpleOutline();
            Monolith.DrawSimpleOutline();
            base.Render();
        }

    }

}
