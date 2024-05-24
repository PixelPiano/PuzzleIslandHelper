
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using ExtendedVariants.Entities.ForMappers;
using ExtendedVariants.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{

    [TrackedAs(typeof(WindowContent))]
    [CustomProgram("Folder")]
    public class Folder : WindowContent
    {
        public List<ComputerIcon> Icons = new();
        public BetterButton UpButton;
        public BetterButton DownButton;
        public Vector2 Padding = Vector2.One * 4;
        private float totalHeight;
        private float scrollOffset;
        public string Text;
        public float ScrollSpeed = 30f * Engine.DeltaTime;

        public Folder(BetterWindow window) : base(window)
        {
            Name = "Folder";
        }
        public override void OnOpened(BetterWindow window)
        {
            base.OnOpened(window);
            //DownButton.Position.X = UpButton.Position.X = Buttons[0].Position.X + Buttons[0].Width + Padding.X;
            DownButton.Position.Y = window.CaseHeight - DownButton.Height - Padding.Y;
            UpButton.Position.Y += 4;
            DownButton.ImageOffset = DownButton.HalfArea;

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            UpButton = new BetterButton(Window, "arrow");
            DownButton = new BetterButton(Window, "arrow");
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