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

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [TrackedAs(typeof(DottedLine))]
    public class DottedLineWall : DottedLine
    {
        public int Layers = 1;
        public float LayerSpacing = 2;
        public DottedLineWall(EntityData data, Vector2 offset) : this(data.Int("layers"), data.Position + offset, data.NodesOffset(offset)[0], data.Float("thickness"), data.Enum<FancyLine.ColorModes>("colorMode"), data.Float("lineLength"), data.Float("spacing"), data.Float("colorInterval"), data.HexColor("color1"), data.HexColor("color2"), data.Float("moveRate"))
        {
        }

        public DottedLineWall(int layers, Vector2 start, Vector2 end, float thickness, FancyLine.ColorModes colorMode, float lineLength, float spacing, float colorInterval, Color color, Color color2, float moveRate) : base(start, end, thickness, colorMode, lineLength, spacing, colorInterval, color, color2, moveRate)
        {
            Layers = layers;
        }
        public override void Render()
        {
            base.Render();
            /*            if (OnScreen)
                        {
                            Vector2 offset = Vector2.UnitX * (-Layers / 2f) * LayerSpacing;
                            for (int i = 0; i < Layers; i++)
                            {
                                RenderOffset(offset + Vector2.UnitX * (i * LayerSpacing));
                            }
                        }*/
        }
        public override bool IsOnScreen()
        {
            Rectangle cam = SceneAs<Level>().Camera.GetBounds().Pad(8 + Layers * 2);
            return cam.Contains(Bounds);
        }
        public override void CleanUpLines()
        {
            toRender.Clear();
            Rectangle level = SceneAs<Level>().Bounds.Pad(Layers * 2);
            Rectangle cam = SceneAs<Level>().Camera.GetBounds().Pad(Layers * 2);

            foreach (FancyLineAngle line in Lines)
            {
                Vector2 s = line.RenderStart, e = s + line.EndOffset;
                if (!Collide.RectToLine(cam, s, e))
                {
                    if (!Collide.RectToLine(level, s, e))
                    {
                        toRemove.Add(line);
                    }
                }
                else
                {
                    toRender.Add(line);
                }
            }
            if (toRemove.Count > 0)
            {
                foreach (FancyLineAngle line in toRemove)
                {
                    line.RemoveSelf();
                }
                toRemove.Clear();
            }
        }
    }
}