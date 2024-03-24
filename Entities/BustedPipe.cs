//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/BustedPipe")]
    [Tracked]
    public class BustedPipe : Solid
    {
        public List<Image> Images = new();
        public bool Vertical;
        public bool Broken
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
        public BustedPipe(Vector2 position, int width, int height, bool vertical, string flag, bool useForCutscene) : base(position, vertical ? 16 : width, vertical ? height : 16, true)
        {

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
        }
        public BustedPipe(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Bool("vertical"), data.Attr("flag"), data.Bool("useForCutscene"))
        {

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            SetVisible();
        }
        public void SetVisible()
        {
            for (int i = 0; i < Images.Count; i++)
            {
                Images[i].Visible = !Broken;
            }
            FirstShard.Visible = SecondShard.Visible = Broken;
            Collidable = !Broken;
        }
        public override void Update()
        {
            base.Update();
            SetVisible();
        }
    }
}
