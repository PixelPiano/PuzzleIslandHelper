using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [CustomEntity("PuzzleIslandHelper/Interface")]
    [Tracked]
    public class Interface : Entity
    {
        public const int ColliderWidth = 6;
        public const int ColliderHeight = 6;
        public const int BorderX = 16;
        public const int BorderY = 10;
        public const int BaseDepth = -1000001;
        public bool FillBehindScreen;
        private float MaxGlitchRange = 0.2f;
        private float timer;
        private float prevLightAlpha;
        private float prevBloomStrength;
        private float ColorLerpRate;
        private float SavedAlpha;
        public bool HoldLight;
        public bool Buffering;
        public bool NightMode = true;
        public bool DraggingWindow;
        public bool InControl;
        public bool CanCloseWindow;
        public bool Teleporting;
        public bool Invalid;
        public bool Interacting;
        private bool Interacted;
        private bool draggingWindow;
        public bool MouseOnBounds
        {
            get
            {
                MouseState mouseState = Mouse.GetState();

                float mouseX = Calc.Clamp(mouseState.X, 0, Engine.ViewWidth);
                float mouseY = Calc.Clamp(mouseState.Y, 0, Engine.ViewHeight);
                return mouseX == BorderX || mouseX == Engine.ViewWidth - BorderX || mouseY == BorderY || mouseY == Engine.ViewHeight - BorderY;
            }
        }
        public bool MouseOutOfBounds
        {
            get
            {
                MouseState mouseState = Mouse.GetState();

                float mouseX = Calc.Clamp(mouseState.X, 0, Engine.ViewWidth);
                float mouseY = Calc.Clamp(mouseState.Y, 0, Engine.ViewHeight);
                return mouseX < BorderX || mouseX > Engine.ViewWidth - BorderX || mouseY < BorderY || mouseY >= Engine.ViewHeight - BorderY;
            }
        }

        private float oobTimer;
        public bool LeftPressed
        {
            get
            {
                bool value = MouseIsFocused && Mouse.GetState().LeftButton == ButtonState.Pressed;
                return value && !Buffering;
            }
        }
        private bool prevLeftClicked;
        private bool leftClicked;
        public bool FirstFrameClick => leftClicked && !prevLeftClicked;

        public bool MouseIsFocused => InControl && Engine.Instance.IsActive;
        private bool intoIdle = false;
        private bool CanClickIcons = true;
        private bool SetDragOffset = false;
        private bool Closing = false;
        private bool GlitchPlayer = false;
        private bool RemovePlayer = false;
        private float clickTimer;

        public string CurrentIconName = "invalid";

        public Rectangle MouseRectangle;
        public bool UsesStartupMonitor => Machine is not null && Machine.UsesStartupMonitor;
        public bool MonitorLoaded;
        public Monitor monitor;
        public PowerButton Power;
        public Window Window;
        public InterfaceCursor Cursor;
        public InterfaceBorder Border;
        public Sprite cursorSprite;
        public SoundSource whirringSfx;
        public InterfaceMachine Machine;

        private ComputerIcon[] Icons;
        public List<WindowContent> Content = new();
        private Level level;
        private Player player;
        public DotX3 Talk;
        public FloppyHUD FloppyHUD;
        private float cursorAlpha = 1;

        public bool FakeStarting;

        public bool ForceHide = false;


        public Vector2 MousePosition
        {
            get
            {
                if (!MouseIsFocused) return prevMousePos;
                if (Closing)
                {
                    return Vector2.Zero;
                }
                MouseState mouseState = Mouse.GetState();
                float mouseX = Calc.Clamp(mouseState.X, 0, Engine.ViewWidth);
                float mouseY = Calc.Clamp(mouseState.Y, 0, Engine.ViewHeight);
                float scale = (float)Engine.Width / Engine.ViewWidth;
                Vector2 position = new Vector2(mouseX, mouseY) * scale;
                prevMousePos = position;
                return position;
            }
        }
        private Vector2 prevMousePos;
        public Vector2 MouseWorldPosition => Cursor.WorldPosition;
        private Vector2 spacing = new Vector2(8, 16); //spacing between the icons
        private Vector2 DragOffset; //used to correctly align Window with Cursor while being dragged
        private Color startColor = Color.LimeGreen;
        public Color BackgroundColor;
        //the order of icons in sequential order. If string is not a valid icon name, is replaced with the "invalid" symbol when drawn.
        public List<string> IconIDs = new();
        public List<string> WindowText = new();
        public List<string> TabText = new();
        public int IDCount => IconIDs.Count;
        public int WindowCount => WindowText.Count;
        public int TabCount => TabText.Count;
        public string InstanceID;
        public string CurrentPreset;

        public MTexture Keyboard = GFX.Game["objects/PuzzleIslandHelper/interface/keyboard"];
        public void AddProgram<T>(T content) where T : WindowContent
        {
            Content.Add(content);
        }
        public WindowContent GetProgram(ComputerIcon icon)
        {
            return GetProgram(icon.Name);
        }
        public WindowContent GetProgram(string name)
        {
            foreach (WindowContent content in Content)
            {
                if (content.Name.ToLower() == name.ToLower())
                {
                    return content;
                }
            }
            return null;
        }
        public void LightCleanup(bool full = false, bool empty = false)
        {
            if (!empty)
            {
                CurrentPreset = "";
                IconIDs.Clear();
                WindowText.Clear();
                TabText.Clear();
                Window?.RemoveSelf();
                Cursor?.RemoveSelf();
                foreach (DesktopClickable dl in Scene.Tracker.GetEntities<DesktopClickable>())
                {
                    dl.RemoveSelf();
                }
                foreach (WindowContent content in Content)
                {
                    if (!full)
                    {
                        if (content.Preserve) continue;
                        if (content is AccessProgram)
                        {
                            if (AccessProgram.AccessTeleporting) continue;
                        }
                    }
                    content?.RemoveSelf();
                }
                Content.Clear();

            }
            Interacting = false;
            PianoModule.Session.Interface = null;
        }
        public void Cleanup(Scene scene)
        {
            LightCleanup();
            if (Interacting)
            {
                SetLightAmount(scene as Level, 1);
            }
        }
        public bool TryGetPreset(string id)
        {
            InterfaceData.Presets preset = PianoModule.InterfaceData.GetPreset(id);
            if (preset is null)
            {
                CurrentPreset = "";
                return false;
            }
            else if (CurrentPreset == id)
            {
                return false;
            }
            else
            {
                CurrentPreset = id;
                IconIDs.Clear();
                WindowText.Clear();
                TabText.Clear();
                foreach (InterfaceData.Presets.IconText text in preset.Icons)
                {
                    IconIDs.Add(text.ID);
                    WindowText.Add(text.Window);
                    TabText.Add(text.Tab);
                }
                return true;
            }
        }
        public Interface(Color background, InterfaceMachine machine) : base()
        {
            BackgroundColor = background;
            Tag |= Tags.TransitionUpdate;
            Add(whirringSfx = new SoundSource());
            startColor = Color.Lerp(BackgroundColor, Color.White, 0.1f);
            Machine = machine;
        }
        public void LoadModules(Scene scene)
        {
            if (!FakeStarting)
            {
                //can you tell this code used to be bad

                //hey it's me, half a year later, and it's still bad but i think it's better this way for the story not gonna lie
                //a scuffed computer that *just* barely runs fits really well with the level of professional the manufacturers of it had
                scene.Add(Window = new Window(Position, this));
                scene.Add(new IconText(this));

                //InterfaceCursor and Border are just generic entities but with tags that tell them to render in screen-space rather than world-space
                scene.Add(Cursor = new InterfaceCursor());
            }
            if (Machine.UsesFloppyLoader)
            {
                scene.Add(new FloppyLoader(this));
            }
            scene.Add(Border = new InterfaceBorder(this));

            if (!FakeStarting)
            {
                //since the game renders entities in order based on their depth
                //these three generic entities have to be made, since sprites don't have a depth field of their own.

                //update: turning two of them into a custom entity the user can click on
                scene.Add(Power = new PowerButton(this));
                scene.Add(new Nightmode(this));
            }
            scene.Add(monitor = new Monitor(startColor, this));

            //create sprites and add them to their corresponding entities
            string path = "objects/PuzzleIslandHelper/interface/";
            //add animations to the sprites

            if (!FakeStarting)
            {
                Cursor.Add(cursorSprite = new Sprite(GFX.Game, path));
                cursorSprite.AddLoop("idle", "Cursor", 1f);
                cursorSprite.AddLoop("pressed", "cursorPress", 1f);
                cursorSprite.AddLoop("buffer", "buffering", 0.1f);
                cursorSprite.Visible = false;
                Cursor.Depth = BaseDepth - 6;
                Window.Drawing = false;
            }
        }
        public class Monitor : Entity
        {
            public Sprite Sprite;
            public float Alpha;
            public Interface Parent;
            public Image Cover;
            public float CoverAlpha;
            public bool TurningOff => Sprite.CurrentAnimationID == "turnOff";
            public bool Idle => Sprite.CurrentAnimationID == "idle";
            public bool StartingUp => Sprite.CurrentAnimationID == "boot";
            public void SetColor(Color color)
            {
                Sprite.SetColor(color);
            }
            public Monitor(Color color, Interface parent) : base()
            {
                Depth = BaseDepth;
                Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/");
                Cover = new Image(GFX.Game["objects/PuzzleIslandHelper/interface/cover00"]);
                Add(Cover);
                Sprite.AddLoop("idle", "idle", 1f);
                Sprite.AddLoop("off", "off", 0.1f);
                Sprite.Add("boot", "startUp", 0.07f, "idle");
                Sprite.Add("turnOff", "shutDown", 0.07f, "off");
                Sprite.SetColor(color);
                Cover.Color = Sprite.Color * 0;

                Add(Sprite);
                Collider = new Hitbox(Sprite.Width, Sprite.Height);
                Parent = parent;
            }
            public override void Render()
            {
                if (Parent.ForceHide) return;
                if (Alpha > 0)
                {
                    Draw.Rect(Collider, Color.Black * Alpha);
                }
                base.Render();
            }
            public void StartUp()
            {
                Sprite.Play("boot");
            }
            public void TurnOff()
            {
                Sprite.Play("turnOff");
            }
            public override void Update()
            {
                base.Update();
                CoverAlpha = Sprite.CurrentAnimationID switch
                {
                    "idle" => Calc.Approach(CoverAlpha, 1, Engine.DeltaTime),
                    "turnOff" => 0,
                    _ => Calc.Approach(CoverAlpha, 0, Engine.DeltaTime)
                };
                Cover.SetColor(Sprite.Color * CoverAlpha);
            }
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Interacting = false;
            level = scene as Level;
            Depth = BaseDepth;
        }
        public void QuickLoadPreset(string preset)
        {
            if (TryGetPreset(preset))
            {
                Add(new Coroutine(FadeIcons()));
            }
        }
        private void SetIconAlpha(float alpha)
        {
            foreach (ComputerIcon icon in Icons)
            {
                icon.Alpha = alpha;
            }
        }
        private IEnumerator FadeIcons()
        {
            InControl = false;
            Window.Close();
            for (float i = 1; i > 0; i -= Engine.DeltaTime * 2)
            {
                SetIconAlpha(i);
                yield return null;
            }
            Scene.Remove(Content.ToArray());
            Scene.Remove(Icons);

            Icons = null;
            Content.Clear();
            AddIcons(Scene);
            ShowIcons();
            SetIconInitialPositions();

            for (float i = 0; i < 1; i += Engine.DeltaTime * 2)
            {
                SetIconAlpha(i);
                yield return null;
            }

            SetIconAlpha(1);
            InControl = true;
        }
        public void AddPrograms(ComputerIcon[] icons)
        {
            foreach (ComputerIcon icon in icons)
            {
                AddProgram(icon);
            }
        }
        public void AddProgram(ComputerIcon icon)
        {
            ProgramLoader.LoadCustomProgram(icon.Name, Window, level);
        }
        public void AddProgram(string name)
        {
            ProgramLoader.LoadCustomProgram(name, Window, level);
        }
        public void AddIcons(Scene scene)
        {
            Icons = new ComputerIcon[IconIDs.Count];
            for (int i = 0; i < IconIDs.Count; i++)
            {
                ComputerIcon icon = new ComputerIcon(this, IconIDs[i], WindowText[i], TabText[i]);
                scene.Add(icon);
                Icons[i] = icon;
                icon.Visible = false;
            }
            AddPrograms(Icons);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = Scene.Tracker.GetEntity<Player>();
        }
        public bool MouseOver(Rectangle rect)
        {
            return MouseRectangle.Intersects(rect) && !Buffering;
        }
        public bool MouseOver(Vector2 position, float width, float height)
        {
            return MouseRectangle.Intersects(new Rectangle((int)position.X, (int)position.Y, (int)width, (int)height)) && !Buffering;
        }
        public bool MouseOver(Vector2 position)
        {
            return MouseRectangle.Contains(new Point((int)position.X, (int)position.Y)) && !Buffering;
        }
        public bool MouseOver(Circle circle)
        {
            return Collide.RectToCircle(MouseRectangle, circle.Position, circle.Radius) && !Buffering;
        }
        public bool MouseOver(Collider collider)
        {
            return MouseRectangle.Intersects(collider.Bounds) && !Buffering;
        }
        public override void Update()
        {
            #region Cursor Update
            timer = timer > 0 ? timer - Engine.DeltaTime : 0;

            if (HoldLight)
            {
                if (Scene is Level level)
                {
                    level.Lighting.Alpha = 0;
                    level.Bloom.Strength = 0;
                }
            }
            if (InControl)
            {
                bool mouseCanAct = MouseIsFocused;
                bool outOfBounds = MouseOutOfBounds;
                float add = (mouseCanAct && !outOfBounds) || oobTimer > 1f ? 2 : -1.3f;
                oobTimer = mouseCanAct && !outOfBounds ? 0 : oobTimer > 1f ? oobTimer + Engine.DeltaTime : oobTimer;
                cursorAlpha = Calc.Clamp(cursorAlpha + add * Engine.DeltaTime, 0, 1);
                cursorSprite.Color = Color.White * cursorAlpha;
                Cursor.Position = MousePosition.Floor();

                if (Collider is not null)
                {
                    Collider.Position = (level.Camera.Position - Position + Cursor.Position / 6).Floor();
                }
                prevLeftClicked = leftClicked;
                if (mouseCanAct)
                {
                    //don't reset click state if the mouse is not in play
                    leftClicked = LeftPressed;
                }

                if (leftClicked && mouseCanAct && !outOfBounds) //If the mouse is clicked
                {
                    OnClicked();
                }
                else
                {
                    DraggingWindow = false;
                    SetDragOffset = false;
                    if (!Buffering)
                    {
                        cursorSprite.Play("idle"); //revert Cursor GlassTexture if not being clicked
                    }
                }
                if (DraggingWindow && Window.Drawing && Window.DraggingEnabled)
                {
                    Window.Position = Collider.Position + GetDragOffset();
                }
            }
            #endregion
            base.Update();
            if (FakeStarting) return;
            if (InControl)
            {
                if (Collider is not null)
                {
                    MouseRectangle = Collider.Bounds;
                }
            }
            if (Buffering && cursorSprite.CurrentAnimationID != "buffer")
            {
                cursorSprite.Play("buffer");
            }
            if (player is not null)
            {
                if (monitor is not null && monitor.Idle && !intoIdle)
                {
                    //Handles icon/Cursor transition from screen off to screen on
                    Add(new Coroutine(TransitionToMain(), true));
                    intoIdle = true;
                }
            }
        }
        public override void Removed(Scene scene)
        {
            Cleanup(scene);
            base.Removed(scene);
        }
        private Vector2 GetDragOffset()
        {
            if (Window is null) return Vector2.Zero;
            if (!SetDragOffset)
            {
                //Get the distance from the tab's position to the Cursor's position
                DragOffset = new Vector2(-(Collider.Position.X - Window.Position.X), Window.TabArea.Y - Collider.Position.Y + Window.tabHeight);
            }
            SetDragOffset = true;
            return DragOffset;
        }
        public void RemoveWindow()
        {
            if (Window is null || !CanCloseWindow)
            {
                return;
            }
            DraggingWindow = false;
            SetDragOffset = false;
            IconText.CurrentIcon = null;
            Window.Close();
            Add(new Coroutine(WaitForClickRelease(delegate { CanClickIcons = true; })));
        }
        private IEnumerator WaitForClickRelease(Action onReleased = null)
        {
            while (LeftPressed)
            {
                yield return null;
            }
            onReleased?.Invoke();
        }
        public void ForceClose(bool fast)
        {
            Add(new Coroutine(CloseInterfaceRoutine(fast)));
        }

        private void OnClicked()
        {
            cursorSprite.Play("pressed"); //play the "click" animation
            if (Window.Drawing && !DraggingWindow && FirstFrameClick)
            {
                if (CollideRect(Window.TabArea))
                {
                    DraggingWindow = true;
                }
                //x button overlaps with tab area, have to check it after just to be safe
                if (CanCloseWindow && CollideCheck(Window.x))
                {
                    //remove button collider, stop drawing Window, and allow user to click icons again
                    RemoveWindow();
                    return;

                }
            }
            if (Closing || !FirstFrameClick)
            {
                return;
            }
            /*            if (!DraggingWindow && CollideCheck(Power))
                        {
                            foreach (Button button in Window.Buttons)
                            {
                                if (button.Pressing)
                                {
                                    return;
                                }
                            }
                            if (!Closing)
                            {
                                Add(new Coroutine(CloseInterfaceRoutine(false), true));
                            }
                        }*/

            #region Check for icon clicked
            bool windowActiveAndNotDrawing = Window is not null && !Window.Drawing && CanClickIcons;
            foreach (DesktopClickable clickable in Scene.Tracker.GetEntities<DesktopClickable>())
            {
                if ((clickable.AlwaysClickable || windowActiveAndNotDrawing) && CollideCheck(clickable))
                {
                    clickable.OnClick();
                    return;
                }
            }

            #endregion
        }
        public void CloseInterface(bool fast, bool lockPlayer = false)
        {
            Add(new Coroutine(CloseInterfaceRoutine(fast, lockPlayer)));
        }
        public void OpenIcon(ComputerIcon icon)
        {
            IconText.CurrentIcon = icon;
            CanCloseWindow = CanClickIcons = false;
            CurrentIconName = Window.Name = icon.Name;
            Add(new Coroutine(WaitForClickRelease(delegate { CanCloseWindow = true; })));

            Window.OpenWindow(icon);
            Window.TextWindow.CurrentID = icon.TextID;
            Window.Drawing = true;
        }
        public void OpenCustom(string name, string textID, string tabText = "")
        {
            ComputerIcon icon = new ComputerIcon(this, name, textID, tabText);
            IconText.CurrentIcon = icon;
            CanCloseWindow = CanClickIcons = false;
            CurrentIconName = Window.Name = icon.Name;
            Add(new Coroutine(WaitForClickRelease(delegate { CanCloseWindow = true; })));
            Window.OpenWindow(icon);
            Window.TextWindow.CurrentID = icon.TextID;
            Window.Drawing = true;
        }
        private IEnumerator CollectFirstFloppyDisk(Player player)
        {
            if (PianoModule.Session.HasFirstFloppy) yield break;
            player.StateMachine.State = Player.StDummy;
            yield return Textbox.Say("findFloppy");
            FloppyDisk firstDisk = new FloppyDisk(Vector2.Zero, "Calidus1", Color.White);
            PianoModule.Session.TryAddDisk(firstDisk);
            PianoModule.Session.HasFirstFloppy = true;
            player.StateMachine.State = Player.StNormal;
            yield return null;
        }
        public IEnumerator FakeStart()
        {
            FakeStarting = true;
            LoadModules(level);
            Start();
            while (monitor is null || !monitor.Idle)
            {
                yield return null;
            }
            //Handles icon/Cursor transition from screen off to screen on
            yield return TransitionToMain();

            yield return 1f;
            yield return Textbox.Say("interfaceNoFloppy");
            yield return 0.1f;
            yield return CloseInterfaceRoutine(false, false);
            FakeStarting = false;
        }
        public void StartPreset(string preset)
        {
            if (string.IsNullOrEmpty(preset)) return;
            LoadModules(level); //see image 2
            TryGetPreset(preset);
            AddIcons(level);
            Start();
        }
        private IEnumerator BeginSequence(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            yield return Machine.OnBegin(player, level);
        }
        public void Start()
        {
            if (Scene is not Level level) return;
            PianoModule.Session.Interface = this;
            intoIdle = false;
            Interacting = Interacted = Border.Visible = monitor.Visible = true;
            monitor.Position = level.Camera.Position + new Vector2(16, 10);
            Border.Position = level.Camera.CameraToScreen(level.Camera.Position) + Vector2.One;
            if (!FakeStarting)
            {
                foreach (DesktopClickable clickable in Scene.Tracker.GetEntities<DesktopClickable>())
                {
                    clickable.Prepare(level);
                }
                foreach (DesktopClickable clickable in Scene.Tracker.GetEntities<DesktopClickable>())
                {
                    clickable.Begin(level);
                }
                SetIconInitialPositions();
            }
            whirringSfx.Play("event:/PianoBoy/interface/Whirring", "Computer state", 0);
            Add(new Coroutine(ScreenOn()));
            Collider = new Hitbox(ColliderWidth, ColliderHeight, monitor.Position.X - Position.X + MousePosition.X / 6, monitor.Position.Y - Position.Y + MousePosition.Y / 6);
        }
        private void SetIconInitialPositions()
        {
            if (Icons is null) return;
            Vector2 iconPosition = monitor.Position + Vector2.One * 2;

            float iconPositionX = 0;
            bool change = false;
            int row = 0;

            //set the icon positions
            for (int i = 0; i < Icons.Length; i++)
            {
                iconPositionX = change ? 0 : iconPositionX + Icons[i].Width + spacing.X;
                change = iconPositionX > monitor.Width - 32;
                if (change)
                {
                    row++;
                    iconPosition = monitor.Position + new Vector2(18, 12 + spacing.Y * row);
                }
                Icons[i].Position = iconPosition;
                if (!change)
                {
                    iconPosition += new Vector2(Icons[i].Width + spacing.X, 0);
                }
            }
        }
        private IEnumerator ScreenOn()
        {
            PianoModule.Session.MonitorActivated = true;
            if (UsesStartupMonitor)
            {
                yield return MonitorIconAnim(true);
                yield return 0.3f;
            }
            Border.Visible = true;
            monitor.Visible = true;
            monitor.Alpha = Border.Alpha = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                monitor.Alpha = i;
                Border.Alpha = i;
                yield return null;
            }
            monitor.Alpha = Border.Alpha = 1;
            HoldLight = true;
            SetLightAmount(Scene as Level, 0);
            yield return 0.2f;
            monitor.StartUp();
            yield return null;
        }

        public void BeginInteract(Player player)
        {
            //play click sound
            FakeStarting = false;
            Add(new Coroutine(BeginSequence(player)));
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Cleanup(scene);
            if (Interacting)
            {
                Audio.SetMusicParam("fade", 1);
            }
            PianoModule.Session.Interface = null;
        }
        private IEnumerator TransitionToMain()
        {
            Interacting = true;
            yield return null;
            int count = 0;
            Closing = false;
            Border.Visible = true;
            InControl = false;
            if (!FakeStarting)
            {
                cursorSprite.Visible = false;
                cursorSprite.Play("idle");
            }

            //Computer logo sequence
            Audio.Play("event:/PianoBoy/interface/InterfaceBootup", Position);
            Audio.SetMusicParam("fade", 0);
            for (float i = 0; i < 1; i += 0.025f)
            {
                if (!FakeStarting)
                {
                    bool state = count % 8 == 0;

                    foreach (DesktopClickable dc in Scene.Tracker.GetEntities<DesktopClickable>())
                    {
                        dc.Visible = state;
                    }
                }
                monitor.SetColor(Color.Lerp(startColor, BackgroundColor, Ease.SineInOut(i)));
                count++;
                yield return null;
            }
            if (!FakeStarting)
            {
                foreach (DesktopClickable dc in Scene.Tracker.GetEntities<DesktopClickable>())
                {
                    dc.Visible = true;
                }
                cursorSprite.Visible = true;

                InControl = true;
            }
            MonitorLoaded = true;
        }
        public IEnumerator FlickerIcons(bool hide)
        {
            int count = 0;
            for (float i = 0; i < 1; i += 0.025f)
            {
                if (count % 8 == 0) ShowIcons(); else HideIcons();
                count++;
                yield return null;
            }
            if (hide)
            {
                HideIcons();
            }
        }
        private void IconVisibility(bool visible)
        {
            foreach (DesktopClickable dc in Scene.Tracker.GetEntities<DesktopClickable>())
            {
                dc.Visible = visible;
            }
            cursorSprite.Visible = visible;

        }
        public void ShowIcons()
        {
            IconVisibility(true);
        }
        public void HideIcons()
        {
            IconVisibility(false);
        }
        public IEnumerator ScreenOff(bool fast)
        {
            monitor.TurnOff();
            while (!fast && monitor.TurningOff)
            {
                yield return null;
            }
            HoldLight = false;
            SetLightAmount(Scene as Level, 1);
            if (!fast)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    float amount = Calc.LerpClamp(1, 0, i);
                    Border.Alpha = amount;
                    monitor.Alpha = amount;
                    yield return null;
                }
            }
            monitor.Alpha = Border.Alpha = 0;
            Border.Visible = false;
            monitor.Visible = false;
            Interacting = false;
        }
        private void SetLightAmount(Level level, float amount)
        {
            level.Lighting.Alpha = Calc.LerpClamp(0, prevLightAlpha, amount);
            level.Bloom.Strength = Calc.LerpClamp(0, prevBloomStrength, amount);
        }
        public bool InFlickerRoutine;
        public IEnumerator FlickerHide()
        {
            InFlickerRoutine = true;
            float interval = 0.06f;
            ForceHide = false;
            for (int i = 0; i < 3; i++)
            {
                ForceHide = true;
                yield return interval;
                ForceHide = false;
                yield return interval;
            }
            ForceHide = true;
            InFlickerRoutine = false;
        }
        public IEnumerator FlickerReveal()
        {
            InFlickerRoutine = true;
            float interval = 0.06f;
            ForceHide = true;
            for (int i = 0; i < 3; i++)
            {
                ForceHide = false;
                yield return interval;
                ForceHide = true;
                yield return interval;
            }
            ForceHide = false;
            InFlickerRoutine = false;

        }
        public IEnumerator CloseInterfaceRoutine(bool fast, bool lockPlayer = false)
        {
            Closing = true;
            InControl = false;
            if (!FakeStarting)
            {
                RemoveWindow();
                cursorSprite.Play("idle");
                if (!fast) yield return FlickerIcons(true);
            }

            if (!UsesStartupMonitor)
            {
                whirringSfx.Param("Computer state", 1);
            }
            yield return ScreenOff(fast);
            Audio.SetMusicParam("fade", 1);
            player.StateMachine.State = lockPlayer ? Player.StDummy : 0;
            LightCleanup(false, FakeStarting);
            if (UsesStartupMonitor)
            {
                yield return 0.8f;
                whirringSfx.Param("Computer state", 1);
                yield return MonitorIconAnim(false);
            }
            yield return null;
        }
        public IEnumerator MonitorIconAnim(bool state)
        {
            InterfaceMonitor monitor = SceneAs<Level>().Tracker.GetEntity<InterfaceMonitor>();
            if (monitor is null) yield break;
            if (state)
            {
                monitor.Activate();
            }
            else
            {
                monitor.Deactivate();
            }
            yield return null;
            while (monitor.InRoutine)
            {
                yield return null;
            }

        }
        public static void InstantTeleport(Scene scene, Player player, string room, bool sameRelativePosition, float positionX, float positionY)
        {
            Level level = scene as Level;
            if (level == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(room))
            {
                Vector2 val = new Vector2(positionX, positionY) - player.Position;
                player.Position = new Vector2(positionX, positionY);
                Camera camera = level.Camera;
                camera.Position += val;
                player.Hair.MoveHairBy(val);
                return;
            }
            level.OnEndOfFrame += delegate
            {
                Vector2 levelOffset = level.LevelOffset;
                Vector2 val2 = player.Position - level.LevelOffset;
                Vector2 val3 = level.Camera.Position - level.LevelOffset;
                Facings facing = player.Facing;
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Level = room;
                Session session = level.Session;
                Level level2 = level;
                Rectangle bounds = level.Bounds;
                float num = bounds.Left;
                bounds = level.Bounds;
                session.RespawnPoint = level2.GetSpawnPoint(new Vector2(num, bounds.Top));
                level.Session.FirstLevel = false;
                level.LoadLevel(Player.IntroTypes.Transition);
                if (sameRelativePosition)
                {
                    level.Camera.Position = level.LevelOffset + val3;
                    level.Add(player);
                    player.Position = level.LevelOffset + val2;
                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                }
                else
                {
                    Vector2 val4 = new Vector2(positionX, positionY) - level.LevelOffset - val2;
                    level.Camera.Position = level.LevelOffset + val3 + val4;
                    level.Add(player);
                    player.Position = new Vector2(positionX, positionY);
                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset + val4);
                }
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }

        public static void TeleportTo(Scene scene, Player player, string room, Player.IntroTypes introType = Player.IntroTypes.Transition, Vector2? nearestSpawn = null)
        {
            Level level = scene as Level;
            if (level != null)
            {
                level.OnEndOfFrame += delegate
                {
                    level.TeleportTo(player, room, introType, nearestSpawn);
                };
            }
        }
        [TrackedAs(typeof(DesktopClickable))]
        public class Nightmode : DesktopClickable
        {
            public Sprite sprite;
            public Nightmode(Interface inter) : base(inter, 10, true)
            {
                Depth = BaseDepth - 1;
                Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/"));
                sprite.AddLoop("sun", "sun", 0.1f);
                sprite.AddLoop("moon", "moon", 0.1f);
                Collider = new Hitbox(sprite.Width, sprite.Height);
                sprite.Play(Interface.NightMode ? "moon" : "sun");
            }
            public override void OnClick()
            {
                base.OnClick();
                bool prev = Interface.NightMode;
                Interface.NightMode = !prev;
                sprite.Play(!prev ? "moon" : "sun");
            }
            public override void Begin(Scene scene)
            {
                base.Begin(scene);
                sprite.Play(Interface.NightMode ? "moon" : "sun");
            }
            public override void Update()
            {
                base.Update();
                if (Interface.Power != null)
                {
                    Position = Interface.Power.BottomRight + new Vector2(8, -Height);
                }
            }

        }
        [TrackedAs(typeof(DesktopClickable))]
        public class PowerButton : DesktopClickable
        {
            public Sprite sprite;
            public PowerButton(Interface inter, string path = "objects/PuzzleIslandHelper/interface/") : base(inter)
            {
                Depth = BaseDepth - 1;
                Add(sprite = new Sprite(GFX.Game, path));
                sprite.AddLoop("idle", "power", 1f);
                Collider = new Hitbox(sprite.Width, sprite.Height);
                sprite.Play("idle");
            }
            public override void Begin(Scene scene)
            {
                base.Begin(scene);
                sprite.Play("idle");

            }
            public override void Update()
            {
                base.Update();
                if (Interface.monitor is not null)
                {
                    Position = Interface.monitor.Position + new Vector2(8, Interface.monitor.Height - Height - 8);
                }
            }
            public override void OnClick()
            {
                base.OnClick();
                if (!Interface.Closing)
                {
                    Interface.CloseInterface(false);
                }
            }
        }
        [Tracked]
        public class InterfaceCursor : Entity
        {
            public InterfaceCursor()
            {
                Tag = TagsExt.SubHUD;
            }
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
            public static Vector2 MousePosition
            {
                get
                {
                    MouseState mouseState = Mouse.GetState();
                    float mouseX = Calc.Clamp(mouseState.X, 0, Engine.ViewWidth);
                    float mouseY = Calc.Clamp(mouseState.Y, 0, Engine.ViewHeight);
                    float scale = (float)Engine.Width / Engine.ViewWidth;
                    Vector2 position = new Vector2(mouseX, mouseY) * scale;
                    return position;
                }
            }

        }
        public class InterfaceBorder : Entity
        {
            public Image Border;
            public Interface Parent;
            public float Alpha
            {
                get
                {
                    return alpha;
                }
                set
                {
                    if (Border is not null)
                    {
                        Border.Color = Color.White * value;
                    }
                    alpha = value;
                }
            }
            private float alpha;
            public const int BorderEdgeWidth = 96;
            public const int BorderEdgeHeight = 60;
            public InterfaceBorder(Interface parent) : base()
            {
                Tag = TagsExt.SubHUD;

                Add(Border = new Image(GFX.Game["objects/PuzzleIslandHelper/interface/border00"]));
                Border.Color = Color.White * 0;
                Depth = BaseDepth - 7;
                Parent = parent;
            }
            public override void Render()
            {
                if (Parent.ForceHide) return;
                base.Render();
            }
        }
    }
}