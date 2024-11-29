using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked(false)]
    public class AfterImage : Entity
    {
        public VirtualRenderTarget Target;
        public float Alpha = 0.8f;
        private bool renderedOnce;
        public Action DrawFunction;
        public float ScaleOutRate;
        public Effect Effect;
        public Entity Entity;
        public AfterImage(Entity entity, Action drawZero, float duration = 1, float startAlpha = 0.8f, Effect effect = null) : this(entity.Position, entity.Width, entity.Height, drawZero, duration, startAlpha, effect)
        {
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
        public AfterImage(Vector2 position, float width, float height, Action drawAtVector2Zero, float duration = 1, float startAlpha = 0.8f, Effect effect = null) : base(position)
        {
            Alpha = startAlpha;
            Effect = effect;
            Target = VirtualContent.CreateRenderTarget("afterImage", (int)width, (int)height);
            Collider = new Hitbox((int)width, (int)height);
            DrawFunction = drawAtVector2Zero;
            Tag |= Tags.TransitionUpdate;
            Add(new BeforeRenderHook(BeforeRender));
            if (duration > 0)
            {
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, duration, true);
                tween.OnUpdate = t =>
                {
                    Alpha = Calc.LerpClamp(startAlpha, 0, t.Eased);
                };
                tween.OnComplete = delegate { RemoveSelf(); };
                Add(tween);
            }
        }
        public Action<Vector2, float> AlphaDraw;
        public AfterImage(Vector2 position, Action<Vector2, float> alphaDraw, float duration = 1, float startAlpha = 0.8f) : base(position)
        {
            Alpha = startAlpha;
            AlphaDraw = alphaDraw;
            Tag |= Tags.TransitionUpdate;
            if (duration > 0)
            {
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, duration, true);
                tween.OnUpdate = t =>
                {
                    Alpha = Calc.LerpClamp(startAlpha, 0, t.Eased);
                };
                tween.OnComplete = delegate { RemoveSelf(); };
                Add(tween);
            }
        }
        public AfterImage(Collider collider, Action drawAtVector2Zero, float duration = 1, float startAlpha = 0.8f, Effect effect = null)
            : this(collider.AbsolutePosition, collider.Width, collider.Height, drawAtVector2Zero, duration, startAlpha, effect)
        {
        }
        public AfterImage(Rectangle rect, Action drawAtVector2Zero, float duration = 1, float startAlpha = 0.8f, Effect effect = null)
            : this(rect.Location.ToVector2(), rect.Width, rect.Height, drawAtVector2Zero, duration, startAlpha, effect)
        {
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
            if (renderedOnce || DrawFunction == null || Target == null) return;
            Target.SetAsTarget(Color.Transparent);
            Draw.SpriteBatch.StandardBegin(Matrix.Identity, Effect);
            DrawFunction?.Invoke();
            Draw.SpriteBatch.End();
            renderedOnce = true;
        }
        public override void Render()
        {
            base.Render();
            if (Alpha > 0)
            {
                if (Target != null)
                {
                    Draw.SpriteBatch.Draw(Target, Position, null, Color.White * Alpha);
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
            AfterImage image = new(position, width, height, drawAtVector2Zero, duration, startAlpha, effect);
            if (Engine.Scene is Level level)
            {
                level.Add(image);
            }
            return image;
        }
        public static AfterImage Create(Entity entity, Action drawZero, float duration = 1, float startAlpha = 0.8f, Effect effect = null)
        {
            AfterImage image = new(entity, drawZero, duration, startAlpha, effect);
            if (Engine.Scene is Level level)
            {
                level.Add(image);
            }
            return image;
        }
    }
}