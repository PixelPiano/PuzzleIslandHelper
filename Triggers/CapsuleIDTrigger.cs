using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/CapsuleIDTrigger")]
    public class CapsuleIDTrigger : Trigger
    {
        public string ID;
        public List<Vector2> Nodes;
        public FlagList Flags;
        public bool AlwaysActive;
        public CapsuleIDTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Nodes = [.. data.NodesOffset(offset)];
            ID = data.Attr("warpID");
            Flags = new FlagList(data.Attr("flags"));
            AlwaysActive = data.Bool("alwaysActive");
            Tag |= Tags.TransitionUpdate;
        }
        public override void Update()
        {
            base.Update();
            if (AlwaysActive)
            {
                Check();
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Check();
        }
        public void Check()
        {
            if (Flags)
            {
                foreach (Vector2 v in Nodes)
                {
                    foreach (WarpCapsule capsule in Scene.Tracker.GetEntities<WarpCapsule>())
                    {
                        if (capsule.CollidePoint(v))
                        {
                            capsule.TargetID = ID;
                        }
                    }
                }
            }
        }
    }
}