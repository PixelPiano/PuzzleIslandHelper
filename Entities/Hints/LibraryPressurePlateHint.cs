using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Hints
{
    [CustomEntity("PuzzleIslandHelper/LibraryPressurePlateHint")]
    [Tracked]
    public class LibraryPressurePlateHint : Entity
    {
        private class box : GraphicsComponent
        {
            public float ColorLerp = 0;
            public Color OffColor;
            private float width, height;
            public box(Color offColor, Color onColor, float width, float height) : base(true)
            {
                this.width = width;
                this.height = height;
                OffColor = offColor;
                Color = onColor;
            }
            public void Flash()
            {
                ColorLerp = 1;
                Tween.Set(Entity, Tween.TweenMode.Oneshot, 0.7f, Ease.SineOut, t =>
                {
                    ColorLerp = 1 - t.Eased;
                });
            }
            public override void Render()
            {
                base.Render();
                Draw.Rect(RenderPosition, width, height, Color.Lerp(OffColor, Color, ColorLerp));
            }
        }
        private List<int> sequence = [];
        private int inputs;
        private Color backgroundColor = Color.Black;
        private FlagList Flag;
        private int currentInput
        {
            get => _currentInput;
            set
            {
                if (value >= 0 && value < inputboxes.Count)
                {
                    Audio.Play("event:/game/general/cassette_block_switch_1");
                    inputboxes[value].Flash();
                }
                _currentInput = value;
            }
        }
        private int _currentInput = -1;
        private float boxWidth = 24;
        private float boxHeight = 16;
        private List<box> boxes = [];
        private List<box> inputboxes = [];
        private float xpad, ypad;
        public LibraryPressurePlateHint(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            xpad = data.Float("xPadding", 1);
            ypad = data.Float("yPadding", 1);
            boxWidth = data.Float("boxWidth", 24);
            boxHeight = data.Float("boxHeight", 16);
            string[] sequence = data.Attr("sequence").Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string s in sequence)
            {
                if (int.TryParse(s, out int result))
                {
                    this.sequence.Add(result);
                }
            }
            Flag = data.FlagList("flag");
            inputs = data.Int("inputs");
            Collider = new Hitbox(xpad + (inputs * 2 + 1) * (boxWidth + xpad), boxHeight + ypad * 2);
            Color darkGrey = Color.Lerp(Color.Black, Color.White, 0.1f);
            Color brown = new Color(124, 69, 0, 1);
            for (int i = 0; i < inputs + inputs + 1; i++)
            {
                box box;
                if (i % 2 == 0)
                {
                    box = new box(brown,brown, boxWidth, boxHeight);
                    boxes.Add(box);
                }
                else
                {
                    box = new box(darkGrey, Color.Lime, boxWidth, boxHeight);
                    inputboxes.Add(box);
                }
                box.Position = new Vector2(xpad + i * (boxWidth + xpad), ypad);
                Add(box);
            }
            currentInput = -1;
            Add(new Coroutine(routine()));

        }
        public override void Render()
        {
            Draw.Rect(Collider, backgroundColor);
            base.Render();
        }
        private IEnumerator routine()
        {
            yield return 1;
            while (true)
            {
                if (!Flag)
                {
                    yield return null;
                    continue;
                }
                foreach (int i in sequence)
                {
                    currentInput = i;
                    yield return 0.9f;
                }
                currentInput = -1;
                yield return 2;
            }
        }

    }
}
