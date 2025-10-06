using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CompassController")]
    [Tracked]
    public class CompassController : Entity
    {
        public FlagList Flag;
        public TalkComponent Talk;
        public CompassController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Collider = new Hitbox(16, 16);
            Rectangle r = new Rectangle(0, 0, 16, 16);
            Flag = data.FlagList();
            Add(Talk = new TalkComponent(r, Vector2.UnitX * 8, player =>
            {
                string key = data.Attr("key");
                if (!string.IsNullOrEmpty(key))
                {
                    foreach (Compass c in Scene.Tracker.GetEntities<Compass>())
                    {
                        if (c.ID == key)
                        {
                            c.Interact(player);
                        }
                    }
                }
            }));
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = Compass.Enabled && Flag;
        }
        public override void Render()
        {
            base.Render();
            if (Compass.Enabled)
            {
                Draw.Rect(Collider, Flag ? Color.Purple : Color.Red);
            }
        }
    }
}