﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [Tracked]
    public class Fader : Entity
    {
        public Color Color;
        public readonly Color From;
        public readonly Color To;
        public float Time;
        public float EndDelay;
        public Ease.Easer Easer;
        public Action OnEnd;
        private Coroutine routine;
        public bool Finished;
        public Fader(Color from, Color to, float time, Ease.Easer ease = null, Action onEnd = null, bool persistent = false) : base()
        {
            Depth = -100001;
            From = from;
            To = to;
            OnEnd = onEnd;
            Easer = ease ?? Ease.Linear;
            Time = time;
            routine = new Coroutine(false) { UseRawDeltaTime = true };
            Add(routine);
            if (persistent)
            {
                AddTag(Tags.Persistent);
                AddTag(Tags.Global);
            }
            Start();
        }
        public Fader() : base()
        {
            Depth = -100001;
            routine = new Coroutine(false) { UseRawDeltaTime = true };
            Add(routine);
        }
        private IEnumerator FadeTo(Color from, Color to)
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / Time)
            {
                Color = Color.Lerp(from, to, Easer(i));
                yield return null;
            }
            OnEnd?.Invoke();
            Finished = true;
        }
        public void Start(bool snap = true)
        {
            if (snap)
            {
                Color = From;
            }
            Finished = false;
            routine.Replace(FadeTo(Color, To));
        }
        public void Reverse(bool snap = true)
        {
            if (snap)
            {
                Color = To;
            }
            Finished = false;
            routine.Replace(FadeTo(Color, From));
        }
        public static IEnumerator Fade(Color color, float time, Ease.Easer ease = null, Action onEnd = null, bool removeOnComplete = true, bool persistent = false)
        {
            yield return Fade(Color.Transparent, color, time, ease, onEnd, removeOnComplete);
        }
        public static IEnumerator Fade(Color from, Color to, float time, Ease.Easer ease = null, Action onEnd = null, bool removeOnComplete = true)
        {
            if (Engine.Scene is Level level)
            {
                Fader fader = new Fader(from, to, time, ease, onEnd);
                level.Add(fader);
                while (!fader.Finished)
                {
                    yield return null;
                }
                if (removeOnComplete)
                {
                    level.Remove(fader);
                }
            }
        }
        public static IEnumerator FadeInOut(Color from, Color to, float time, float delay, Action onWait = null,Action onEnd = null, Ease.Easer ease = null, bool persistent = true)
        {
            if (Engine.Scene is Level level)
            {
                Fader fader = new Fader(from, to, time, ease, onWait, persistent);
                level.Add(fader);
                while (!fader.Finished)
                {
                    yield return null;
                }
                onWait?.Invoke();
                yield return delay;
                fader.OnEnd = onEnd;
                fader.Reverse(false);
                while (!fader.Finished)
                {
                    yield return null;
                }
                level.Remove(fader);

            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is Level level && Color != Color.Transparent)
            {
                Draw.Rect(level.Camera.Position, 320, 180, Color);
            }
        }
    }
}
