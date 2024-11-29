using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using FMOD;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using FMOD.Studio;
using System;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/WipEntity")]
    [Tracked]
    public class FirfilShape : Entity
    {
        public enum Shapes
        {
            Infinity,
            Circle,
            Square,
            Triangle
        }
        public Shapes Shape;
        public List<FirfilDetector> Detectors = new();
        public const int MaxDetectors = 20;
        public FirfilShape(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(80, 40);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.PushOutOfSolids(-Vector2.UnitY);
            ArrangeDetectors();
        }
        public void ArrangeDetectors()
        {
            switch (Shape)
            {
                case Shapes.Infinity:
                    Vector2 center = Center;
                    float rate = 1f / MaxDetectors;
                    for (float i = 0; i < 1; i += rate)
                    {
                        Vector2 pos = lemniscate(center, Width / 2, Height, 0, i * MathHelper.TwoPi);
                        FirfilDetector d = new(pos, 4);
                        Detectors.Add(d);
                        Scene.Add(d);
                    }
                    break;
            }
        }
        public bool AllActivated()
        {
            foreach (FirfilDetector d in Detectors)
            {
                if (!d.Activated)
                {
                    return false;
                }
            }
            return true;
        }
        public void LockAll()
        {
            foreach (FirfilDetector d in Detectors)
            {
                d.Locked = true;
            }
        }
        public void UnlockAll()
        {
            foreach (FirfilDetector d in Detectors)
            {
                d.Locked = false;
            }
        }
        public override void Update()
        {
            base.Update();
            if (AllActivated())
            {
                LockAll();
            }
            else
            {
                UnlockAll();
            }
        }

        private static Vector2 lemniscate(Vector2 center, float radius, float rotation, float t)
            => lemniscate(radius, t).Rotate(rotation) + center;
        private static Vector2 lemniscate(Vector2 center, float width, float height, float rotation, float t)
            => lemniscate(width, height, t).Rotate(rotation) + center;
        private static Vector2 lemniscate(float radius, float t)
            => new(x: radius * (float)Math.Cos(t) / (1 + (float)Math.Pow(Math.Sin(t), 2)),
                y: radius * (float)Math.Sin(t) * (float)Math.Cos(t) / (1 + (float)Math.Pow(Math.Sin(t), 2)));
        private static Vector2 lemniscate(float wideness, float tallness, float t)
            => new(x: wideness * (float)Math.Cos(t) / (1 + (float)Math.Pow(Math.Sin(t), 2)),
                    y: tallness * (float)Math.Sin(t) * (float)Math.Cos(t) / (1 + (float)Math.Pow(Math.Sin(t), 2)));
    }
    [Tracked]
    public class FirfilDetector : Entity
    {
        public bool Locked;
        public bool Activated => Percent >= 1;
        public float Percent;
        public float MOETimer;
        public FirfilDetector(Vector2 position, float size) : base(position)
        {
            Collider = new Hitbox(size, size, -size / 2, -size / 2);
        }
        public override void Update()
        {
            base.Update();
            if (Locked)
            {
                return;
            }
            MOETimer -= Engine.DeltaTime;
            if (CollideCheck<Firfil>())
            {
                MOETimer = 0.1f;
                Percent += Engine.DeltaTime;
            }
            if (MOETimer <= 0)
            {
                Percent = Calc.Approach(Percent, 0, Engine.DeltaTime / 2f);
                MOETimer = 0;
            }
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(Collider, Locked ? Color.Green : Color.Red * Percent);
        }
    }
}