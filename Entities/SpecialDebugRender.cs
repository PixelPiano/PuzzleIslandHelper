using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class SpecialDebugRender
    {
        [OnLoad]
        public static void Load()
        {
            //On.Celeste.Level.Render += Level_Render;
        }

        private static void Level_Render(On.Celeste.Level.orig_Render orig, Level self)
        {
            throw new System.NotImplementedException();
        }
    }

}
