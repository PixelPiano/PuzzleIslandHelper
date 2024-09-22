using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/TilesColorgrade")]
    public class TilesColorgrade : Backdrop
    {
        private string Colorgrade = "oldsite";
        private string _Colorgrade;
        private float Opacity;
        private bool Decrement;
        private Color RandomColor;
        private float RandomLimit = 1;
        public bool FG;
        public BlendState Blend;
        public enum Blends
        {
            AlphaBlend,
            Additive,
            Opaque,
            NonPremultiplied
        }
        public static Dictionary<Blends, BlendState> BlendStates = new()
        {
            {Blends.AlphaBlend,BlendState.AlphaBlend },
            {Blends.Additive, BlendState.Additive },
            {Blends.Opaque, BlendState.Opaque },
            {Blends.NonPremultiplied,BlendState.NonPremultiplied }
        };
        public TilesColorgrade(BinaryPacker.Element data) : this(data.AttrBool("fg"), data.Attr("colorgrade"),
            (Blends)Enum.Parse(typeof(Blends), data.Attr("blend","AlphaBlend")))
        { }
        public TilesColorgrade(bool fg, string colorgrade, Blends blend)
        {
            Blend = BlendStates[blend];
            FG = fg;
            Colorgrade = "none";
            _Colorgrade = colorgrade;
            RandomColor = new Color(Calc.Random.Range(0, 255), Calc.Random.Range(0, 255), Calc.Random.Range(0, 255));
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (IsVisible(scene as Level))
            {
                Colorgrade = _Colorgrade;
                if (Opacity >= RandomLimit)
                {
                    Decrement = true;
                }
                if (Opacity <= 0)
                {
                    RandomColor = new Color(Calc.Random.Range(100, 255), Calc.Random.Range(100, 255), Calc.Random.Range(100, 255));
                    RandomLimit = Calc.Random.Range(0.4f, 0.7f);
                    Decrement = false;
                }
                Opacity += Decrement ? -Calc.Approach(0, 1, Engine.DeltaTime * 0.1f) : Calc.Approach(0, 1, Engine.DeltaTime * 0.1f);
            }
            else
            {
                Colorgrade = "none";
            }
        }
        private static void Render(On.Monocle.TileGrid.orig_Render orig, TileGrid self)
        {
            Level level = self.Scene as Level;
            bool drawn = false;
            foreach (TilesColorgrade backdrop in level.Background.GetEach<TilesColorgrade>())
            {
                if (backdrop.Colorgrade != "none")
                {
                    TileGrid grid = backdrop.FG ? level.SolidTiles.Tiles : level.BgTiles.Tiles;
                    Color color = grid.Color;
                    grid.Color = Color.Lerp(Color.White, backdrop.RandomColor, backdrop.Opacity);
                    Effect colorGradeEffect = GFX.FxColorGrading;
                    colorGradeEffect.CurrentTechnique = colorGradeEffect.Techniques["ColorGradeSingle"];
                    Texture texture = Engine.Graphics.GraphicsDevice.Textures[1];

                    Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[backdrop.Colorgrade].Texture.Texture_Safe;


                    Draw.SpriteBatch.End();
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred,
                                           backdrop.Blend,
                                           SamplerState.PointWrap,
                                           DepthStencilState.None,
                                           RasterizerState.CullNone,
                                           colorGradeEffect,
                                           level.GameplayRenderer.Camera.Matrix);
                    orig(self);
                    Draw.SpriteBatch.End();
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred,
                                           BlendState.AlphaBlend,
                                           SamplerState.PointWrap,
                                           DepthStencilState.None,
                                           RasterizerState.CullNone,
                                           null,
                                           level.GameplayRenderer.Camera.Matrix);
                    Engine.Graphics.GraphicsDevice.Textures[1] = texture;
                    grid.Color = color;
                    drawn = true;
                }
            }
            if (!drawn)
            {
                orig(self);
            }
        }
        internal static void Load()
        {
            On.Monocle.TileGrid.Render += Render;
        }
        internal static void Unload()
        {
            On.Monocle.TileGrid.Render -= Render;
        }
    }
}

