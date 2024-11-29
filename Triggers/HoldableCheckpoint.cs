using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/HoldableCheckpoint")]
    [Tracked]
    public class HoldableCheckpoint : Trigger
    {
        public string GroupID;
        public string SubID;
        public HoldableCheckpoint(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            GroupID = data.Attr("groupId");
            SubID = data.Attr("subId");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (string.IsNullOrEmpty(GroupID) || string.IsNullOrEmpty(SubID))
            {
                RemoveSelf();
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            CheckForHoldable();
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            CheckForHoldable();
        }
        private void CheckForHoldable()
        {
            if (CollideFirst<HoldableEntity>() is HoldableEntity entity)
            {
                if (entity.GroupID == GroupID)
                {
                    PianoModule.Session.HoldableCheckpointIDs.TryAdd(SubID);
                }
            }
        }
    }
}
