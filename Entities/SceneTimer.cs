using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked(false)]
    public class SceneTimer : Entity
    {
        public SceneTimer(float time, Action onEnd)
        {
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, onEnd, time, true));
        }
    }
}