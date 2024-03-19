using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/GearCheckpoint")]
    [Tracked]
    public class GearCheckpoint : Trigger
    {
        public string ContinuityID;
        public string SubID;
        public GearCheckpoint(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            ContinuityID = data.Attr("continuityId");
            SubID = data.Attr("subId");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (string.IsNullOrEmpty(ContinuityID) || string.IsNullOrEmpty(SubID))
            {
                RemoveSelf();
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            CheckForGear();
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            CheckForGear();
        }
        private void CheckForGear()
        {
            if (CollideFirst<Gear>() is Gear gear)
            {
                if (gear.ContinuityID == ContinuityID)
                {
                    PianoModule.Session.GearCheckpointIDs.CheckAdd(SubID);
                }
            }
        }
    }
}
