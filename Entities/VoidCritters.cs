using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;

// PuzzleIslandHelper.VoidCritters
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/VoidCritters")]
    [Tracked]
    public class VoidCritters : Entity
    {
        private DynamicData data;
        private Particle[] Particles;
        private ParticleSystem system;
        private bool CutsceneOnDeactivate;
        private bool WhiteOut;
        private float WhiteOutFade;
        private bool InLight;
        private readonly int Limit = 500;
        private float Size = 3;
        private ParticleType Critters = new ParticleType
        {
            Size = 2f,
            Color = Color.Black,
            ColorMode = ParticleType.ColorModes.Static,
            LifeMin = 0.2f,
            SpeedMin = 20f,
            SpeedMax = 30f,
            SpeedMultiplier = 0.2f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 6f
        };
        public VoidCritters(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
        }
        public float MaxTime = 2.6f;
        public float GraceTime = 0.5f;
        public float Timer;
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Timer = MaxTime + GraceTime;
            Collider = new Hitbox(40, 40, -20, -20);
            scene.Add(system = new ParticleSystem(-1, Limit));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            system?.RemoveSelf();
        }
        private void HandlePosition(Player player)
        {
            data = DynamicData.For(system);
            Particles = data.Get<Particle[]>("particles");
            bool justEnteredLight = !wasInLight && InLight;
            for (int i = 0; i < Particles.Length; i++)
            {
                if (Particles[i].Life < Critters.LifeMin + (Critters.LifeMax - Critters.LifeMin) / 1.5f)
                {
                    Vector2 target = new Vector2(Calc.Random.Range(player.Center.X - player.Width, player.Center.X + player.Width), Calc.Random.Range(player.Center.Y - player.Height, player.Center.Y + player.Height));
                    if (justEnteredLight)
                    {
                        float length = Calc.Random.Range(6, 20);
                        Particles[i].Speed = 10 * Calc.AngleToVector(Calc.Random.NextAngle(), length);
                    }
                    else if (!InLight)
                    {
                        Particles[i].Position.X = Calc.Approach(Particles[i].Position.X, target.X, 6);
                        Particles[i].Position.Y = Calc.Approach(Particles[i].Position.Y, target.Y, 6);
                    }

                }
                if (justEnteredLight)
                {
                    Particles[i].Life = Calc.Random.Range(0.5f, 1f);
                }
                //Particles[i].StartColor = Color.Lerp(Color.Black, Color.White, (MaxTime + GraceTime) / (Timer + GraceTime));
            }
            data.Set("particles", Particles);
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            if (WhiteOut && CutsceneOnDeactivate)
            {
                Draw.Rect(level.Camera.X, level.Camera.Y, 320, 180, Color.White * WhiteOutFade);
                if (WhiteOutFade < 1)
                {
                    WhiteOutFade += Engine.DeltaTime;
                }
                else
                {
                    WhiteOut = false;
                    level.Session.SetFlag("voidCritterEnd");
                }
            }
        }
        /*        private IEnumerator InDark()
                {
                    if (Scene is not Level level || level.GetPlayer() is not Player player) yield break;
                    InRoutine = true;
                    system.Clear();
                    yield return EmitFor(player, 1, 10, Engine.DeltaTime);
                    yield return EmitFor(player, 2, 12, 0.1f);
                    yield return EmitFor(player, 3, 8, 0.05f);
                    yield return EmitFor(player, 5, 28, 0.01f);
                    if (player is not null && !player.Dead)
                    {
                        player.Die(Vector2.Zero);
                    }
                    system.RemoveSelf();
                    RemoveSelf();

                }*/
        private IEnumerator EmitFor(Player player, int amountPerTick, int loops, float interval)
        {
            for (int i = 0; i < loops; i++)
            {
                CritterParticles(player, amountPerTick);
                yield return interval;
            }
        }
        private bool wasInLight;

        private void CritterParticles(Player player, int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                Critters.LifeMax = Size;
                Critters.Direction = Calc.Random.NextAngle();
                Vector2 offset = Calc.AngleToVector(Critters.Direction, Size * 4);

                system.Emit(Critters, system.Center - offset, Color.Lerp(Color.Black, Color.White, Calc.Random.Range(0, 0.1f)));
                if (player is not null && !player.Dead)
                {
                    system.Center = player.Center;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player || player.Dead || player.JustRespawned) return;
            Position = player.Position;
            float timeMult = 1;
/*            if (CollideCheck<VoidLightHelperEntity>())
            {
                timeMult = 0.65f;
            }*/
            InLight = VoidSafeZone.Check(player);
            HandlePosition(player);
            if (!InLight)
            {
                if (wasInLight)
                {
                    system.Clear();
                    Timer = MaxTime + GraceTime;
                }
                else
                {
                    float amount = Calc.Max(0, 1 - Timer / MaxTime);
                    int emitAmount = (int)Calc.LerpClamp(1, 5, amount);
                    float interval = Calc.LerpClamp(0.2f, Engine.DeltaTime, amount);
                    if (Timer <= -GraceTime)
                    {
                        player.Die(Vector2.Zero);
                        RemoveSelf();
                        return;
                    }
                    else
                    {
                        Timer -= Engine.DeltaTime * timeMult;
                        if (Scene.OnInterval(interval))
                        {
                            CritterParticles(player, emitAmount);
                        }
                    }
                }
            }
            wasInLight = InLight;
        }
    }
}
