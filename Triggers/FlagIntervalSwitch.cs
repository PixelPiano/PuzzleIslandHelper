using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/FlagIntervalSwitch")]
    [Tracked]
    public class FlagIntervalSwitch : Trigger
    {
        private string flag;
        private string[] flags;
        private float interval;
        private bool repeat;
        private bool invertOnRepeat;
        private bool oneAtATime;
        private bool state;
        private float endWaitTime;
        public enum ActivationTypes
        {
            OnLevelStart,
            OnEnter,
            OnLeave
        }
        private ActivationTypes type;
        public FlagIntervalSwitch(EntityData data, Vector2 offset) : base(data, offset)
        {
            flag = data.Attr("flag");
            flags = data.Attr("intervalFlags").Replace(" ", "").Split(',');
            interval = data.Float("interval");
            repeat = data.Bool("repeatOnEnd");
            invertOnRepeat = data.Bool("invertOnRepeat");
            state = data.Bool("intervalFlagState");
            oneAtATime = data.Bool("oneAtATime");
            endWaitTime = data.Float("endWaitTime");
            type = data.Enum<ActivationTypes>("activationMethod");
        }
        private IEnumerator Routine(Level level)
        {
            while (!string.IsNullOrEmpty(flag) && !level.Session.GetFlag(flag))
            {
                yield return null;
            }
            while (true)
            {
                if (flags.Length == 0)
                {
                    break;
                }
                foreach (string flag in flags)
                {
                    level.Session.SetFlag(flag, state);
                    if (!oneAtATime)
                    {
                        foreach (string flag2 in flags)
                        {
                            if (flag2 != flag)
                            {
                                level.Session.SetFlag(flag, !state);
                            }
                        }
                    }
                    if (interval > 0)
                    {
                        yield return interval;
                    }
                }
                if (repeat)
                {
                    if (invertOnRepeat)
                    {
                        state = !state;
                    }
                    yield return endWaitTime;
                }
                else
                {
                    yield return null;
                    break;
                }
            }

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (type == ActivationTypes.OnLevelStart)
            {
                Start(scene);
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (type == ActivationTypes.OnEnter)
            {
                Start(Scene);
            }
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (type == ActivationTypes.OnLeave)
            {
                Start(Scene);
            }
        }
        private void Start(Scene scene)
        {
            Add(new Coroutine(Routine(scene as Level)));
        }
    }
}