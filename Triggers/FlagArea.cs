using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/FlagArea")]
    [Tracked]
    public class FlagArea : Trigger
    {
        private string activeFlag;
        private bool activeFlagInverted;
        private string flag;
        private bool inverted;
        private bool everyFrame;
        private bool wasColliding;
        public FlagArea(EntityData data, Vector2 offset) : base(data, offset)
        {
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            activeFlag = data.Attr("activeFlag");
            activeFlagInverted = data.Bool("activeFlagInverted");
            everyFrame = data.Bool("checkEveryFrame");
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (!everyFrame) flag.SetFlag(activeFlag.GetFlag(activeFlagInverted) != inverted);
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (!everyFrame) flag.SetFlag(activeFlag.GetFlag(activeFlagInverted) == inverted);
        }
        public override void Update()
        {
            base.Update();
            if (everyFrame)
            {
                flag.SetFlag((activeFlag.GetFlag(activeFlagInverted) && PlayerIsInside) != inverted);
            }
        }
    }
}
