using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PipeScrew")]
    [Tracked]
    public class PipeScrew : Actor
    {
        public string flag;
        public bool Launched
        {
            get
            {
                return PianoModule.Session.PipeScrewLaunched;
            }
            set
            {
                PianoModule.Session.PipeScrewLaunched = value;
            }
        }
        public Vector2 Speed;
        public bool UsedInCutscene;
        public Sprite Screw;
        public const float AirFriction = 5f;
        public const float MaxFallSpeed = 280f;
        public const float Gravity = 160f;
        public const float GroundFriction = 330f;
        private float OnGroundTimer;
        private Vector2 previousLiftSpeed;
        public Vector2 originalPosition;
        public PipeScrew(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            UsedInCutscene = data.Bool("usedInCutscene");
            flag = data.Attr("flag");
            Screw = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/fluidPipe/screw/");
            Screw.AddLoop("idle", "screw", 0.1f);
            Screw.AddLoop("spin", "screwSpin", 0.05f);
            Screw.AddLoop("resting", "screwSpin", 0.1f, PianoModule.Session.PipeScrewRestingFrame);
            Screw.Play("idle");
            Add(Screw);
            Collider = new Hitbox(Screw.Width, Screw.Height - 3, 0, 1);
            originalPosition = Position;
        }
        private void OnCollideV(CollisionData data)
        {
            if (OnGroundTimer > 0.1f)
            {
                Speed.X = Calc.Approach(Speed.X, 0, GroundFriction * Engine.DeltaTime);
            }
            else if (Speed.Y > 0)
            {
                Speed.Y = -Speed.Y / 2.4f;
                Audio.Play("event:/PianoBoy/pipes/screw drop", Center);
            }
            else
            {
                Speed.Y = 0;
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || string.IsNullOrEmpty(flag) || PianoModule.Session.PipeScrewRestingPoint.HasValue || !UsedInCutscene)
            {
                return;
            }
            if (!Launched)
            {
                return;
            }
            bool onGround = OnGround();

            float y = Speed.Y;
            float num = (onGround ? 75f : 5f);
            Speed.X = Calc.Approach(Speed.X, 0f, num * Engine.DeltaTime);
            if (!onGround)
            {
                Speed.Y = Calc.Approach(Speed.Y, MaxFallSpeed, Gravity * Engine.DeltaTime);
                OnGroundTimer = 0;
            }
            else
            {
                OnGroundTimer += Engine.DeltaTime;
            }
            if (Speed != Vector2.Zero && Launched)
            {
                Vector2 position = Position;
                MoveH(Speed.X * Engine.DeltaTime);
                MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
                bool interval = Scene.OnInterval(0.035f);
                if (Speed.Y != 0f)
                {
                    Speed.Y += Engine.DeltaTime * Gravity;
                }
                if (Speed.X != 0f)
                {
                    if (onGround)
                    {
                        Speed.X = Calc.Approach(Speed.X, 0, Engine.DeltaTime * GroundFriction);
                    }
                    else
                    {
                        Speed.X = Calc.Approach(Speed.X, 0, Engine.DeltaTime * AirFriction);
                    }
                }
                else
                {
                    Speed.Y = 0;
                    if (Screw.Animating)
                    {
                        PianoModule.Session.PipeScrewRestingPoint = Position;
                        PianoModule.Session.PipeScrewRestingFrame = Screw.CurrentAnimationFrame;
                        Screw.Stop();
                    }
                }
            }
            if (previousLiftSpeed != Vector2.Zero && LiftSpeed == Vector2.Zero)
            {
                Speed += previousLiftSpeed;
            }
            previousLiftSpeed = LiftSpeed;
            Collidable = false;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (PianoModule.Session.PipeScrewRestingPoint.HasValue && UsedInCutscene)
            {
                Screw.Play("resting");
            }
        }
        public override void Render()
        {
            if (Launched && UsedInCutscene)
            {
                GFX.Game["objects/PuzzleIslandHelper/fluidPipe/screw/screwHole"].Draw(originalPosition);
                if (PianoModule.Session.PipeScrewRestingPoint.HasValue)
                {
                    Screw.RenderPosition = PianoModule.Session.PipeScrewRestingPoint.Value;
                }
            }
            base.Render();
        }
        public void Launch()
        {
            Launched = true;
            Screw.Play("spin");
            Position.X -= 8;
            Speed.X = -120f;
            Speed.Y = 40f;
        }
    }
}