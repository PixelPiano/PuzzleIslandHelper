using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using FrostHelper.ModIntegration;
using Celeste.Mod.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities //Replace with your mod's namespace
{
    [CustomEntity("PuzzleIslandHelper/WallButton")]
    [Tracked]
    public class WallButton : Entity
    {
        public string Flag;
        public Image Image;
        public WallButton(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Flag = data.Attr("flag");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Image = new Image(GFX.Game["objects/PuzzleIslandHelper/wallbutton00"])
            {
                Color = Flag.GetFlag() ? Color.Lime : Color.Red
            };
            Add(Image);
            Collider = Image.Collider();
            Add(new PlayerCollider(OnPlayer));
        }
        private void OnPlayer(Player p)
        {
            Image.Color = Color.Lime;
            Flag.SetFlag();
        }
    }

}