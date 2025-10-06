using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/Everblossom")]
    [Tracked]
    public class Everblossom : Entity
    {
        public Image Image;
        public float Life = 1;
        public static int BlossomsInScene;
        public Everblossom(EntityData data, Vector2 offset) : this(data.Position + offset)
        {
        }
        public Everblossom(Vector2 position) : base(position)
        {
            Add(Image = new Image(GFX.Game["objects/PuzzleIslandHelper/everblossom"]));
            Image.JustifyOrigin(new Vector2(0.5f, 1));
            Image.Position += Image.Origin;
            Collider = Image.Collider();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            BlossomsInScene++;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            BlossomsInScene--;
        }
        public override void Update()
        {
            base.Update();
            float prev = Life;
            Life = Calc.Approach(Life, 0, Engine.DeltaTime / 2f);
            Image.Scale = Vector2.One * Life;
            if (prev >= 0.1f && Life < 0.1f)
            {
                Die();
            }
        }
        public void Die()
        {
            switch (Calc.Random.Choose(0, 1, 2))
            {
                case 0:
                    Scene.Add(new EverblossomSeed(this, 1));
                    break;
                case 1:
                    Scene.Add(new EverblossomSeed(this, -1));
                    break;
                case 2:
                    Scene.Add(new EverblossomSeed(this, -1));
                    Scene.Add(new EverblossomSeed(this, 1));
                    break;
            }
            RemoveSelf();
        }
        [Tracked]
        public class EverblossomSeed : Actor
        {
            public int Direction;
            public Everblossom Parent;
            public Vector2 Speed;
            public string LevelName;
            public EverblossomSeed(Everblossom parent, int direction) : base(parent.BottomCenter)
            {
                Parent = parent;
                Direction = direction;
                Collider = new Hitbox(1, 1);
                Speed.Y = -120f;
                Speed.X = Direction * 60f;
            }
            public override void Render()
            {
                base.Render();
                Draw.Point(Position, Color.Cyan);
                Draw.Point(Position + Vector2.UnitX, Color.Magenta);
                Draw.Point(Position - Vector2.UnitX, Color.Magenta);
                Draw.Point(Position + Vector2.UnitY, Color.Magenta);
                Draw.Point(Position - Vector2.UnitY, Color.Magenta);
            }
            public override void Update()
            {
                base.Update();
                Rectangle bounds = SceneAs<Level>().Bounds;
                if (!bounds.Contains(Position, 4))
                {
                    RemoveSelf();
                    return;
                }
                Speed.Y = Calc.Approach(Speed.Y, 90f, 200f * Engine.DeltaTime);
                MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
            }
            public void OnCollideV(CollisionData data)
            {
                if (Speed.Y < 0)
                {
                    Speed.Y *= -1;
                }
                else
                {
                    if (BlossomsInScene < 200 && !CollideCheck<Everblossom>())
                    {
                        Scene.Add(new Everblossom(Position));
                    }
                    RemoveSelf();
                }
            }
            public void OnCollideH(CollisionData data)
            {
                Speed.X *= -0.8f;
                if (Math.Abs(Speed.X) < 1)
                {
                    Speed.X = 0;
                }
            }
        }
    }
}
