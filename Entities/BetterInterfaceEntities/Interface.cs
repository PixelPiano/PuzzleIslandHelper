using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{
    [CustomEntity("PuzzleIslandHelper/Interface")]
    [Tracked]
    public class Interface : Entity
    {
        #region Variables
        public static Interface ActiveInstance;
        public static bool Buffering;
        public bool HoldLight;
        private string roomName;
        private float timer = 0;
        public static bool NightMode = true;
        public const int ColliderWidth = 6;
        public const int ColliderHeight = 6;
        private float prevLightAlpha;
        private float prevBloomStrength;
        public static Rectangle MouseRectangle;
        public static bool LeftClicked => Mouse.GetState().LeftButton == ButtonState.Pressed;
        public static bool DraggingWindow = false;
        public bool InControl = false;
        private bool intoIdle = false;
        private bool CanClickIcons = true;
        private bool SetDragOffset = false;
        public static bool Loading = false;
        private string id = "SAMPLE_DIALOG_TEXT";
        public static string CurrentIconName = "invalid";
        public static int BaseDepth = -1000001;
        private InterfaceBorder Border;
        private EventInstance whirringSfx;
        private EventInstance loadSfx;
        private Entity NightDay;
        private Entity Monitor;
        public Cursor cursor;
        private Sprite MonitorSprite;
        private Sprite BorderSprite;
        public Sprite cursorSprite;
        private Sprite PowerSprite;
        private Sprite SunMoon;
        private Entity Power;
        //private LoadSequence loadSequence;
        private FreqTimeline memContent;
        public PipeWindowContent pipeContent;
        public GameOfLife GameOfLife;
        public DigiAccess digiContent;
        private ComputerIcon[] Icons;
        private BetterWindow Window;
        private Level l;
        private Player player;
        private Sprite Machine;
        private Entity MachineEntity;
        private Coroutine accessRoutine;
        public TalkComponent Talk;
        public FloppyHUD FloppyHUD;
        public bool CanCloseWindow;
        public enum Versions
        {
            Lab,
            Pipes
        }
        public Versions Version;
        public static bool MouseOnBounds
        {
            get
            {
                MouseState mouseState = Mouse.GetState();
                float mouseX = Calc.Clamp(mouseState.X, 0, Engine.ViewWidth);
                float mouseY = Calc.Clamp(mouseState.Y, 0, Engine.ViewHeight);
                return mouseX == BorderX || mouseX == Engine.ViewWidth - BorderX || mouseY == BorderY || mouseY == Engine.ViewHeight - BorderY;
            }
        }
        public const int BorderX = 16;
        public const int BorderY = 10;

        public static Vector2 MousePosition
        {
            get
            {
                if (Closing)
                {
                    return Vector2.Zero;
                }
                MouseState mouseState = Mouse.GetState();
                float mouseX = Calc.Clamp(mouseState.X, 0, Engine.ViewWidth);
                float mouseY = Calc.Clamp(mouseState.Y, 0, Engine.ViewHeight);
                float scale = (float)Engine.Width / Engine.ViewWidth;
                Vector2 position = new Vector2(mouseX, mouseY) * scale;
                return position;
            }
        }
        public Vector2 MouseWorldPosition
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
        private Vector2 CursorBoundsA = new Vector2(16, 10);
        private Vector2 CursorBoundsB;
        private Vector2 CursorMiddle = Vector2.One;
        private Vector2 spacing = new Vector2(8, 16); //spacing between the icons
        private Vector2 DragOffset; //used to correctly align Window with cursor while being dragged
        private Color startColor = Color.LimeGreen;
        private Color backgroundColor;
        private float GlitchAmount = 0;
        private float GlitchAmplitude = 100f;
        private Color PlayerTransitionColor = Color.White;
        //the order of icons in sequential order. If string is not a valid icon name, is replaced with the "invalid" symbol when drawn.
        public static List<string> IconIDs = new();
        public static List<string> WindowText = new();
        public static List<string> TabText = new();

        private static bool Closing = false;
        private bool AccessEnding = false;
        private bool GlitchPlayer = false;
        private float MaxGlitchRange = 0.2f;
        private bool RemovePlayer = false;
        private float ColorLerpRate = 0;
        #endregion


        private bool Interacted;
        public static bool Interacting;
        private float SavedAlpha;
        private static VirtualRenderTarget _PlayerObject;
        public static VirtualRenderTarget PlayerObject => _PlayerObject ??= VirtualContent.CreateRenderTarget("PlayerObject", 320, 180);

        private static VirtualRenderTarget _PlayerMask;
        public static VirtualRenderTarget PlayerMask => _PlayerMask ??= VirtualContent.CreateRenderTarget("PlayerMask", 320, 180);


        private static VirtualRenderTarget _Light;
        public static VirtualRenderTarget Light => _Light ??= VirtualContent.CreateRenderTarget("Light", 320, 180);


        public string InstanceID;

        public bool Invalid;

        public void Cleanup(Scene scene)
        {
            IconIDs.Clear();
            WindowText.Clear();
            TabText.Clear();
            _PlayerObject?.Dispose();
            _PlayerMask?.Dispose();
            _Light?.Dispose();
            _PlayerObject = null;
            _Light = null;
            _PlayerMask = null;
            scene.Remove(Power, Window);

            if (Icons is not null)
            {
                foreach (var icon in Icons)
                {
                    if (icon is not null)
                    {
                        scene.Remove(icon);
                    }
                }
            }
            if (Interacting)
            {
                (scene as Level).Lighting.Alpha = prevLightAlpha;
                (scene as Level).Bloom.Strength = prevBloomStrength;
            }
            Interacting = false;
            ActiveInstance = null;
        }
        public void GetPreset(string id)
        {
            IconIDs.Clear();
            WindowText.Clear();
            TabText.Clear();
            InterfaceData.Presets preset = PianoModule.InterfaceData.GetPreset(id);
            if (preset is null)
            {
                Invalid = true;
            }
            else
            {
                foreach (InterfaceData.Presets.IconText text in preset.Icons)
                {
                    IconIDs.Add(text.ID);
                    WindowText.Add(text.Window);
                    TabText.Add(text.Tab);
                }
            }
        }
        public Interface(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Buffering = false;
            Version = data.Enum<Versions>("type");
            Tag |= Tags.TransitionUpdate;
            roomName = data.Attr("teleportTo", "4t");  
            switch (Version)
            {
                case Versions.Lab:
                    backgroundColor = Color.Green;
                    Machine = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/");
                    Machine.FlipX = data.Bool("flipX");
                    Machine.AddLoop("idle", "interface", 0.1f);
                    Machine.AddLoop("noPower", "interfaceNoPower", 0.1f);
                    break;
                case Versions.Pipes:
                    backgroundColor = Color.Orange;
                    Machine = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/pipes/");
                    Machine.FlipX = data.Bool("flipX");
                    Machine.AddLoop("idle", "machine", 0.1f);
                    break;
            }
            startColor = Color.Lerp(backgroundColor, Color.White, 0.1f);

            float talkX = Machine.FlipX ? 8 : 0;
            talkX += Version == Versions.Lab ? 0 : 4;
            float talkYOffset = Version == Versions.Lab ? 8 : -8;

            Add(Talk = new TalkComponent(new Rectangle(0, 0, (int)Machine.Width, (int)Machine.Height - (int)talkYOffset), new Vector2(Machine.Width / 2, 0), Interact));
            if (Version == Versions.Pipes)
            {
                Talk.PlayerMustBeFacing = false;
            }
            Add(new BeforeRenderHook(BeforeRender));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Interacting = false;
            l = scene as Level;
            scene.Add(MachineEntity = new Entity(Position));
            MachineEntity.Depth = 2;
            scene.Add(NightDay = new Entity());
            NightDay.Add(SunMoon = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/icons/"));
            SunMoon.AddLoop("sun", "sun", 0.1f);
            SunMoon.AddLoop("moon", "moon", 0.1f);
            MachineEntity.Add(Machine);
            BetterWindow.Drawing = false;
            scene.Add(new IconText());
            scene.Add(Power = new Entity());
            Power.Add(PowerSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/"));
            PowerSprite.AddLoop("idle", "power", 1f);
            PowerSprite.Play("idle");
            #region Monitor/Border Setup
            scene.Add(Monitor = new Entity());
            scene.Add(Border = new InterfaceBorder());
            Monitor.Add(MonitorSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/"));
            Border.Add(BorderSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/"));
            BorderSprite.AddLoop("idle", "border", 0.1f);
            BorderSprite.Add("fadeIn", "borderIn", 0.1f, "idle");
            MonitorSprite.AddLoop("idle", "idle", 1f);
            MonitorSprite.Add("boot", "startUp", 0.07f, "idle");
            MonitorSprite.AddLoop("off", "off", 0.1f);
            MonitorSprite.Add("turnOff", "shutDown", 0.07f, "off");
            MonitorSprite.SetColor(startColor);
            PowerSprite.Visible = false;
            SunMoon.Visible = false;
            #endregion
            #region Cursor Setup
            scene.Add(cursor = new Cursor());
            cursor.Add(cursorSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/"));
            cursorSprite.AddLoop("idle", "cursor", 1f);
            cursorSprite.AddLoop("pressed", "cursorPress", 1f);
            cursorSprite.AddLoop("buffer", "buffering", 0.1f);
            cursorSprite.Visible = false;
            #endregion

            scene.Add(Window = new BetterWindow(Position, this));
            //loadSequence = new LoadSequence(Window.Depth - 1, Window.Position);

            Depth = BaseDepth;
            Power.Depth = BaseDepth - 1;
            NightDay.Depth = Power.Depth;
            Monitor.Depth = BaseDepth;
            cursor.Depth = BaseDepth - 6;
            Border.Depth = BaseDepth - 7;
            if (Version == Versions.Lab)
            {
                if (PianoModule.Session.RestoredPower && !Invalid)
                {
                    Machine.Play("idle");
                }
                else
                {
                    Machine.Play("noPower");
                    Machine.OnLastFrame = (string s) =>
                    {
                        if (s == "noPower" && PianoModule.Session.RestoredPower)
                        {
                            Machine.Play("idle");
                        }
                    };
                }
            }
            else
            {
                Machine.Play("idle");
            }

        }
        public void AddPrograms(Scene scene, List<string> ids)
        {
            foreach (string id in ids)
            {
                switch (id)
                {
                    case "access": scene.Add(digiContent = new DigiAccess(Window)); break;
                    case "Freq": scene.Add(memContent = new FreqTimeline(Window)); break;
                    case "pipe": scene.Add(pipeContent = new PipeWindowContent(Window)); break;
                    case "life": scene.Add(GameOfLife = new GameOfLife(Window)); break;
                }
            }
        }
        public void AddIcons(Scene scene)
        {
            Icons = new ComputerIcon[IconIDs.Count];

            if (IconIDs.Count > Icons.Length)
            {
                RemoveSelf();
            }
            for (int i = 0; i < Icons.Length; i++)
            {
                scene.Add(Icons[i] = new ComputerIcon(IconIDs[i], WindowText[i], TabText[i]));

                Icons[i].Sprite.Visible = false;
            }

            AddPrograms(scene, IconIDs);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = Scene.Tracker.GetEntity<Player>();
        }
        public static bool MouseOver(Rectangle rect)
        {
            return MouseRectangle.Intersects(rect) && !Buffering;
        }
        public static bool MouseOver(Vector2 position, float width, float height)
        {
            return MouseRectangle.Intersects(new Rectangle((int)position.X, (int)position.Y, (int)width, (int)height)) && !Buffering;
        }
        public static bool MouseOver(Vector2 position)
        {
            return MouseRectangle.Contains(new Point((int)position.X, (int)position.Y)) && !Buffering;
        }
        public static bool MouseOver(Circle circle)
        {
            return Collide.RectToCircle(MouseRectangle, circle.Position, circle.Radius) && !Buffering;
        }
        public static bool MouseOver(Collider collider)
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
                if (!MouseOnBounds)
                {
                    cursor.Position = MousePosition.ToInt();
                }
                //Enforce CursorBoundsA and CursorBoundsB if bounds are exceeded
                cursor.Position.X = cursor.Position.X < CursorBoundsA.X ? CursorBoundsA.X : cursor.Position.X > CursorBoundsB.X ? CursorBoundsB.X : cursor.Position.X;
                cursor.Position.Y = cursor.Position.Y < CursorBoundsA.Y ? CursorBoundsA.Y : cursor.Position.Y > CursorBoundsB.Y ? CursorBoundsB.Y : cursor.Position.Y;
                if (Collider is not null)
                {
                    Collider.Position = new Vector2(Monitor.Position.X - Position.X + cursor.Position.X / 6, Monitor.Position.Y - Position.Y + cursor.Position.Y / 6).ToInt();
                }
                if (LeftClicked && !MouseOnBounds && !Buffering) //if mouse is clicked
                {
                    if (CollideCheck(NightDay) && timer <= 0 && !Closing)
                    {
                        NightMode = !NightMode;
                        string id = NightMode ? "moon" : "sun";
                        SunMoon.Play(id);
                        timer = Engine.DeltaTime * 10;
                    }
                    cursorSprite.Play("pressed"); //play the "click" animation
                    if (!BetterWindow.Drawing)
                    {
                        OnClicked(); //if Window isn't being drawn, run OnClicked
                    }
                    if (Window != null && BetterWindow.Drawing) //if the Window is valid...
                    {
                        if (CollideRect(Window.TabArea))
                        {
                            DraggingWindow = true;
                        }
                        if (CollideCheck(Window.x) && CanCloseWindow && !DraggingWindow)
                        {
                            BetterWindow.Drawing = false;
                        }
                    }
                }
                else
                {
                    DraggingWindow = false;
                    SetDragOffset = false;
                    if (!Buffering)
                    {
                        cursorSprite.Play("idle"); //revert cursor Texture if not being clicked
                    }
                }
                if (DraggingWindow && BetterWindow.Drawing)
                {
                    Window.Position = Collider.Position + GetDragOffset();
                }
            }
            else
            {
                cursor.Position = CursorMiddle; //cursor default position
            }
            #endregion
            base.Update();
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
            if (GlitchPlayer && !RemovePlayer && ColorLerpRate < 1)
            {
                PlayerTransitionColor = Color.Lerp(Color.White, Color.Green, ColorLerpRate += Engine.DeltaTime);
            }

            #region Player check
            if (player == null)
            {
                return;
            }
            #endregion
            if (GlitchPlayer)
            {
                GlitchAmplitude = Calc.Approach(0.01f, 100f, Ease.SineIn(MaxGlitchRange));
                if (MaxGlitchRange < 1)
                {
                    MaxGlitchRange += 0.01f;
                }
            }
            if (MonitorSprite.CurrentAnimationID == "idle" && !intoIdle)
            {
                //Handles icon/cursor transition from screen off to screen on
                Add(new Coroutine(TransitionToMain(), true));
                intoIdle = true;
            }
        }
        public void StartAccessEnding()
        {
            //loadSequence.ButtonPressed = true;
            Loading = true;
            if (!AccessEnding)
            {
                Add(accessRoutine = new Coroutine(AccessRoutine(), true));
            }
        }
        public override void Removed(Scene scene)
        {
            Cleanup(scene);
            base.Removed(scene);
        }
        private Vector2 GetDragOffset()
        {
            if (!SetDragOffset)
            {
                //Get the distance from the tab's position to the cursor's position
                DragOffset = new Vector2(-(Collider.Position.X - Window.Position.X), Window.TabArea.Y - Collider.Position.Y + BetterWindow.tabHeight);
            }
            SetDragOffset = true;
            return DragOffset;
        }
        public void RemoveWindow()
        {
            if (!CanCloseWindow)
            {
                return;
            }
            IconText.CurrentIcon = null;
            Window.Close();
            Add(new Coroutine(WaitForClickRelease(delegate { CanClickIcons = true; })));
        }
        private IEnumerator WaitForClickRelease(Action onReleased = null)
        {
            while (LeftClicked)
            {
                yield return null;
            }
            if (onReleased != null)
            {
                onReleased.Invoke();
            }
        }
        public void ForceClose(bool fast)
        {
            Add(new Coroutine(CloseInterface(fast)));
        }
        private void OnClicked()
        {
            if (Closing)
            {
                return;
            }
            if (CollideCheck(Power) && !DraggingWindow)
            {
                if (!DraggingWindow)
                {
                    foreach (BetterWindowButton button in Window.Buttons)
                    {
                        if (button.Pressing)
                        {
                            return;
                        }
                    }
                    if (!Closing)
                    {
                        Add(new Coroutine(CloseInterface(false), true));
                    }
                }
            }
            #region Check for x button clicked
            if (CollideCheck(Window.x) && CanCloseWindow) //If "close Window" button was clicked
            {
                if (!DraggingWindow)
                {
                    //remove button collider, stop drawing Window, and allow user to click icons again
                    RemoveWindow();
                    return;
                }
            }
            #endregion

            #region Check for icon clicked
            if (CanClickIcons)
            {
                for (int i = 0; i < Icons.Length; i++) //for each icon on screen...
                {
                    if (CollideCheck(Icons[i])) //if mouse is colliding with an icon...
                    {
                        //prevent icons from reacting to clicks, set Window type, and start drawing Window
                        CurrentIconName = Icons[i].Name;
                        IconText.CurrentIcon = Icons[i];
                        CanClickIcons = false;
                        Window.Name = Icons[i].Name; //send the type of Window to draw to Target.cs
                        CanCloseWindow = false;
                        Add(new Coroutine(WaitForClickRelease(delegate { CanCloseWindow = true; })));
                        Window.PrepareWindow();
                        TextWindow.CurrentID = Icons[i].Text;
                        BetterWindow.Drawing = true;
                        return;
                    }

                }
            }
            #endregion
        }
        private IEnumerator BeginSequence(Player player)
        {
            if (Scene is not Level level)
            {
                yield break;
            }
            if (PianoModule.SaveData.CollectedDisks.Count == 0)
            {
                yield break;
            }
            player.StateMachine.State = Player.StDummy;
            level.Add(FloppyHUD = new FloppyHUD());
            yield return FloppyHUD.Sequence();
            if (FloppyHUD is null || FloppyHUD.LeftEarly)
            {
                player.StateMachine.State = Player.StNormal;
                FloppyHUD?.RemoveSelf();
                yield break;
            }
            GetPreset(FloppyHUD.SelectedDisk.Preset);
            FloppyHUD.RemoveSelf();
            AddIcons(level);
            Start();
            yield return null;
        }
        public void Start()
        {
            ActiveInstance = this;
            Interacted = true;
            intoIdle = false;
            MonitorSprite.Visible = true;
            BorderSprite.Visible = true;
            MonitorSprite.SetColor(startColor);
            Monitor.Position = new Vector2(l.Camera.Position.X, l.Camera.Position.Y);
            Border.Position = SceneAs<Level>().Camera.CameraToScreen(Monitor.Position) + Vector2.One;
            NightDay.Collider = new Hitbox(SunMoon.Width * 2, SunMoon.Height * 2);
            SunMoon.Play(NightMode ? "moon" : "sun");

            //set middle of screen and cursor bounds based on Border position
            CursorMiddle = new Vector2(Monitor.Position.X + Monitor.Width / 2, Monitor.Position.Y + Monitor.Height / 2);
            CursorBoundsA = new Vector2(16, 10) * 6;
            CursorBoundsB = new Vector2((MonitorSprite.Width - 11 - (cursorSprite.Width / 6)) * 6, (MonitorSprite.Height - 9 - (cursorSprite.Height / 6)) * 6);

            Power.Collider = new Hitbox(PowerSprite.Width, PowerSprite.Height);
            Power.Position = Monitor.Position + new Vector2(-4, MonitorSprite.Height) - new Vector2(-Power.Width, Power.Height * 2 - 8) + Vector2.UnitY * 2;
            PowerSprite.Play("idle");
            NightDay.Position = Power.Position + Vector2.UnitY * (SunMoon.Height - 8) + (Vector2.UnitX * (MonitorSprite.Width - (SunMoon.Width * 3)));
            #region Set Icon Positions
            Vector2 iconPosition = Monitor.Position + new Vector2(18, 12);

            float iconPositionX = 0;
            bool change = false;
            int row = 0;

            //set the icon positions
            for (int i = 0; i < Icons.Length; i++)
            {
                iconPositionX = change ? 0 : iconPositionX + Icons[i].Width + spacing.X;
                change = iconPositionX > MonitorSprite.Width - 32;
                if (change)
                {
                    row++;
                    iconPosition = Monitor.Position + new Vector2(18, 12 + spacing.Y * row);
                }
                Icons[i].Position = iconPosition;
                Icons[i].Sprite.Play("idle");
                if (!change)
                {
                    iconPosition += new Vector2(Icons[i].Width + spacing.X, 0);
                }
            }

            whirringSfx = Audio.Play("event:/PianoBoy/interface/Whirring", Position, "Computer FlagState", 0);
            #endregion
            BorderSprite.Visible = true;
            MonitorSprite.Play("boot");
            BorderSprite.Play("fadeIn");

            //ScreenCoords collider
            Collider = new Hitbox(ColliderWidth, ColliderHeight, Monitor.Position.X - Position.X + MousePosition.X / 6, Monitor.Position.Y - Position.Y + MousePosition.Y / 6);
        }
        private void Interact(Player player)
        {
            //play click sound
            if (Version == Versions.Lab)
            {
                if (!PianoModule.Session.RestoredPower)
                {
                    return;
                }
            }
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
            ActiveInstance = null;
        }
        private IEnumerator orig_AccessRoutine()
        {
            if (!PianoModule.SaveData.HasArtifact)
            {
                yield break;
            }
            if (!AccessEnding)
            {
                loadSfx = Audio.Play("event:/PianoBoy/interface/Loading");
            }
            else
            {
                loadSfx.start();
            }
            AccessEnding = true;
            while (!LoadSequence.DoneLoading)
            {
                yield return null;
            }
            loadSfx.stop(STOP_MODE.ALLOWFADEOUT);
            Audio.Play("event:/PianoBoy/interface/WindowsFanfare");
            yield return 1.3f;
            //Show loading complete fanfare or something
            //then close the Window, force close the computer, and digitize Madeline away and teleport to specified room
            yield return 0.05f;
            RemoveWindow(/*button*/);
            yield return 0.2f;
            if (!Closing)
            {
                Add(new Coroutine(CloseInterface(false), true));
            }
            Loading = false;
        } //used LoadSequence, made obsolete by DigiAccess
        private IEnumerator AccessRoutine()
        {
            if (!PianoModule.SaveData.HasArtifact) //if not allowed to use program
            {
                yield break;
            }

            AccessEnding = true;
            while (!LoadSequence.DoneLoading) //while searching for valid password
            {
                yield return null;
            }
            yield return Calc.Random.Range(1f, 3f);
            //then close the Window, force close the computer, and digitize Madeline away and teleport to specified room
            yield return 0.05f;
            RemoveWindow(/*button*/);
            yield return 0.2f;
            yield return CloseInterface(false);
            Loading = false;
        }
        private IEnumerator TransitionToMain()
        {
            Interacting = true;
            int count = 0;
            Closing = false;
            BorderSprite.Visible = true;
            InControl = false;
            cursorSprite.Visible = false;
            cursorSprite.Play("idle");
            Level level = Scene as Level;
            prevLightAlpha = level.Lighting.Alpha;
            prevBloomStrength = level.Bloom.Strength;
            HoldLight = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime * 4)
            {
                level.Lighting.Alpha = Calc.LerpClamp(prevLightAlpha, 0, i);
                level.Bloom.Strength = Calc.LerpClamp(prevBloomStrength, 0, i);
                yield return null;
            }
            //Computer logo sequence
            Audio.Play("event:/PianoBoy/interface/InterfaceBootup", Position);
            Audio.SetMusicParam("fade", 0);
            for (float i = 0; i < 1; i += 0.025f)
            {
                for (int j = 0; j < Icons.Length; j++)
                {
                    Icons[j].Sprite.Visible = count % 8 == 0;
                    PowerSprite.Visible = count % 8 == 0;
                    SunMoon.Visible = count % 8 == 0;
                }
                MonitorSprite.SetColor(Color.Lerp(startColor, backgroundColor, Ease.SineInOut(i)));
                count++;
                yield return null;
            }

            cursorSprite.Visible = true;
            SunMoon.Visible = true;
            for (int j = 0; j < Icons.Length; j++)
            {
                Icons[j].Sprite.Visible = true;
            }
            InControl = true;
        }
        public IEnumerator InstantShutdown()
        {
            Closing = true;
            InControl = false;
            RemoveWindow();
            cursorSprite.Play("idle");
            cursorSprite.Visible = false;
            PowerSprite.Visible = false;
            SunMoon.Visible = false;
            for (int j = 0; j < Icons.Length; j++)
            {
                Icons[j].Sprite.Visible = false;
            }
            whirringSfx.setParameterValue("Computer FlagState", 1);
            MonitorSprite.Visible = false;
            BorderSprite.Visible = false;
            Level level = Scene as Level;
            level.Lighting.Alpha = prevLightAlpha;
            level.Bloom.Strength = prevBloomStrength;
            Audio.SetMusicParam("fade", 1);
            Interacting = false;
            player.StateMachine.State = 0;
            yield return null;
        }
        public IEnumerator CloseInterface(bool fast)
        {
            Closing = true;
            int count = 0;
            InControl = false;
            RemoveWindow();
            cursorSprite.Play("idle");
            if (!fast)
            {
                for (float i = 0; i < 1; i += 0.025f)
                {
                    for (int j = 0; j < Icons.Length; j++)
                    {
                        bool visible = count % 8 == 0;
                        Icons[j].Sprite.Visible = visible;
                        cursorSprite.Visible = visible;
                        PowerSprite.Visible = visible;
                        SunMoon.Visible = visible;
                    }
                    count++;
                    yield return null;
                }
            }
            cursorSprite.Visible = PowerSprite.Visible = SunMoon.Visible = false;
            for (int j = 0; j < Icons.Length; j++)
            {
                Icons[j].Sprite.Visible = false;
            }
            whirringSfx.setParameterValue("Computer FlagState", 1);
            MonitorSprite.Play("turnOff");
            Color _color = BorderSprite.Color;
            while (MonitorSprite.CurrentAnimationID == "turnOff")
            {
                BorderSprite.Color *= 0.95f;
                yield return null;
            }
            BorderSprite.Visible = false;
            BorderSprite.SetColor(_color);
            Interacting = false;
            Level level = Scene as Level;
            HoldLight = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime * 4)
            {
                level.Lighting.Alpha = Calc.LerpClamp(0, prevLightAlpha, i);
                level.Bloom.Strength = Calc.LerpClamp(0, prevBloomStrength, i);
                yield return null;
            }
            Audio.SetMusicParam("fade", 1);
            player.StateMachine.State = 0;
            ActiveInstance = null;
            yield return null;
            //RemoveSelf();
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(PlayerObject, l.Camera.Position, Color.White);
        }
        private void BeforeRender()
        {
            if (!GlitchPlayer || player is null)
            {
                return;
            }
            EasyRendering.SetRenderMask(PlayerMask, player, l);
            EasyRendering.DrawToObject(PlayerObject, Drawing, l);
            EasyRendering.MaskToObject(PlayerObject, PlayerMask);
            EasyRendering.AddGlitch(PlayerObject, GlitchAmount, GlitchAmplitude);
        }
        private void Drawing()
        {
            player.Render();
            Draw.Rect(l.Bounds, PlayerTransitionColor);
        }
        private IEnumerator Transition()
        {
            //player effects
            SceneAs<Level>().Add(new TransitionManager(TransitionManager.Type.BeamMeUp, roomName));
            TransitionManager.Finished = false;
            while (!TransitionManager.Finished)
            {
                yield return null;
            }
            yield return null;
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
        [Tracked]
        public class Cursor : Entity
        {

            public Cursor()
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
        private class InterfaceBorder : Entity
        {
            public InterfaceBorder()
            {
                Tag = TagsExt.SubHUD;
            }
        }
    }
}