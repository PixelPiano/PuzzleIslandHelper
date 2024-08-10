using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [Tracked]
    public class WindowContent : Entity
    {
        public static Window LastWindow;
        public Window Window;
        public MiniLoader MiniLoader;
        public Interface Interface;
        public List<WindowComponent> ProgramComponents = new();
        public bool Preserve;
        public bool DraggingEnabled = true;
        public bool ClosingEnabled = true;
        public string Name;

        public WindowContent(Window window)
        {
            Window = window;
            Interface = window.Interface;
            Collider = new Hitbox(Window.CaseWidth, Window.CaseHeight);
            Position = Window.DrawPosition.Floor();
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

        }
        public virtual void WindowRender()
        {

        }
        public virtual void OnClosed(Window window)
        {
        }
        
        public virtual void OnOpened(Window window)
        {
            Window = window;
            Interface = window.Interface;
            Depth = Window.Depth - 1;
        }
        public override void Update()
        {
            if (Window != null)
            {
                Collider.Width = Window.CaseWidth;
                Collider.Height = Window.CaseHeight;
                Position = Window.DrawPosition.Floor();
            }
            base.Update();
        }
        public override void Render()
        {
            base.Render();
        }
    }
}