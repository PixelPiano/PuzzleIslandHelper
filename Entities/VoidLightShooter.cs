using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.IO.Ports;
using System.Runtime.InteropServices;
using static Celeste.Overworld;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public enum Directions
    {
        Left, Right, Up, Down
    }
    [CustomEntity("PuzzleIslandHelper/VoidLightShooter")]
    [Tracked]
    public class VoidLightShooter : Entity
    {
        public Sprite Sprite;
        public Directions Direction;
        public int Radius;
        public float Interval;
        public float Speed;
        public VoidLightShooter(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/wip/texture");
            Sprite.AddLoop("idle", "", 0.1f);
            Add(Sprite);
            Sprite.Play("idle");
            Collider = new Hitbox(Sprite.Width, Sprite.Height);
            Direction = data.Enum("direction", Directions.Left);

            Radius = data.Int("bulletRadius", 16);
            Speed = data.Float("bulletSpeed", 30);
            Interval = Calc.Max(data.Float("shootInterval", 1.2f), Engine.DeltaTime);
        }
        public override void Update()
        {
            base.Update();
            if (Scene.OnInterval(Interval))
            {
                Scene.Add(new VoidLightBullet(Center, Direction, Speed, Radius));
            }
        }
    }
    [Tracked]
    public class VoidLightBullet : Actor
    {
        public Vector2 Dir;
        private float speed;
        private VertexLight light;
        public float Radius;
        public Directions Direction;
        public bool FadingOut { get; private set; }
        public VoidLightBullet(Vector2 position, Directions direction, float speed, int radius) : base(position)
        {
            Dir = direction switch
            {
                Directions.Left => -Vector2.UnitX,
                Directions.Right => Vector2.UnitX,
                Directions.Up => -Vector2.UnitY,
                Directions.Down => Vector2.UnitY,
                _ => Vector2.Zero
            };
            Direction = direction;
            this.speed = speed;
            Add(light = new VertexLight(Vector2.Zero, Color.White, 1, radius, radius + 16));
            CritterLight cLight = new CritterLight(radius, light);
            cLight.Enabled = true;
            Add(cLight);
            Radius = radius;
            Collider = new Hitbox(8, 8, -4, -4);
        }
        public override void DebugRender(Camera camera)
        {
            if (Collider != null)
            {
                Draw.HollowRect(Collider, FadingOut ? Color.Blue * light.Alpha : Color.Red);
            }
        }
        public IEnumerator FadeOutRoutine()
        {
            FadingOut = true;
            float from = light.Alpha;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.2f)
            {
                light.Alpha = Calc.LerpClamp(from, 0, Ease.SineIn(i));
                yield return null;
            }
            RemoveSelf();
        }
        public void FadeOut()
        {
            FadingOut = true;
            Add(new Coroutine(FadeOutRoutine()));
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            NaiveMove(Dir * speed * Engine.DeltaTime);
            if (!FadingOut)
            {
                if (CollideCheck<Solid>(Position + Dir * speed * Engine.DeltaTime))
                {
                    speed = 0;
                    FadeOut();
                }
                else if (!level.Bounds.Colliding(Collider.Bounds))
                {
                    FadeOut();
                }
            }
        }
    }
}
