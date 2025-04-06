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
using System.Linq;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public class InterfaceData
    {
        public class Preset
        {
            public void Add(string id = "", string tab = "", string window = "")
            {
                IconData newIcon = new()
                {
                    ID = id,
                    Tab = tab,
                    Window = window
                };
                Icons.Add(newIcon);
            }
            public class IconData
            {
                public void ParseData()
                {
                    ID ??= "";
                    Tab ??= "";
                    Window ??= "";
                }
                public string ID { get; set; }
                public string Tab { get; set; }
                public string Window { get; set; }
            }
            public List<IconData> Icons { get; set; }
        }

        public Preset Default => Layouts["Default"];
        public Dictionary<string, Preset> Layouts { get; set; }
        public Preset this[string id]
        {
            get => IsValid(id) ? Layouts[id] : null;
            set => Layouts[id] = value;
        }
        public bool IsValid(string id) => Layouts != null && Layouts.ContainsKey(id);

        [OnLoad]
        public static void Load()
        {
            Everest.Content.OnUpdate += Content_OnUpdate;
        }
        [OnUnload]
        public static void Unload()
        {
            Everest.Content.OnUpdate -= Content_OnUpdate;
        }
        private static void Content_OnUpdate(ModAsset from, ModAsset to)
        {
            if (to.Format == "yml" || to.Format == ".yml")
            {
                try
                {

                    AssetReloadHelper.Do("Reloading Interface Presets", () =>
                    {
                        if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/InterfacePresets", out var asset)
                            && asset.TryDeserialize(out InterfaceData myData))
                        {
                            PianoModule.SaveData.InterfaceData = myData;
                        }
                    }, () =>
                    {
                        (Engine.Scene as Level)?.Reload();
                    });

                }
                catch (Exception e)
                {
                    Logger.LogDetailed(e);
                }
            }

        }
    }

    [CustomEntity("PuzzleIslandHelper/Interface")]
    [Tracked]
    public class Interface : Entity
    {
        public enum Priority
        {
            Power,
            Nightmode,
            Icon
        }
        public const int ColliderWidth = 6;
        public const int ColliderHeight = 6;
        public const int BorderX = 16;
        public const int BorderY = 10;
        public const int BaseDepth = -1000001;
        private float prevLightAlpha;
        private float prevBloomStrength;
        private float oobTimer;
        public static bool MouseOutOfBounds
        {
            get
            {
                MouseState mouseState = Mouse.GetState();

                float mouseX = Calc.Clamp(mouseState.X, 0, Engine.ViewWidth);
                float mouseY = Calc.Clamp(mouseState.Y, 0, Engine.ViewHeight);
                return mouseX < BorderX || mouseX > Engine.ViewWidth - BorderX || mouseY < BorderY || mouseY >= Engine.ViewHeight - BorderY;
            }
        }
        public bool FirstFrameClick => leftClicked && !prevLeftClicked;
        public bool MouseFocused => InControl && Engine.Instance.IsActive;
        public bool DraggingWindow;
        public bool UsesStartupMonitor => Machine is not null && Machine.UsesStartupMonitor;
        public bool LeftPressed
        {
            get
            {
                bool value = MouseFocused && Mouse.GetState().LeftButton == ButtonState.Pressed;
                return value && !Buffering;
            }
        }
        public bool InControl;
        public bool Buffering;
        public bool CanCloseWindow;
        public bool NightMode = true;
        public bool HoldLight;
        public bool Interacting;
        public bool MonitorLoaded;
        private bool prevLeftClicked;
        private bool leftClicked;
        private bool intoIdle = false;
        private bool CanClickIcons = true;
        private bool SetDragOffset = false;
        public bool Closing = false;
        public bool InFlickerRoutine;
        public string CurrentIconName = "invalid";
        public string InstanceID;
        public string CurrentPresetID;
        public bool FakeStarting;
        public bool ForceHide = false;

        public Rectangle MouseRectangle;
        public Rectangle MouseBounds;
        public Vector2 MousePosition
        {
            get
            {
                if (!MouseFocused) return prevMousePos;
                if (Closing)
                {
                    return Vector2.Zero;
                }
                MouseState mouseState = Mouse.GetState();
                float mouseX = Calc.Clamp(mouseState.X, 0, Engine.ViewWidth);
                float mouseY = Calc.Clamp(mouseState.Y, 0, Engine.ViewHeight);
                float scale = (float)Engine.Width / Engine.ViewWidth;
                Vector2 position = (new Vector2(mouseX, mouseY) * scale).Clamp(MouseBounds);
                prevMousePos = position;
                return position;
            }
        }
        public Vector2 MouseWorldPosition => Cursor.WorldPosition;
        private Vector2 prevMousePos;
        private Vector2 iconSpacing = new Vector2(8, 16);
        private Vector2 DragOffset;
        public Color BackgroundColor;
        private Color startColor = Color.LimeGreen;

        public MTexture Keyboard = GFX.Game["objects/PuzzleIslandHelper/interface/keyboard"];
        public Monitor Monitor;
        public Power Power;
        public Window Window;

        public Cursor Cursor;
        public Border Border;
        public Machine Machine;
        public DotX3 Talk;
        public FloppyHUD FloppyHUD;
        public SoundSource Whirring;
        public List<Icon> Icons = [];
        public List<WindowContent> Content = [];
        public InterfaceData.Preset CurrentPreset;
        public static InterfaceData Data => PianoModule.SaveData.InterfaceData;

        public Interface(Color background, Machine machine) : base()
        {
            BackgroundColor = background;
            Tag |= Tags.TransitionUpdate;
            Add(Whirring = new SoundSource());
            startColor = Color.Lerp(BackgroundColor, Color.White, 0.1f);
            Machine = machine;
            CurrentPreset = new();
            CurrentPreset.Icons = [];
        }
        //Inherited
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Interacting = false;
            Depth = BaseDepth;
        }
        public override void Update()
        {
            if (Scene is not Level level) return;
            if (HoldLight)
            {
                level.Lighting.Alpha = 0;
                level.Bloom.Strength = 0;
            }
            if (InControl)
            {
                bool mouseFocused = MouseFocused;
                bool mouseOOB = MouseOutOfBounds;
                float add = (mouseFocused && !mouseOOB) || oobTimer > 1f ? 2 : -1.3f;
                oobTimer = mouseFocused && !mouseOOB ? 0 : oobTimer > 1f ? oobTimer + Engine.DeltaTime : oobTimer;
                Cursor.Alpha = Calc.Clamp(Cursor.Alpha + add * Engine.DeltaTime, 0, 1);
                if (mouseFocused && !mouseOOB)
                {
                    Cursor.Position = MousePosition.Floor();
                }
                if (Collider is not null)
                {
                    Collider.Position = (level.Camera.Position - Position + Cursor.Position / 6).Floor();
                }
                prevLeftClicked = leftClicked;
                if (mouseFocused)
                {
                    leftClicked = LeftPressed;
                }
                if (leftClicked && mouseFocused && !mouseOOB)
                {
                    OnClick();
                }
                else
                {
                    DraggingWindow = false;
                    SetDragOffset = false;
                    if (!Buffering)
                    {
                        Cursor.Idle();
                    }
                }
                if (DraggingWindow && Window.Drawing && Window.DraggingEnabled)
                {
                    Window.Position = Collider.Position + GetDragOffset();
                }
            }
            base.Update();
            if (FakeStarting) return;
            if (InControl)
            {
                if (Collider is not null)
                {
                    MouseRectangle = Collider.Bounds;
                }
            }
            if (Buffering)
            {
                Cursor.Buffering();
            }
            if (Monitor is not null && Monitor.Idle && !intoIdle)
            {
                //Handles icon/Cursor transition from screen off to screen on
                Add(new Coroutine(bootUpSequence(), true));
                intoIdle = true;
            }
        }
        public override void Removed(Scene scene)
        {
            Cleanup(scene);
            base.Removed(scene);
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
        //Logic
        private void OnClick()
        {
            Cursor.Pressed();
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
                    CloseWindow();
                    return;
                }
            }
            if (Closing || !FirstFrameClick)
            {
                return;
            }
            #region Check for icon clicked
            bool windowActiveAndNotDrawing = Window is not null && !Window.Drawing;
            List<DesktopComponent> collided = [];
            foreach (DesktopComponent component in Scene.Tracker.GetComponents<DesktopComponent>())
            {
                if (component.CollideCheck(this))
                {
                    if (!(component.Entity is Icon && !CanClickIcons) && !Window.CollidePoint(Position))
                    {
                        collided.Add(component);
                    }
                }
            }
            if (collided.Count > 0)
            {
                collided.OrderBy(item => item.Priority).First().Click();
            }
            #endregion
        }
        public void OpenIcon(Icon icon)
        {
            IconText.CurrentIcon = icon;
            CanCloseWindow = CanClickIcons = false;
            CurrentIconName = Window.Name = icon.Name;
            Add(new Coroutine(waitForClickRelease(delegate { CanCloseWindow = true; })));

            Window.OpenWindow(icon);
            Window.TextWindow.CurrentID = icon.TextID;
            Window.Drawing = true;
        }
        public void CreateAndOpenIcon(string name, string textID, string tabText = null, string preset = null)
        {
            Icon icon = new Icon(this, name, textID, tabText);
            OpenIcon(icon);
        }
        public static void AddToPreset(Icon icon, string preset)
        {
            AddToPreset(preset, icon.Name, icon.TabText, icon.TextID);
        }
        [Command("addtopreset", "")]
        public static void AddToPreset(string preset, string name, string tab, string window)
        {
            if (Data.IsValid(preset))
            {
                Data[preset].Add(name, tab, window);
            }
        }

        public bool TryLoadPreset(string id)
        {
            if (Data[id] is InterfaceData.Preset preset)
            {
                CurrentPreset = new InterfaceData.Preset();
                CurrentPreset.Icons = [.. preset.Icons];
                CurrentPresetID = id;
                return true;
            }
            return false;
            /*            if (preset is null)
                        {
                            CurrentPresetID = "";
                            return false;
                        }
                        else if (CurrentPresetID == id)
                        {
                            return false;
                        }
                        else
                        {
                            CurrentPresetID = id;
                            CurrentPreset.Icons.Clear();
                            foreach (InterfaceData.Preset.IconData data in preset.Icons)
                            {
                                CurrentPreset.Icons.Add(data);
                            }
                            return true;
                        }*/
        }
        public void QuickLoadPreset(string preset)
        {
            if (TryLoadPreset(preset))
            {
                Add(new Coroutine(reloadIcons()));
            }
        }
        public void ForceClose(bool fast)
        {
            Add(new Coroutine(ShutDown(fast)));
        }
        public void CloseInterface(bool fast, bool lockPlayer = false)
        {
            Add(new Coroutine(ShutDown(fast, lockPlayer)));
        }
        public void SetIconAlpha(float alpha)
        {
            foreach (Icon icon in Icons)
            {
                icon.Alpha = alpha;
            }
        }
        public void Start()
        {
            if (Scene is not Level level) return;
            PianoModule.Session.Interface = this;
            intoIdle = false;
            Monitor.Position = level.Camera.Position + new Vector2(16, 10);
            Border.Position = level.Camera.CameraToScreen(level.Camera.Position) + Vector2.One;
            if (!FakeStarting)
            {
                foreach (DesktopComponent d in Scene.Tracker.GetComponents<DesktopComponent>())
                {
                    d.Prepare(level);
                }
                foreach (DesktopComponent d in Scene.Tracker.GetComponents<DesktopComponent>())
                {
                    d.Begin(level);
                }
                SetIconInitialPositions(Icons);
            }
            StartWhirring();
            Add(new Coroutine(turnOnMonitor()));
            Vector2 pos = Monitor.Position - Position + MousePosition / 6;
            Collider = new Hitbox(ColliderWidth, ColliderHeight, pos.X, pos.Y);
            MouseBounds.Width = ((int)Monitor.Width - (int)Width) * 6;
            MouseBounds.Height = ((int)Monitor.Height - (int)Height) * 6;
            MouseBounds.X = (int)(Monitor.X - level.Camera.X) * 6;
            MouseBounds.Y = (int)(Monitor.Y - level.Camera.Y) * 6;
        }
        public void StartWhirring()
        {
            Whirring.Play("event:/PianoBoy/interface/Whirring", "Computer state", 0);
        }
        public void StopWhirring(bool instant = false)
        {
            if (instant)
            {
                Whirring.Stop();
            }
            else if (Whirring.Playing)
            {
                Whirring.Param("Computer state", 1);
            }
        }
        public void StartWithPreset(string preset)
        {
            if (string.IsNullOrEmpty(preset) || Scene is not Level level) return;
            LoadModules(level); //see image 2
            TryLoadPreset(preset);
            CreateAndAddIcons(level);
            Start();
        }
        public void CloseWindow()
        {
            if (Window is null || !CanCloseWindow)
            {
                return;
            }
            DraggingWindow = false;
            SetDragOffset = false;
            IconText.CurrentIcon = null;
            Window.Close();
            Add(new Coroutine(waitForClickRelease(delegate { CanClickIcons = true; })));
        }
        public void ShowIcons()
        {
            foreach (DesktopComponent component in Scene.Tracker.GetComponents<DesktopComponent>())
            {
                component.Entity.Visible = true;
            }
            Cursor.Visible = true;
        }
        public void HideIcons()
        {
            foreach (DesktopComponent component in Scene.Tracker.GetComponents<DesktopComponent>())
            {
                component.Entity.Visible = false;
            }
            Cursor.Visible = false;
        }

        //Utility Functions
        public void Cleanup(Scene scene)
        {
            LightCleanup();
            if (Interacting)
            {
                SetLightAmount(scene as Level, 1);
            }
        }
        public void LightCleanup(bool full = false, bool empty = false)
        {
            if (!empty)
            {
                CurrentPreset.Icons.Clear();
                Window?.RemoveSelf();
                Cursor?.RemoveSelf();
                foreach (DesktopComponent component in Scene.Tracker.GetComponents<DesktopComponent>())
                {
                    component.Entity.RemoveSelf();
                }
                Icons.Clear();
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
        private void SetLightAmount(Level level, float amount)
        {
            level.Lighting.Alpha = Calc.LerpClamp(0, prevLightAlpha, amount);
            level.Bloom.Strength = Calc.LerpClamp(0, prevBloomStrength, amount);
        }
        public Vector2 GetDragOffset()
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

        //Loading
        private void SetIconInitialPositions(IEnumerable<Icon> icons)
        {
            if (Icons.Count == 0) return;
            Vector2 start = Monitor.Position + Vector2.One * 2;
            float limit = 320 - 32;
            float x = 0;
            float y = 0;
            Vector2 space = iconSpacing;

            foreach (Icon icon in icons)
            {
                icon.Position = start + new Vector2(x, y);
                x += icon.Width + space.X;
                if (x > limit)
                {
                    x = 0;
                    y += 12 + space.Y;
                }
            }
        }
        public void CreateAndAddIcons(Scene scene)
        {
            for (int i = 0; i < CurrentPreset.Icons.Count; i++)
            {
                Icon icon = new Icon(this, CurrentPreset.Icons[i]);
                scene.Add(icon);
                Icons.Add(icon);
                icon.Visible = false;
            }
            LoadPrograms(Icons);
        }
        public void AddProgram<T>(T content) where T : WindowContent
        {
            Content.Add(content);
        }
        public void LoadModules(Scene scene)
        {
            if (!FakeStarting)
            {
                scene.Add(Window = new Window(Position, this));
                scene.Add(new IconText(this)); //
                scene.Add(Cursor = new Cursor());
                scene.Add(Power = new Power(this));
                scene.Add(new Nightmode(this));
                Cursor.Visible = true;
                Cursor.Color = Color.White;
                Cursor.Alpha = 0;
                Cursor.Depth = BaseDepth - 6;
                Window.Drawing = false;
            }
            if (Machine.UsesFloppyLoader) scene.Add(new FloppyLoader(this));
            scene.Add(Border = new Border(this));
            scene.Add(Monitor = new Monitor(startColor, this));

        }
        public void LoadPrograms(IEnumerable<Icon> icons)
        {
            foreach (Icon icon in icons)
            {
                LoadProgram(icon);
            }
        }
        public void LoadProgram(Icon icon)
        {
            ProgramLoader.LoadCustomProgram(icon.Name, Window, Scene as Level);
        }
        public void LoadProgram(string name)
        {
            ProgramLoader.LoadCustomProgram(name, Window, Scene as Level);
        }
        public WindowContent GetProgram(Icon icon)
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

        //Collision
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

        //Routines
        private IEnumerator reloadIcons()
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
            Icons.Clear();
            Content.Clear();
            CreateAndAddIcons(Scene);
            ShowIcons();
            SetIconInitialPositions(Icons);

            for (float i = 0; i < 1; i += Engine.DeltaTime * 2)
            {
                SetIconAlpha(i);
                yield return null;
            }

            SetIconAlpha(1);
            InControl = true;
        }
        private IEnumerator waitForClickRelease(Action onReleased = null)
        {
            while (LeftPressed)
            {
                yield return null;
            }
            onReleased?.Invoke();
        }
        private IEnumerator turnOnMonitor()
        {
            PianoModule.Session.MonitorActivated = true;
            if (UsesStartupMonitor)
            {
                yield return ScreenIconAnimation(true);
                yield return 0.3f;
            }
            Border.Visible = true;
            Monitor.Visible = true;
            Monitor.Alpha = Border.Alpha = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Monitor.Alpha = i;
                Border.Alpha = i;
                yield return null;
            }
            Monitor.Alpha = Border.Alpha = 1;
            HoldLight = true;
            SetLightAmount(Scene as Level, 0);
            yield return 0.2f;
            Monitor.StartAnimation();
            yield return null;
        }
        private IEnumerator turnOffMonitor(bool fast)
        {
            Monitor.EndAnimation();
            Machine.OnMonitorOff();
            while (!fast && Monitor.TurningOff)
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
                    Monitor.Alpha = amount;
                    yield return null;
                }
            }
            Machine.OnEnd();
            Monitor.Alpha = Border.Alpha = 0;
            Border.Visible = false;
            Monitor.Visible = false;
            Interacting = false;
        }
        private IEnumerator bootUpSequence()
        {
            Interacting = true;
            yield return null;
            int count = 0;
            Closing = false;
            Border.Visible = true;
            InControl = false;
            if (!FakeStarting)
            {
                Cursor.Visible = false;
                Cursor.Idle();
            }
            //Computer logo sequence
            Audio.Play("event:/PianoBoy/interface/InterfaceBootup", Position);
            Audio.SetMusicParam("fade", 0);
            for (float i = 0; i < 1; i += 0.025f)
            {
                if (!FakeStarting)
                {
                    bool state = count % 8 == 0;

                    foreach (DesktopComponent component in Scene.Tracker.GetComponents<DesktopComponent>())
                    {
                        component.Entity.Visible = state;
                    }
                }
                Monitor.Sprite.SetColor(Color.Lerp(startColor, BackgroundColor, Ease.SineInOut(i)));
                count++;
                yield return null;
            }
            if (!FakeStarting)
            {
                ShowIcons();
                InControl = true;
            }
            MonitorLoaded = true;
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
        }
        public IEnumerator FakeStart()
        {
            if (Scene is not Level level) yield break;
            FakeStarting = true;
            LoadModules(level);
            Start();
            while (Monitor is null || !Monitor.Idle)
            {
                yield return null;
            }
            //Handles icon/Cursor transition from screen off to screen on
            yield return bootUpSequence();

            yield return 1f;
            yield return Textbox.Say("interfaceNoFloppy");
            yield return 0.1f;
            yield return ShutDown(false, false);
            FakeStarting = false;
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
        public IEnumerator ShutDown(bool fast, bool lockPlayer = false)
        {
            Closing = true;
            InControl = false;
            if (!FakeStarting)
            {
                CloseWindow();
                Cursor.Idle();
                if (!fast) yield return FlickerIcons(true);
            }

            if (!UsesStartupMonitor)
            {
                StopWhirring();
            }

            yield return turnOffMonitor(fast);
            Audio.SetMusicParam("fade", 1);
            if (Scene.GetPlayer() is Player player)
            {
                player.StateMachine.State = lockPlayer ? Player.StDummy : 0;
            }
            LightCleanup(false, FakeStarting);
            if (UsesStartupMonitor)
            {
                yield return 0.8f;
                StopWhirring();
                yield return ScreenIconAnimation(false);
            }
            yield return null;
        }
        public IEnumerator ScreenIconAnimation(bool state)
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
        //Teleporting
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
    }
}