using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FluidPipe")]
    [Tracked]
    public class FluidPipe : Entity
    {
        public enum FluidType
        {
            Dangerous,
            Bubble,
            Weird
        }
        public FluidType Fluid;

        public bool Broken
        {
            get
            {
                if (string.IsNullOrEmpty(flag))
                {
                    return false;
                }
                bool flagState = SceneAs<Level>().Session.GetFlag(flag);
                return inverted ? !flagState : flagState;
            }
        }
        private bool breakInstantly;
        private string flag;
        private bool inverted;


        public FluidPipe(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
    }
}