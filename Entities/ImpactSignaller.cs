using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Monocle;
using System;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [ConstantEntity("PuzzleIslandHelper/ImpactSignaller")]
    [Tracked]
    public class ImpactSignaller : Entity
    {
        public ImpactSignaller() : base()
        {
            Tag |= Tags.Global;
        }
        public void EmitKey(ImpactSender from)
        {
            foreach(ImpactReceiver r in Scene.Tracker.GetEntities<ImpactReceiver>())
            {
                r.AddInput(from.Key);
            }
        }
    }
}