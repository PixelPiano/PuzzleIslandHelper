using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.IO;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [CustomEntity("PuzzleIslandHelper/TransitLabComputer")]
    [Tracked]
    public class TransitLabComputer : Entity
    {
        public Sprite Sprite;
        public DotX3 Talk;
        public TransitLabComputer(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 2;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/keyboard");
            Sprite.AddLoop("idle", "", 0.1f);
            Add(Sprite);
            Sprite.Play("idle");
            Add(Talk = new DotX3(0, 0, Sprite.Width, Sprite.Height, new Vector2(Sprite.Width / 2, 0), Interact));
            Talk.PlayerMustBeFacing = false;
        }
        public void Interact(Player player)
        {
            if (Scene.Tracker.GetEntity<TransitMonitor>() is var monitor)
            {
                player.DisableMovement();
                monitor.Activate();
            }
        }
    }

}