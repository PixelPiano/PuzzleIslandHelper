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

    [Tracked]
    public class Item : Image
    {
        public bool OneUse;
        public string Name;
        public bool CanUse;
        public bool Used;
        public Action OnUse;

        public override void Update()
        {
            base.Update();
            if (EscapeInv.Disabled)
            {
                Color = Color.Lerp(Color.White, Color.Black, 0.2f);
            }
            else
            {
                Color = Color.White;
            }
        }
        public Item(string name, Vector2 position, Action onUse) : base(GFX.Game["objects/PuzzleIslandHelper/EscapeRoom/Inventory/" + name], true)
        {
            OnUse = onUse;
            Name = name;
            Position = position;
        }


        public virtual void Use()
        {
            OnUse?.Invoke();
        }
    }
}