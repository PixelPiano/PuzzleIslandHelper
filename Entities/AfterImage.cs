using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked(false)]
    public class AfterImage : Actor
    {
        public VirtualRenderTarget Target;
        public float Alpha = 0.8f;
        private bool renderedOnce;
        public bool DrawOnce = true;
        public Action DrawFunction;
        public bool ScaleOut;
        public Ease.Easer ScaleEase = Ease.Linear;
        private float scaleMult = 1;
        public Effect Effect;
        public Entity Entity;
        public Vector2 Speed;
        public Vector2 Acceleration;
        public float Friction;
        public Vector2 Scale
        {
            get => _scale * scaleMult;
            set => _scale = value;
        }
        public float Duration;
        private Vector2 _scale = Vector2.One;
        public float StartAlpha;
        public Action<Vector2, float> AlphaDraw;
        public Action<AfterImage, CollisionData> OnCollideH;
        public Action<AfterImage, CollisionData> OnCollideV;
        public AfterImage(Entity entity, Action drawZero, float duration = 1, float startAlpha = 0.8f, Effect effect = null, int? depth = null) : this(entity.Position, drawZero, entity.Width, entity.Height, duration, startAlpha, effect)
        {
            Depth = depth ?? entity.Depth;
        }
        public void DrawZero()
        {
            if (Entity != null)
            {
                Vector2 prev = Entity.Position;
                Entity.Position = Vector2.Zero;
                Entity.Render();
                Entity.Position = prev;
            }
        }
        public AfterImage(Vector2 position, Action drawAtVector2Zero, float width, float height, float duration = 1, float startAlpha = 0.8f, Effect effect = null) : base(position)
        {
            Duration = duration;
            StartAlpha = Alpha = startAlpha;
            Effect = effect;
            Target = VirtualContent.CreateRenderTarget("afterImage", (int)width, (int)height);
            Collider = new Hitbox((int)width, (int)height);
            DrawFunction = drawAtVector2Zero;
            Tag |= Tags.TransitionUpdate;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public AfterImage(Vector2 position, Action<Vector2, float> alphaDraw, float duration = 1, float startAlpha = 0.8f) : base(position)
        {
            Alpha = StartAlpha = startAlpha;
            AlphaDraw = alphaDraw;
            Duration = duration;
            Tag |= Tags.TransitionUpdate;
        }
        public AfterImage(Collider collider, Action drawAtVector2Zero, float duration = 1, float startAlpha = 0.8f, Effect effect = null)
            : this(collider.AbsolutePosition, drawAtVector2Zero, collider.Width, collider.Height, duration, startAlpha, effect)
        {
        }
        public AfterImage(Rectangle rect, Action drawAtVector2Zero, float duration = 1, float startAlpha = 0.8f, Effect effect = null)
            : this(rect.Location.ToVector2(), drawAtVector2Zero, rect.Width, rect.Height, duration, startAlpha, effect)
        {
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Tween.Set(this, Tween.TweenMode.Oneshot, Duration, Ease.Linear, t =>
            {
                Alpha = StartAlpha * (1 - t.Eased);
                if (ScaleOut)
                {
                    scaleMult = 1 - ScaleEase(t.Eased);
                }
            }, t2 =>
            {
                RemoveSelf();
            });

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Effect?.Dispose();
            Effect = null;
            Target?.Dispose();
            Target = null;
        }
        public void BeforeRender()
        {
            if ((DrawOnce && renderedOnce) || DrawFunction == null || Target == null) return;
            Target.SetAsTarget(Color.Transparent);
            Draw.SpriteBatch.StandardBegin(Matrix.Identity, Effect);
            DrawFunction?.Invoke();
            Draw.SpriteBatch.End();
            renderedOnce = true;
        }
        private void onCollideH(CollisionData data)
        {
            if (Collidable) OnCollideH?.Invoke(this, data);
        }
        private void onCollideV(CollisionData data)
        {
            if (Collidable) OnCollideV?.Invoke(this, data);
        }
        public override void Update()
        {
            base.Update();
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            Speed += Acceleration * Engine.DeltaTime;
            Speed = Calc.Approach(Speed, Vector2.Zero, Friction * Engine.DeltaTime);
        }
        public override void Render()
        {
            base.Render();
            if (Alpha > 0)
            {
                if (Target != null)
                {
                    Draw.SpriteBatch.Draw(Target, Position + Collider.HalfSize, null, Color.White * Alpha, 0, Collider.HalfSize, Scale, SpriteEffects.None, 0);
                }
                else
                {
                    AlphaDraw?.Invoke(Position, Alpha);
                }
            }
        }
        public static AfterImage Create(Vector2 position, Action<Vector2, float> render, float duration = 1, float startAlpha = 0.8f)
        {
            AfterImage image = new(position, render, duration, startAlpha);
            if (Engine.Scene is Level level)
            {
                level.Add(image);
            }
            return image;
        }
        public static AfterImage Create(Vector2 position, float width, float height, Action drawAtVector2Zero, float duration = 1, float startAlpha = 0.8f, Effect effect = null)
        {
            AfterImage image = new(position, drawAtVector2Zero, width, height, duration, startAlpha, effect);
            if (Engine.Scene is Level level)
            {
                level.Add(image);
            }
            return image;
        }
        public static AfterImage Create(Entity entity, Action drawZero, float duration = 1, float startAlpha = 0.8f, Effect effect = null, int? depth = null)
        {
            AfterImage image = new(entity, drawZero, duration, startAlpha, effect, depth);
            if (Engine.Scene is Level level)
            {
                level.Add(image);
            }
            return image;
        }
    }
}