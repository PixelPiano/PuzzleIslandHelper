using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// PuzzleIslandHelper.VoidCritters
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [TrackedAs(typeof(ParticleSystem))]
    public class ParticleSystemExt : Entity
    {
        public Particle[] particles;

        public int nextSlot;
        private bool flashing;

        public ParticleSystemExt(int depth, int maxParticles)
        {
            particles = new Particle[maxParticles];
            base.Depth = depth;
        }
        public void Flash(Tween.TweenMode mode, Color color, float duration, Ease.Easer ease)
        {
            Color[] colors = new Color[particles.Length];
            Color[] newColors = new Color[particles.Length];
            for (int i = 0; i < particles.Length; i++)
            {
                colors[i] = particles[i].Color;
            }
            Tween t = Tween.Create(mode, ease, duration, true);
            t.OnUpdate = (Tween t) =>
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Color = Color.Lerp(color, colors[i], t.Eased);
                }
            };
            
            t.OnComplete = delegate { flashing = false; };
            Add(t);
            flashing = true;
        }
        public void Flash(Tween.TweenMode mode, Color colorMin, Color colorMax, float maxLerp, float duration, Ease.Easer ease)
        {
            Color[] colors = new Color[particles.Length];
            Color[] newColors = new Color[particles.Length];
            for (int i = 0; i < particles.Length; i++)
            {
                colors[i] = particles[i].Color;
                newColors[i] = Color.Lerp(colorMin, colorMax, Calc.Random.NextFloat() * maxLerp);
            }
            Tween t = Tween.Create(mode, ease, duration, true);
            t.OnUpdate = (Tween t) =>
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Color = Color.Lerp(newColors[i], colors[i], t.Eased);
                }
            };
            t.OnComplete = delegate { flashing = false; };
            Add(t);
            flashing = true;
        }
        public void Clear()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Active = false;
            }
            Components.RemoveAll<Tween>();
            flashing = false;
        }

        public void ClearRect(Rectangle rect, bool inside)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                Vector2 position = particles[i].Position;
                if ((position.X > (float)rect.Left && position.Y > (float)rect.Top && position.X < (float)rect.Right && position.Y < (float)rect.Bottom) == inside)
                {
                    particles[i].Active = false;
                }
            }
        }

        public override void Update()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].Active)
                {
                    particles[i] = particleUpdate(particles[i]);
                    //particles[i].Update();
                }
            }
            base.Update();
        }
        public Particle particleUpdate(Particle particle)
        {
            float num = 0f;
            num = (particle.Type.UseActualDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime);
            float num2 = particle.Life / particle.StartLife;
            particle.Life -= num;
            if (particle.Life <= 0f)
            {
                particle.Active = false;
                return particle;
            }

            if (particle.Type.RotationMode == ParticleType.RotationModes.SameAsDirection)
            {
                if (particle.Speed != Vector2.Zero)
                {
                    particle.Rotation = particle.Speed.Angle();
                }
            }
            else
            {
                particle.Rotation += particle.Spin * num;
            }

            float num3 = ((particle.Type.FadeMode == ParticleType.FadeModes.Linear) ? num2 : ((particle.Type.FadeMode == ParticleType.FadeModes.Late) ? Math.Min(1f, num2 / 0.25f) : ((particle.Type.FadeMode != ParticleType.FadeModes.InAndOut) ? 1f : ((num2 > 0.75f) ? (1f - (num2 - 0.75f) / 0.25f) : ((!(num2 < 0.25f)) ? 1f : (num2 / 0.25f))))));
            if (num3 == 0f)
            {
                particle.Color = Color.Transparent;
            }
            else
            {
                if (particle.Type.ColorMode == ParticleType.ColorModes.Static)
                {
                    particle.Color = particle.StartColor;
                }
                else if (particle.Type.ColorMode == ParticleType.ColorModes.Fade)
                {
                    particle.Color = Color.Lerp(particle.Type.Color2, particle.StartColor, num2);
                }
                else if (particle.Type.ColorMode == ParticleType.ColorModes.Blink)
                {
                    particle.Color = (Calc.BetweenInterval(particle.Life, 0.1f) ? particle.StartColor : particle.Type.Color2);
                }
                else if (particle.Type.ColorMode == ParticleType.ColorModes.Choose)
                {
                    particle.Color = particle.StartColor;
                }

                if (num3 < 1f)
                {
                    particle.Color *= num3;
                }
            }



            particle.Position += particle.Speed * num;
            particle.Speed += particle.Type.Acceleration * num;
            particle.Speed = Calc.Approach(particle.Speed, Vector2.Zero, particle.Type.Friction * num);
            if (particle.Type.SpeedMultiplier != 1f)
            {
                particle.Speed *= (float)Math.Pow(particle.Type.SpeedMultiplier, num);
            }

            if (particle.Type.ScaleOut)
            {
                particle.Size = particle.StartSize * Ease.CubeOut(num2);
            }
            return particle;

        }

        public override void Render()
        {
            Particle[] array = particles;
            for (int i = 0; i < array.Length; i++)
            {
                Particle particle = array[i];
                if (particle.Active)
                {
                    particle.Render();
                }
            }
        }

        public void Render(float alpha)
        {
            Particle[] array = particles;
            for (int i = 0; i < array.Length; i++)
            {
                Particle particle = array[i];
                if (particle.Active)
                {
                    particle.Render(alpha);
                }
            }
        }

        public void Simulate(float duration, float interval, Action<ParticleSystemExt> emitter)
        {
            float num = 0.016f;
            for (float num2 = 0f; num2 < duration; num2 += num)
            {
                if ((int)((num2 - num) / interval) < (int)(num2 / interval))
                {
                    emitter(this);
                }

                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i].Active)
                    {
                        particles[i].Update(num);
                    }
                }
            }
        }

        public void Add(Particle particle)
        {
            particles[nextSlot] = particle;
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, Vector2 position)
        {
            type.Create(ref particles[nextSlot], position);
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, Vector2 position, float direction)
        {
            type.Create(ref particles[nextSlot], position, direction);
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, Vector2 position, Color color)
        {
            type.Create(ref particles[nextSlot], position, color);
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, Vector2 position, Color color, float direction)
        {
            type.Create(ref particles[nextSlot], position, color, direction);
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange)
        {
            for (int i = 0; i < amount; i++)
            {
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange));
            }
        }

        public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange, float direction)
        {
            for (int i = 0; i < amount; i++)
            {
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), direction);
            }
        }

        public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange, Color color)
        {
            for (int i = 0; i < amount; i++)
            {
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), color);
            }
        }

        public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange, Color color, float direction)
        {
            for (int i = 0; i < amount; i++)
            {
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), color, direction);
            }
        }

        public void Emit(ParticleType type, Entity track, int amount, Vector2 position, Vector2 positionRange, float direction)
        {
            for (int i = 0; i < amount; i++)
            {
                type.Create(ref particles[nextSlot], track, Calc.Random.Range(position - positionRange, position + positionRange), direction, type.Color);
                nextSlot = (nextSlot + 1) % particles.Length;
            }
        }
    }
}

