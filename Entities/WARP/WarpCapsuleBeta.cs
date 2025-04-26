using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    [CustomEntity("PuzzleIslandHelper/WarpCapsuleBeta")]
    [Tracked]
    public class WarpCapsuleBeta : WarpCapsule
    {
        public bool LockPlayerState;
        public WarpCapsuleBeta(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, id, data.Flag("disableFlag", "invertFlag"),
                  "objects/PuzzleIslandHelper/protoWarpCapsule/")
        {
            RoomName = data.Attr("room");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            //Add(new DebugComponent(Keys.H, delegate { PullInteract(Scene.GetPlayer()); }, true));
        }

        public override void Interact(Player player)
        {
            Teleport(player, false, LockPlayerState);
        }
        public void PullInteract(Player player, Action onEnd = null)
        {
            Teleport(player, true, LockPlayerState, onEnd);
        }
        public void Teleport(Player player, bool fast, bool lockPlayer = false, Action onEnd = null)
        {
            if (PianoMapDataProcessor.BetaWarpData.TryGetValue(Scene.GetAreaKey(), out var list))
            {
                if (list.Find(item => item.Room == RoomName) is var data)
                {
                    Scene.Add(new WarpCutscene(this, data, player, fast, !lockPlayer, onEnd));
                }
            }
        }
        public override bool WarpEnabled()
        {
            return !string.IsNullOrEmpty(RoomName);
        }
    }
}
