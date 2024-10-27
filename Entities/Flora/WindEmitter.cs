using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/WindEmitter")]
    [Tracked]
    public class WindEmitter : Entity
    {
        public enum Directions
        {
            Up, Right, Left, Down
        }
        public Directions Direction;
        public float Interval;
        public float IntervalVariance;
        private float timer;
        public float MinSpeed;
        public float MaxSpeed;
        public float MinAccel;
        public float MaxAccel;
        public WindEmitter(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Direction = data.Enum<Directions>("direction");
            Interval = data.Float("interval", 0.1f);
            IntervalVariance = data.Float("intervalVariance");
            Collider = new Hitbox(data.Width, data.Height);
            MinSpeed = data.Float("minSpeed");
            MaxSpeed = data.Float("maxSpeed");
            MinAccel = data.Float("minAcceleration");
            MaxAccel = data.Float("maxAcceleration");
        }
        public override void Update()
        {
            base.Update();
            if (timer > 0)
            {
                timer -= Engine.DeltaTime;
            }
            else
            {
                float abs = Math.Abs(IntervalVariance);
                timer = Calc.Max(Interval + Calc.Random.Range(-abs, abs), 0);
                CreateNewParticle();
            }
        }
        public Vector2 GetPosition()
        {
            return Direction switch
            {
                Directions.Left or Directions.Right => TopCenter,
                _ => CenterLeft
            };
        }
        public void CreateNewParticle()
        {
            Scene.Add(new WindParticle(GetPosition(), Width, Height, Direction, 80f, 0));
        }
        [Tracked]
        public class WindParticle : Actor
        {
            public float Speed;
            public Directions Direction;
            public float Acceleration;
            private Vector2 dirMult;
            public WindParticle(Vector2 position, float width, float height, Directions direction, float speed, float acceleration) : base(position)
            {
                Collider = direction switch
                {
                    Directions.Up or Directions.Down => new Hitbox(width, 1),
                    Directions.Left or Directions.Right => new Hitbox(1, height),
                    _ => null
                };
                Speed = speed;
                Direction = direction;
                Acceleration = acceleration;
                dirMult = direction switch
                {
                    Directions.Up => -Vector2.UnitY,
                    Directions.Right => Vector2.UnitX,
                    Directions.Left => -Vector2.UnitX,
                    Directions.Down => Vector2.UnitY,
                    _ => Vector2.Zero
                };
                Add(new Coroutine(Variance()));
            }
            private IEnumerator Variance()
            {
                while (true)
                {
                    float from = Acceleration;
                    float to = Calc.Random.Range(-4, 4f);
                    float time = Calc.Random.Range(0.3f, 1.5f);
                    for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                    {
                        Acceleration = Calc.LerpClamp(from, to, Ease.SineInOut(i));
                        yield return null;
                    }

                }
            }
            public override void Update()
            {
                base.Update();
                Position += dirMult * (Speed + Acceleration) * Engine.DeltaTime;
                Speed = Calc.Approach(Speed, 0, 10f * Engine.DeltaTime);
                foreach (Statid s in CollideAll<Statid>())
                {
                    float speed = (Speed + Acceleration) * Engine.DeltaTime * 0.06f;
                    s.TargetMult = Calc.Min(s.TargetMult + speed, 1);
                }
                if (Speed < 4)
                {
                    RemoveSelf();
                }
            }
        }
    }
}
