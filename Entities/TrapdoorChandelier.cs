using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TrapdoorChandelier")]
    [Tracked]
    public class TrapdoorChandelier : Entity
    {
        [Tracked]
        public class GlobalUpdater : Entity
        {
            public static bool Paused;
            public const float FRICTION = 30f;
            public const float MAX = 120f;
            public static float Speed;
            public static float Amount;
            public static float Buffer;
            private static bool fromDeath;
            public GlobalUpdater() : base()
            {
                Tag |= Tags.Global | Tags.TransitionUpdate;
            }
            [OnLoad]
            public static void Load()
            {
                fromDeath = false;
                Speed = 0;
                Paused = false;
                Everest.Events.Player.OnDie += Player_OnDie;
                Everest.Events.Player.OnSpawn += Player_OnSpawn;
            }

            private static void Player_OnSpawn(Player obj)
            {
                if (fromDeath)
                {
                    Speed = 0;
                    Amount = 0;
                }
                fromDeath = false;
            }
            private static void Player_OnDie(Player obj)
            {
                fromDeath = true;
            }
            [OnUnload]
            public static void Unload()
            {
                Everest.Events.Player.OnDie -= Player_OnDie;
                Everest.Events.Player.OnSpawn -= Player_OnSpawn;
            }
            public override void Update()
            {
                base.Update();
                if (Scene is not Level level ||
                    level.GetPlayer() is not Player player ||
                    player.StateMachine.State == Player.StDummy ||
                    Paused || level.Transitioning) return;
                Amount = Calc.Clamp(Amount + Speed, 0, MAX) / MAX;
                if (Buffer > 0)
                {
                    Buffer = Calc.Clamp(Buffer, 0, MAX / 3f);
                    Buffer = Calc.Approach(Buffer, 0f, FRICTION * Engine.DeltaTime);
                }
                else
                {
                    float mult = Speed > 30f ? 1 : Speed / 30f * 1.3f;
                    Speed = Calc.Approach(Speed, 0f, FRICTION * mult * Engine.DeltaTime);
                }
            }
        }
        public MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/elderTrapdoor/chandelier0" + frame];
        private int frame => Calc.Clamp((int)(GlobalUpdater.Speed + GlobalUpdater.Buffer) % maxFrames, 0, maxFrames);
        private int maxFrames = 4;
        public static string Flag = "ElderTrapdoorPuzzleSolved";
        public TrapdoorChandelier(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(Texture.Width, Texture.Height);
            Position -= Collider.HalfSize;
            Add(new PlayerCollider(OnPlayer, Collider));
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position, Color.White);
        }
        private void OnPlayer(Player player)
        {
            if (player.DashAttacking)
            {
                Vector2 dir = player.DashDir;
                if (GlobalUpdater.Speed >= GlobalUpdater.MAX)
                {
                    GlobalUpdater.Buffer += Math.Abs(dir.X) * 15f;
                }
                else
                {
                    GlobalUpdater.Speed += Math.Abs(dir.X) * 15f;
                }

            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (PianoUtils.SeekController<GlobalUpdater>(scene) == null)
            {
                scene.Add(new GlobalUpdater());
            }
        }

    }
}