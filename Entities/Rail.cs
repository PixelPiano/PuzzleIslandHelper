using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Rail")]
    [Tracked]
    public class Rail : Entity
    {
        public bool UsesBarriers;
        public List<Image> Images = new();
        public float StartX;
        public float EndX;
        public Image LeftBarrier;
        public Image RightBarrier;
        public Rail(Vector2 position, int width, bool usesBarriers = true) : base(position + Vector2.UnitY * 4)
        {
            Collider = new Hitbox(width, 4);
            UsesBarriers = usesBarriers;
            StartX = Position.X;
            EndX = Position.X + Width;
            string path = "objects/PuzzleIslandHelper/minecart/rail";
            MTexture tex = GFX.Game[path];
            Image image = new Image(tex.GetSubtexture(0, 4, 8, 4));
            Add(image);
            Images.Add(image);

            for (float i = 8; i < Width - 8; i += 8)
            {
                Image image2 = new Image(tex.GetSubtexture(8, 4, 8, 4));
                image2.Position.X = i;
                Add(image2);
                Images.Add(image2);
            }
            Image image3 = new Image(tex.GetSubtexture(16, 4, 8, 4));
            image3.Position.X = Width - 8;
            Add(image3);
            Images.Add(image3);

            if (usesBarriers)
            {

                MTexture t = GFX.Game["objects/PuzzleIslandHelper/minecart/barrier"];
                LeftBarrier = new Image(t)
                {
                    Position = new Vector2(0, -(12 - Height))
                };
                RightBarrier = new Image(t)
                {
                    Position = new Vector2(Width - 4, -(12 - Height)),
                    FlipX = true
                };

                Add(LeftBarrier, RightBarrier);
                Images.Add(LeftBarrier);
                Images.Add(RightBarrier);
            }
        }
        public override void Render()
        {
            if (UsesBarriers)
            {
                LeftBarrier.DrawSimpleOutline();
                RightBarrier.DrawSimpleOutline();
            }
            base.Render();
        }

        public Rail(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Bool("hasBarrier"))
        {

        }
    }
}
