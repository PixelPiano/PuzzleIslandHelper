using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [ConstantEntity("PuzzleIslandHelper/CatHelper")]
    [Tracked]
    public class CatHelper : Entity
    {
        [Command("cat","toggle cat helper")]
        public static void Toggle()
        {
            Enabled = !Enabled;
        }
        [Command("catstate","set cat helper state")]
        public static void CatState(bool state = true)
        {
            State = state;
        }
        public static bool State;
        public static bool Enabled;
        private MTexture snug => GFX.Game["objects/PuzzleIslandHelper/catsnug"];
        private MTexture plant => GFX.Game["objects/PuzzleIslandHelper/catplant"];
        public CatHelper() : base()
        {
            Tag |= Tags.Global | TagsExt.SubHUD;
        }
        public override void Render()
        {
            base.Render();
            if (Enabled)
            {
                (State ? snug : plant).Draw(Vector2.Zero, Vector2.Zero, Color.White, 3);
            }
        }
    }
}
