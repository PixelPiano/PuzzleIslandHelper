using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [TrackedAs(typeof(WindowContent))]
    [CustomProgram("Camera")]
    public class CameraProgram : WindowContent
    {
        public List<CameraView> Cameras = new();
        public class CameraView : WindowComponent
        {
            public SecurityCamData Data;
            public VirtualRenderTarget Target;
            private bool drawing;
            public CameraView(Window window, SecurityCamData data, Scene scene) : base(window)
            {
                Data = data;
            }

            public override void OnOpened(Scene scene)
            {
                base.OnOpened(scene);

            }
            public override void OnClosed(Scene scene)
            {
                base.OnClosed(scene);

            }
            public override void Render()
            {
                base.Render();

            }
            public override void Removed(Entity entity)
            {
                base.Removed(entity);
                Target?.Dispose();
            }

        }
        public CameraProgram(Window window) : base(window)
        {
            Name = "Cameras";
        }
        public override void Update()
        {
            base.Update();
        }
        public override void OnOpened(Window window)
        {
            base.OnOpened(window);
            Initialize(window);
        }
        public void Initialize(Window window)
        {
            Cameras.Clear();
            float limitx = window.CaseWidth;
            float limity = window.CaseHeight;
            float width = limitx / 4;
            float height = limity / 4;

            float x = limitx / 3 + limitx / 6 - width / 2;
            float y = limity / 3 + limity / 6 - height / 2;
            foreach (SecurityCamData data in PianoMapDataProcessor.SecurityCams)
            {
                CameraView view = new CameraView(window, data, Scene);
                view.Position.X = x;
                view.Position.Y = y;
                ProgramComponents.Add(view);
                Cameras.Add(view);
                x += limitx / 3;
                if (x >= limitx)
                {
                    y += limity / 3;
                    x = limitx / 3 + limitx / 6 - width / 2;
                }
            }
        }

    }
}