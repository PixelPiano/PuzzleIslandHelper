
using Celeste.Mod.Entities;

using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/SceneSwitch")]
    [Tracked]
    public class SceneSwitch : Trigger
    {
        public enum Transitions
        {
            Forest,
            Resort,
            Home
        }
        public bool TempleOnLeft;
        public bool InTemple
        {
            get
            {
                return !SceneAs<Level>().Session.GetFlag("sceneSwitch");
            }
            set
            {
                SceneAs<Level>().Session.SetFlag("sceneSwitch", !value);
            }
        }
        private Transitions Transition;
        public SceneSwitch(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            Transition = data.Enum<Transitions>("transition");
            TempleOnLeft = data.Bool("templeOnLeft");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (player.Speed.X > 0 || player.Speed.X < 0)
            {
                DoStuff(Transition, player.Speed.X < 0);
            }

        }
        public void ToTemple()
        {
            InTemple = true;
        }
        public void ToOther()
        {
            InTemple = false;
        }
        public void DoStuff(Transitions t, bool exitLeft)
        {
            if (t == Transitions.Forest)
            {
                if (TempleOnLeft)
                {
                    if (!InTemple && exitLeft)
                    {
                        ToTemple();
                        //switch to temple
                    }
                    else if (InTemple && !exitLeft)
                    {
                        ToOther();
                        //switch to other
                    }
                }
                else
                {
                    if (InTemple && exitLeft)
                    {
                        ToOther();
                        //switch to other
                    }
                    else if (!InTemple && !exitLeft)
                    {
                        ToTemple();
                        //switch to temple
                    }
                }
            }
        }
    }
}
