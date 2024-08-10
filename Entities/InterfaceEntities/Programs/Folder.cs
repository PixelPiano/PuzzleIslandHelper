using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{

    [TrackedAs(typeof(WindowContent))]
    [CustomProgram("Folder")]
    public class Folder : WindowContent
    {
        public List<ComputerIcon> Icons = new();
        public Button UpButton;
        public Button DownButton;
        public Vector2 Padding = Vector2.One * 4;
        private float totalHeight;
        private float scrollOffset;
        public string Text;
        public float ScrollSpeed = 30f * Engine.DeltaTime;

        public Folder(Window window) : base(window)
        {
            Name = "Folder";
        }
        public override void OnOpened(Window window)
        {
            base.OnOpened(window);
            //DownButton.Offset.X = UpButton.Offset.X = Buttons[0].Offset.X + Buttons[0].Width + Padding.X;
            DownButton.Position.Y = window.CaseHeight - DownButton.Height - Padding.Y;
            UpButton.Position.Y += 4;
            DownButton.ImageOffset = DownButton.HalfArea;

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            UpButton = new Button(Window, "arrow");
            DownButton = new Button(Window, "arrow");
            DownButton.Outline = UpButton.Outline = true;
            DownButton.CenterOrigin();
            DownButton.Rotation = 180f.ToRad();
            ProgramComponents.Add(UpButton);
            ProgramComponents.Add(DownButton);

        }



        public override void Update()
        {
            scrollOffset += (UpButton.Pressing ? -1 : DownButton.Pressing ? 1 : 0) * ScrollSpeed;
            scrollOffset = Calc.Clamp(scrollOffset, 0, Math.Max(totalHeight - Window.CaseHeight, 0));

     
            base.Update();


        }
        public override void WindowRender()
        {
            base.WindowRender();
        }
    }
}