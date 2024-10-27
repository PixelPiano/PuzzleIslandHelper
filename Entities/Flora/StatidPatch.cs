using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/StatidPatch")]
    [Tracked]
    public class StatidPatch : Entity
    {
        public float HalfStepChance;
        public float Spacing;
        public bool Digital;
        public int Petals;
        public int PetalRange;

        public List<Statid> Flowers = new();
        private Entity collide;
        public StatidPatch(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            HalfStepChance = data.Float("halfStepChance");
            Spacing = data.Float("spacing", 8);
            Digital = data.Bool("digital");
            Petals = data.Int("petals", 4);
            PetalRange = data.Int("petalRange");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            collide = new Entity(Position);
            collide.Collider = new Hitbox(1, 1);
            scene.Add(collide);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            int absRange = Math.Abs(PetalRange);
            Level level = scene as Level;
            while (collide.X < Right)
            {
                collide.Y = Top;
                int dist = 0;
                while (!collide.CollideCheck<Solid>() && collide.Y <= Bottom)
                {
                    collide.Y++;
                    if (collide.Y > level.Bounds.Bottom)
                    {
                        dist = -1;
                        break;
                    }
                    dist++;
                }
                if (dist > 1)
                {
                    if (dist > 7) dist -= 3;
                    float y = Calc.Random.Range(0, dist) + Top;
                    int p = Petals;
                    if (PetalRange > 0)
                    {
                        p += Calc.Random.Range(-absRange, absRange + 1);
                    }

                    Statid flower = new Statid(new Vector2(collide.Position.X, y), p, Digital, Vector2.One * 4, 2);
                    EntityID id = new EntityID(Guid.NewGuid().ToString(),0);
                    flower.ID = id;
                    level.Add(flower);
                    flower.Depth = Calc.Random.Choose(-1, 1);
                    Flowers.Add(flower);
                }
                collide.X += (int)(Calc.Random.Chance(HalfStepChance) ? Spacing / 2f : Spacing);
            }
            level.Remove(collide);

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (Statid statid in Flowers)
            {
                statid.RemoveSelf();
            }
        }
    }
}
