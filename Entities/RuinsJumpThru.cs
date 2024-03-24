using Celeste.Mod.Entities;

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/RuinsJumpThru")]
    [Tracked]
    public class RuinsJumpThru : JumpThru
    {
        private List<Image> images = new();
        private const string path = "objects/PuzzleIslandHelper/ruinsJumpThru/";
        public RuinsJumpThru(Vector2 position, int width, bool collidable) : base(position, width, collidable)
        {
            Collidable = collidable;
            Depth = -60;
            SurfaceSoundIndex = 13;
            MTexture tex = GFX.Game[path + "texture"];
            Image left = new Image(tex.GetSubtexture(0, 0, 8, 4));
            Add(left);
            images.Add(left);
            for (int i = 8; i < width; i += 8)
            {
                Image mid = new Image(tex.GetSubtexture(8, 0, 8, 4));
                mid.Position.X = i;
                Add(mid);
                images.Add(mid);
            }
            Image end = new Image(tex.GetSubtexture(16, 0, 2, 4));
            end.Position.X = Width;
            Add(end);
            images.Add(end);

        }
        public RuinsJumpThru(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Bool("collidable")) { }
    }
}