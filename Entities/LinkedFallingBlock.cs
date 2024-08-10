using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LinkedFallingBlock")]
    [TrackedAs(typeof(LameFallingBlock))]
    public class LinkedFallingBlock : LameFallingBlock
    {
        public string linkId;
        public LinkedFallingBlock(EntityData data, Vector2 offset) : base(data, offset)
        {
            linkId = data.Attr("linkId");
        }
        public override void OnFall()
        {
            base.OnFall();
            if(Scene is not Level level || string.IsNullOrEmpty(linkId)) return;
            foreach(LinkedFallingBlock block in level.Tracker.GetEntities<LinkedFallingBlock>())
            {
                if(block != this && block.linkId == linkId)
                {
                    block.Triggered = true;
                }
            }
        }
    }
}