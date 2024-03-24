using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;

// PuzzleIslandHelper.VoidCritters
namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/VoidCritters")]
    [Tracked]
    public class VoidCritters : Entity
    {
        private Player player;
        private bool SetLife;

        private Level l;
        private DynamicData data;
        private Particle[] Particles;
        private ParticleSystem system;
        private Coroutine Routine;
        private bool InRoutine;

        private bool UsesFlag;
        private string flag;
        private bool CutsceneOnDeactivate;
        public static bool Enabled;
        private bool WhiteOut;
        private bool Continue;
        private float WhiteOutFade;
        private bool InLight
        {
            get
            {
                return VoidLightDetect.InLight;
            }
        }
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
            Enabled = UsesFlag && (scene as Level).Session.GetFlag(flag) || !UsesFlag;
        }
        private void HandlePosition()
        {
            data = DynamicData.For(system);
            Particles = data.Get<Particle[]>("particles");
            List<Particle> temp = new();
            foreach (Particle particle in Particles)
            {
                Particle _Particle = particle;
                if (particle.Life < Critters.LifeMin + (Critters.LifeMax - Critters.LifeMin) / 1.5f)
                {
                    Vector2 Target = new Vector2(Calc.Random.Range(player.Center.X - player.Width, player.Center.X + player.Width), Calc.Random.Range(player.Center.Y - player.Height, player.Center.Y + player.Height));
                    Vector2 Rate = new Vector2(Calc.Random.Range(2, 5), Calc.Random.Range(2, 5));
                    if (InLight)
                    {
                        _Particle.Position.X += _Particle.Speed.X / 2;
                        _Particle.Position.Y += _Particle.Speed.Y / 2;
                    }
                    else
                    {
                        _Particle.Position.X = Calc.Approach(particle.Position.X, Target.X, 6);
                        _Particle.Position.Y = Calc.Approach(particle.Position.Y, Target.Y, 6);
                    }
                }
                if (InLight && !SetLife)
                {
                    _Particle.Life = Calc.Random.Range(0.1f, 0.5f);
                }
                temp.Add(_Particle);
            }
            SetLife = true;
            Particles = temp.ToArray();
            data.Set("particles", Particles);

        }
        public override void Render()
        {
            base.Render();
            if (Scene as Level is null)
            {
                return;
            }
            l = Scene as Level;
            if (WhiteOut && CutsceneOnDeactivate)
            {
                Draw.Rect(l.Camera.X, l.Camera.Y, 320, 180, Color.White * WhiteOutFade);
                if (WhiteOutFade < 1)
                {
                    WhiteOutFade += Engine.DeltaTime;
                }
                else
                {
                    Continue = true;
                    WhiteOut = false;
                    l.Session.SetFlag("voidCritterEnd");
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
            //yield return 0.5f;
            for (int i = 0; i < 10; i++)
            {
                CritterParticles();
                system.Clear();
                yield return null;
            }
            for (int i = 0; i < 12; i++)
            {
                CritterParticles();
                CritterParticles();
                yield return 0.05f;
            }
            for (int i = 0; i < 8; i++)
            {
                CritterParticles();
                CritterParticles();
                CritterParticles();
                yield return 0.02f;
            }
            for (int i = 0; i < 28; i++)
            {
                CritterParticles();
                CritterParticles();
                CritterParticles();
                CritterParticles();
                CritterParticles();
                yield return 0.01f;
            }
            if (player is not null && !player.Dead)
            {
                player.Die(Vector2.Zero);
            }
            system.RemoveSelf();
            RemoveSelf();

        }
        private void CritterParticles()
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
        public override void Update()
        {
            base.Update();
            if (player is null)
            {
                return;
            }
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
