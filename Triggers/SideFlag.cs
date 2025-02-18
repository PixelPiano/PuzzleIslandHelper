
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/SideFlag")]
    [Tracked]
    public class SideFlag : Trigger
    {
        public enum States
        {
            True,
            False,
            Inactive
        }
        public bool Vertical;
        public States State1;
        public States State2;
        public string Flag;
        public bool OnlyOnLeave;
        public bool UseCenter;
        public bool OnlyIfBetweenOtherBounds;
        public SideFlag(EntityData data, Vector2 offset) : base(data, offset)
        {
            Flag = data.Attr("flag");
            Vertical = data.Bool("vertical");
            UseCenter = data.Bool("useCenter");

            if (Vertical)
            {
                State1 = data.Enum<States>("left");
                State2 = data.Enum<States>("right");
                OnlyIfBetweenOtherBounds = data.Bool("onlyBetweenTopAndBottom");
            }
            else
            {
                State1 = data.Enum<States>("top");
                State2 = data.Enum<States>("bottom");
                OnlyIfBetweenOtherBounds = data.Bool("onlyBetweenLeftAndRight");
            }
            OnlyOnLeave = data.Bool("onlyOnLeave");
            if (data.Bool("transitionUpdate"))
            {
                Tag |= Tags.TransitionUpdate;
            }
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (OnlyOnLeave)
            {
                SetState(player);
            }
        }
        public void SetState(Player player)
        {
            float check1, check2, pcheck1, pcheck2;
            if (UseCenter)
            {
                check1 = check2 = Vertical ? CenterX : CenterY;
                pcheck1 = pcheck2 = Vertical ? player.CenterX : player.CenterY;
            }
            else if (Vertical)
            {
                check1 = Left;
                check2 = Right;
                pcheck1 = player.Left;
                pcheck2 = player.Right;
            }
            else
            {
                check1 = Top;
                check2 = Bottom;
                pcheck1 = player.Top;
                pcheck2 = player.Bottom;
            }
            if (!OnlyIfBetweenOtherBounds ||
                (Vertical ? player.Top >= Top && player.Bottom <= Bottom : player.Left >= Left && player.Right <= Right))
            {
                if (pcheck1 < check1 && State1 != States.Inactive)
                {
                    Flag.SetFlag(State1 == States.True);
                }
                else if (pcheck2 > check2 && State2 != States.Inactive)
                {
                    Flag.SetFlag(State2 == States.True);
                }
            }
/*            if (Vertical)
            {
                if ((player.Top >= Top && player.Bottom <= Bottom) || !OnlyIfBetweenOtherBounds)
                {
                    if (player.Left < check1 && State1 != States.Inactive)
                    {
                        Flag.SetFlag(State1 == States.True);
                    }
                    else if (player.Right > check2 && State2 != States.Inactive)
                    {
                        Flag.SetFlag(State2 == States.True);
                    }

                }
            }
            else if ((player.Left >= Left && player.Right <= Right) || !OnlyIfBetweenOtherBounds)
            {
                if (player.Top < Top && State1 != States.Inactive)
                {
                    Flag.SetFlag(State1 == States.True);
                }
                else if (player.Bottom > Bottom && State2 != States.Inactive)
                {
                    Flag.SetFlag(State2 == States.True);
                }
            }*/
        }
        public override void Update()
        {
            base.Update();
            if (!OnlyOnLeave && Scene.GetPlayer() is Player player)
            {
                SetState(player);
            }
        }
    }
}
