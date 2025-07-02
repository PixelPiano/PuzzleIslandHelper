using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/3D")]
    [Tracked]
    public class ThreeDeePassenger : VertexPassenger
    {
        private bool dialgo => HasDialogCutscenes;
        private int index => DialogIndex;
        private bool cutscene => HasDataCutscene;
        private bool watched => CutsceneWatched;
        public ThreeDeePassenger(EntityData data, Vector2 offset) : base(data.Position + offset, 20, 22, data.Attr("cutsceneID"), data.Attr("dialog"), Vector2.One, new(0.2f, 1), 1f)
        {
            MinWiggleTime = 1;
            MaxWiggleTime = 1.4f;
            AddTriangle(new(10, 0), new(0, 16), new(5, 16), 0.8f, Vector2.One, new(1, Ease.Linear, Color.DarkGreen, Color.DarkGreen, Color.Green));
            AddTriangle(new(10, 0), new(20, 16), new(6, 16), 1, Vector2.One, new(1, Ease.Linear, Color.Green, Color.Lime, Color.LightGreen));

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
    }
}
