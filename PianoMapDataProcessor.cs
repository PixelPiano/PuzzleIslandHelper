using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using PrismaticHelper.Effects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;

namespace Celeste.Mod.PuzzleIslandHelper
{

    public class PianoMapDataProcessor : EverestMapDataProcessor
    {
        private string levelName;
        public override Dictionary<string, Action<BinaryPacker.Element>> Init()
        {

            return new Dictionary<string, Action<BinaryPacker.Element>> {
                {
                    "level", level =>
                    {
                        // be sure to write the level name down.
                        levelName = level.Attr("name").Split(':')[0];
                        if (levelName.StartsWith("lvl_")) {
                            levelName = levelName.Substring(4);
                        }
                    }
                }
            };
        }

        public override void Reset()
        {
            // reset the dictionary for the current map and mode.
        }

        public override void End()
        {
            levelName = null;
        }
    }
}
