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
        private Action onEnd;
        private Action onStart;
        private IEnumerator onPressedRoutine;
        private bool isRoutine;
        private Coroutine routine;
        private Keys key;
        private bool onlyInRender;
        public bool OncePerPress;
        public bool InRoutine => isRoutine && onPressedRoutine != null && routine != null && routine.Finished;
        public DebugComponent(Keys key, Action onPressed, bool oncePerPress, bool onlyInRender = false) : base(true, true)
        {
            this.onPressed = onPressed;
            this.key = key;
            this.onlyInRender = onlyInRender;
            OncePerPress = oncePerPress;
        }
        private bool wasPressed;
        private bool isPressed;
        public DebugComponent(Keys key, Action onHeld, Action onPressed, Action onReleased, bool onlyInRender = false) : base(true, true)
        {
            this.onPressed = onPressed;
            this.key = key;
            this.onlyInRender = onlyInRender;
            OncePerPress = false;
            this.onPressed = onHeld;
            this.onStart = onPressed;
            this.onEnd = onReleased;
        }
        public DebugComponent(Keys key, IEnumerator onPressed) : base(true, true)
        {
            this.key = key;
            routine = new Coroutine(onPressedRoutine = onPressed, false);
            isRoutine = true;
        }
        public bool GetKeyState()
        {
            return key != 0 && (OncePerPress || isRoutine ? MInput.Keyboard.Pressed(key) : MInput.Keyboard.CurrentState.IsKeyDown(key));
        }
        public override void Update()
        {
            base.Update();
            wasPressed = isPressed;
            isPressed = GetKeyState();
            if (!onlyInRender)
            {
                DoActions(false);
            }
        }
        public void DoActions(bool render)
        {
            if (Entity is null || InRoutine) return;
            if (isRoutine && !render)
            {
                routine.Replace(onPressedRoutine);
                Entity.Add(routine);
            }
            else
            {
                if (!wasPressed && isPressed)
                {
                    onStart?.Invoke();
                }
                if (wasPressed && !isPressed)
                {
                    onEnd?.Invoke();
                    return;
                }
                if (isPressed && !(OncePerPress && wasPressed))
                {
                    onPressed?.Invoke();
                }
            }

        }
        public override void Render()
        {
            base.Render();
            if (onlyInRender)
            {
                DoActions(true);
            }
        }
    }
}