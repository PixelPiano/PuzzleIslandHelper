using Celeste.Mod.Backdrops;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/TilesShader")]
    public class TilesShader : Backdrop
    {
        public enum Options
        {
            Background,
            Foreground
        }
        private Options option;
        private string effectName;
        public float Amplitude;
        public BlendState BlendState = BlendState.AlphaBlend;
        public bool Background => option is Options.Background;
        public bool Foreground => option is Options.Foreground;
        public TilesShader(BinaryPacker.Element data) : this(data.Attr("effect"), Enum.Parse<Options>(data.Attr("tiles", "Foreground"))) { }
        public TilesShader(string effect, Options op) : base()
        {
            effectName = effect;
            option = op;
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (IsVisible(scene as Level))
            {
                Amplitude = (float)(Math.Sin(scene.TimeActive) + 1f) / 2f;
            }
            else
            {
                Amplitude = 0;
            }
        }
        public Effect ApplyParameters(Level level, Effect effect)
        {
            effect.ApplyParameters(level, level.GameplayRenderer.Camera.Matrix, Amplitude);
            return effect;
        }
        private static void TileRender(On.Monocle.TileGrid.orig_Render orig, TileGrid self)
        {
            bool bg = self.Entity is BackgroundTiles;
            bool fg = self.Entity is Solid;
            bool valid = bg || fg;
            if (Engine.Scene is Level level && valid && level.Background.GetEach<TilesShader>()
                .Where(item => item.IsVisible(level) && ((item.Background && bg) || (item.Foreground && fg)))
                .ToList() is List<TilesShader> list && list.Count > 0)
            {
                Draw.SpriteBatch.End();
                foreach (var b in list)
                {
                    Effect e = ShaderHelperIntegration.TryGetEffect(b.effectName);
                    e.ApplyParameters(level, level.GameplayRenderer.Camera.Matrix, b.Amplitude);
                    Draw.SpriteBatch.StandardBegin(level.GameplayRenderer.Camera.Matrix, e);
                    orig(self);
                    Draw.SpriteBatch.End();
                }
                Draw.SpriteBatch.StandardBegin(level.GameplayRenderer.Camera.Matrix);
            }
            else
            {
                orig(self);
            }
        }
        [OnLoad]
        public static void Load()
        {
            On.Monocle.TileGrid.Render += TileRender;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Monocle.TileGrid.Render -= TileRender;
        }
    }
}

