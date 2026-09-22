using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Singularity
{
    [CustomEntity("PuzzleIslandHelper/SingularityFlagTrigger")]
    [Tracked]
    public class SingularityFlagTrigger : Trigger
    {
        public FlagList Flag;
        public SingularityFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Flag = data.FlagList("flag");
        }
    }
}