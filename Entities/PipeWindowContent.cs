using Celeste.Mod.PuzzleIslandHelper.Entities.Windows;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class PipeWindowContent : Entity
    {
        public Entity loader;
        public bool ButtonPressed = false;
        private bool flipped;
        public bool Flipped
        {
            get
            {
                return flipped;
            }
            set
            {
                flipped = value;
            }
        }

        public PipeWindowContent(int Depth)
        {
            this.Depth = Depth - 1;
            Collider = new Hitbox(Window.CaseWidth, Window.CaseHeight);
            Position = Window.DrawPosition.ToInt();

        }
        public void Switch()
        {
            Flipped = !Flipped;
        }
        public override void Update()
        {
            Visible = Window.Drawing && Interface.CurrentIconName == "pipe";
            Position = Window.DrawPosition.ToInt();
            base.Update();
        }
        public override void Render()
        {
            GFX.Game["objects/PuzzleIslandHelper/organ/windowContent"].Draw(Window.DrawPosition.ToInt(), Vector2.Zero, Color.White, 1, 0, Flipped ? SpriteEffects.FlipVertically : SpriteEffects.None);
            base.Render();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

        }

    }
}