using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Calidus;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [CalidusCutscene("FirstIntro")]
    public class FirstWarpIntro : CalidusCutscene
    {
        public FirstWarpIntro(Player player = null, Calidus calidus = null, Arguments start = null, Arguments end = null) : base(player, calidus, start, end)
        {

        }
        private bool panningOut;
        public override IEnumerator Cutscene(Level level)
        {
            if (Capsule != null)
            {
                yield return CapsuleIntro(Player, level, Capsule, 7.4f, Facings.Right);
            }
            yield return Textbox.Say("wtc1", PanOut, WaitForPanOut);
            yield return Level.ZoomBack(0.8f);
            EndCutscene(Level);
        }
        public IEnumerator PanOut()
        {
            Add(new Coroutine(ActuallyPanOut()));
            yield return null;
        }
        public IEnumerator ActuallyPanOut()
        {
            panningOut = true;
            yield return Level.ZoomBack(4.3f);
            panningOut = false;
            Level.ResetZoom();
        }
        public IEnumerator WaitForPanOut()
        {
            while (panningOut)
            {
                yield return null;
            }
        }
        public override void OnBegin(Level level)
        {
            base.OnBegin(level);
        }
        public override void OnEnd(Level level)
        {
            base.OnEnd(level);
        }
    }
}
