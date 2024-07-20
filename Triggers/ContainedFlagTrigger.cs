
using Celeste.Mod.Entities;

using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/ContainedFlagTrigger")]
    [Tracked]
    public class ContainedFlagTrigger : Trigger
    {
        public string flag;
        public bool inverted;
        public bool updateWhenAdded;
        public bool updateWhenRemoved;
        public ContainedFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            updateWhenAdded = data.Bool("updateFlagWhenAdded");
            updateWhenRemoved = data.Bool("updateFlagWhenRemoved");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (updateWhenAdded)
            {
                (scene as Level).Session.SetFlag(flag, inverted);
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Scene is Level level)
            {
                level.Session.SetFlag(flag, !inverted);
            }
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (Scene is Level level)
            {
                level.Session.SetFlag(flag, inverted);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (updateWhenRemoved)
            {
                (scene as Level).Session.SetFlag(flag, inverted);
            }
        }
    }
}
