using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    //[CustomEntity("PuzzleIslandHelper/TrailerChips")]
    public class TrailerChipsBeta : Entity
    {
        private string[] validSplits = [",", "\n", "{break}"];
        public string[] Phrases;
        public TrailerChipsBeta(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            string ph = data.Attr("phrases");
            string str = Dialog.Get(ph);
            Phrases = str.Split(validSplits, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Vector2 position = Position;
            foreach (string s in Phrases)
            {
                ChipPhrase chip = new ChipPhrase(position, s, Vector2.Zero, 1, 8, Color.White);
                scene.Add(chip);
                position.X += chip.Width + 16;
            }
        }
    }
}