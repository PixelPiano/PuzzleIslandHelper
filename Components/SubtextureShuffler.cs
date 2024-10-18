using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [TrackedAs(typeof(Image))]
    public class SubtextureShuffler : Image
    {
        public MTexture MainTexture;
        private MTexture PrevTexture;
        private float prevAlpha, alpha;
        public bool Paused;
        private float width;
        private float height;
        private float interval;
        private bool fade;
        public SubtextureShuffler(MTexture texture, float width, float height, float interval = 0.1f, bool fade = false) : base(null, true)
        {
            MainTexture = texture;
            this.width = Calc.Min(texture.Width, width);
            this.height = Calc.Min(texture.Height, height);
            this.interval = Calc.Max(Engine.DeltaTime, Math.Abs(interval));
            this.fade = fade;
            AdvanceTexture();
        }
        public MTexture RandomSubtexture()
        {
            return MainTexture.GetSubtexture((int)Calc.Random.Range(0, MainTexture.Width - width), (int)Calc.Random.Range(0, MainTexture.Height - height), (int)width, (int)height);
        }
        public void AdvanceTexture()
        {
            PrevTexture = Texture;
            Texture = RandomSubtexture();
        }
        public void Pause()
        {
            Paused = true;
        }
        public void Unpause()
        {
            Paused = false;
        }
        private float timer;
        public override void Update()
        {
            if (Paused) return;
            base.Update();
            alpha = timer / interval;
            prevAlpha = 1 - alpha;
            timer += Engine.DeltaTime;
            if (timer > interval)
            {
                AdvanceTexture();
                timer %= interval;
            }
        }
        public override void Render()
        {
            if (fade)
            {
                if (Texture != null)
                {
                    Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, RenderPosition, Texture.ClipRect, Color * alpha, Rotation, Origin, Scale, Effects, 0);
                }
                if (PrevTexture != null)
                {
                    Draw.SpriteBatch.Draw(PrevTexture.Texture.Texture_Safe, RenderPosition, PrevTexture.ClipRect, Color * prevAlpha, Rotation, Origin, Scale, Effects, 0);
                }
            }
            else if (Texture != null)
            {
                Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, RenderPosition, Texture.ClipRect, Color, Rotation, Origin, Scale, Effects, 0);
            }

        }
    }
}