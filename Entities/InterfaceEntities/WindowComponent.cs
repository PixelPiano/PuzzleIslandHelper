using Celeste.Mod.CommunalHelper;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using FrostHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class WindowComponent : Component
    {
        public BetterWindow Window;
        public Interface Interface => Window.Interface;
        public Vector2 RenderPosition
        {
            get
            {
                return ((Window == null) ? Vector2.Zero : Window.DrawPosition) + Position;
            }
            set
            {
                Position = value - ((Window == null) ? Vector2.Zero : Window.DrawPosition);
            }
        }
        public Vector2 Position;
        public WindowComponent(BetterWindow window) : base(true, true)
        {
            Window = window;
        }
        public WindowComponent(BetterWindow window, bool active) : base(active, true)
        {
            Window = window;
        }

        public virtual void OnOpened(Scene scene)
        {
        }
        public virtual void OnClosed(Scene scene)
        {
        }
        public override void Update()
        {
            base.Update();
        }
    }
}