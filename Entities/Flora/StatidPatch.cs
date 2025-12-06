using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
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
        public float DeadChance;
        public List<Statid> Flowers = new();
        private Entity collide;
        private Random Random;
        private EntityID id;
        public StatidPatch(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            HalfStepChance = data.Float("halfStepChance");
            Spacing = data.Float("spacing", 8);
            Digital = data.Bool("digital");
            Petals = data.Int("petals", 4);
            PetalRange = data.Int("petalRange");
            DeadChance = data.Float("deadChance");
            int y = (int)Y;
            int x = (int)X;
            if (x == 0) x = 1;
            if (y == 0) y = 1;
            Random = new Random(x + y + (y * x) - (x * x));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            collide = new Entity(Position);
            collide.Collider = new Hitbox(1, 1);
            scene.Add(collide);
        }
        public override void Update()
        {
            base.Update();
            bool inView = InView();
            foreach (Statid s in Flowers)
            {
                s.Sleeping = !inView;
            }
        }
        public bool InView()
        {
            Camera camera = (Scene as Level).Camera;
            float xPad = Width;
            float yPad = Height;
            if (X > camera.X - xPad && Y > camera.Y - yPad && X < camera.X + 320f + xPad)
            {
                return Y < camera.Y + 180f + yPad;
            }
            return false;
        }
        public List<Statid.Data> Data = null;
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            int count = 0;
            int absRange = Math.Abs(PetalRange);
            Level level = scene as Level;
            PianoModule.Session.StatidPatchData.TryGetValue(this.id, out Data);
            bool createData = Data == null;
            if (createData)
            {
                Data = [];
                PianoModule.Session.StatidPatchData.Add(this.id, Data);
            }
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
                    float y = Random.Range(0, dist) + Top;
                    int p = Petals;
                    if (PetalRange > 0)
                    {
                        p += Random.Range(-absRange, absRange + 1);
                    }

                    EntityID id = new EntityID(Guid.NewGuid().ToString(), 0);
                    int depth = Random.Choose(-1, 2, -10000);
                    bool dead = Random.Chance(DeadChance);
                    Statid flower = new Statid(new Vector2(collide.Position.X, y), p, Digital, Vector2.One * 4, 2, id, depth, Color.White, Color.Black, dead);
                    if (createData)
                    {
                        Statid.Data flowerData = new()
                        {
                            HasSap = false,
                            IsSapped = false,
                        };
                        Data.Add(flowerData);
                    }
                    flower.data = Data[count];
                    level.Add(flower);
                    if (flower.Depth < -1)
                    {
                        flower.GroundOffset = Random.Range(0, 7) * Vector2.UnitY;
                    }
                    Flowers.Add(flower);
                    count++;
                }
                collide.X += (int)(Random.Chance(HalfStepChance) ? Spacing / 2f : Spacing);
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
