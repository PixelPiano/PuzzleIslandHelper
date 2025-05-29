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
        private Action onRelease;
        private Action onHeld;
        private Action<int> onScroll;
        private Action<Vector2> onMouse;
        private IEnumerator onPressedRoutine;
        private bool isRoutine;
        private Coroutine routine;
        private Keys key;
        private enum inputType
        {
            mousePosition,
            scrollWheel,
            key
        }
        private inputType type;
        private bool onlyInRender;
        public bool OncePerInput;
        public bool InRoutine => isRoutine && onPressedRoutine != null && routine != null && routine.Finished;

        public static DebugComponent ForMousePosition(Entity entity, Action<Vector2> onMove, bool onlyInRender = false)
        {
            DebugComponent dc = new()
            {
                type = inputType.mousePosition,
                onlyInRender = onlyInRender,
                onMouse = onMove,
                OncePerInput = true,
            };
            entity?.Add(dc);
            return dc;
        }
        public static DebugComponent ForScrollWheel(Entity entity, Action<int> onScroll, bool onlyInRender = false)
        {
            DebugComponent dc = new()
            {
                type = inputType.scrollWheel,
                onlyInRender = onlyInRender,
                onScroll = onScroll,
                OncePerInput = true,
            };
            entity?.Add(dc);
            return dc;
        }
        public static DebugComponent ForKey(Entity entity, Keys key, Action onPressed, bool onlyInRender = false, Action onHeld = null, Action onReleased = null)
        {
            DebugComponent dc = new()
            {
                type = inputType.key,
                key = key,
                onlyInRender = onlyInRender,
                onPressed = onPressed,
                onHeld = onHeld,
                onRelease = onReleased,
                OncePerInput = onHeld == null
            };
            entity?.Add(dc);
            return dc;
        }
        public DebugComponent() : base(true, true)
        {

        }
        public DebugComponent(Keys key, Action onPressed, bool oncePerPress, bool onlyInRender = false) : base(true, true)
        {
            type = inputType.key;
            this.onPressed = onPressed;
            this.key = key;
            this.onlyInRender = onlyInRender;
            OncePerInput = oncePerPress;
        }
        private bool wasActive;
        private bool isActive;
        public DebugComponent(Keys key, Action onHeld, Action onPressed, Action onReleased, bool onlyInRender = false) : base(true, true)
        {
            type = inputType.key;
            this.onPressed = onPressed;
            this.key = key;
            this.onlyInRender = onlyInRender;
            OncePerInput = false;
            this.onPressed = onHeld;
            this.onHeld = onPressed;
            this.onRelease = onReleased;
        }
        public DebugComponent(Keys key, IEnumerator onPressed) : base(true, true)
        {
            type = inputType.key;
            this.key = key;
            routine = new Coroutine(onPressedRoutine = onPressed, false);
            isRoutine = true;
        }
        private Vector2 prevPosition;
        private float prevScroll;
        private MouseState mouse;
        public bool InputDetected()
        {
            switch (type)
            {
                case inputType.mousePosition:
                    return new Vector2(mouse.X, mouse.Y) != prevPosition;
                case inputType.scrollWheel:
                    return mouse.ScrollWheelValue != prevScroll;
                case inputType.key:
                    return MInput.Keyboard.CurrentState.IsKeyDown(key);
            }
            return false;
        }
        public bool NewInputDetected()
        {
            switch (type)
            {
                case inputType.mousePosition:
                    return new Vector2(mouse.X, mouse.Y) != prevPosition;
                case inputType.scrollWheel:
                    return mouse.ScrollWheelValue != prevScroll;
                case inputType.key:
                    return MInput.Keyboard.Pressed(key);
            }
            return false;
        }
        public bool GetState()
        {
            bool detected = (OncePerInput || isRoutine ? NewInputDetected() : InputDetected());
            return detected && (type != inputType.key || key != 0);
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            mouse = Mouse.GetState();
            prevPosition = new Vector2(mouse.X, mouse.Y);
            prevScroll = mouse.ScrollWheelValue;
        }
        public override void Update()
        {
            base.Update();
            mouse = Mouse.GetState();

            wasActive = isActive;
            isActive = GetState();
            if (!onlyInRender)
            {
                DoActions();
            }
            prevPosition = new Vector2(mouse.X, mouse.Y);
            prevScroll = mouse.ScrollWheelValue;
        }
        public void DoActions()
        {
            if (Entity is null || InRoutine) return;
            if (isRoutine && !onlyInRender)
            {
                routine.Replace(onPressedRoutine);
                Entity.Add(routine);
            }
            else
            {
                if (type == inputType.key)
                {
                    if (!wasActive && isActive)
                    {
                        onHeld?.Invoke();
                    }

                    if (wasActive && !isActive)
                    {
                        onRelease?.Invoke();
                        return;
                    }
                    if (isActive && !(OncePerInput && wasActive))
                    {
                        onPressed?.Invoke();
                    }
                }
                else
                {
                    if (type == inputType.mousePosition)
                    {
                        onMouse.Invoke(new Vector2(mouse.X, mouse.Y));
                    }
                    else
                    {
                        onScroll.Invoke(mouse.ScrollWheelValue);
                    }
                }
            }
        }
        public override void Render()
        {
            base.Render();
            if (onlyInRender)
            {
                DoActions();
            }
        }
    }
}