using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.EscapeRoomEntities
{

    [CustomEntity("PuzzleIslandHelper/EscapeEntities/ItemArea")]
    [Tracked]
    public class ItemArea : Entity
    {
        public string ItemName;
        public bool Disabled;
        public override void Render()
        {
            base.Render();
        }
        public ItemArea(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || Disabled)
            {
                return;
            }
            if (CollideCheck<Player>())
            {
                SetItemUsability(level);
            }
        }
        public void SetItemUsability(Level level)
        {
            foreach (Item item in level.Tracker.GetComponents<Item>())
            {
                item.CanUse = item.Name == ItemName;
            }
        }
    }
}