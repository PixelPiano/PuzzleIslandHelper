using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ImageOverlay")]
    [Tracked]
    public class ImageOverlay : Entity
    {
        private class overlay : Entity
        {
            private VirtualRenderTarget target;
            private float alpha;
            private Tween tween;
            private bool ignoreInputs = true;
            public bool Finished;
            public overlay(MTexture texture) : base()
            {
                target = VirtualContent.CreateRenderTarget("ImageOverlay", 320, 180);
                tween = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t => alpha = t.Eased, t => ignoreInputs = false);
                Add(new BeforeRenderHook(() =>
                {
                    target.SetAsTarget(Color.Black);
                    Draw.SpriteBatch.Begin();
                    Vector2 p = new Vector2(160, 90) - texture.HalfSize();
                    texture.Draw(p);
                    Draw.SpriteBatch.End();
                }));
            }
            public override void Update()
            {
                base.Update();
                if (!ignoreInputs && Input.MenuCancel.Pressed)
                {
                    ignoreInputs = true;
                    tween?.RemoveSelf();
                    tween = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t => alpha = 1 - t.Eased, t => Finished = true);
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                target?.Dispose();
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(target, SceneAs<Level>().Camera.Position, Color.White * alpha);
            }
        }
        private overlay imageOverlay;
        public DotX3 talk;
        private Image worldImage;
        private FlagList enabled;
        public ImageOverlay(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(worldImage = new Image(GFX.Game[data.Attr("decalPath")]));
            Collider = worldImage.Collider();
            MTexture overlayTexture = GFX.Game[data.Attr("overlayPath")];
            enabled = data.FlagList();
            Add(talk = new DotX3(Collider, p =>
            {
                imageOverlay?.RemoveSelf();
                p.DisableMovement();
                p.ForceCameraUpdate = true;
                Scene.Add(imageOverlay = new(overlayTexture));
            }));
        }
        public override void Update()
        {
            base.Update();
            if (imageOverlay != null && imageOverlay.Finished)
            {
                if (Scene.GetPlayer() is Player player)
                {
                    player.EnableMovement();
                }
                imageOverlay.RemoveSelf();
                imageOverlay = null;
            }
            talk.Enabled = imageOverlay == null && enabled;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if(imageOverlay != null)
            {
                scene.GetPlayer()?.EnableMovement();
                imageOverlay.RemoveSelf();
                imageOverlay = null;
            }
        }
    }
}
