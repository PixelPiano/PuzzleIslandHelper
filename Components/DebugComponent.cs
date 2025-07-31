using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class DebugComponent : Component
    {
        public Action OnPressed;
        public Action OnRelease;
        public Action OnHeld;
        public Action<int> OnScroll;
        public Action<Vector2> OnMouse;
        public IEnumerator OnPressedRoutine;
        private bool isRoutine => OnPressedRoutine != null;
        private Coroutine routine;
        private HashSet<Keys> Keys = [];
        public enum KeyCheckModes
        {
            Any,
            All
        }
        public KeyCheckModes KeyCheckMode;
        private enum inputType
        {
            mousePosition,
            scrollWheel,
            key
        }
        private inputType type;
        public bool OnlyInRender;
        public bool OncePerInput = true;
        public bool InRoutine => isRoutine && routine != null && !routine.Finished;
        private bool wasActive;
        private bool isActive;
        public static DebugComponent ForMousePosition(Entity entity, Action<Vector2> onMove, bool onlyInRender = false)
        {
            DebugComponent dc = new()
            {
                type = inputType.mousePosition,
                OnlyInRender = onlyInRender,
                OnMouse = onMove,
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
                OnlyInRender = onlyInRender,
                OnScroll = onScroll,
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
                Keys = [key],
                OnlyInRender = onlyInRender,
                OnPressed = onPressed,
                OnHeld = onHeld,
                OnRelease = onReleased,
                OncePerInput = onHeld == null
            };
            entity?.Add(dc);
            return dc;
        }
        public DebugComponent() : base(true, true)
        {

        }
        public DebugComponent(object function, params Keys[] keys) : base(true, true)
        {
            type = inputType.key;
            Keys = [.. keys];
            if (function is Action action)
            {
                OnPressed = action;
            }
            else if (function is IEnumerator enumerator)
            {
                routine = new Coroutine(OnPressedRoutine = enumerator, false);
            }
        }
        public DebugComponent(Action onHeld, Action onPressed, Action onReleased, params Keys[] keys) : base(true, true)
        {
            type = inputType.key;
            OnPressed = onPressed;
            Keys = [.. keys];
            OncePerInput = false;
            OnPressed = onHeld;
            OnHeld = onPressed;
            OnRelease = onReleased;
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
                    switch (KeyCheckMode)
                    {
                        case KeyCheckModes.Any:
                            foreach (Keys k in Keys)
                            {
                                if (k != 0 && MInput.Keyboard.CurrentState.IsKeyDown(k))
                                {
                                    return true;
                                }
                            }
                            return false;
                        case KeyCheckModes.All:
                            foreach (Keys k in Keys)
                            {
                                if (k != 0 && !MInput.Keyboard.CurrentState.IsKeyDown(k))
                                {
                                    return false;
                                }
                            }
                            return true;
                    }
                    break;
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
                    switch (KeyCheckMode)
                    {
                        case KeyCheckModes.Any:
                            foreach (Keys k in Keys)
                            {
                                if (k != 0 && MInput.Keyboard.Pressed(k))
                                {
                                    return true;
                                }
                            }
                            return false;
                        case KeyCheckModes.All:
                            bool oneJustPressed = false;
                            foreach (Keys k in Keys)
                            {
                                if (k != 0)
                                {
                                    if (MInput.Keyboard.Pressed(k))
                                    {
                                        oneJustPressed = true;
                                        continue;
                                    }
                                    if (!MInput.Keyboard.CurrentState.IsKeyDown(k))
                                    {
                                        return false;
                                    }
                                }
                            }
                            return oneJustPressed;
                    }
                    break;

            }
            return false;
        }
        public bool GetState()
        {
            bool detected = (OncePerInput || isRoutine ? NewInputDetected() : InputDetected());
            return detected && (type != inputType.key || Keys.Any(item => item != 0));
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
            if (!OnlyInRender)
            {
                DoActions();
            }
            prevPosition = new Vector2(mouse.X, mouse.Y);
            prevScroll = mouse.ScrollWheelValue;
        }
        public void DoActions()
        {
            if (Entity is null || InRoutine) return;
            if (isRoutine && !OnlyInRender)
            {
                routine.Replace(OnPressedRoutine);
                Entity.Add(routine);
            }
            else
            {
                if (type == inputType.key)
                {
                    if (!wasActive && isActive)
                    {
                        OnHeld?.Invoke();
                    }

                    if (wasActive && !isActive)
                    {
                        OnRelease?.Invoke();
                        return;
                    }
                    if (isActive && !(OncePerInput && wasActive))
                    {
                        OnPressed?.Invoke();
                    }
                }
                else
                {
                    if (type == inputType.mousePosition)
                    {
                        OnMouse.Invoke(new Vector2(mouse.X, mouse.Y));
                    }
                    else
                    {
                        OnScroll.Invoke(mouse.ScrollWheelValue);
                    }
                }
            }
        }
        public override void Render()
        {
            base.Render();
            if (OnlyInRender)
            {
                DoActions();
            }
        }
    }
}