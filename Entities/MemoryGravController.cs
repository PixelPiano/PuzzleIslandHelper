using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MemoryGravController")]
    [Tracked]
    public class MemoryGravController : Entity
    {
        private bool inSequence;
        public FlagList Flag;
        public string Key;
        public DotX3 Talk;
        public Session.Slider Slider;
        public class MemoryGravUI : Entity
        {
            private MemoryGravController parent;
            private VirtualRenderTarget target;
            private float alpha;
            private bool enabled;
            private Vector2 sliderCenter = new Vector2(1920 / 2, 1080 / 2);
            private readonly float sliderWidth = 32;
            private readonly float sliderHeight = 1080 / 3;
            private float sliderX => sliderCenter.X - sliderWidth / 2;
            private float sliderY => sliderCenter.Y - sliderHeight / 2;
            public MemoryGravUI(MemoryGravController parent) : base()
            {
                this.parent = parent;
                Tag |= TagsExt.SubHUD;
                target = VirtualContent.CreateRenderTarget("MemoryGravUI", 1920, 1080);
                Add(new BeforeRenderHook(beforeRender));
            }
            private void beforeRender()
            {
                target.SetAsTarget(true);
                Draw.SpriteBatch.Begin();
                Rectangle rect = new Rectangle((int)sliderX, (int)sliderY, (int)sliderWidth, (int)sliderHeight);
                Draw.Rect(rect, Color.Black);
                float x = rect.X - (int)sliderWidth;
                float h = sliderWidth;
                float w = sliderWidth * 3;
                float y = rect.Y + (parent.Slider.Value / 100f) * rect.Height - h / 2;
                Draw.Rect(x, y, w, h, Color.Gray);
                Draw.HollowRect(x, y, w, h, Color.Gray);
                Draw.SpriteBatch.End();
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineOut, t => alpha = t.Eased, t => { alpha = 1; enabled = true; });
            }
            public override void Update()
            {
                base.Update();
                if (enabled)
                {
                    if (Input.DashPressed)
                    {
                        enabled = false;
                        Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineIn, t => alpha = t.Eased, t => RemoveSelf());
                    }
                    else
                    {
                        parent.Slider.Value = Calc.Clamp(parent.Slider.Value + Input.MoveY, 0, 100);
                    }
                }
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(target, Vector2.Zero, Color.White * alpha);
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                target?.Dispose();
            }
        }
        public MemoryGravController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Flag = data.FlagList("flag");
            Key = data.Attr("key");
            Add(Talk = new DotX3(Collider, Interact));
        }
        public void Interact(Player player)
        {
            Add(new Coroutine(sequence(player)));
        }
        private IEnumerator sequence(Player player)
        {
            inSequence = true;
            player.DisableMovement();
            MemoryGravUI ui = new(this);
            Scene.Add(ui);
            while (ui.Scene != null)
            {
                yield return null;
            }
            player.EnableMovement();
            inSequence = false;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (inSequence && scene.GetPlayer() is Player player)
            {
                player.EnableMovement();
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Slider = (scene as Level).Session.GetSliderObject("MemoryGrav:" + Key);
            Talk.Enabled = Flag;
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = Flag;
        }
        public override void Render()
        {
            base.Render();
            Draw.HollowRect(Collider, Color.White);
        }
    }
}
