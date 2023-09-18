using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/VoidLightDetect")]
    [TrackedAs(typeof(Trigger))]
    public class VoidLightDetect : Trigger
    {
        public static bool InLight;

        public VoidLightDetect(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public override void OnEnter(Player player)
        {
            InLight = true;
        }

        public override void OnLeave(Player player)
        {
            InLight = false;
        }
      
    }
}
