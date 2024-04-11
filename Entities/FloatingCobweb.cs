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
        private DynamicData cobwebData;
        private Color fColor;
        private string hex;
        public static EntityData CreateData(Vector2 position, Vector2[] nodes)
        {
            EntityData data = new EntityData();
            data.Position = position;
            data.Nodes = nodes;
            return data;
        }
        public FloatingCobweb(EntityData data, Vector2 offset) : this(data.Position, offset, data.Nodes, data.HexColor("color"))
        {
        }
        public FloatingCobweb(Vector2 position, Vector2 offset, Vector2[] nodes, Color color) : base(CreateData(position, nodes), offset)
        {
            fColor = color;
            cobwebData = DynamicData.For(this);
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

            cobwebData.Set("color", fColor);

            cobwebData.Set("edge", Color.Lerp(cobwebData.Get<Color>("color"), Calc.HexToColor("0f0e17"), 0.2f));
            areaData.CobwebColor = cobwebColor;
        }

        [MonoModLinkTo("Celeste.Entity", "System.Void BeenAdded(Monocle.Scene)")] //Tells the entity to use Celeste.Entity's BeenAdded instead of Celeste.Cobweb's BeenAdded 
        public void base_Added(Scene scene)
        {
        }
    }
}
