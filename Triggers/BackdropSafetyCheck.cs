using Celeste.Mod.Backdrops;
using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomBackdrop("PuzzleIslandHelper/BackdropSafetyCheck")]
    public class BackdropSafetyCheck : Backdrop
    {
        public string Area;


        public BackdropSafetyCheck(BinaryPacker.Element data) : this(data.Attr("area")){ }
        public BackdropSafetyCheck(string area) : base()
        {
            Area = area;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Level level = scene as Level;
            if (IsVisible(level))
                AreaFlagHelper.SetFlags(level, "In" + Area);
        }
    }
}
