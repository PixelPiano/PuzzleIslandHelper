using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomFancyDashBlock")]
    [Tracked]
    public class CustomFancyDashBlock : FancyDashBlock
    {
        public string Flag;
        public bool Value;
        public CustomFancyDashBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            Flag = data.Attr("flag");
            Value = data.Bool("setFlagTo");
        }

        [OnLoad]
        public static void Load()
        {
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool += DashBlock_Break_Vector2_Vector2_bool_bool;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool -= DashBlock_Break_Vector2_Vector2_bool_bool;
        }

        private static void DashBlock_Break_Vector2_Vector2_bool_bool(On.Celeste.DashBlock.orig_Break_Vector2_Vector2_bool_bool orig, DashBlock self, Vector2 from, Vector2 direction, bool playSound, bool playDebrisSound)
        {
            if(self is CustomFancyDashBlock block)
            {
                block.Flag.SetFlag(block.Value);
            }
            orig(self, from, direction, playSound, playDebrisSound);
        }
    }
}