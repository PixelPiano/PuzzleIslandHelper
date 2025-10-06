//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BustedPipe")]
    [Tracked]
    public class BustedPipe : Solid
    {
        public List<Image> Images = new();
        public bool Vertical;
        public bool Hide
        {
            get
            {
                if (UseForCutscene)
                {
                    return PianoModule.Session.GetPipeState() > 1;
                }
                if (Scene is not Level level)
                {
                    return false;
                }
                return string.IsNullOrEmpty(flag) || level.Session.GetFlag(flag);
            }
        }
        private string flag;
        public Image FirstShard;
        public Image SecondShard;
        public bool UseForCutscene;
        public bool Breaks;
        public bool Permenant;
        public EntityID ID;
        private int life = 2;
        public enum BreakDirections
        {
            Right,
            Up,
            Left,
            Down
        }
        private float angle => (float)Math.PI / -2f * (float)breakDir;
        private BreakDirections breakDir;
        public BustedPipe(Vector2 position, int width, int height, bool vertical, string flag, bool useForCutscene, bool breaks, bool permenant, BreakDirections breakDir, EntityID id) : base(position, vertical ? 16 : width, vertical ? height : 16, true)
        {
            ID = id;
            Breaks = breaks;
            Permenant = permenant;
            Tag |= Tags.TransitionUpdate;
            UseForCutscene = useForCutscene;
            this.flag = flag;
            Vertical = vertical;
            string path = "objects/PuzzleIslandHelper/bustedPipe/" + (vertical ? "vertical" : "horizontal");
            MTexture tex = GFX.Game[path];
            for (float i = 0; i < (Vertical ? Height : Width); i += 8)
            {
                Image image = new Image(tex);
                image.Position = new Vector2(Vertical ? 0 : i, Vertical ? i : 0);
                Add(image);
                Images.Add(image);
            }
            FirstShard = new Image(tex.GetSubtexture(16, 0, 16, 8));
            SecondShard = new Image(tex.GetSubtexture(16, 0, 16, 8));
            if (!Vertical) FirstShard.Rotation = 270f.ToRad();
            SecondShard.Rotation = FirstShard.Rotation + 180f.ToRad();

            if (Vertical)
            {
                FirstShard.Position.Y = Height - 4;
                SecondShard.Position.Y = 0;
            }
            else
            {
                FirstShard.Position.X = Width - 4;
                FirstShard.Position.X = 0;
            }
            Add(FirstShard, SecondShard);
            Images.Add(FirstShard);
            Images.Add(SecondShard);
            this.breakDir = breakDir;
            OnDashCollide = (player, dir) =>
            {
                if (!Hide && Breaks)
                {
                    if (breakDir switch
                    {
                        BreakDirections.Left => dir.X > 0 && player.Right < Left,
                        BreakDirections.Right => dir.X < 0 && player.Left > Right,
                        BreakDirections.Up => dir.Y > 0 && player.Bottom < Top,
                        BreakDirections.Down => dir.Y < 0 && player.Top > Bottom,
                        _ => false
                    })
                    {
                        life--;
                        SceneAs<Level>().Shake();
                        if (life <= 0)
                        {
                            if (Permenant)
                            {
                                SceneAs<Level>().Session.DoNotLoad.Add(ID);
                            }
                            RemoveSelf();
                        }
                        return DashCollisionResults.Rebound;
                    }
                    switch (breakDir)
                    {
                        case BreakDirections.Left:
                            break;
                    }
                    life--;
                }
                return DashCollisionResults.NormalCollision;
            };
        }
        public BustedPipe(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data.Width, data.Height, data.Bool("vertical"), data.Attr("flag"), data.Bool("useForCutscene"), data.Bool("breakable"), data.Bool("permenant"), data.Enum<BreakDirections>("breakDirection"), id)
        {

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Collidable = !Hide;
        }
        public override void Render()
        {
            if (Hide) return;
            base.Render();
            MTexture cracks = GFX.Game["objects/PuzzleIslandHelper/bustedPipe/cracks0" + Calc.Clamp(2 - life, 0, 1)];
            float angle = this.angle;
            Vector2 position = default;
            if(breakDir is BreakDirections.Right or BreakDirections.Left)
            {
                position.Y = Height / 2 - cracks.Height / 2;
            }
            else
            {
                position.X = Width / 2 - cracks.Height / 2;
            }

        }
        /*        public void SetVisible()
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        Images[i].Visible = !Hide;
                    }
                    FirstShard.Visible = SecondShard.Visible = Hide;
                    Collidable = !Hide;
                }*/
        public override void Update()
        {
            base.Update();
            Collidable = !Hide;
        }
    }
}
