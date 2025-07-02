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
    [CalidusCutscene("SecondIntro")]
    public class SecondWarpIntro : CalidusCutscene
    {
        public SecondWarpIntro(Player player = null, Calidus calidus = null, Arguments start = null, Arguments end = null) : base(player, calidus, start, end)
        {

        }
        public override IEnumerator Cutscene(Level level)
        {
            level.InCutscene = true;
            yield return CapsuleIntro(Player, level, Capsule, 2, Facings.Right);
            yield return 1.3f;
            Player.ForceCameraUpdate = false;
            yield return Player.DummyWalkTo(Player.X + 20);
            yield return 1f;
            yield return Textbox.Say("Cb0", PlayerLookLeft, PlayerLookRight);
            yield return Level.ZoomBack(1.5f);
            EndCutscene(Level);
        }
    }
}
