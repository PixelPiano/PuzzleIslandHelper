
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/DisableCalidusBlip")]
    [Tracked]
    public class DisableCalidusBlip : Trigger
    {
        public DisableCalidusBlip(EntityData data, Vector2 offset) : base(data, offset)
        {

        }
    }
}
