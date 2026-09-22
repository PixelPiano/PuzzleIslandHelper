using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class DepthParticleSystem : ParticleSystem
    {
        public struct DepthParticle : IComparable<DepthParticle>
        {
            public int Index;
            public float Depth;
            public int CompareTo(DepthParticle other) => Depth.CompareTo(other.Depth);
        }
        private DepthParticle[] depthParticles;
        private bool reSort;
        public DepthParticleSystem(int depth, int maxParticles) : base(depth, maxParticles)
        {
            depthParticles = new DepthParticle[maxParticles];
        }
        public override void Update()
        {
            base.Update();
            if (reSort)
            {
                Array.Sort(depthParticles);
                reSort = false;
            }
        }
        public override void Render()
        {
            for (int i = 0; i < depthParticles.Length; i++)
            {
                Particle particle = particles[depthParticles[i].Index];
                if (particle.Active)
                {
                    particle.Render();
                }
            }
        }
        public void EmitDepth(ParticleType type, Vector2 position, float depth)
        {
            type.Create(ref particles[nextSlot], position);
            depthParticles[nextSlot].Depth = depth;
            depthParticles[nextSlot].Index = nextSlot;
            nextSlot = (nextSlot + 1) % particles.Length;
            reSort = true;
        }


        public void EmitDepth(ParticleType type, Vector2 position, float direction, float depth)
        {
            type.Create(ref particles[nextSlot], position, direction);
            depthParticles[nextSlot].Depth = depth;
            depthParticles[nextSlot].Index = nextSlot;
            nextSlot = (nextSlot + 1) % particles.Length;
            reSort = true;
        }


        public void EmitDepth(ParticleType type, Vector2 position, Color color, float depth)
        {
            type.Create(ref particles[nextSlot], position, color);
            depthParticles[nextSlot].Depth = depth;
            depthParticles[nextSlot].Index = nextSlot;
            nextSlot = (nextSlot + 1) % particles.Length;
            reSort = true;
        }


        public void EmitDepth(ParticleType type, Vector2 position, Color color, float direction, float depth)
        {
            type.Create(ref particles[nextSlot], position, color, direction);
            depthParticles[nextSlot].Depth = depth;
            depthParticles[nextSlot].Index = nextSlot;
            nextSlot = (nextSlot + 1) % particles.Length;
            reSort = true;
        }


        public void EmitDepth(ParticleType type, int amount, Vector2 position, Vector2 positionRange, float depth)
        {
            for (int i = 0; i < amount; i++)
            {
                EmitDepth(type, Calc.Random.Range(position - positionRange, position + positionRange), depth);
            }
        }


        public void EmitDepth(ParticleType type, int amount, Vector2 position, Vector2 positionRange, float direction, float depth)
        {
            for (int i = 0; i < amount; i++)
            {
                EmitDepth(type, Calc.Random.Range(position - positionRange, position + positionRange), direction, depth);
            }
        }


        public void EmitDepth(ParticleType type, int amount, Vector2 position, Vector2 positionRange, Color color, float depth)
        {
            for (int i = 0; i < amount; i++)
            {
                EmitDepth(type, Calc.Random.Range(position - positionRange, position + positionRange), color, depth);
            }
        }


        public void EmitDepth(ParticleType type, int amount, Vector2 position, Vector2 positionRange, Color color, float direction, float depth)
        {
            for (int i = 0; i < amount; i++)
            {
                EmitDepth(type, Calc.Random.Range(position - positionRange, position + positionRange), color, direction, depth);
            }
        }


        public void EmitDepth(ParticleType type, Entity track, int amount, Vector2 position, Vector2 positionRange, float direction, float depth)
        {
            for (int i = 0; i < amount; i++)
            {
                type.Create(ref particles[nextSlot], track, Calc.Random.Range(position - positionRange, position + positionRange), direction, type.Color);
                depthParticles[nextSlot].Index = nextSlot;
                depthParticles[nextSlot].Depth = depth;
                nextSlot = (nextSlot + 1) % particles.Length;
            }
            reSort = true;
        }
    }
}
