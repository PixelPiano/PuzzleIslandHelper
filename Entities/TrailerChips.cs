using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [TrackedAs(typeof(Chip))]
    public class BackgroundChip : Chip
    {
        public ChipBox[,] Grid;
        public BackgroundChip() : base(Vector2.Zero, Vector2.Zero, 1, 9, Color.White)
        {
            UseShader = false;
            Tag |= Tags.Global;
            Add(new DebugComponent(Keys.B, Cycle, true));
        }
        private int cycle;
        public void Cycle()
        {
            for (int i = 0; i < rows / 2; i++)
            {
                ChangeGroupAlphaMult(i, 0);
            }
            cycle = (cycle + 1) % (rows / 2);
            ChangeGroupAlphaMult(cycle, 1 - ((float)cycle / (rows / 2)));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        private int cols, rows;
        public void ChangeGroupAlphaMult(int group, float alpha)
        {
            for (int c = group; c < cols - group; c++)
            {
                Grid[c, group].AlphaMult = alpha;
                Grid[c, (rows - 1) - group].AlphaMult = alpha;
            }
            for (int r = group; r < rows - group; r++)
            {
                Grid[group, r].AlphaMult = alpha;
                Grid[(cols - 1) - group, r].AlphaMult = alpha;
            }
        }
        public void ChangeAllGroupAlphaMult(float startAlpha, float increment)
        {
            float alpha = startAlpha;
            for (int i = 0; i < rows / 2; i++)
            {
                ChangeGroupAlphaMult(i, alpha);
                alpha += increment;
            }
        }
        public override void Awake(Scene scene)
        {
            for (int i = 0; i < 320; i += BoxSize)
            {
                if (i < 180)
                {
                    rows++;
                }
                cols++;
            }
            GenerateGrid(Vector2.Zero, cols, rows, BoxSize, Color, out Grid, 1f);
            Bins = [.. Grid];
            CreateCollider();
            Add([.. Bins]);
            Bake();
            base.Awake(scene);
            for (int i = 0; i < Bins.Count; i++)
            {
                Add(new Coroutine(Bins[i].EndlessFlicker(-0.3f, 0.3f, 0.2f)));
            }
        }
        public override void Update()
        {
            Position = SceneAs<Level>().Camera.Position;
            Alpha = 1;
            Color = Color.White;
            Visible = true;
            base.Update();
        }
    }
    //[CustomEntity("PuzzleIslandHelper/TrailerChips")]
    public class TrailerChips : Entity
    {
        public List<TrailerChip> Chips = [];
        public int Index = -1;
        private string[] phrases =
            ["is that me",
             "i cant tell",
             "who am i",
             "who am i"];
        public List<ChipPhrase.Word> Highlights = [];

        [TrackedAs(typeof(Chip))]
        public class TrailerChip : ChipPhrase
        {
            public bool Selected;
            public TrailerChip(Vector2 position, string phrase) : base(position, phrase, Vector2.Zero, 1, 10, Color.Lerp(Color.Black, Color.White, 0.8f))
            {
                UseShader = false;
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                for (int i = 0; i < Bins.Count; i++)
                {
                    Add(new Coroutine(Bins[i].EndlessFlicker(-0.3f, 0.3f, 0.2f)));
                }
            }
        }
        private void next()
        {
            foreach (var c in Chips)
            {
                c.Visible = c.Selected = false;
            }
            Index = (Index + 1) % Chips.Count;
            var chip = Chips[Index];
            chip.Selected = chip.Visible = true;
            SceneAs<Level>().Camera.Position = chip.Center - new Vector2(160, 90);
        }

        private void flashWord()
        {
            if (Index >= 0)
            {
                foreach (ChipPhrase.Word word in Chips[Index].ChipWords)
                {
                    if (word.Text is "me" or "i")
                    {
                        foreach (Chip.ChipBox bin in word.Boxes)
                        {
                            bin.Color = Color.White;
                            bin.Alpha = 1;
                            bin.CancelEndlessFlicker();
                            bin.FadeToColor(Color.Cyan, 1, 0.5f, 0.05f);
                        }
                    }
                }
            }
        }
        public TrailerChips(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(new DebugComponent(Keys.O, next, true));
            Add(new DebugComponent(Keys.N, flashWord, true));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            for (int i = 0; i < phrases.Length; i++)
            {
                TrailerChip chip = new TrailerChip(Position, phrases[i]);
                Chips.Add(chip);
                scene.Add(chip);
                Position += Vector2.UnitX * (chip.Width + 24);
                foreach (ChipPhrase.Word word in chip.ChipWords)
                {
                    if (word.Text is "me" or "i")
                    {
                        Highlights.Add(word);
                    }
                }
            }
            Position = Chips[0].Position;
            Collider = new Hitbox(8, 8, -8, -8);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetPlayer() is Player player)
            {
                player.DisableMovement();
            }
        }
        [Command("create_chip", "")]
        public static void CreateChip(string s = "chip", string c = "FFFFFF")
        {
            if (Engine.Scene is not null && Engine.Scene.GetPlayer() is Player player)
            {
                TrailerChip chip = new TrailerChip(player.Position - Vector2.UnitY * 24, s);
                Engine.Scene.Add(chip);
            }
        }

    }
}