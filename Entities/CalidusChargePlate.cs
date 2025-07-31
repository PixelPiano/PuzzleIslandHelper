using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// PuzzleIslandHelper.SecurityLaser
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CalidusChargePlate")]
    [Tracked]
    public class CalidusChargePlate : Entity
    {
        private string path = "characters/PuzzleIslandHelper/Calidus/";
        public MTexture PlateTex => GFX.Game[path + "chargePlate"];
        public MTexture OrbIndentTex => GFX.Game[path + "indentOrb"];
        public MTexture LeftIndentTex => GFX.Game[path + "indentArmLeft"];
        public MTexture RightIndentTex => GFX.Game[path + "indentArmRight"];
        public Image Plate, OrbIndent, LeftIndent, RightIndent;
        public CalidusChargePlate(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Plate = new Image(PlateTex));
            Collider = new Hitbox(Plate.Width - 1, Plate.Height);
            Add(LeftIndent = new Image(LeftIndentTex));
            LeftIndent.Position = Vector2.One * 2;
            Add(RightIndent = new Image(RightIndentTex));
            RightIndent.Position = new Vector2(Width - 2 - RightIndent.Width, 2);
            Add(OrbIndent = new Image(OrbIndentTex));
            OrbIndent.Position = new Vector2(11, 2);
        }
    }
}