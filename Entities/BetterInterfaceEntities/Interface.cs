using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using FMOD.Studio;
using FrostHelper.Helpers;
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


        public const int ColliderWidth = 6;
        public const int ColliderHeight = 6;
        public const int BorderX = 16;
        public const int BorderY = 10;
        public const int BaseDepth = -1000001;

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
        public bool LeftClicked => Mouse.GetState().LeftButton == ButtonState.Pressed;
        private bool intoIdle = false;
        private bool CanClickIcons = true;
        private bool SetDragOffset = false;
        private bool Closing = false;
        private bool GlitchPlayer = false;
        private bool RemovePlayer = false;

        public string CurrentIconName = "invalid";

        public Rectangle MouseRectangle;

        private Entity NightDay;
        private Entity Monitor;
        private Entity Power;
        private Entity MachineEntity;

        public AccessProgram AccessProgram;
        public BetterWindow Window;
        public InterfaceCursor Cursor;
        private InterfaceBorder Border;
        private Sprite MonitorSprite;
        private Sprite BorderSprite; //change to image
        public Sprite cursorSprite; //change to image
        private Sprite PowerSprite; //change to image
        private Sprite SunMoon; //change to image
        private Sprite Machine;
        private SoundSource whirringSfx;
        private FreqProgram FreqProgram;
        public PipeProgram PipeProgram;
        public FountainProgram FountainProgram;
        public GameOfLifeProgram GameOfLifeProgram;
        private ComputerIcon[] Icons;
        public List<WindowContent> Content = new();
        private Level level;
        private Player player;
        public DotX3 Talk;
        public FloppyHUD FloppyHUD;

        public enum Versions
        {
            Lab,
            Pipes
        }
        public Versions Version;


        public Vector2 MousePosition
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
        private Vector2 DragOffset; //used to correctly align Window with Cursor while being dragged
        private Color startColor = Color.LimeGreen;
        private Color backgroundColor;
        //the order of icons in sequential order. If string is not a valid icon name, is replaced with the "invalid" symbol when drawn.
        public List<string> IconIDs = new();
        public List<string> WindowText = new();
        public List<string> TabText = new();

        public string InstanceID;


        public MTexture Keyboard = GFX.Game["objects/PuzzleIslandHelper/interface/keyboard"];

        public void LightCleanup(Scene scene, bool full = false)
        {
            IconIDs.Clear();
            WindowText.Clear();
            TabText.Clear();
            Power?.RemoveSelf();
            Window?.RemoveSelf();
            Cursor?.RemoveSelf();
            SunMoon?.RemoveSelf();

            foreach (WindowContent content in /*scene.Tracker.GetEntities<WindowContent>()*/Content)
            {
                switch (content.Name)
                {
                    default:
                        break;
                }
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
            if (Icons is not null)
            {
                foreach (var icon in Icons)
                {
                    icon?.RemoveSelf();
                }
            }
            Interacting = false;
            PianoModule.Session.Interface = null;
        }
        public void Cleanup(Scene scene)
        {
            LightCleanup(scene);
            if (Interacting)
            {
                (scene as Level).Lighting.Alpha = prevLightAlpha;
                (scene as Level).Bloom.Strength = prevBloomStrength;
            }
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
            Add(whirringSfx = new SoundSource());
            switch (Version)
            {
                case Versions.Lab:
                    backgroundColor = Color.Green;
                    Machine = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/");
                    Machine.FlipX = data.Bool("flipX");
                    Machine.AddLoop("idle", "keyboard", 0.1f);
                    break;
                case Versions.Pipes:
                    backgroundColor = Color.Orange;
                    Machine = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/pipes/");
                    Machine.FlipX = data.Bool("flipX");
                    Machine.AddLoop("idle", "machine", 0.1f);
                    break;
            }
            startColor = Color.Lerp(backgroundColor, Color.White, 0.1f);

            float talkYOffset = Version == Versions.Lab ? 8 : -8;

            Add(Talk = new DotX3(0, 0, Machine.Width, Machine.Height - (int)talkYOffset, new Vector2(Machine.Width / 2, 0), Interact));
            Talk.PlayerMustBeFacing = false;

        }
        public void LoadModules(Scene scene)
        {
            //can you tell this code used to be bad
            scene.Add(Window = new BetterWindow(Position, this));
            scene.Add(new IconText(this));

            //InterfaceCursor and Border are just generic entities but with tags that tell them to render in screen-space rather than world-space
            scene.Add(Cursor = new InterfaceCursor());
            scene.Add(Border = new InterfaceBorder());

            //since the game renders entities in order based on their depth
            //these three generic entities have to be made, since sprites don't have a depth field of their own.
            scene.Add(Power = new Entity());
            scene.Add(NightDay = new Entity());
            scene.Add(Monitor = new Entity());

            //create sprites and add them to their corresponding entities
            string path = "objects/PuzzleIslandHelper/interface/";
            NightDay.Add(SunMoon = new Sprite(GFX.Game, path));
            Power.Add(PowerSprite = new Sprite(GFX.Game, path));
            Cursor.Add(cursorSprite = new Sprite(GFX.Game, path));
            Monitor.Add(MonitorSprite = new Sprite(GFX.Game, path));
            Border.Add(BorderSprite = new Sprite(GFX.Game, path));
            //add animations to the sprites
            
            SunMoon.AddLoop("sun", "sun", 0.1f);
            SunMoon.AddLoop("moon", "moon", 0.1f);
            PowerSprite.AddLoop("idle", "power", 1f);
            MonitorSprite.AddLoop("idle", "idle", 1f);
            MonitorSprite.AddLoop("off", "off", 0.1f);
            BorderSprite.AddLoop("idle", "border", 0.1f);
            cursorSprite.AddLoop("idle", "Cursor", 1f);
            cursorSprite.AddLoop("pressed", "cursorPress", 1f);
            cursorSprite.AddLoop("buffer", "buffering", 0.1f);

            BorderSprite.Add("fadeIn", "borderIn", 0.1f, "idle");
            MonitorSprite.Add("boot", "startUp", 0.07f, "idle");
            MonitorSprite.Add("turnOff", "shutDown", 0.07f, "off");

            MonitorSprite.SetColor(startColor);

            //the "screen" hasn't started up yet, so don't draw certain sprites until it has
            cursorSprite.Visible = PowerSprite.Visible = SunMoon.Visible = false;

            //set depth values for rendering
            Power.Depth = BaseDepth - 1;
            NightDay.Depth = Power.Depth;
            Monitor.Depth = BaseDepth;
            Cursor.Depth = BaseDepth - 6;
            Border.Depth = BaseDepth - 7;

            Window.Drawing = false;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Interacting = false;
            level = scene as Level;
            Depth = BaseDepth;
            scene.Add(MachineEntity = new Entity(Position));
            MachineEntity.Depth = 2;
            MachineEntity.Add(Machine);
            Machine.Play("idle");
        }
        public void AddPrograms(Scene scene, List<string> ids)
        {

            foreach (string id in ids)
            {
                switch (id.ToLower())
                {
                    case "access":
                        if (AccessProgram != null)
                        {
                            AccessProgram.Window = Window;
                            break;
                        }
                        scene.Add(AccessProgram = new AccessProgram(Window));
                        break;
                    case "freq":
                        scene.Add(FreqProgram = new FreqProgram(Window));
                        break;
                    case "pipe":
                        if (PipeProgram != null)
                        {
                            PipeProgram.Window = Window;
                            break;
                        }
                        scene.Add(PipeProgram = new PipeProgram(Window));
                        break;
                    case "gameoflife":
                        scene.Add(GameOfLifeProgram = new GameOfLifeProgram(Window));
                        break;
                    case "fountain":
                        scene.Add(FountainProgram = new FountainProgram(Window));
                        break;
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
                scene.Add(Icons[i] = new ComputerIcon(this, IconIDs[i], WindowText[i], TabText[i]));
                Icons[i].Visible = false;
            }

            AddPrograms(scene, IconIDs);
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
                if (!MouseOnBounds)
                {
                    Cursor.Position = MousePosition.ToInt();
                }
                //Enforce CursorBoundsA and CursorBoundsB if bounds are exceeded
                Cursor.Position.X = Cursor.Position.X < CursorBoundsA.X ? CursorBoundsA.X : Cursor.Position.X > CursorBoundsB.X ? CursorBoundsB.X : Cursor.Position.X;
                Cursor.Position.Y = Cursor.Position.Y < CursorBoundsA.Y ? CursorBoundsA.Y : Cursor.Position.Y > CursorBoundsB.Y ? CursorBoundsB.Y : Cursor.Position.Y;
                if (Collider is not null)
                {
                    Collider.Position = new Vector2(Monitor.Position.X - Position.X + Cursor.Position.X / 6, Monitor.Position.Y - Position.Y + Cursor.Position.Y / 6).ToInt();
                }
                if (LeftClicked && !MouseOnBounds && !Buffering) //If the mouse is clicked
                {
                    if (CollideCheck(NightDay) && timer <= 0 && !Closing)
                    {
                        NightMode = !NightMode;
                        SunMoon.Play(NightMode ? "moon" : "sun");
                        timer = Engine.DeltaTime * 10;
                    }
                    cursorSprite.Play("pressed"); //play the "click" animation
                    if (!Window.Drawing)
                    {
                        OnClicked(); //if Window isn't being drawn, run OnClicked
                    }
                    if (Window != null && Window.Drawing) //if the Window is valid...
                    {
                        if (CollideRect(Window.TabArea))
                        {
                            DraggingWindow = true;
                        }
                        if (CollideCheck(Window.x) && CanCloseWindow && !DraggingWindow)
                        {
                            Window.Drawing = false;
                        }
                    }
                }
                else
                {
                    DraggingWindow = false;
                    SetDragOffset = false;
                    if (!Buffering)
                    {
                        cursorSprite.Play("idle"); //revert Cursor Texture if not being clicked
                    }
                }
                if (DraggingWindow && Window.Drawing)
                {
                    Window.Position = Collider.Position + GetDragOffset();
                }
            }
            else if (Cursor is not null)
            {
                Cursor.Position = CursorMiddle; //Cursor default position
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
            if (player is not null)
            {
                if (MonitorSprite is not null && MonitorSprite.CurrentAnimationID == "idle" && !intoIdle)
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
            onReleased?.Invoke();
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
            if (!DraggingWindow && CollideCheck(Power))
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
            #region Check for x button clicked
            if (CanCloseWindow && CollideCheck(Window.x)) //If "close Window" button was clicked
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
            if (CanClickIcons && Window is not null && !Window.Drawing)
            {
                for (int i = 0; i < Icons.Length; i++) //for each icon on screen...
                {
                    if (CollideCheck(Icons[i])) //if mouse is colliding with an icon...
                    {
                        OpenIcon(Icons[i]);
                        return;
                    }

                }
            }
            #endregion
        }
        public void OpenIcon(ComputerIcon icon)
        {
            IconText.CurrentIcon = icon;
            CanCloseWindow = CanClickIcons = false;
            CurrentIconName = Window.Name = icon.Name;
            Add(new Coroutine(WaitForClickRelease(delegate { CanCloseWindow = true; })));
            Window.PrepareWindow();
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
        private IEnumerator BeginSequence(Player player)
        {
            if (Scene is not Level level) yield break;
            if (PianoModule.Session.CollectedDisks.Count == 0 && Version == Versions.Lab) //if it's the computer in the lab area
            {
                if (!PianoModule.Session.HasFirstFloppy) //if player has not collected the floppy disk inside the computer by default
                {
                    yield return CollectFirstFloppyDisk(player); //play a cutscene where the player collects it
                }
                yield break; //abort the rest of the interaction 
            }

            player.StateMachine.State = Player.StDummy; //prevent the player from moving
            string preset = "";

            //currently overengineered switch statement
            //just in case I add more versions of this entity in the future
            switch (Version)
            {
                case Versions.Lab:
                    //add the floppy disk selection hud to the level
                    //then play the floppy disk select sequence and don't procede until it's finished
                    level.Add(FloppyHUD = new FloppyHUD());
                    yield return FloppyHUD.Sequence();

                    //if entity is removed or the player backs out of the hud
                    //reset the player's state back to normal and ensure the hud gets removed from the scene, then abort the interaction
                    if (FloppyHUD is null || FloppyHUD.LeftEarly)
                    {
                        player.StateMachine.State = Player.StNormal;
                        FloppyHUD?.RemoveSelf();
                        yield break;
                    }
                    //assign the selected preset name to our variable
                    preset = FloppyHUD.SelectedDisk.Preset;
                    break;
                case Versions.Pipes:
                    preset = "Pipes"; //the best switch case known to man
                    break;
            }
            if (string.IsNullOrEmpty(preset)) yield break;

            PianoModule.Session.Interface = this; //safety check for the next couple of methods

            //create and add sprites, assign depth values so everything is displayed correctly
            LoadModules(level); //see image 2

            //load programs associated with the preset name (see image 3)
            GetPreset(preset);
            FloppyHUD?.RemoveSelf(); //remove the floppyhud from the scene if it exists

            //add icons and programs to scene, but don't render them yet
            AddIcons(level);
            //play start up animations, audio, set up colliders for icons, etc.
            Start();
            yield return null;
        }
        public void Start()
        {
            //setting various variables, adding sprites, setting position of icons, etc.
            PianoModule.Session.Interface = this;
            intoIdle = false;
            Interacting = Interacted = BorderSprite.Visible = MonitorSprite.Visible = true;

            MonitorSprite.SetColor(startColor);
            Monitor.Position = level.Camera.Position;
            Border.Position = level.Camera.CameraToScreen(Monitor.Position) + Vector2.One;
            NightDay.Collider = new Hitbox(SunMoon.Width * 2, SunMoon.Height * 2);
            SunMoon.Play(NightMode ? "moon" : "sun");

            //set middle of screen and Cursor bounds based on Border position
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
                if (!change)
                {
                    iconPosition += new Vector2(Icons[i].Width + spacing.X, 0);
                }
            }

            whirringSfx.Play("event:/PianoBoy/interface/Whirring", "Computer State", 0);
            #endregion
            Add(new Coroutine(MonitorSequence()));
            //ScreenCoords collider
            Collider = new Hitbox(ColliderWidth, ColliderHeight, Monitor.Position.X - Position.X + MousePosition.X / 6, Monitor.Position.Y - Position.Y + MousePosition.Y / 6);
        }
        private IEnumerator MonitorSequence()
        {
            PianoModule.Session.MonitorActivated = true;
            if (Version == Versions.Lab)
            {
                yield return MonitorIconAnim(true);
                yield return 0.3f;
            }
            BorderSprite.Visible = true;
            MonitorSprite.Play("boot");
            BorderSprite.Play("fadeIn");
            yield return null;
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
            PianoModule.Session.Interface = null;
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
                    Icons[j].Visible = count % 8 == 0;
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
                Icons[j].Visible = true;
            }
            InControl = true;
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
            cursorSprite.Visible = PowerSprite.Visible = SunMoon.Visible = visible;
            for (int j = 0; j < Icons.Length; j++)
            {
                Icons[j].Visible = visible;
            }
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
            whirringSfx.Param("Computer State", 1);
            MonitorSprite.Play("turnOff");
            Color _color = BorderSprite.Color;
            while (MonitorSprite.CurrentAnimationID == "turnOff" && !fast)
            {
                BorderSprite.Color *= 0.95f;
                yield return null;
            }
            BorderSprite.Visible = false;
            MonitorSprite.Visible = false;
            BorderSprite.SetColor(_color);
            Interacting = false;
        }
        private IEnumerator RestoreLights(bool fast)
        {
            Level level = Scene as Level;
            HoldLight = false;
            if (!fast)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime * 4)
                {
                    level.Lighting.Alpha = Calc.LerpClamp(0, prevLightAlpha, i);
                    level.Bloom.Strength = Calc.LerpClamp(0, prevBloomStrength, i);
                    yield return null;
                }
            }
            level.Lighting.Alpha = prevLightAlpha;
            level.Bloom.Strength = prevBloomStrength;
        }
        public IEnumerator CloseInterface(bool fast, bool lockPlayer = false)
        {
            Closing = true;
            InControl = false;
            RemoveWindow();
            cursorSprite.Play("idle");
            if (!fast) yield return FlickerIcons(true);
            yield return ScreenOff(fast);
            yield return RestoreLights(fast);
            Audio.SetMusicParam("fade", 1);
            player.StateMachine.State = lockPlayer ? Player.StDummy : 0;
            LightCleanup(Scene);
            if (Version == Versions.Lab)
            {
                yield return 0.8f;
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
        private class InterfaceBorder : Entity
        {
            public InterfaceBorder()
            {
                Tag = TagsExt.SubHUD;
            }
        }
    }
}