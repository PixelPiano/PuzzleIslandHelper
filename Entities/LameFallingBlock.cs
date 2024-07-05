using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LameFallingBlock")]
    [TrackedAs(typeof(FallingBlock))]
    public class LameFallingBlock : FallingBlock
    {
        private string flag;
        private bool inverted;
        private bool triggered => Triggered;
        public bool FlagState => (string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag)) != inverted;
        public LameFallingBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Char("tiletype"), data.Width, data.Height, data.Bool("finalBoss"), data.Bool("behind"), false)
        {
            flag = data.Attr("flag");
            inverted = data.Bool("invertFlag");
        }
        public static void Load()
        {
            On.Celeste.FallingBlock.PlayerFallCheck += FallingBlock_PlayerFallCheck;
        }
        public static void Unload()
        {
            On.Celeste.FallingBlock.PlayerFallCheck -= FallingBlock_PlayerFallCheck;
        }

        private static bool FallingBlock_PlayerFallCheck(On.Celeste.FallingBlock.orig_PlayerFallCheck orig, FallingBlock self)
        {
            if (self is LameFallingBlock block && !block.FlagState) return false;
            return orig(self);
        }
    }
}