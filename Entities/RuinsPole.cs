using Celeste.Mod.Entities;

using FrostHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/RuinsPole")]
    [Tracked]
    public class RuinsPole : Entity
    {
        private List<Image> images = new();
        private const string path = "objects/PuzzleIslandHelper/ruinsPole/";
        private const int maxBroken = 2;
        private bool forElevator;
        public RuinsPole(Vector2 position, int height, int topNum, int bottomNum, bool forElevator, bool crystalized, bool brokenTop, bool brokenBottom) : base(position)
        {
            Depth = 9001;

            MTexture tex = GFX.Game[path + (crystalized ? "crystalTexture" : "texture")];
            int y = forElevator ? 8 : 0;
            int width = forElevator ? 13 : 5;
            if (!forElevator)
            {
                Image top = new Image(tex.GetSubtexture(brokenTop ? Calc.Clamp(topNum * 5, 0, maxBroken * 5) : 0, 0, 5, 8));
                Add(top);
                images.Add(top);
            }

            if (crystalized && !forElevator)
            {
                for (int i = 8; i < height - 8; i += 8)
                {
                    Image mid = new Image(tex.GetSubtexture(0, y, width, 8));
                    mid.Position.Y = i;
                    Add(mid);
                    images.Add(mid);
                }
            }
            else
            {
                Image mid = new Image(tex.GetSubtexture(0, y, width, 8));
                mid.Scale.Y = Calc.Max(height - (forElevator ? 0 : 16), 8) / 8;
                mid.Position.Y = forElevator ? 0 : 8;
                Add(mid);
                images.Add(mid);
            }
            if (!forElevator)
            {
                Image bottom = new Image(tex.GetSubtexture(brokenBottom ? Calc.Clamp(bottomNum * 5, 0, maxBroken * 5) : 0, 0, 5, 8));
                bottom.Position.Y = height;
                bottom.Scale.Y = -1;
                Add(bottom);
                images.Add(bottom);
            }

            this.forElevator = forElevator;
        }
        public RuinsPole(EntityData data, Vector2 offset) : this(data.Position + offset, data.Height, data.Int("topNum"), data.Int("bottomNum"), data.Bool("forElevator"), data.Bool("crystalized"), data.Bool("brokenTop"),data.Bool("brokenBottom")) { }
    }
}