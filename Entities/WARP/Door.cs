using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    public class Door : Entity
    {
        private class Lock : Entity
        {
            public Image Image;
            public Door Door;
            public Lock(Door door) : base(door.Position)
            {
                Door = door;
                Depth = 7;
                Image = new Image(GFX.Game[WARPData.DefaultPath + "lock"]);
                Image.JustifyOrigin(0.5f, 1);
                Image.Scale.X = door.xScale;
                Collider = new Hitbox(Image.Width, Image.Height, -Image.Width / 2, -Image.Height / 2);
                Image.Position.Y += Height / 2;
                Add(Image);
            }
            public override void Render()
            {
                Position = Door.Position;
                base.Render();
            }
        }
        public Image Image;
        private Lock LockPlate;
        public Vector2 Scale = Vector2.One;
        public float xScale;
        public Vector2 Orig;
        private float xOffset;
        private string path;
        public Door(Vector2 position, int xScale, float xOffset, string path, bool hasLock = true) : base(position)
        {
            this.path = path;
            Depth = 8;
            Orig = position;
            this.xScale = xScale;
            this.xOffset = xOffset * xScale;
            Image = new Image(GFX.Game[path + "doorFill00"]);
            Image.JustifyOrigin(0.5f, 1);
            Image.Scale.X = xScale;
            Collider = new Hitbox(Image.Width, Image.Height, -Image.Width / 2, -Image.Height / 2);
            Image.Position.Y += Height / 2;
            Add(Image);
            if (hasLock) LockPlate = new Lock(this);
        }
        public override void Update()
        {
            base.Update();
            if(LockPlate != null)
            {
                LockPlate.Visible = Visible;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (LockPlate != null) scene.Add(LockPlate);

        }
        public override void Render()
        {
            ChangeTexture(Scale.Y >= 1.4f);
            Image.Scale = new Vector2(Scale.X * xScale, Scale.Y);
            if (LockPlate != null) LockPlate.Image.Scale = Image.Scale;
            base.Render();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            LockPlate?.RemoveSelf();
        }
        public void ChangeTexture(bool extend)
        {
            Image.Texture = GFX.Game[path + "doorFill0" + (extend ? 1 : 0)];
        }
        public void SetTo(float percent)
        {
            Position.X = (int)Math.Round(Orig.X + xOffset * (1 - percent));
        }
        public void MoveToFg()
        {
            Depth = -2;
            if (LockPlate != null) LockPlate.Depth = -3;
        }
        public void MoveToBg()
        {
            Depth = 8;
            if (LockPlate != null) LockPlate.Depth = 7;
        }
    }
}
