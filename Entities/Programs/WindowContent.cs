using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    [Tracked]
    public class WindowContent : Entity
    {
        public Interface Interface => Window.Interface;
        public BetterWindow Window;
        public MiniLoader MiniLoader;
        public string Name;
        public bool IsActive => BetterWindow.Drawing && Interface.CurrentIconName == Name;

        public WindowContent(BetterWindow window)
        {
            Depth = window.Depth - 1;
            Collider = new Hitbox(BetterWindow.CaseWidth, BetterWindow.CaseHeight);
            Position = BetterWindow.DrawPosition.ToInt();
            Window = window;
        }
        public IEnumerator PlayAndWait(SoundSource source, string audio)
        {
            source.Play(audio);
            while (source.InstancePlaying)
            {
                yield return null;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Interface == null)
            {
                RemoveSelf();
            }
        }
        public override void Update()
        {
            Collider.Width = BetterWindow.CaseWidth;
            Collider.Height = BetterWindow.CaseHeight;
            Visible = BetterWindow.Drawing && Interface.CurrentIconName == Name;
            Position = BetterWindow.DrawPosition.ToInt(); 
            base.Update();
        }
        public override void Render()
        {
            if (!BetterWindow.Drawing)
            {
                base.Render();
                return;
            }
            base.Render();
        }
    }
}