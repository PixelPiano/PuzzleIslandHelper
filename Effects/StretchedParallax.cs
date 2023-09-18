using Microsoft.Xna.Framework;
using Monocle;
using System.ComponentModel;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    public class StretchedParallax : Backdrop
    {
        private string flag;
        private Sprite sprite;
        private bool State;
        private bool start;
        private Entity e;
        private Parallax bg;
        public StretchedParallax(string flag, string path)
        {
            this.flag = flag;
            sprite = new Sprite(GFX.Game, "bg/PuzzleIslandHelper/");
            sprite.AddLoop("idle", "labBgBBig", 0.1f);
            e = new Entity
            {
                sprite
            };
            e.Depth = 10000;
            
        }
        public override void Update(Scene scene)
        {
            if (!start)
            {
                start = true;
                Level level = scene as Level;
                sprite.Play("idle");
                level.Add(e);
                e.Position = level.LevelOffset;
                Vector2 Area = new Vector2(level.Bounds.Width, level.Bounds.Height);
                sprite.Scale = new Vector2(Area.X / sprite.Width, Area.Y / sprite.Height);
            }

            base.Update(scene);
            State = (scene as Level).Session.GetFlag(flag) || string.IsNullOrEmpty(flag);
            sprite.Visible = IsVisible(scene as Level) && State;
        }
    }
}

