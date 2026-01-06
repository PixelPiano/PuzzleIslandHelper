using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [TrackedAs(typeof(WindowContent))]
    [CustomProgram("ChatLog","chatlog","chat log","Chat log","Chat Log")]
    public class ChatLogProgram : WindowContent
    {
        public string DialogID;
        public ExtraFancyText.Text Text;
        private ChatText Helper;
        public ChatLogProgram(Window window) : base(window)
        {
            Name = "ChatLog";
            Text = ExtraFancyText.Parse(Dialog.Get(DialogID), (int)Width, 30, Vector2.Zero);
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
            private ChatLogProgram Track;
            public ChatText(ChatLogProgram track) : base(Vector2.Zero)
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