using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public static class AdvancedGlitch
    {
        private static string effectPath = "PuzzleIslandHelper/Shaders/advancedGlitch1";
        public static float Value;
        public static Effect ApplyParameters(bool[] combo, Vector2 dimensions, float amplitude, float amount, float timer, float seed)
        {
            Effect fxGlitch = ShaderHelperIntegration.GetEffect(effectPath);
            fxGlitch.Parameters["combo"].SetValue(combo);
            fxGlitch.Parameters["dimensions"].SetValue(dimensions);
            fxGlitch.Parameters["amplitude"].SetValue(amplitude);
            fxGlitch.Parameters["minimum"].SetValue(-1f);
            fxGlitch.Parameters["glitch"].SetValue(amount);
            fxGlitch.Parameters["timer"].SetValue(timer);
            fxGlitch.Parameters["seed"].SetValue(seed);
            return fxGlitch;
        }
        public static Effect ApplyParameters(bool[] combo, float amplitude, float glitchAmount, float timer, float seed)
        {
            return ApplyParameters(combo, new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height), amplitude, glitchAmount, timer, seed);
        }

    }

    //[ConstantEntity("PuzzleIslandHelper/AdvancedGlitchTest")]
    [Tracked]
    public class GlitchTest : Entity
    {
        [Command("testglitch", "")]
        public static void testGlitch(float value, float amplitude)
        {
            GlitchTest.value = value;
            GlitchTest.amplitude = amplitude;
        }
        [Command("testseed", "")]
        public static void testSeed()
        {
            seed = Calc.Random.Next(0, int.MaxValue);
        }
        private bool[] combo = new bool[4];
        public static MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/testDecal"];
        public static VirtualTexture Padded;
        private static float seed;
        private static float amplitude;
        private static float value;
        private static float timer;
        public GlitchTest() : base()
        {
            Tag |= Tags.Global;
            Depth = -4;
            combo.Initialize();
            combo[1] = true;
            combo[3] = true;

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Padded = Texture.PadTexture(Texture.Width, Texture.Height, Color.Transparent);
        }
        public override void Update()
        {
            base.Update();
            if (!SceneAs<Level>().Paused)
            {
                timer += Engine.DeltaTime;
                seed = Calc.Random.NextFloat();
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            Draw.SpriteBatch.End();
            for (int i = 0; i < 4; i++)
            {
                combo[i] = false;
            }
            combo[Calc.Random.Range(0, 4)] = true;
            combo[Calc.Random.Range(0, 4)] = true;
            Effect e = AdvancedGlitch.ApplyParameters(combo, new Vector2(Padded.Width, Padded.Height), (float)Math.PI * 2f, value, timer, seed);
            Draw.SpriteBatch.StandardBegin(level.Camera.Matrix, e);
            Draw.SpriteBatch.Draw(Padded.Texture, player.Position - Texture.Size() * 2, Color.White);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
            Draw.HollowRect(player.Position - Texture.Size() * 2, Padded.Width, Padded.Height, Color.White);
        }
    }
}
