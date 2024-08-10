using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LameFallingBlock")]
    [TrackedAs(typeof(FallingBlock))]
    public class LameFallingBlock : FallingBlock
    {
        public string Flag;
        private bool inverted;
        private bool triggered => Triggered;
        public bool FlagState => (string.IsNullOrEmpty(Flag) || SceneAs<Level>().Session.GetFlag(Flag)) != inverted;
        public LameFallingBlock(Vector2 position, string flag, bool inverted, char tiletype, float width, float height, bool finalboss, bool behind, bool climbfall) : base(position, tiletype, (int)width, (int)height, finalboss, behind, climbfall)
        {
            this.Flag = flag;
            this.inverted = inverted;
        }
        public LameFallingBlock(EntityData data, Vector2 offset) :
            this(data.Position + offset, data.Attr("flag"), data.Bool("invertFlag"), data.Char("tiletype"), data.Width, data.Height, data.Bool("finalBoss"), data.Bool("behind"), false)
        {
        }
        public static void Load()
        {
            On.Celeste.FallingBlock.PlayerFallCheck += FallingBlock_PlayerFallCheck;
        }
        public static void Unload()
        {
            On.Celeste.FallingBlock.PlayerFallCheck -= FallingBlock_PlayerFallCheck;
        }
        public virtual void OnFall()
        {

        }
        private static bool FallingBlock_PlayerFallCheck(On.Celeste.FallingBlock.orig_PlayerFallCheck orig, FallingBlock self)
        {
            if (self is LameFallingBlock block)
            {
                if (!block.FlagState) return false;
                block.OnFall();
            }
            return orig(self);
        }
    }
}