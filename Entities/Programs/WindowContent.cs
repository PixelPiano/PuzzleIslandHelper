using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    [Tracked]
    public class WindowContent : Entity
    {
        public static BetterWindow LastWindow;
        public Interface Interface => Window.Interface;
        public BetterWindow Window;
        public MiniLoader MiniLoader;
        public bool Preserve;
        public string Name;
        public bool IsActive => Window != null && Window.Drawing && Interface.CurrentIconName.ToLower() == Name.ToLower();

        public WindowContent(BetterWindow window)
        {
            Window = window;
            Depth = Window.Depth - 1;
            Collider = new Hitbox(Window.CaseWidth, Window.CaseHeight);
            Position = Window.DrawPosition.ToInt();
        }
        public IEnumerator PlayAndWait(SoundSource source, string audio)
        {
            source.Play(audio);

            while (source.InstancePlaying)
            {
                yield return null;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            LastWindow = Window;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Interface == null)
            {
                RemoveSelf();
            }
        }
        public virtual void OnAdded()
        {

        }
        public virtual void OnRemoved()
        {

        }
        public override void Update()
        {
            Collider.Width = Window.CaseWidth;
            Collider.Height = Window.CaseHeight;
            Visible = IsActive;
            Position = Window.DrawPosition.ToInt();
            base.Update();
        }
        public override void Render()
        {
            base.Render();
        }
    }
}