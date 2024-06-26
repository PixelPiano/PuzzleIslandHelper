using Celeste.Mod.Backdrops;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/ParallaxWindow")]
    public class ParallaxWindow : Backdrop
    {
        private readonly string flag;
        private bool State
        {
            get
            {
                if (string.IsNullOrEmpty(flag))
                {
                    return true;
                }
                if (Engine.Scene is not Level level)
                {
                    return false;
                }
                return level.Session.GetFlag(flag);
            }
        }
        private Level level;
        private readonly VirtualRenderTarget Mask = VirtualContent.CreateRenderTarget("ParallaxWindowMask", 320, 180);
        private readonly VirtualRenderTarget Target = VirtualContent.CreateRenderTarget("ParallaxWindowTarget", 320, 180);
        public ParallaxWindow(BinaryPacker.Element data) : this(data.Attr("flag")){ }
        public ParallaxWindow(string flag) : base()
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
            List<Backdrop> list = (scene as Level).Background.GetEach<Backdrop>("ParallaxWindow").ToList();
            foreach (Backdrop b in list)
            {
                b.Render(scene);
            }
        }

        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            level = scene as Level;
            if (State)
            {
                EasyRendering.SetRenderMask(Mask, delegate { DrawTargets(level); }, level);
                EasyRendering.DrawToObject(Target, delegate { GetValidBackdrops(scene); }, level, true, true);
                EasyRendering.MaskToObject(Target, Mask);
            }
        }
        public override void Render(Scene scene)
        {
            base.Render(scene);

                Draw.SpriteBatch.Draw(Target, (scene as Level).LevelOffset, Color.White);
            
        }
    }
}

