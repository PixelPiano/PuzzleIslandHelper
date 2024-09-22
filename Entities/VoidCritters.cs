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
        private Player player;
        private bool SetLife;
        private DynamicData data;
        private Particle[] Particles;
        private ParticleSystem system;
        private Coroutine Routine;
        private bool InRoutine;

        private bool UsesFlag;
        private string flag;
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
            CutsceneOnDeactivate = data.Bool("cutsceneOnDeactivate");
            UsesFlag = data.Bool("usesFlag");
            flag = data.Attr("flag");

            Routine = new Coroutine(InDark());
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(system = new ParticleSystem(-1, Limit));
            player = (scene as Level).Tracker.GetEntity<Player>();
        }
        private void HandlePosition()
        {
            data = DynamicData.For(system);
            Particles = data.Get<Particle[]>("particles");
            for (int i = 0; i < Particles.Length; i++)
            {
                if (Particles[i].Life < Critters.LifeMin + (Critters.LifeMax - Critters.LifeMin) / 1.5f)
                {
                    Vector2 target = new Vector2(Calc.Random.Range(player.Center.X - player.Width, player.Center.X + player.Width), Calc.Random.Range(player.Center.Y - player.Height, player.Center.Y + player.Height));
                    Vector2 rate = new Vector2(Calc.Random.Range(2, 5), Calc.Random.Range(2, 5));
                    if (InLight)
                    {
                        Particles[i].Position.X += Particles[i].Speed.X / 2;
                        Particles[i].Position.Y += Particles[i].Speed.Y / 2;
                    }
                    else
                    {
                        Particles[i].Position.X = Calc.Approach(Particles[i].Position.X, target.X, 6);
                        Particles[i].Position.Y = Calc.Approach(Particles[i].Position.Y, target.Y, 6);
                    }
                }
                if (InLight && !SetLife)
                {
                    Particles[i].Life = Calc.Random.Range(0.1f, 0.5f);
                }
            }
            SetLife = true;
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
        private IEnumerator End()
        {

            InRoutine = true;
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 12; i++)
                {
                    for (int k = 0; k <= j; k++)
                    {
                        CritterParticles();
                    }
                    yield return 0.05f;
                }
            }
            WhiteOut = true;

            if (player is not null && !player.Dead)
            {
                player.Die(Vector2.Zero);
            }
            system.RemoveSelf();
            RemoveSelf();

        }
        private IEnumerator InDark()
        {
            InRoutine = true;
            system.Clear();
            yield return EmitFor(1, 10, Engine.DeltaTime);
            yield return EmitFor(2, 12, 0.05f);
            yield return EmitFor(3, 8, 0.02f);
            yield return EmitFor(5, 28, 0.01f);
            if (player is not null && !player.Dead)
            {
                player.Die(Vector2.Zero);
            }
            system.RemoveSelf();
            RemoveSelf();

        }
        private IEnumerator EmitFor(int amountPerTick, int loops, float interval)
        {
            for (int i = 0; i < loops; i++)
            {
                CritterParticles(amountPerTick);
                yield return interval;
            }
        }
        private void CritterParticles(int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                Critters.LifeMax = Size;
                Critters.Direction = Calc.Random.NextAngle();
                Vector2 offset = Calc.AngleToVector(Critters.Direction, Size * 4);
                system.Emit(Critters, system.Center - offset);
                if (player is not null)
                {
                    system.Center = player.Center;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (player is null)
            {
                return;
            }
            InLight = player.CollideFirst<VoidLightHelperEntity>() is VoidLightHelperEntity entity && entity.State;
            HandlePosition();
            if (Routine.Active && InLight)
            {
                SetLife = false;
                Routine.Active = false;
                Remove(Routine);
                InRoutine = false;
            }
            if (!InLight && !InRoutine)
            {
                Add(Routine = new Coroutine(InDark()));
            }
        }
    }
}
