using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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