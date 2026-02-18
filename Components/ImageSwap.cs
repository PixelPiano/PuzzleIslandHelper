using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    public class ImageSwap : Image
    {
        public bool State
        {
            get => state;
            set
            {
                state = value;
                if (value)
                {
                    Texture = OnTexture;
                    offset = OnOffset;
                }
                else
                {
                    Texture = OffTexture;
                    offset = OffOffset;
                }
            }
        }
        private bool state;
        public MTexture OnTexture, OffTexture;
        public Vector2 OnOffset, OffOffset;
        private Vector2 offset;
        public ImageSwap(Atlas atlas, string path, bool state = true) : this(atlas[path + "On"], atlas[path + "Off"], state)
        {

        }
        public ImageSwap(MTexture onTexture, MTexture offTexture, bool state = true) : base(null, false)
        {
            OnTexture = onTexture;
            OffTexture = offTexture;
            State = state;
        }
        public override void Render()
        {
            if (Texture != null)
            {
                Texture.Draw(base.RenderPosition + offset, Origin, Color, Scale, Rotation, Effects);
            }
        }
    }
}
