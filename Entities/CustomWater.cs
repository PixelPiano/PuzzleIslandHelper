//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomWater")]
    [TrackedAs(typeof(Water))]
    public class CustomWater : Water
    {
        private string displacementState;

        private bool invertFlag;

        public CustomWater(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Bool("topSurface",true), data.Bool("hasBottom"), data.Width, data.Height)
        {
            Get<DisplacementRenderHook>().RenderDisplacement = RenderDisplacementFlagged;
            displacementState = data.Attr("displacementFlag");
            invertFlag = data.Bool("invertFlag");
        }
       

        public void RenderDisplacementFlagged()
        {
            if (SceneAs<Level>().Session.GetFlag(displacementState))
            {
                RenderDisplacement();
            }
        }
    }
}
