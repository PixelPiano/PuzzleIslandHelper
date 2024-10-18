using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [TrackedAs(typeof(Sprite))]
    public class FrameCycler : Sprite
    {
        private MTexture PrevTexture;
        private float prevAlpha, alpha;
        public bool Paused;
        public FrameCycler(Atlas atlas, string path) : base(atlas, path)
        {
        }
        [OnLoad]
        public static void Load()
        {
            On.Monocle.Sprite.SetFrame += Sprite_SetFrame;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Monocle.Sprite.SetFrame -= Sprite_SetFrame;
        }
        public MTexture RandomTexture(string from)
        {
            return Animations[from].Frames.Random();
        }
        public void SetTexture(MTexture texture)
        {
            PrevTexture = Texture;
            Texture = texture;
        }
        private static void Sprite_SetFrame(On.Monocle.Sprite.orig_SetFrame orig, Sprite self, MTexture texture)
        {
            if (self is FrameCycler cycler)
            {
                cycler.PrevTexture = self.Texture;

            }
            orig(self, texture);
        }
        public void Pause()
        {
            Paused = true;
        }
        public void Unpause()
        {
            Paused = false;
        }
        public override void Update()
        {
            if(Paused) return;
            base.Update();
            if (Animating)
            {
                float amount = Math.Abs(animationTimer) / currentAnimation.Delay;
                alpha = amount;
                prevAlpha = 1 - alpha;
            }
        }
        public override void Render()
        {
            if (Texture != null)
            {
                Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, RenderPosition, null, Color * alpha, Rotation, Origin, Scale, Effects, 0);
            }
            if (PrevTexture != null)
            {
                Draw.SpriteBatch.Draw(PrevTexture.Texture.Texture_Safe, RenderPosition, null, Color * prevAlpha, Rotation, Origin, Scale, Effects, 0);
            }

        }
    }
}