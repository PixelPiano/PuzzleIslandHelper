using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Ladder")]
    [Tracked]
    public class Ladder : Entity
    {
        [Tracked]
        public class Step : JumpThru
        {
            public Image Image;
            public Ladder Parent;
            public Step(Ladder parent, string path, Vector2 position, int width, bool safe) : base(parent.Position + position, width, safe)
            {
                Parent = parent;
                Image = new Image(GFX.Game[path]);
                Image.Y -= 3;
                Add(Image);
                Visible = false;
            }
            public override void Update()
            {
                base.Update();
                Image.SetColor(Color.White * Parent.Alpha);
            }
            public void DrawOutline()
            {
                Image.DrawSimpleOutline();
            }
        }
        public List<Step> rungs = [];
        private bool collidable = true;
        private bool colliding;
        public float Alpha = 1;
        public string TexturePath;
        public Ladder(EntityData data, Vector2 offset) : this(data.Position + offset, data.Height, data.Attr("texture", "objects/PuzzleIslandHelper/ladder"), data.Bool("visible"), data.Int("depth")) { }
        public Ladder(Vector2 position, int height, string texture, bool visible, int depth) : base(position)
        {
            Collider = new Hitbox(16, height);
            Depth = depth;
            Visible = visible;
            TexturePath = texture;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            bool wasColliding = colliding;
            colliding = player.CollideCheck(this);
            bool flag = CollideFirst<TowerHead>() is TowerHead head && head.PlayerInside;
            if (colliding)
            {
                if (collidable)
                {
                    bool aimValid = Input.MoveY.Value > 0.5f && Math.Abs(Input.MoveX.Value) < 0.5f;
                    if (rungs.Find(item => player.IsRiding(item) && aimValid) != null)
                    {
                        collidable = false;
                    }
                }
                else if (Input.MoveY.Value <= -1 || Input.Jump.Pressed || player.CollideCheck<Platform, Step>(player.Position + Vector2.UnitY))
                {
                    collidable = true;
                }
            }
            else if (wasColliding)
            {
                collidable = true;
            }
            foreach (var item in rungs)
            {
                item.Collidable = !flag && collidable && Collidable;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            for (int i = 0; i < Height; i += 8)
            {
                Step rung = new Step(this, TexturePath, Vector2.UnitY * (i + 3), 16, true);
                rungs.Add(rung);
                scene.Add(rung);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (var p in rungs)
            {
                p.RemoveSelf();
            }
        }
        public override void Render()
        {
            base.Render();
            foreach (var p in rungs)
            {
                p.Render();
            }
        }
    }
}