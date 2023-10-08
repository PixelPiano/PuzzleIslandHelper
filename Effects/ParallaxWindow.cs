using Celeste.Mod.Backdrops;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/ParallaxWindow")]
    public class ParallaxWindow : Backdrop
    {
        private readonly string flag;
        private bool State;
        private Level level;
        private readonly string Tag;
        private readonly VirtualRenderTarget Mask = VirtualContent.CreateRenderTarget("ParallaxWindowMask", 320, 180);
        private readonly VirtualRenderTarget Target = VirtualContent.CreateRenderTarget("ParallaxWindowTarget", 320, 180);

        public ParallaxWindow(string flag)
        {
            this.flag = flag;
        }

        private void DrawTargets(Level level)
        {
            foreach (ParallaxWindowTarget target in level.Tracker.GetEntities<ParallaxWindowTarget>())
            {
                Draw.Rect(target.Collider, Color.White);
            }
        }
        private void GetValidBackdrops(Scene scene)
        {
            if (State)
            {
                List<Backdrop> list = (scene as Level).Background.GetEach<Backdrop>("ParallaxWindow").ToList();
                foreach (Backdrop b in list)
                {
                    b.Render(scene);
                }
            }
        }

        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            level = scene as Level;
            EasyRendering.SetRenderMask(Mask, delegate { DrawTargets(level); }, level);
            EasyRendering.DrawToObject(Target, delegate { GetValidBackdrops(scene); }, level,true,true);
            EasyRendering.MaskToObject(Target, Mask);
        }
        public override void Render(Scene scene)
        {
            base.Render(scene);
            Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White);
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            State = (scene as Level).Session.GetFlag(flag) || string.IsNullOrEmpty(flag);
        }
    }
}

