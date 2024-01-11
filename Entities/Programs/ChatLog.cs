using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    public class ChatLog : WindowContent
    {
        public string DialogID;
        public FancyTextExt.Text Text;
        private ChatText Helper;
        public ChatLog(BetterWindow window) : base(window)
        {
            Name = "chatlog";
            Text = FancyTextExt.Parse(Dialog.Get(DialogID),(int)Width, 30, Vector2.Zero);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Helper = new ChatText(this));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(Helper);
        }
        public override void Update()
        {
            base.Update();
         
        }
        public override void Render()
        {
            base.Render();
            
        }
        public class ChatText : Entity
        {
            private ChatLog Track;
            public ChatText(ChatLog track) : base(Vector2.Zero)
            {
                Tag |= TagsExt.SubHUD;
                Track = track;
            }
            public override void Render()
            {
                base.Render();
                Track.Text.Draw(Track.Position, Vector2.Zero, Vector2.One, 1, Color.White);
            }
        }

    }
}