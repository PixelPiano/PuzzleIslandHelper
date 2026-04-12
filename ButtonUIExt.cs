using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public static class ButtonUIExt
    {
        public static void RenderOutline(Vector2 position, string label, VirtualButton button, Color textColor, Color buttonColor, float outlineSize, float scale, float justifyX = 0.5f, float wiggle = 0f, float alpha = 1f, Vector2 buttonOffset = default, Vector2 textOffset = default)
        {
            MTexture mTexture = Input.GuiButton(button, "controls/keyboard/oemquestion");
            float num = ButtonUI.Width(label, button);
            position.X -= scale * num * (justifyX - 0.5f);
            mTexture.DrawLazyOutline(position + buttonOffset, new Vector2((float)mTexture.Width - num / 2f, (float)mTexture.Height / 2f), buttonColor * alpha, scale + wiggle, 0, Color.Black, outlineSize);
            DrawTextOutline(label, position + textOffset, textColor, Color.Black, outlineSize, num / 2f, scale + wiggle, alpha);
        }
        public static void DrawTextOutline(string text, Vector2 position, Color color, Color outlineColor, float outlineSize, float justify, float scale, float alpha)
        {
            float x = ActiveFont.Measure(text).X;
            ActiveFont.DrawOutline(text, position, new Vector2(justify / x, 0.5f), Vector2.One * scale, color * alpha, outlineSize, outlineColor * alpha);
        }
        public static float Height(string label, VirtualButton button)
        {
            MTexture mTexture = Input.GuiButton(button, "controls/keyboard/oemquestion");
            return Math.Max(ActiveFont.Measure(label).Y, (float)mTexture.Height);
        }
        public static Vector2 Size(string label, VirtualButton button)
        {
            MTexture mTexture = Input.GuiButton(button, "controls/keyboard/oemquestion");
            Vector2 measure = ActiveFont.Measure(label);
            return new Vector2(ActiveFont.Measure(label).X + 8f + mTexture.Width, Math.Max(measure.Y, mTexture.Height));
        }
    }
}
