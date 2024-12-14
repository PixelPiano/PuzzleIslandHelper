using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using ExtendedVariants.Variants;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;
using System.Collections.Generic;
using VivHelper.Entities;
using static Celeste.Mod.PuzzleIslandHelper.Entities.HatchMachine;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/HatchMachine")]
    [Tracked]
    public class HatchMachine : Entity
    {
        private const string path = "objects/PuzzleIslandHelper/machines/hatchMachine/";
        public class Hatch : GraphicsComponent
        {
            private MTexture texture => GFX.Game[path + "hatch" + (Open ? "01" : "00")];
            public bool Open;
            public Hatch() : base(true)
            {
            }
            public override void Render()
            {
                texture?.Draw(RenderPosition, Origin, Color, Scale, Rotation, Effects);
            }
        }
        public class Hint : GraphicsComponent
        {
            private MTexture bg => GFX.Game[path + "display"];
            private MTexture hint => GFX.Game[path + "hint"];

            public Hint(Vector2 position, Vector2 scale) : base(true)
            {
                Scale = scale;
                Position = position;

            }
            public override void Render()
            {
                if (hint != null)
                {
                    Vector2 p = RenderPosition;
                    Vector2 offset = hint.HalfSize();
                    bg?.Draw(p, Vector2.Zero, Color, Vector2.One, 0);
                    hint.Draw(p + offset, offset, Color, Scale, Rotation, Effects);
                }
            }
        }
        public class Light : GraphicsComponent
        {
            private MTexture glass => GFX.Game[path + "lightGlass"];
            private MTexture fill => GFX.Game[path + "lightFill"];
            public Light(Vector2 position) : base(true)
            {
                Position = position;
            }
            public void SetColor(Color color)
            {
                Color = color;
            }
            public override void Render()
            {
                Vector2 p = RenderPosition;
                fill?.Draw(p, Vector2.Zero, Color, Scale, Rotation, Effects);
                glass?.Draw(p, Vector2.Zero, Color.White, Scale, 0, Effects);
            }
        }
        private SnapSolid Platform;
        private Hatch hatch;
        private Hint[] hints = new Hint[4];
        private Light[] lights = new Light[4];
        private Image machine;
        private string[] flags = new string[4];
        public HatchMachine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(machine = new Image(GFX.Game[path + "machine"]));
            Collider = machine.Collider();

            hatch = new Hatch();
            Vector2 half = machine.HalfSize();
            Vector2[] hintOffsets = [new(-21, -16), new(13, -16), new(-21, 10), new(13, 10)];
            Vector2[] lightOffsets = [new(0, 9), new(0, 9), new(0, -9), new(0, -9)];
            Vector2[] hintScale = [new(-1, 1), Vector2.One, -Vector2.One, new(1, -1)];
            string flag = "HatchMachineLights";
            for (int i = 0; i < 4; i++)
            {
                hints[i] = new Hint(half + hintOffsets[i], hintScale[i]);
                lights[i] = new Light(hints[i].Position + lightOffsets[i]);
                flags[i] = flag + i;
            }
            Add(hints);
            Add(lights);
            Add(hatch);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(Platform = new SnapSolid(Position + new Vector2(11, 45), 25, 3, true));
        }
        public override void Render()
        {
            machine.DrawSimpleOutline();
            base.Render();

        }
        public override void Update()
        {
            hatch.Open = true;
            for (int i = 0; i < 4; i++)
            {
                bool flag1 = flags[i].GetFlag();
                hatch.Open &= flag1;
                lights[i].SetColor(flag1 ? Color.Lime : Color.Red);
            }
            base.Update();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform.RemoveSelf();
        }
    }
}