using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/BgTilesColorgrade")]
    public class BgTilesColorgrade : Backdrop
    {
        private static string Colorgrade;
        private string _Colorgrade;
        private float Alpha;
        private bool Decrement;
        private Color RandomColor;
        private float RandomLimit = 1;
        public bool Fade;
        public BgTilesColorgrade(BinaryPacker.Element data) : this(data.Attr("colorgrade"), data.AttrFloat("alpha", 1), data.AttrBool("randomize", true)) { }
        public BgTilesColorgrade(string colorgrade, float alpha, bool randomize)
        {
            Alpha = alpha;
            Fade = randomize;
            _Colorgrade = colorgrade;
            RandomColor = new Color(Calc.Random.Range(0, 255), Calc.Random.Range(0, 255), Calc.Random.Range(0, 255));
        }
        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            Colorgrade = _Colorgrade = null;
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (IsVisible(scene as Level))
            {
                Colorgrade = _Colorgrade;
                if (Fade)
                {
                    if (Alpha >= RandomLimit)
                    {
                        Decrement = true;
                    }
                    if (Alpha <= 0)
                    {
                        RandomColor = new Color(Calc.Random.Range(100, 255), Calc.Random.Range(100, 255), Calc.Random.Range(100, 255));
                        RandomLimit = Calc.Random.Range(0.4f, 0.7f);
                        Decrement = false;
                    }
                    Alpha += Decrement ? -Calc.Approach(0, 1, Engine.DeltaTime * 0.1f) : Calc.Approach(0, 1, Engine.DeltaTime * 0.1f);
                }
                (scene as Level).BgTiles.Tiles.Color = Color.Lerp(Color.White, RandomColor, Alpha);
            }
            else
            {
                Colorgrade = null;
                (scene as Level).BgTiles.Tiles.Color = Color.White;
            }
        }
        private static void BgRender(On.Monocle.TileGrid.orig_Render orig, TileGrid self)
        {
            Level level = self.Scene as Level;
            if (self.Entity is BackgroundTiles && !string.IsNullOrEmpty(Colorgrade) && Colorgrade != "none")
            {
                Texture texture = Engine.Graphics.GraphicsDevice.Textures[1];
                Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[Colorgrade].Texture.Texture_Safe;
                Matrix matrix = level.GameplayRenderer.Camera.Matrix;
                Effect colorGradeEffect = GFX.FxColorGrading;
                colorGradeEffect.CurrentTechnique = colorGradeEffect.Techniques["ColorGradeSingle"];
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.StandardBegin(matrix, colorGradeEffect);
                orig(self);
                Draw.SpriteBatch.End();

                Draw.SpriteBatch.StandardBegin(matrix);
                Engine.Graphics.GraphicsDevice.Textures[1] = texture;
            }
            else
            {
                orig(self);
            }

        }

        [OnLoad]
        public static void Load()
        {
            On.Monocle.TileGrid.Render += BgRender;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Monocle.TileGrid.Render -= BgRender;
        }
    }
}

