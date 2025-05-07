using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [TrackedAs(typeof(Alarm))]
    public class ConditionAlarm : Alarm
    {
        public Func<bool> Condition;
        public static ConditionAlarm Create(AlarmMode mode, Func<bool> condition, Action onComplete, float duration = 1f, bool start = false)
        {
            ConditionAlarm alarm = new ConditionAlarm();
            alarm.Init(mode, onComplete, duration, start);
            alarm.Condition = condition;
            return alarm;
        }

        public static ConditionAlarm Set(Entity entity, float duration, Action onComplete, Func<bool> condition, AlarmMode alarmMode = AlarmMode.Oneshot)
        {
            ConditionAlarm alarm = Create(alarmMode, condition, onComplete, duration, start: true);
            entity.Add(alarm);
            return alarm;
        }
        public override void Update()
        {
            if (Condition == null || Condition.Invoke())
            {
                base.Update();
            }
        }

    }
}
