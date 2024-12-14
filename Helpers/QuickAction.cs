using System;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Helpers
{
    public class QuickAction : Entity
    {
        public float Time;
        public Action OnStart;
        public Action OnEnd;

        public QuickAction(float time, Action onStart, Action onEnd) : base()
        {
            Time = time;
            OnStart = onStart;
            OnEnd = onEnd;
        }
        public void Start()
        {
            Alarm alarm = Alarm.Create(Alarm.AlarmMode.Oneshot, End, Time, true);
            Add(alarm);
        }
        public void End()
        {
            OnEnd?.Invoke();
            RemoveSelf();
        }
        public static void DoAction(float time, Action onStart, Action onEnd)
        {
            if(Engine.Scene is not Level level) return;
            QuickAction action = new(time, onStart, onEnd);
            level.Add(action);
            action.Start();
        }
    }
}
