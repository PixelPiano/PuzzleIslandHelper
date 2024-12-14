using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class DebugComponent : Component
    {
        private Action onPressed;
        private IEnumerator onPressedRoutine;
        private bool isRoutine;
        private Coroutine routine;
        private Keys key;
        private const float buffer = 0.8f;
        private float timer = buffer;
        private bool onlyInRender;
        private bool waitingForRelease;

        private bool oncePerPress;

        private bool ModeState
        {
            get
            {
                if (key == 0) return false;
                if (OncePerPress || isRoutine) return MInput.Keyboard.Pressed(key);
                else return MInput.Keyboard.CurrentState.IsKeyDown(key);
            }
        }
        public bool OncePerPress;
        public bool InRoutine => isRoutine && onPressedRoutine != null && routine != null && routine.Finished;
        public DebugComponent(Keys key, Action onPressed, bool oncePerPress, bool onlyInRender = false) : base(true, true)
        {
            this.onPressed = onPressed;
            this.key = key;
            this.onlyInRender = onlyInRender;
            OncePerPress = oncePerPress;
        }
        public DebugComponent(Keys key, IEnumerator onPressed) : base(true, true)
        {
            this.key = key;
            routine = new Coroutine(onPressedRoutine = onPressed, false);
            isRoutine = true;
        }
        public override void Update()
        {
            base.Update();
            if (!onlyInRender && ModeState)
            {
                Pressed();
            }

        }
        public override void Render()
        {
            base.Render();
            if (onlyInRender && ModeState)
            {
                if(Entity is not null)
                {
                    Draw.HollowRect(Entity.Collider, Color.Cyan);
                }
                Pressed();
            }

        }
        public void Pressed()
        {
            if (Entity is null || InRoutine) return;
            if (isRoutine)
            {
                routine.Replace(onPressedRoutine);
                Entity.Add(routine);
            }
            else
            {
                onPressed?.Invoke();
            }

        }
    }
}