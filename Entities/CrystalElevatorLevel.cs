using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/CrystalElevatorLevel")]
    [Tracked]
    public class CrystalElevatorLevel : Entity
    {
        public List<string> Flags = new();
        public bool Passed;
        public bool OnLevel;
        public int FloorNum;
        public Vector2[] nodes;
        public List<GearHolder> Holders = new();
        public bool Free
        {
            get
            {
                return Holders is null || Holders.Count <= 0;
            }
        }
        public CrystalElevatorLevel(EntityData data, Vector2 offset)
    : base(data.Position + offset)
        {
            FloorNum = data.Int("levelId");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            foreach (GearHolder holder in (scene as Level).Tracker.GetEntities<GearHolder>())
            {
                if (holder.ID == FloorNum)
                {
                    Holders.Add(holder);
                }
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (FloorNum < 0) RemoveSelf();
        }
        public bool Fixed()
        {
            if (Free) return true;
            foreach (GearHolder holder in Holders)
            {
                if (!holder.Fixed)
                {
                    return false;
                }
            }
            return true;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(Holders);
        }
    }
}
