using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TouchSwitchVerifier")]
    [Tracked]
    public class TouchSwitchVerifier : TouchSwitch
    {
        
        public TouchSwitchVerifier(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
        }
    }
}
