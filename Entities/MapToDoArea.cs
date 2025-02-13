using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MapToDoArea")]
    public class MapToDoDummy : Entity
    {
        public class TextRenderEntity : Entity
        {
            public string Text;
            public Entity Parent;
            public FancyText.Text Fancy;
            public TextRenderEntity(string text, Entity parent) : base()
            {
                Text = text;
                Parent = parent;
                Tag |= TagsExt.SubHUD;
                Fancy = FancyText.Parse(text, (int)parent.Width * 6, 20);
            }
            public override void Render()
            {
                base.Render();
                Vector2 pos = SceneAs<Level>().WorldToScreen(Parent.Position) * 6f;
                Fancy.Draw(pos, new Vector2(0.5f), Vector2.One, 1);
            }
        }
        public string Message;
        public TextRenderEntity TextRender;
        public bool DisplayInGame;
        public Color Border, Fill;
        public MapToDoDummy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Message = data.Attr("note");
            Collider = new Hitbox(data.Width, data.Height);
            DisplayInGame = data.Bool("displayInGame");
            float val = Color.White.R;
            Border = new Color(val * 0.3f, val * 0.3f, val * 0.3f, 0.6f);
            Fill = Color.White * 0.8f;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (DisplayInGame)
            {
                TextRender = new TextRenderEntity(Message, this);
                scene.Add(TextRender);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            TextRender?.RemoveSelf();
        }
        public override void Render()
        {
            base.Render();
            if (DisplayInGame)
            {
                Draw.HollowRect(Collider, Border);
                Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, Fill);
            }
        }
    }
}