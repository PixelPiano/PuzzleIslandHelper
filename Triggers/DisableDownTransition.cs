
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/DisableDownTransition")]
    [Tracked]
    public class DisableDownTransition : Trigger
    {
        public bool OrigValue;
        public DisableDownTransition(EntityData data, Vector2 offset) : base(data, offset)
        {

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            OrigValue = (scene as Level).Session.LevelData.DisableDownTransition;
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            Level level = SceneAs<Level>();
            if (!level.Transitioning)
            {
                Reset(level);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Reset(scene as Level);
        }
        public void Reset(Level level)
        {
            level.Session.LevelData.DisableDownTransition = OrigValue;
        }
    }
}
