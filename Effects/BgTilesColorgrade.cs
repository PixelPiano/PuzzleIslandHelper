using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    public class BgTilesColorgrade : Backdrop
    {
        private static string Colorgrade = "oldsite";
        private string _Colorgrade;
        private static float Opacity;
        private bool Decrement;
        private Color RandomColor;
        private float RandomLimit = 1;
        public BgTilesColorgrade(string colorgrade)
        {
            Colorgrade = colorgrade;
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
                (scene as Level).BgTiles.Tiles.Color = Color.Lerp(Color.White, RandomColor, Opacity);
            }
            else
            {
                Colorgrade = "none";
                (scene as Level).BgTiles.Tiles.Color = Color.White;

            }
        }
        private static void BgRender(On.Monocle.TileGrid.orig_Render orig, TileGrid self)
        {
            
            if (self.Entity is BackgroundTiles)
            {
                Effect colorGradeEffect = GFX.FxColorGrading;
                colorGradeEffect.CurrentTechnique = colorGradeEffect.Techniques["ColorGradeSingle"];
                Texture texture = Engine.Graphics.GraphicsDevice.Textures[1];

                Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[Colorgrade].Texture.Texture_Safe;


                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred,
                                       BlendState.AlphaBlend,
                                       SamplerState.PointWrap,
                                       DepthStencilState.None,
                                       RasterizerState.CullNone,
                                       colorGradeEffect,
                                       (self.Scene as Level).GameplayRenderer.Camera.Matrix);
                orig(self);
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred,
                                       BlendState.AlphaBlend,
                                       SamplerState.PointWrap,
                                       DepthStencilState.None,
                                       RasterizerState.CullNone,
                                       null,
                                       (self.Scene as Level).GameplayRenderer.Camera.Matrix);
                Engine.Graphics.GraphicsDevice.Textures[1] = texture;


            }
            else
            {
                orig(self);
            }
        }
        internal static void Load()
        {
            On.Monocle.TileGrid.Render += BgRender;
        }
        internal static void Unload()
        {
            On.Monocle.TileGrid.Render -= BgRender;
        }
    }
}

