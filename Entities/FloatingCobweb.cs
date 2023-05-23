using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;
// PuzzleIslandHelper.FloatingCobweb
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FloatingCobweb")]
    [TrackedAs(typeof(Cobweb))]
    public class FloatingCobweb : Cobweb
    {
        private DynamicData cD;
        private Color fColor;
        private string hex;
        public FloatingCobweb(EntityData data, Vector2 offset) : base(data, offset)
        {
            cD = DynamicData.For(this);
            hex = data.Attr("color");
            fColor = Calc.HexToColor(hex);
        }
        public override void Added(Scene scene)
        {
            base_Added(scene);
            AreaData areaData = AreaData.Get(scene);
            Color[] cobwebColor = areaData.CobwebColor;
            if (OverrideColors != null)
            {
                areaData.CobwebColor = OverrideColors;
            }
            cD.Set("color", fColor);
            cD.Set("edge",Color.Lerp(cD.Get<Color>("color"), Calc.HexToColor("0f0e17"), 0.2f));
            areaData.CobwebColor = cobwebColor;
        }

        [MonoModLinkTo("Celeste.Entity", "System.Void Added(Monocle.Scene)")] //Tells the entity to use Celeste.Entity's Added instead of Celeste.Cobweb's Added 
        public void base_Added(Scene scene)
        {
        }
    }
}
