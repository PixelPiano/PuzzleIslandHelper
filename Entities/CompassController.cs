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
        public Sprite Sprite;
        public Sprite Flash;
        public CompassController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/compass/");
            Sprite.AddLoop("active", "lecternOn", 0.1f);
            Sprite.AddLoop("inactive", "lecternOff", 0.1f);
            Flash = new Sprite(GFX.Game,"objects/PuzzleIslandHelper/compass/");
            Flash.Add("flash","lecternFlash",0.1f);
            Add(Sprite,Flash);
            Sprite.Play("inactive");
            Collider = Sprite.Collider();;
            Rectangle r = new Rectangle(0, 0, 16, 16);
            Flag = data.FlagList();
            Add(Talk = new TalkComponent(r, Vector2.UnitX * 8, player =>
            {
                string key = data.Attr("key");
                bool interacted = false;
                if (!string.IsNullOrEmpty(key))
                {
                    foreach (Compass c in Scene.Tracker.GetEntities<Compass>())
                    {
                        if (c.ID == key)
                        {
                            interacted = true;
                            c.Interact(player);
                        }
                    }
                }
                if (interacted)
                {
                    Flash.Play("flash");
                }
            }));
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = Compass.Enabled && Flag;
            if(Talk.Enabled && Sprite.CurrentAnimationID != "active")
            {
                Sprite.Play("active");
            }
            if(!Talk.Enabled && Sprite.CurrentAnimationID != "inactive")
            {
                Sprite.Play("inactive");
            }
        }
        public override void Render()
        {
            base.Render();
        }
    }
}