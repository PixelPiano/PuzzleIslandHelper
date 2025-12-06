using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class KeyComponent : Component
    {
        public static KeyComponent Left(object function, bool wasd = false)
        {
            return new KeyComponent(function, wasd ? Keys.A : Keys.Left);
        }
        public static KeyComponent Right(object function, bool wasd = false)
        {
            return new KeyComponent(function, wasd ? Keys.D : Keys.Right);
        }
        public static KeyComponent Up(object function, bool wasd = false)
        {
            return new KeyComponent(function, wasd ? Keys.W : Keys.Up);
        }
        public static KeyComponent Down(object function, bool wasd = false)
        {
            return new KeyComponent(function, wasd ? Keys.S : Keys.Down);
        }
        public static KeyComponent Space(object function)
        {
            return new KeyComponent(function, Keys.Space);
        }
        public static KeyComponent Bool(Entity entity, string nameofValue, bool? value, params Keys[] keys)
        {
            KeyComponent component = new KeyComponent(nameofValue, value, false, keys);
            entity?.Add(component);
            return component;
        }
        public static KeyComponent[] Bool(Entity entity, string nameofValue, Keys? True = null, Keys? False = null, Keys? Invert = null)
        {
            List<KeyComponent> list = [];
            if (True.HasValue)
            {
                list.Add(Bool(entity, nameofValue, true, True.Value));
            }
            if (False.HasValue)
            {
                list.Add(Bool(entity, nameofValue, false, False.Value));
            }
            if (Invert.HasValue)
            {
                list.Add(Bool(entity, nameofValue, null, Invert.Value));
            }
            return [.. list];
        }
        public static KeyComponent[] Float(Entity entity, string nameofValue, float value, bool add, Keys increment, Keys decrement)
        {
            KeyComponent[] a = [new KeyComponent(nameofValue, value, add, increment), new KeyComponent(nameofValue, -value, add, decrement)];
            entity?.Add(a);
            return a;
        }
        public static KeyComponent[] Arrows(object left = null, object up = null, object right = null, object down = null)
        {
            return [Left(left), Up(up), Right(right), Down(down)];
        }
        public static KeyComponent[] Wasd(object left = null, object up = null, object right = null, object down = null)
        {
            return [Left(left, true), Up(up, true), Right(right, true), Down(down, true)];
        }
        public static KeyComponent ForMousePosition(Entity entity, Action<Vector2> onMove, bool onlyInRender = false)
        {
            KeyComponent dc = new()
            {
                type = inputType.mousePosition,
                OnlyInRender = onlyInRender,
                OnMouse = onMove,
                OncePerInput = true,
            };
            entity?.Add(dc);
            return dc;
        }
        public static KeyComponent ForScrollWheel(Entity entity, Action<int> onScroll, bool onlyInRender = false)
        {
            KeyComponent dc = new()
            {
                type = inputType.scrollWheel,
                OnlyInRender = onlyInRender,
                OnScroll = onScroll,
                OncePerInput = true,
            };
            entity?.Add(dc);
            return dc;
        }
        public static KeyComponent ForKey(Entity entity, Keys key, Action onPressed, bool onlyInRender = false, Action onHeld = null, Action onReleased = null)
        {
            KeyComponent dc = new()
            {
                type = inputType.key,
                componentKeys = [key],
                OnlyInRender = onlyInRender,
                OnPressed = onPressed,
                OnHeld = onHeld,
                OnRelease = onReleased,
                OncePerInput = onHeld == null
            };
            entity?.Add(dc);
            return dc;
        }
        public Action OnPressed;
        public Action OnRelease;
        public Action OnHeld;
        public Action<int> OnScroll;
        public Action<Vector2> OnMouse;
        public IEnumerator OnPressedRoutine;
        private Coroutine routine;
        private HashSet<Keys> componentKeys = [];
        public enum KeyCheckModes
        {
            Any,
            All
        }
        private enum inputType
        {
            mousePosition,
            scrollWheel,
            key
        }
        private inputType type;
        public KeyCheckModes KeyCheckMode;
        public bool DebugOnly;
        private bool isRoutine => OnPressedRoutine != null;
        public bool OnlyInRender;
        public bool OncePerInput = true;
        public bool InRoutine => isRoutine && routine != null && !routine.Finished;
        private bool wasActive;
        private bool isActive;
        private float prevScroll;
        private Vector2 prevPosition;
        private MouseState mouse;
        public KeyComponent() : base(true, true)
        {

        }
        public string NameOfValue;

        public void ChangeValue()
        {
            var data = DynamicData.For(Entity);
            Engine.Commands.Log("aaa");
            Assembly assembly = typeof(PianoModule).Assembly;
            Type[] types = assembly.GetTypesSafe();
            int numMult = AddToValue ? 1 : 0;

            Type type = Entity.GetType();
            var fields = type.GetFields();
            foreach (var f in fields)
            {
                if (f.Name == NameOfValue)
                {
                    object? setTo = null;

                    var v = f.GetValue(Entity);
                    if (Value is float && v is float)
                    {
                        setTo = (float)Value + (float)v * numMult;
                    }
                    else if (Value is int && v is int)
                    {
                        setTo = (int)Value + (int)v * numMult;
                    }
                    else if (Value is string && v is string)
                    {
                        if (numMult != 0)
                        {
                            setTo = (string)v + (string)Value;
                        }
                        else
                        {
                            setTo = (string)Value;
                        }
                    }
                    else if (v is bool)
                    {
                        if (Value == null)
                        {
                            setTo = !(bool)v;
                        }
                        else
                        {
                            setTo = (bool)Value;
                        }
                    }
                    else if (Value is double && v is double)
                    {
                        setTo = (double)Value + (double)v * numMult;
                    }
                    else
                    {
                        setTo = Value;
                    }
                    if (setTo != null)
                    {
                        try
                        {
                            f.SetValue(Entity, setTo);
                        }
                        catch (Exception e)
                        {
                            Engine.Commands.Log(string.Format("Could not modify value \"{0}\" of Entity \"{1}\".", NameOfValue, type.Name));
                        }
                    }
                    break;
                }
            }
        }
        public KeyComponent(object function, params Keys[] keys) : base(true, true)
        {
            type = inputType.key;
            componentKeys = [.. keys];
            if (function is Action action)
            {
                OnPressed = action;
            }
            else if (function is IEnumerator enumerator)
            {
                routine = new Coroutine(OnPressedRoutine = enumerator, false);
            }
        }
        public object Value;
        public bool AddToValue;
        public KeyComponent(string valueName, object modifyBy, bool add, params Keys[] keys) : base(true, true)
        {
            NameOfValue = valueName;
            Value = modifyBy;
            type = inputType.key;
            componentKeys = [.. keys];
            OnPressed = ChangeValue;
            AddToValue = add;
        }
        public KeyComponent(Action onHeld, Action onPressed, Action onReleased, params Keys[] keys) : base(true, true)
        {
            type = inputType.key;
            OnPressed = onPressed;
            componentKeys = [.. keys];
            OncePerInput = false;
            OnPressed = onHeld;
            OnHeld = onPressed;
            OnRelease = onReleased;
        }
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
                            foreach (Keys k in componentKeys)
                            {
                                if (k != 0 && MInput.Keyboard.CurrentState.IsKeyDown(k))
                                {
                                    return true;
                                }
                            }
                            return false;
                        case KeyCheckModes.All:
                            foreach (Keys k in componentKeys)
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
                            foreach (Keys k in componentKeys)
                            {
                                if (k != 0 && MInput.Keyboard.Pressed(k))
                                {
                                    return true;
                                }
                            }
                            return false;
                        case KeyCheckModes.All:
                            bool oneJustPressed = false;
                            foreach (Keys k in componentKeys)
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
            return detected && (type != inputType.key || componentKeys.Any(item => item != 0));
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            mouse = Mouse.GetState();
            prevPosition = new Vector2(mouse.X, mouse.Y);
            prevScroll = mouse.ScrollWheelValue;
            EntityType = entity.GetType();
        }
        public Type EntityType;
        public override void Update()
        {
            base.Update();
            if (DebugOnly && !(GameplayRenderer.RenderDebug || Engine.Commands.Open))
            {
                wasActive = false;
                isActive = false;
                prevPosition = default;
                prevScroll = default;
                return;
            }
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
            if (DebugOnly && !(GameplayRenderer.RenderDebug || Engine.Commands.Open))
            {
                return;
            }
            base.Render();
            if (OnlyInRender)
            {
                DoActions();
            }
        }
    }
}