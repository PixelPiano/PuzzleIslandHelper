using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/GearCheckpoint")]
    [Tracked]
    public class GearCheckpoint : Trigger
    {
        public string ContinuityID;
        public string SubID;
        public bool RequiresGear = true;
        public bool RequiresGearHeld = true;
        public bool IgnoreContinuity = false;
        public GearCheckpoint(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            ContinuityID = data.Attr("continuityId");
            SubID = data.Attr("subId");
            RequiresGear = data.Bool("requiresGear", true);
            RequiresGearHeld = data.Bool("mustBeHeld", true);
            IgnoreContinuity = data.Bool("ignoreContinuity", false);
        }
        public override void Update()
        {
            base.Update();
            if (RequiresGear && !RequiresGearHeld)
            {
                foreach (Gear gear in Scene.Tracker.GetEntities<Gear>())
                {
                    if (CollideCheck(gear)) UpdateGear(gear);
                }
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if ((!IgnoreContinuity && string.IsNullOrEmpty(ContinuityID)) || string.IsNullOrEmpty(SubID))
            {
                RemoveSelf();
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            UpdateRegistry(player);
        }
        public void UpdateGear(Gear gear)
        {
            if (IgnoreContinuity || gear.ContinuityID == ContinuityID)
            {
                RegisterSubID();
            }
        }
        public void UpdateRegistry(Player player)
        {
            if (RequiresGear)
            {
                if (RequiresGearHeld)
                {
                    if (PlayerIsHoldingGear(player, out Gear gear))
                    {
                        UpdateGear(gear);
                    }
                }
            }
            else
            {
                RegisterSubID();
            }
        }
        public bool PlayerIsHoldingGear(Player player, out Gear held)
        {
            held = null;
            if (player.Holding is Holdable h && h.Entity is Gear gear)
            {
                held = gear;
                return true;
            }
            return false;
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            UpdateRegistry(player);
        }
        public void RegisterSubID()
        {
            PianoModule.Session.GearCheckpointIDs.TryAdd(SubID);
        }
    }
}
