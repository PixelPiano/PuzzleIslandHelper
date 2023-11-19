using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Windows;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Interface")]
    [Tracked]
    public class Interface : Entity
    {
        #region Variables
        private string roomName;
        private TransitionManager chooser;
        private float timer = 0;
        public static bool NightMode = true;
        public static bool LeftClicked
        {
            get
            {
                MouseState state = Mouse.GetState();
                return state.LeftButton == ButtonState.Pressed;
            }
        }
        public static bool DraggingWindow = false;
        private bool inControl = false;
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
        private Entity cursor;
        private Sprite MonitorSprite;
        private Sprite BorderSprite;
        private Sprite cursorSprite;
        private Sprite PowerSprite;
        private Sprite SunMoon;
        private Entity Power;
        private Stool stool;
        private LoadSequence loadSequence;
        private MemoryWindowContent memContent;
        private readonly ComputerIcon[] Icons;
        private Window window;
        private Level l;
        private Player player;
        private Sprite Machine;
        private Entity MachineEntity;
        private Coroutine accessRoutine;
        private Coroutine destructRoutine;
        public static bool MouseOnBounds = false;

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
                MouseOnBounds = mouseX == 0 || mouseX == Engine.ViewWidth || mouseY == 0 || mouseY == Engine.ViewHeight;
                float scale = (float)Engine.Width / Engine.ViewWidth;
                Vector2 position = new Vector2(mouseX, mouseY) * scale;
                return position;
            }
        }
        private Vector2 CursorBoundsA = new Vector2(16, 10);
        private Vector2 CursorBoundsB;
        private Vector2 CursorMiddle = Vector2.One;
        private Vector2 spacing = new Vector2(8, 16); //spacing between the icons
        private Vector2 DragOffset; //used to correctly align window with cursor while being dragged
        private Color startColor = Color.LimeGreen;
        private float GlitchAmount = 0;
        private float GlitchAmplitude = 100f;
        private Color PlayerTransitionColor = Color.White;
        //the order of icons in sequential order. If string is not a valid icon name, is replaced with the "invalid" symbol when drawn.
        public static string[] iconNames = { "unknown", "text", "pico", "info", "folder", "invalid", "access", "sus", "ram" };
        public static string[] textIDs = { "TODO", "INFO", "ACCESS" };
        public static string[] iconText = { "a", "aa", "Aaa", "aaaa", "aaaaa", "aaaaaa", "aaa", "aaaa", "aaa" };

        private static bool Closing = false;
        private bool AccessEnding = false;
        private bool DestructEnding = false;
        private int DesktopInstance = 0;
        private bool GlitchPlayer = false;
        private float MaxGlitchRange = 0.2f;
        private bool RemovePlayer = false;
        private float ColorLerpRate = 0;
        #endregion
        private bool Interacted;
        private float SavedAlpha;
        private string filename = "ModFiles/PuzzleIslandHelper/InterfacePresets";
        private static VirtualRenderTarget _PlayerObject;
        public static VirtualRenderTarget PlayerObject => _PlayerObject ??= VirtualContent.CreateRenderTarget("PlayerObject", 320, 180);

        private static VirtualRenderTarget _PlayerMask;
        public static VirtualRenderTarget PlayerMask => _PlayerMask ??= VirtualContent.CreateRenderTarget("PlayerMask", 320, 180);


        private static VirtualRenderTarget _Light;
        public static VirtualRenderTarget Light => _Light ??= VirtualContent.CreateRenderTarget("Light", 320, 180);

        public static string ReadModAsset(string filename)
        {
            return Everest.Content.TryGet(filename, out var asset) ? ReadModAsset(asset) : null;
        }
        public static string ReadModAsset(ModAsset asset)
        {
            using var reader = new StreamReader(asset.Stream);

            return reader.ReadToEnd();
        }
        private void AddRandom()
        {
            string content = Everest.Content.TryGet(filename, out var asset) ? ReadModAsset(asset) : null;
            string[] array = content.Split('\n');
            string toAdd = "";
            foreach (string s in array)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    if (!string.IsNullOrWhiteSpace(toAdd))
                    {
                        //toAdd = toAdd.Replace('1', TileType);
                        //RandomList.Add(toAdd);
                    }
                    toAdd = "";
                    continue;
                }
                //RealWidth = s.Length * 8;
                toAdd += s + '\n';
            }
            //RealHeight = array.Length * 8;
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
        public Interface(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            roomName = data.Attr("teleportTo", "4t");
            DesktopInstance = data.Int("instance");
            switch (DesktopInstance)
            {
                case 0:
                    iconNames = new string[] { "unknown", "text", "pico", "info", "folder", "invalid", "access", "sus", "ram" };
                    textIDs = new string[] { "TODO", "INFO", "ACCESS" };
                    iconText = new string[] { "a", "aa", "Aaa", "aaaa", "aaaaa", "aaaaaa", "aaa", "aaaa", "aaa" };
                    break;

                case 1:
                    iconNames = new string[] { "text", "text", "text", "info", "access" };
                    textIDs = new string[] { "TEXT1", "TEXT2", "TEXT4", "INFO1", "ACCESS" };
                    iconText = new string[] { "NT1", "NT2", "NT4", "NI1", "NACCESS" };
                    break;

                case 2:
                    iconNames = new string[] { "unknown", "unknown", "text", "ram", "text", "access" };
                    textIDs = new string[] { "TEXT3", "SCHOOLNOTEHINT", "ACCESS" };
                    iconText = new string[] { "NU1", "NU2", "NT3", "NRAM", "NSNH", "NACCESS" };
                    break;

                case 3:
                    iconNames = new string[] { "unknown", "text", "access" };
                    textIDs = new string[] { "TEXT4", "ACCESS" };
                    iconText = new string[] { "NU3", "NT4", "NACCESS" };
                    break;
                case 4:
                    iconNames = new string[] { "destruct" };
                    textIDs = new string[] { "DESTRUCT" };
                    iconText = new string[] { "NDESTRUCT" };
                    break;
                case 5:
                    iconNames = new string[] { "memory" };
                    textIDs = new string[] { };
                    iconText = new string[] { "NMEMORY" };
                    break;
                case 6:
                    iconNames = new string[] { "text", "text", "access" };
                    textIDs = new string[] { "3kb", "CryForHelp", "ACCESS" };
                    iconText = new string[] { "N3kb", "NCryForHelp", "NACCESS" };
                    break;

            }
            Machine = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/");
            Machine.FlipX = data.Bool("flipX");
            Machine.AddLoop("idle", "interface", 0.1f);
            Machine.AddLoop("noPower", "interfaceNoPower", 0.1f);

            float talkX = Machine.FlipX ? 8 : 0;
            Add(new TalkComponent(new Rectangle(0, 0, (int)Machine.Width, (int)Machine.Height - 8), new Vector2(19.5f + talkX, 0), Interact));
            Icons = new ComputerIcon[iconNames.Length];
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            l = scene as Level;
            scene.Add(MachineEntity = new Entity(Position));
            scene.Add(NightDay = new Entity());
            NightDay.Add(SunMoon = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/icons/"));
            SunMoon.AddLoop("sun", "sun", 0.1f);
            SunMoon.AddLoop("moon", "moon", 0.1f);
            MachineEntity.Add(Machine);
            Window.ButtonsUsed.Clear();
            WindowButton.Buttons.Clear();
            Window.Drawing = false;
            scene.Add(new IconText());
            scene.Add(Power = new Entity());
            Power.Add(PowerSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/"));
            PowerSprite.AddLoop("idle", "power", 1f);
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
            cursorSprite.Visible = false;
            #endregion



            scene.Add(window = new Window(Position));
            scene.Add(loadSequence = new LoadSequence(window.Depth - 1, window.Position));
            scene.Add(memContent = new MemoryWindowContent(window.Depth - 1, window.Position));

            Depth = BaseDepth;
            Power.Depth = BaseDepth - 1;
            NightDay.Depth = Power.Depth;
            Monitor.Depth = BaseDepth;
            cursor.Depth = BaseDepth - 6;
            Border.Depth = BaseDepth - 7;
            if (PianoModule.Session.RestoredPower)
            {
                Machine.Play("idle");
            }
            else
            {
                Machine.Play("noPower");
                Machine.OnLastFrame = (string s) =>
                {
                    if(s == "noPower" && PianoModule.Session.RestoredPower)
                    {
                        Machine.Play("idle");
                    }
                };
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = Scene.Tracker.GetEntity<Player>();
            #region Icon Setup
            int textIndex = 0;
            if ((scene as Level).Tracker.GetEntities<Interface>().Count > 1)
            {
                RemoveSelf();
            }
            if (iconNames.Length > Icons.Length)
            {
                RemoveSelf();
            }
            for (int i = 0; i < Icons.Length; i++)
            {
                bool increment = false;

                if (ComputerIcon.TextDictionary.Contains(iconNames[i]) && textIndex < textIDs.Length)
                {
                    if (iconNames[i] != "unknown" && ComputerIcon.dictionary.Contains(iconNames[i]))
                    {
                        id = textIDs[textIndex];
                        increment = true;
                    }
                }
                Scene.Add(Icons[i] = new ComputerIcon(iconNames[i], id, iconText[i]));
                Icons[i].Sprite.Visible = false;

                if (increment)
                {
                    textIndex++;
                }
            }
            #endregion
        }
        public override void Update()
        {
            base.Update();
            if (AccessEnding)
            {
                loadSfx.setPitch(1 + LoadSequence.BarProgress);
            }
            if (GlitchPlayer && !RemovePlayer && ColorLerpRate < 1)
            {
                PlayerTransitionColor = Color.Lerp(Color.White, Color.Green, ColorLerpRate += Engine.DeltaTime);
            }
            Collider = new Hitbox(8, 10, Monitor.Position.X - Position.X + MousePosition.X / 6, Monitor.Position.Y - Position.Y + MousePosition.Y / 6);

            #region Player check
            if (player == null)
            {
                return;
            }
            MachineEntity.Depth = player.Depth + 2;
            //player.Visible = !GlitchPlayer;
            //player.Light.Visible = true;
            #endregion
            if (GlitchPlayer)
            {
                GlitchAmplitude = Calc.Approach(0.01f, 100f, Ease.SineIn(MaxGlitchRange));
                if (MaxGlitchRange < 1)
                {
                    MaxGlitchRange += 0.01f;
                }
            }
            #region Button Sequence Picker
            foreach (WindowButton button in Window.ButtonsUsed)
            {
                if (button.Waiting && !LeftClicked && CollideCheck(button)) //TODO does button close the window?
                {
                    switch (button.Type)
                    {
                        case WindowButton.ButtonType.Ok:
                            //remove window and then do something based on context
                            RemoveWindow(button);
                            //do something here
                            break;

                        case WindowButton.ButtonType.Quit:
                            //remove window
                            switch (CurrentIconName)
                            {
                                case "pico":
                                    RemoveWindow(button);
                                    break;
                            }
                            //RemoveWindow(button);
                            break;

                        case WindowButton.ButtonType.Start:
                            //start a specified sequence
                            switch (CurrentIconName)
                            {
                                case "access":
                                    loadSequence.ButtonPressed = true;
                                    Loading = true;
                                    if (!AccessEnding)
                                    {
                                        Add(accessRoutine = new Coroutine(AccessRoutine(button), true));
                                    }
                                    break;
                            }
                            break;
                    }
                    return;
                }
                else
                {
                    button.Waiting = false;
                }
            }
            #endregion

            #region Cursor Update
            timer = timer > 0 ? timer - Engine.DeltaTime : 0;
            if (inControl)
            {
                if (LeftClicked && !MouseOnBounds) //if mouse is clicked
                {
                    if (CollideCheck(NightDay) && timer <= 0 && !Closing)
                    {
                        NightMode = !NightMode;
                        string id = NightMode ? "moon" : "sun";
                        SunMoon.Play(id);
                        timer = Engine.DeltaTime * 10;
                    }
                    cursorSprite.Play("pressed"); //play the "click" animation
                    if (!Window.Drawing)
                    {
                        OnClicked(); //if Target isn't being drawn, run OnClicked
                    }
                    if (window != null) //if the window is valid...
                    {
                        if (CollideRect(window.TabArea) || DraggingWindow)
                        {
                            DraggingWindow = true;
                        }
                        if (CollideCheck(window.x) && !DraggingWindow)
                        {
                            Window.Drawing = false;
                        }
                    }
                }
                else
                {
                    DraggingWindow = false;
                    SetDragOffset = false;
                    cursorSprite.Play("idle"); //revert cursor Texture if not being clicked
                }
                if (DraggingWindow)
                {
                    Window.DrawPosition = Collider.Position + GetDragOffset();
                }
                if (!MouseOnBounds)
                {
                    cursor.Position = MousePosition;
                }
                //Enforce CursorBoundsA and CursorBoundsB if bounds are exceeded
                cursor.Position.X = cursor.Position.X < CursorBoundsA.X ? CursorBoundsA.X : cursor.Position.X > CursorBoundsB.X ? CursorBoundsB.X : cursor.Position.X;
                cursor.Position.Y = cursor.Position.Y < CursorBoundsA.Y ? CursorBoundsA.Y : cursor.Position.Y > CursorBoundsB.Y ? CursorBoundsB.Y : cursor.Position.Y;
            }
            else
            {
                cursor.Position = CursorMiddle; //cursor default position
            }
            #endregion

            if (MonitorSprite.CurrentAnimationID == "idle" && !intoIdle)
            {
                //Handles icon/cursor transition from screen off to screen on
                Add(new Coroutine(TransitionToMain(), true));
                intoIdle = true;
            }
        }
        public override void Removed(Scene scene)
        {
            scene.Remove(Power, window);
            scene.Remove(Icons);

            if (player != null && Interacted)
            {
                player.Light.Alpha = SavedAlpha;
            }
            base.Removed(scene);
        }
        private Vector2 GetDragOffset()
        {
            if (!SetDragOffset)
            {
                //Get the distance from the tab's position to the cursor's position
                DragOffset = new Vector2(-(Collider.Position.X - Window.DrawPosition.X), (window.TabArea.Y - Collider.Position.Y) + Window.tabHeight);
            }
            SetDragOffset = true;
            return DragOffset;
        }
        public void RemoveWindow([Optional] WindowButton remove)
        {
            IconText.CurrentIcon = null;
            window.x.Collider = null;
            Window.Drawing = false;
            CanClickIcons = true;
            Window.RemoveButtons(Scene);
            if (remove is not null)
            {
                remove.Waiting = false;
            }

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
                    foreach (WindowButton button in Window.ButtonsUsed)
                    {
                        if (button.Waiting)
                        {
                            return;
                        }
                    }
                    if (!Closing)
                    {
                        Add(new Coroutine(CloseInterface(), true));
                    }
                }
            }
            #region Check for x button clicked
            if (CollideCheck(window.x)) //If "close window" button was clicked
            {
                if (AccessEnding)
                {
                    loadSfx.stop(STOP_MODE.IMMEDIATE);
                    accessRoutine.RemoveSelf();
                    AccessEnding = false;
                }
                if (DestructEnding)
                {
                    destructRoutine.RemoveSelf();
                    DestructEnding = false;
                }
                if (!DraggingWindow)
                {
                    //remove button collider, stop drawing window, and allow user to click icons again
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
                        //prevent icons from reacting to clicks, set window type, and start drawing window
                        CurrentIconName = Icons[i].Name;
                        IconText.CurrentIcon = Icons[i];
                        CanClickIcons = false;
                        window.Name = Icons[i].Name; //send the type of window to draw to Target.cs
                        TextWindow.CurrentID = Icons[i].GetID();
                        Window.Drawing = true;
                        return;
                    }

                }
            }
            #endregion
        }
        private void Interact(Player player)
        {
            if (!PianoModule.Session.RestoredPower)
            {
                //play click sound

                return;
            }
            Interacted = true;
            SavedAlpha = player.Light.Alpha;
            intoIdle = false;
            MonitorSprite.SetColor(startColor);
            Monitor.Position = new Vector2(l.Camera.Position.X, l.Camera.Position.Y);
            Border.Position = SceneAs<Level>().Camera.CameraToScreen(Monitor.Position) + Vector2.One;
            NightDay.Collider = new Hitbox(SunMoon.Width * 2, SunMoon.Height * 2);
            //NightDay.Position = Border.Position + new Vector2(MonitorSprite.Width-4, MonitorSprite.Height) - new Vector2(-SunMoon.Width, SunMoon.Height * 2 - 8) + Vector2.UnitY * 2;
            if (NightMode)
            {
                SunMoon.Play("moon");
            }
            else
            {
                SunMoon.Play("sun");
            }
            //set middle of screen and cursor bounds based on Border position
            CursorMiddle = new Vector2(Monitor.Position.X + Monitor.Width / 2, Monitor.Position.Y + Monitor.Height / 2);
            CursorBoundsA = new Vector2(16, 10) * 6;
            CursorBoundsB = new Vector2((MonitorSprite.Width - 11 - (cursorSprite.Width / 6)) * 6, (MonitorSprite.Height - 9 - (cursorSprite.Height / 6)) * 6);

            Power.Collider = new Hitbox(PowerSprite.Width, PowerSprite.Height);
            Power.Position = Monitor.Position + new Vector2(-4, MonitorSprite.Height) - new Vector2(-Power.Width, Power.Height * 2 - 8) + Vector2.UnitY * 2 /*- new Vector2(Power.Width, Power.Height)*/;
            PowerSprite.Play("idle");
            NightDay.Position = Power.Position + Vector2.UnitY * (SunMoon.Height - 8) + (Vector2.UnitX * (MonitorSprite.Width - (SunMoon.Width * 3)));
            loadSequence.Position = window.Position;
            memContent.Position = window.Position;
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
            whirringSfx = Audio.Play("event:/PianoBoy/interface/Whirring", Position, "Computer Laser", 0);
            #endregion
            BorderSprite.Visible = true;
            MonitorSprite.Play("boot");
            BorderSprite.Play("fadeIn");

            //ScreenCoords collider
            Collider = new Hitbox(8, 10, Monitor.Position.X - Position.X + MousePosition.X / 6, Monitor.Position.Y - Position.Y + MousePosition.Y / 6);

            player.StateMachine.State = 11; //Disable player movement
        }

        private IEnumerator AccessRoutine(WindowButton button)
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
            //then close the window, force close the computer, and digitize Madeline away and teleport to specified room
            yield return 0.05f;
            RemoveWindow(button);
            yield return 0.2f;
            if (!Closing)
            {
                Add(new Coroutine(CloseInterface(), true));
            }
            Loading = false;
        }
        private IEnumerator TransitionToMain()
        {
            int count = 0;
            Closing = false;
            BorderSprite.Visible = true;
            inControl = false;
            cursorSprite.Visible = false;
            cursorSprite.Play("idle");
            stool = Scene.Tracker.GetEntity<Stool>();

            foreach (Entity entity in Scene.Entities)
            {
                foreach (VertexLight light in entity.Components.GetAll<VertexLight>())
                {
                    light.Visible = false;
                }
                foreach (BloomPoint bloom in entity.Components.GetAll<BloomPoint>())
                {
                    bloom.Visible = false;
                }
            }

            for (float i = 0; i < 1; i += 0.1f)
            {
                if (stool is not null)
                {
                    stool.Light.Alpha = Calc.LerpClamp(1, 0, i);
                }
                player.Light.Alpha = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            //Computer logo sequence
            Audio.Play("event:/PianoBoy/interface/InterfaceBootup", Position);
            for (float i = 0; i < 1; i += 0.025f)
            {
                for (int j = 0; j < Icons.Length; j++)
                {
                    Icons[j].Sprite.Visible = count % 8 == 0;
                    PowerSprite.Visible = count % 8 == 0;
                    SunMoon.Visible = count % 8 == 0;
                }
                MonitorSprite.SetColor(Color.Lerp(startColor, Color.Green, Ease.SineInOut(i)));
                count++;
                yield return null;
            }

            cursorSprite.Visible = true;
            SunMoon.Visible = true;
            for (int j = 0; j < Icons.Length; j++)
            {
                Icons[j].Sprite.Visible = true;
            }
            inControl = true;
        }
        private IEnumerator CloseInterface()
        {
            Closing = true;
            int count = 0;
            inControl = false;
            cursorSprite.Play("idle");
            for (float i = 0; i < 1; i += 0.025f)
            {
                for (int j = 0; j < Icons.Length; j++)
                {
                    Icons[j].Sprite.Visible = count % 8 == 0;
                    cursorSprite.Visible = count % 8 == 0;
                    PowerSprite.Visible = count % 8 == 0;
                    SunMoon.Visible = count % 8 == 0;
                }
                count++;
                yield return null;
            }
            cursorSprite.Visible = false;
            PowerSprite.Visible = false;
            SunMoon.Visible = false;
            for (int j = 0; j < Icons.Length; j++)
            {
                Icons[j].Sprite.Visible = false;
            }
            whirringSfx.setParameterValue("Computer Laser", 1);
            MonitorSprite.Play("turnOff");
            Color _color = BorderSprite.Color;
            while (MonitorSprite.CurrentAnimationID == "turnOff")
            {
                BorderSprite.Color *= 0.95f;
                yield return null;
            }
            BorderSprite.Visible = false;
            BorderSprite.SetColor(_color);
            stool = Scene.Tracker.GetEntity<Stool>();
            foreach (Entity entity in Scene.Entities)
            {
                foreach (VertexLight light in entity.Components.GetAll<VertexLight>())
                {
                    light.Visible = true;
                }
                foreach (BloomPoint bloom in entity.Components.GetAll<BloomPoint>())
                {
                    bloom.Visible = true;
                }
            }
            player.Light.Visible = true;
            for (float i = 0; i < 1; i += 0.1f)
            {
                if (stool is not null)
                {
                    stool.Light.Alpha = Calc.LerpClamp(0, 1, i);
                }
                if (player is not null)
                {
                    player.Light.Alpha = Calc.LerpClamp(0, 1, i);
                }
                yield return null;
            }
            player.StateMachine.State = 0;
            if (AccessEnding)
            {
                Add(new Coroutine(Transition(), true));
            }
            yield return null;
            //RemoveSelf();
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