using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class MouseComponent : Component
    {
        public Action OnLeftClick;
        public Action OnRightClick;
        public Action OnLeftRelease;
        public Action OnRightRelease;
        public Action OnLeftHeld;
        public Action OnRightHeld;
        public Action OnLeftIdle;
        public Action OnRightIdle;
        private bool lastLeftClicked, lastRightClicked;
        public bool LeftClicked, RightClicked;
        public bool JustLeftClicked => LeftClicked && !lastLeftClicked;
        public bool JustRightClicked => RightClicked && !lastRightClicked;
        public bool JustLeftReleased => !LeftClicked && lastLeftClicked;
        public bool JustRightReleased => !RightClicked && lastRightClicked;
        public Vector2 WorldPosition
        {
            get
            {
                if (Scene is not Level level)
                {
                    return Vector2.Zero;
                }
                return level.Camera.Position + MousePosition / 6;
            }
        }
        public Vector2 MousePosition
        {
            get
            {
                MouseState mouseState = State;
                float mouseX = Calc.Clamp(mouseState.X, 0, Engine.ViewWidth);
                float mouseY = Calc.Clamp(mouseState.Y, 0, Engine.ViewHeight);
                float scale = (float)Engine.Width / Engine.ViewWidth;
                Vector2 position = new Vector2(mouseX, mouseY) * scale;
                return position;
            }
        }
        public MouseState State;
        public MouseComponent(bool active, bool visible) : base(active, visible)
        {

        }
        public MouseComponent(Action onLeftClick = null, Action onRightClick = null, Action onLeftRelease = null, Action onRightRelease = null, Action onLeftIdle = null, Action onRightIdle = null, Action onLeftHeld = null, Action onRightHeld = null) : base(true, true)
        {
            OnLeftClick = onLeftClick;
            OnRightClick = onRightClick;
            OnLeftRelease = onLeftRelease;
            OnRightRelease = onRightRelease;
            OnLeftIdle = onLeftIdle;
            OnRightIdle = onRightIdle;
            OnLeftHeld = onLeftHeld;
            OnRightHeld = onRightHeld;
        }
        public override void Update()
        {
            base.Update();
            if (Engine.Instance.IsActive)
            {
                State = Mouse.GetState();
                lastLeftClicked = LeftClicked;
                lastRightClicked = RightClicked;
                LeftClicked = State.LeftButton.Equals(ButtonState.Pressed);
                RightClicked = State.RightButton.Equals(ButtonState.Pressed);

                if (LeftClicked)
                {
                    if (!lastLeftClicked)
                    {
                        OnLeftClick?.Invoke();
                    }
                    OnLeftHeld?.Invoke();
                }
                else
                {
                    if (lastLeftClicked)
                    {
                        OnLeftRelease?.Invoke();
                    }
                    OnLeftIdle?.Invoke();
                }

                if (RightClicked)
                {
                    if (!lastRightClicked)
                    {
                        OnRightClick?.Invoke();
                    }
                    OnRightHeld?.Invoke();
                }
                else
                {
                    if (lastRightClicked)
                    {
                        OnRightRelease?.Invoke();
                    }
                    OnRightIdle?.Invoke();
                }
            }
        }
    }
}
