using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [Tracked]
    public class ElderLock : Entity
    {
        public class Button : Component
        {
            public enum States
            {
                Unpressed = 3,
                Pressed = 1,
                PressedIdle = 2,
            }
            public void RevertState()
            {
                State = StateBeforePressed;
            }
            public void ResetColor()
            {
                Color = Color.White;
            }
            public void Reset(Color? color = null)
            {
                if (State == States.Pressed)
                {
                    State = StateBeforePressed;
                }
                Color = color ?? Color.White;
            }
            public States State = States.Unpressed;
            public States StateBeforePressed;
            public Coroutine Coroutine;
            public int Row, Column;
            public bool AutoUnpress;
            public float AutoUnpressDelay;
            private float autoUnpressTimer;
            private float selectedAlpha = 0.3f;
            private float selectedAlphaEase;
            private int outlines => (int)State;
            public bool Selected => (Entity as ElderLock).Selected == this;
            public Color OutlineColor = Color.Magenta;
            public Color Color = Color.White;
            public MTexture Texture => GFX.Game[path + id + State.ToString()];
            public Vector2 Position;
            public Action OnPressed, OnUnpressed;
            private string path = "objects/PuzzleIslandHelper/elderPadlock/";
            private string id;
            public Rectangle Bounds
            {
                get
                {
                    Rectangle r = default;
                    r.X = (int)Position.X;
                    r.Y = (int)Position.Y;
                    r.Width = Texture.Width;
                    r.Height = Texture.Height;
                    return r;
                }
            }
            public Button(int row, int column, string id) : base(true, false)
            {
                this.id = id;
                Row = row;
                Column = column;
            }
            public bool Colliding(Vector2 mouse)
            {
                return Bounds.Contains(mouse);
            }
            public virtual void Press()
            {
                State = States.PressedIdle;
                OnPressed?.Invoke();
                autoUnpressTimer = AutoUnpressDelay;
            }
            public virtual void Unpress()
            {
                State = States.Unpressed;
                OnUnpressed?.Invoke();
                autoUnpressTimer = 0;
            }
            public override void Render()
            {
                Texture?.DrawOutline(Position, Vector2.Zero, Color, 1, 0);
            }
            public void DrawOutlines()
            {
                return;
                if (Texture != null && selectedAlpha > 0)
                {
                    Rectangle r = Bounds;
                    float inc = 0.8f / outlines;
                    float alpha = 0.8f - (inc / 2);
                    for (int i = outlines; i > 0; i--)
                    {
                        SubHUDHollowRect(r.X - i * 6, r.Y - i * 6, r.Width + i * 6 * 2, r.Height + i * 6 * 2, OutlineColor * alpha * selectedAlpha, 6);
                        alpha -= inc;
                    }
                }
            }
            public void SubHUDHollowRect(float x, float y, float width, float height, Color color, int thickness)
            {
                Draw.Rect(x - thickness, y - thickness, width + thickness * 2, thickness, color);
                Draw.Rect(x - thickness, y + height, width + thickness * 2, thickness, color);
                Draw.Rect(x - thickness, y, thickness, height, color);
                Draw.Rect(x + width, y, thickness, height, color);
            }
            public override void Update()
            {
                if (AutoUnpress && State == States.PressedIdle)
                {
                    if (autoUnpressTimer > 0)
                    {
                        autoUnpressTimer -= Engine.DeltaTime;
                    }
                    if (autoUnpressTimer <= 0)
                    {
                        autoUnpressTimer = 0;
                        Unpress();
                    }
                }
                if (Selected)
                {
                    selectedAlphaEase = Calc.Approach(selectedAlphaEase, 1, Engine.DeltaTime * 3);
                    selectedAlpha = Calc.Approach(selectedAlpha, 1, Engine.DeltaTime * 5 * selectedAlphaEase);
                }
                else
                {
                    selectedAlphaEase = 0f;
                    selectedAlpha = Calc.Approach(selectedAlpha, 0.3f, Engine.DeltaTime);
                }
            }
        }
        public string[] Combination =
        {
            "0100010",
            "1101011",
            "0100010",
            "0011100",
            "1001001",
            "0100010",
            "0010100",
            "0001000",
            "E,,C,,S"
        };
        public int Rows;
        public int Columns;
        public VirtualRenderTarget Target;
        public VirtualRenderTarget PadlockTarget;
        public VirtualRenderTarget LeftTarget, RightTarget;
        private float lerp;
        private float blackAlpha = 0;
        public VirtualMap<Button> Buttons;
        public Vector2 PadlockPosition;
        public float PadlockWidth, PadlockHeight, PadlockRenderHeight;
        public bool InControl;
        public bool UsingMouse;
        private Vector2 previousMousePosition;
        public Button Selected;
        public Color SelectedColor = Color.Pink;
        public Button PreviousSelected;
        private float moveBuffer;
        public bool Running = true;
        private bool solveOnSubmitUnpressed;
        public bool Solved;
        public bool ConfirmedExit;
        private float iconAlpha;
        private float selectAlpha;
        private bool mouseWasPressed;
        private bool mousePressed;
        private bool padWasPressed;
        private bool padPressed;
        public bool IsSplit;
        private Vector2 lastMoveDir;
        private Vector2 moveDir;
        public bool RenderedSplit;
        private float splitRotation;
        private float splitFall;
        public ElderLock() : base()
        {
            Tag |= TagsExt.SubHUD;
            PadlockTarget = VirtualContent.CreateRenderTarget("ElderLock:Padlock", Engine.Width, Engine.Height);
            LeftTarget = VirtualContent.CreateRenderTarget("ElderLock:LeftTarget", Engine.Width / 2, Engine.Height);
            RightTarget = VirtualContent.CreateRenderTarget("ElderLock:RightTarget", Engine.Width / 2, Engine.Height);
            Target = VirtualContent.CreateRenderTarget("ElderLock:Target", Engine.Width, Engine.Height);
            Add(new BeforeRenderHook(() =>
            {
                if (!IsSplit)
                {
                    PadlockTarget.SetAsTarget(true);
                    Draw.SpriteBatch.Begin();
                    Draw.Rect(PadlockPosition, PadlockWidth, PadlockRenderHeight, Color.DarkSlateGray);
                    for (int x = 0; x < Columns; x++)
                    {
                        for (int y = 0; y < Rows; y++)
                        {
                            Buttons[x, y]?.Render();
                        }
                    }
                    Draw.SpriteBatch.End();

                    Target.SetAsTarget(Color.Black * blackAlpha); //dim the level around
                    Draw.SpriteBatch.Begin();
                    Vector2 origin = PadlockTarget.HalfSize();
                    Draw.SpriteBatch.Draw(PadlockTarget, origin + Vector2.UnitY * (Engine.Height * 1.5f) * (1 - lerp), null, Color.White, MathHelper.PiOver2 * (1 - lerp), origin, 1, SpriteEffects.None, 0); //draw padlock stuff
                    if (UsingMouse && iconAlpha > 0)
                    {
                        string path = "objects/PuzzleIslandHelper/elderPadlock/cursor";
                        if (mousePressed) path += "Click";
                        MTexture t = GFX.Game[path];
                        t.DrawCentered(previousMousePosition, Color.White * iconAlpha, 1); //provide visual indicator of mouse if using mouse and mouse is not visible on screen
                    }
                    Draw.SpriteBatch.End();
                }
                else
                {
                    if (!RenderedSplit)
                    {
                        LeftTarget.SetAsTarget(true);
                        Draw.SpriteBatch.Begin();
                        Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White);
                        Draw.SpriteBatch.End();
                        RightTarget.SetAsTarget(true);
                        Draw.SpriteBatch.Begin();
                        Draw.SpriteBatch.Draw(Target, -Vector2.UnitX * RightTarget.Width, Color.White);
                        Draw.SpriteBatch.End();
                        RenderedSplit = true;
                    }
                    Target.SetAsTarget(true);
                    Vector2 origin = LeftTarget.HalfSize();
                    Vector2 xOffset = -Vector2.UnitX * 40;
                    Vector2 fallOffset = Vector2.UnitY * splitFall;
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw(LeftTarget, origin + xOffset + fallOffset, null, Color.White, splitRotation, origin, 1, SpriteEffects.None, 0);
                    Draw.SpriteBatch.Draw(RightTarget, Vector2.UnitX * RightTarget.Width + origin - xOffset + fallOffset, null, Color.White, splitRotation, origin, 1, SpriteEffects.None, 0);
                    Draw.SpriteBatch.End();
                }

            }));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            MTexture sample = GFX.Game["objects/PuzzleIslandHelper/elderPadlock/cellUnpressed"];
            int cellX = 0, cellY = 0;
            float cellWidth = sample.Width;
            float cellHeight = sample.Height;
            float space = 16;
            Vector2 offset = Vector2.One * 8;
            Columns = Combination[0].Length;
            Rows = Combination.Length;
            PadlockWidth = Columns * (cellWidth + space);
            PadlockHeight = (Rows - 1) * (cellHeight + space);
            PadlockRenderHeight = Rows * (cellHeight + space);
            PadlockPosition = new Vector2(Engine.Width / 2 - PadlockWidth / 2, Engine.Height / 2 - PadlockRenderHeight / 2);
            Buttons = new VirtualMap<Button>(Columns, Rows);
            foreach (string s in Combination)
            {
                cellX = 0;
                foreach (char c in s)
                {
                    if (c != ',')
                    {
                        string id = "";
                        Action onPressed = null, onUnpressed = null;
                        bool autoUnpress = false;
                        float autoUnpressDelay = 0;
                        switch (c)
                        {
                            case '0' or '1':
                                id = "cell";
                                break;
                            case 'E':
                                id = "exit";
                                onPressed = () => InControl = false;
                                onUnpressed = () => ConfirmedExit = true;
                                autoUnpress = true;
                                autoUnpressDelay = 0.5f;
                                break;
                            case 'C':
                                id = "clear";
                                autoUnpress = true;
                                autoUnpressDelay = 0.5f;
                                onPressed = Clear;
                                break;
                            case 'S':
                                id = "submit";
                                autoUnpress = true;
                                autoUnpressDelay = 0.4f;
                                onPressed = () =>
                                {
                                    InControl = false;
                                    Submit();
                                };
                                onUnpressed = () =>
                                {
                                    Add(new Coroutine(submitUnpressedRoutine(solveOnSubmitUnpressed)));
                                };

                                break;
                        }
                        Button button = new Button(cellY, cellX, id)
                        {
                            OnPressed = onPressed,
                            OnUnpressed = onUnpressed,
                            AutoUnpress = autoUnpress,
                            AutoUnpressDelay = autoUnpressDelay,
                            Position = PadlockPosition + offset + new Vector2(cellX * (cellWidth + space), cellY * (cellHeight + space))
                        };
                        Buttons[cellX, cellY] = button;
                        Add(button);
                    }
                    cellX++;
                }
                cellY++;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            MouseState mouse = Mouse.GetState();
            previousMousePosition = clampPos(mouse);

            Running = true;
            Add(new Coroutine(Routine()));
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (mousePressed)
            {
                Draw.Rect(0, 0, 32, 32, Color.White);
            }
            else
            {
                Draw.Rect(0, 0, 32, 32, Color.Black);
            }
            if (Selected != null)
            {
                Draw.Line(0, 0, Selected.Position.X, Selected.Position.Y, Color.Yellow, 10);
            }
        }
        public override void Update()
        {
            base.Update();
            if (IsSplit) return;
            MouseState state = Mouse.GetState();
            if (Engine.Instance.IsActive)
            {
                mouseWasPressed = mousePressed;
                mousePressed = state.LeftButton == ButtonState.Pressed;
            }
            padWasPressed = padPressed;
            padPressed = Input.MenuConfirm;
            Vector2 mousePosition = clampPos(state);

            if (Running && InControl && Engine.Instance.IsActive)
            {
                if (Input.MenuCancel || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    ConfirmedExit = true;
                    return;
                }
                selectAlpha = Calc.Approach(selectAlpha, 1, Engine.DeltaTime);
                if (previousMousePosition != mousePosition)
                {
                    UsingMouse = true;
                    moveBuffer = 0;
                }
                lastMoveDir = moveDir;
                moveDir = new Vector2(Input.MenuLeft ? -1 : Input.MenuRight ? 1 : 0, Input.MenuUp ? -1 : Input.MenuDown ? 1 : 0);
                if (lastMoveDir != moveDir || moveDir == Vector2.Zero)
                {
                    moveBuffer = 0;
                }
                if (moveDir != Vector2.Zero || Input.MenuConfirm)
                {
                    UsingMouse = false;
                }
                if (UsingMouse)
                {
                    if (!Engine.Instance.IsMouseVisible)
                    {
                        iconAlpha = Calc.Approach(iconAlpha, 1, Engine.DeltaTime);
                    }

                    if (!mousePressed) //if mouse is idle
                    {
                        Button colliding = GetColliding(mousePosition);
                        if (colliding == null && Selected != null)
                        {
                            Selected.Reset();
                        }
                        else if (Selected != colliding)
                        {
                            Selected?.Reset();
                            Selected = colliding;
                            Selected.Color = SelectedColor;
                        }
                    }
                    //if a button has been selected...
                    if (Selected != null)
                    {
                        //on click, set the button to the pre-pressed state
                        if (mousePressed && !mouseWasPressed)
                        {
                            if (Selected.State != Button.States.Pressed)
                            {
                                //save the previous state in case the user changes their mind before releasing the button
                                Selected.StateBeforePressed = Selected.State;
                            }
                            Selected.State = Button.States.Pressed;

                        }
                        //on click released, lock in the selected button's state
                        else if (!mousePressed && mouseWasPressed)
                        {
                            //if selected button is prepped for a new state...
                            if (Selected.Colliding(mousePosition) && Selected.State == Button.States.Pressed)
                            {
                                //lock in the state opposite to it's previous state.
                                if (Selected.StateBeforePressed == Button.States.Unpressed)
                                {
                                    Selected.Press();
                                }
                                else
                                {
                                    Selected.Unpress();
                                }
                            }
                        }
                        //if mouse held down
                        else if (mousePressed && mouseWasPressed)
                        {
                            bool colliding = Selected.Colliding(mousePosition);
                            //if mouse leaves the area of the selected button...
                            if (!colliding)
                            {
                                //reset the button's visuals
                                Selected.Reset();
                            }
                            else
                            {
                                Selected.Color = SelectedColor;
                                if (Selected.State != Button.States.Pressed)
                                {
                                    Selected.StateBeforePressed = Selected.State;
                                    Selected.State = Button.States.Pressed;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Selected ??= Buttons[0, 0];
                    if (Selected.State != Button.States.Pressed)
                    {
                        if (moveBuffer <= 0)
                        {
                            int moveX = (int)moveDir.X;
                            int moveY = (int)moveDir.Y;
                            if (moveX != 0 || moveY != 0)
                            {
                                //find the x and y index of the next valid node after moving based on the player's inputs, keeping in mind array bounds and empty spaces. If diagonal input made, prioritize the x movement over the y movement.
                                int nextColumn = Selected.Column, nextRow = Selected.Row;
                                if (moveX != 0)
                                {
                                    for (int i = 0; i < Columns; i++)
                                    {
                                        nextColumn = nextColumn.Wrap(0, Columns - 1, moveX);
                                        if (Combination[Selected.Row][nextColumn] != ',') break;
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < Rows; i++)
                                    {
                                        nextRow = nextRow.Wrap(0, Rows - 1, moveY);
                                        if (Combination[nextRow][Selected.Column] != ',') break;
                                    }
                                }
                                Selected.Reset();
                                Selected = Buttons[nextColumn, nextRow];
                                Selected.Color = SelectedColor;
                                moveBuffer = 0.2f;
                            }
                        }
                        else
                        {
                            moveBuffer -= Engine.DeltaTime;
                        }
                    }
                    if (padPressed)
                    {
                        if (Selected.State != Button.States.Pressed)
                        {
                            Selected.StateBeforePressed = Selected.State;
                            Selected.State = Button.States.Pressed;
                        }
                    }
                    else
                    {
                        if (padWasPressed)
                        {
                            if (Selected.StateBeforePressed == Button.States.Unpressed)
                            {
                                Selected.Press();
                            }
                            else
                            {
                                Selected.Unpress();
                            }
                        }
                    }
                }
            }
            else
            {
                selectAlpha = Calc.Approach(selectAlpha, 0, Engine.DeltaTime);
            }
            previousMousePosition = mousePosition;
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            PadlockTarget?.Dispose();
            Target?.Dispose();
            LeftTarget?.Dispose();
            RightTarget?.Dispose();
            Running = false;
        }
        public void Split()
        {
            IsSplit = true;
            RenderedSplit = false;
        }
        public Coroutine AddButtonColorRoutine(int x, int y, Color colorTo, float duration, float hold)
        {
            Button b = Buttons[x, y];
            if (b != null)
            {
                b.Coroutine?.Cancel();
                b.Coroutine?.RemoveSelf();
                b.Coroutine = new Coroutine(false);
                Add(b.Coroutine);
                b.Coroutine.Replace(buttonColorRoutine(b, x, y, colorTo, duration, hold));
                return b.Coroutine;
            }
            return null;
        }
        public void SetButtonColors(Color color)
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Button b = Buttons[x, y];
                    if (b != null)
                    {
                        b.Coroutine?.Cancel();
                        b.Color = color;
                    }
                }
            }
        }
        public Button GetColliding(Vector2 position)
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Button b = Buttons[x, y];
                    if (b != null && b.Colliding(position)) //find the colliding node, if it exists
                    {
                        return b;
                    }
                }
            }
            return null;
        }
        public void Clear()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows - 1; y++)
                {
                    Buttons[x, y].Unpress();
                }
            }
        }
        public void Submit()
        {
            solveOnSubmitUnpressed = false;
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows - 1; y++)
                {
                    Button b = Buttons[x, y];
                    char c = Combination[y][x];
                    if (!(b.State == Button.States.PressedIdle && c == '1' || b.State == Button.States.Unpressed && c == '0'))
                    {
                        return;
                    }
                }
            }
            solveOnSubmitUnpressed = true;
        }
        private Vector2 clampPos(MouseState state)
        {
            float mouseX = Calc.Clamp(state.X, 0, Engine.ViewWidth);
            float mouseY = Calc.Clamp(state.Y, 0, Engine.ViewHeight);
            float scale = (float)Engine.Width / Engine.ViewWidth;
            return new Vector2(mouseX, mouseY) * scale;
        }
        public IEnumerator Routine()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                float eased = Ease.SineInOut(i);
                blackAlpha = Math.Min(eased * 1.2f, 1);
                lerp = eased;
                yield return null;
            }
            InControl = true;
            while (!ConfirmedExit) yield return null;
            InControl = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                float eased = Ease.SineInOut(i);
                blackAlpha = 1 - eased;
                lerp = 1 - Math.Min(eased * 1.2f, 1);
                yield return null;
            }
            RemoveSelf();
        }
        private IEnumerator buttonColorRoutine(Button b, int x, int y, Color colorTo, float duration, float hold)
        {
            Color orig = b.Color;
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration / 2)
            {
                b.Color = Color.Lerp(orig, colorTo, Ease.SineInOut(i));
                yield return null;
            }
            if (hold > 0) yield return hold;
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration / 2)
            {
                b.Color = Color.Lerp(colorTo, orig, Ease.SineInOut(i));
                yield return null;
            }
            b.Color = orig;
        }
        private IEnumerator submitUnpressedRoutine(bool correct)
        {
            InControl = false;
            Color color = correct ? Color.Lime : SelectedColor;
            if (!correct)
            {
                for (int l = 0; l < 3; l++)
                {
                    for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
                    {
                        SetButtonColors(Color.Lerp(Color.White, SelectedColor, Ease.SineIn(i)));
                        yield return null;
                    }
                    for (float i = 1; i > 0; i -= Engine.DeltaTime / 0.8f)
                    {
                        SetButtonColors(Color.Lerp(Color.White, SelectedColor, Ease.SineOut(i)));
                        yield return null;
                    }
                }
                SetButtonColors(Color.White);
                Clear();
            }
            else
            {
                SetButtonColors(Color.White);
                Coroutine[] coroutines =
                {
                    new Coroutine(FlashBoard(Color.Lime, 0.2f, 0.05f, 0.05f)),
                    new Coroutine(FlashBoard(Color.Lime, 0.2f, 0.05f, 0.05f))
                };
                Add(coroutines[0]);
                yield return 1;
                Add(coroutines[1]);
                foreach (Coroutine coroutine in coroutines)
                {
                    while (!coroutine.Finished) yield return null;
                }
            }
            if (!correct)
            {
                Clear();
                InControl = true;
            }
            else
            {
                yield return 0.6f;
                Split();
                yield return 1f;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    float eased = Ease.SineInOut(i);
                    splitFall = eased * Engine.Height;
                    splitRotation = eased * MathHelper.PiOver4 * 1.5f;
                    yield return null;
                }
                Solved = true;
                yield return null;
                RemoveSelf();
            }
        }
        private IEnumerator FlashBoard(Color color, float time, float hold, float delay)
        {
            List<Coroutine> routines = [];
            for (int i = 0; i < Rows; i++)
            {
                Coroutine routine = new Coroutine(FlashRow(i, color, time, hold, delay));
                routines.Add(routine);
                Add(routine);
                yield return delay;
            }
            foreach (Coroutine r in routines)
            {
                while (!r.Finished) yield return null;
            }
        }
        private IEnumerator FlashRow(int row, Color color, float time, float hold, float delay)
        {
            if (row >= Rows) yield break;
            List<Coroutine> coroutines = [];
            for (int i = 0; i < Columns; i++)
            {
                Coroutine coroutine = AddButtonColorRoutine(i, row, color, time, hold);
                if (coroutine != null)
                {
                    coroutines.Add(coroutine);
                }
                yield return delay;
            }
            foreach (Coroutine coroutine in coroutines)
            {
                while (!coroutine.Finished) yield return null;
            }
        }
    }
}