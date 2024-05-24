using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [Tracked]
    public class WindowContent : Entity
    {
        public static BetterWindow LastWindow;
        public BetterWindow Window;
        public MiniLoader MiniLoader;
        public Interface Interface;
        public List<WindowComponent> ProgramComponents = new();
        public bool Preserve;

        public string Name;

        public WindowContent(BetterWindow window)
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
        public virtual void OnClosed(BetterWindow window)
        {
        }
        
        public virtual void OnOpened(BetterWindow window)
        {
            Window = window;
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