using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/SealBook")]
    public class SealBook : Entity
    {
        public SealBook(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
        }
        public void OnEnd()
        {
            Scene?.EnableMovement();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Image image;
            Add(image = new Image(GFX.Game["objects/PuzzleIslandHelper/sealBook"]));
            Vector2 ground = this.GroundedPosition();
            Collider = new Hitbox(image.Width, ground.Y - Y);
            Add(new DotX3(Collider, p => scene.Add(new InteractUI(this)))
            {
                PlayerMustBeFacing = false
            });
        }
        [Tracked]
        public class InteractUI : Entity
        {
            private float lerp;
            private bool inControl;
            private MTexture texture;
            private SealBook book;
            private bool cancelled;
            public InteractUI(SealBook book) : base()
            {
                this.book = book;
                Tag |= TagsExt.SubHUD;
            }
            public override void Render()
            {
                base.Render();
                if (lerp > 0)
                {
                    Draw.Rect(0, 0, Engine.Width, Engine.Height, Color.Black * lerp * 0.7f);
                    texture.DrawCentered(new Vector2(Engine.Width / 2, Engine.Height / 2) + Vector2.UnitY * (Engine.Height * (1 - lerp)), Color.White, 1, (1 - lerp) * MathHelper.PiOver4);
                }
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                texture = GFX.Game["objects/PuzzleIslandHelper/hud/sealBookScribbles"];
                Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t =>
                {
                    lerp = t.Eased;
                }, t => inControl = true);
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                scene.DisableMovement();
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                if (cancelled)
                {
                    book.OnEnd();
                }
            }
            public override void Update()
            {
                base.Update();
                if (inControl)
                {
                    if (Input.MenuCancel)
                    {
                        inControl = false;
                        cancelled = true;
                        Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t => lerp = 1 - t.Eased, t =>
                        {
                            Scene.EnableMovement();
                            RemoveSelf();
                        });
                    }
                }
            }
        }

    }

}