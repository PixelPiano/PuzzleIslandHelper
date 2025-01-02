using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

// PuzzleIslandHelper.LabFallingBlock
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DigitalLever")]
    [Tracked]
    public class DigiLever : Entity
    {
        private string path = "objects/PuzzleIslandHelper/digiLever0";
        public Image Image;
        public string Flag;
        public DotX3 Talk;
        public int Facing;
        private bool state => Flag.GetFlag();
        public DigiLever(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Facing = data.Bool("right") ? 1 : -1;
            Image = new Image(GFX.Game[path + "0"]);
            if(Facing < 0)
            {
                Image.Effects = SpriteEffects.FlipVertically;
            }
            Add(Image);
            Collider = new Hitbox(Image.Width, Image.Height + 16);
            Add(Talk = new DotX3(Collider, Interact));
        }
        public void Interact(Player player)
        {
            SwapState();
        }
        public void SetState(bool state)
        {
            Flag.SetFlag(state);
            Image.Texture = GFX.Game[path + (state ? '1' : '0')];
        }
        public void SwapState()
        {
            SetState(!state);
        }
        public override void Render()
        {
            Image.DrawSimpleOutline();
            base.Render();
        }
    }

}