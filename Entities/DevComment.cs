using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Celeste.Overworld;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DevComment")]
    [Tracked]
    public class DevComment : Entity
    {
        private class textHelper : Entity
        {
            private DevComment comment;
            private FancyText.Text text;
            public textHelper(DevComment comment) : base()
            {
                this.comment = comment;
                Tag |= TagsExt.SubHUD;
                text = FancyText.Parse(comment.Text, (int)(comment.Width * 6), 100);
            }
            public override void Render()
            {
                base.Render();
                
                text.DrawJustifyPerLine(SceneAs<Level>().WorldToScreen(comment.Center), Vector2.One * 0.5f, Vector2.One, 1);
            }
        }
        public string Text;
        private textHelper helper;
        public DevComment(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Text = data.Attr("text");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(helper = new textHelper(this));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            helper?.RemoveSelf();
        }
    }
}