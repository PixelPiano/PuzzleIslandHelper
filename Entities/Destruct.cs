using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Celeste.Mod.PuzzleIslandHelper.Entities.Windows;

//I give up this actually sucks so much
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Destruct")]
    [Tracked]
    public class Destruct : Entity
    {
        public static List<DWindowButton> Buttons = new();
        private DIcon Icon;
        #region Variables
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
        private DWindow window;
        private Level l;
        private Player player;
        private Sprite Machine;
        private Entity MachineEntity;
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
        private Vector2 DragOffset; //used to correctly align window with cursor while being dragged
        private Color startColor = Color.LimeGreen;
        private static bool Closing = false;
        #endregion

        private static VirtualRenderTarget _Light;
        public static VirtualRenderTarget Light => _Light ??= VirtualContent.CreateRenderTarget("Light", 320, 180);

        public Destruct(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Machine = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/");
            Machine.FlipX = data.Bool("flipX");
            Machine.AddLoop("idle", "interface", 0.1f);
            float talkX = Machine.FlipX ? 8 : 0;
            Add(new TalkComponent(new Rectangle(0, 0, (int)Machine.Width, (int)Machine.Height - 8), new Vector2(19.5f + talkX, 0), Interact));

        }
        private IEnumerator Sequence()
        {

            yield return null;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            #region Base
            scene.Add(MachineEntity = new Entity(Position));
            scene.Add(NightDay = new Entity());
            NightDay.Add(SunMoon = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/icons/"));
            SunMoon.AddLoop("sun", "sun", 0.1f);
            SunMoon.AddLoop("moon", "moon", 0.1f);
            MachineEntity.Add(Machine);

            DWindow.ButtonsUsed.Clear();
            DWindowButton.Buttons.Clear();
            DWindow.Drawing = false;
            scene.Add(new DIconText());
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



            scene.Add(window = new DWindow(Position));
            Depth = BaseDepth;
            Power.Depth = BaseDepth - 1;
            NightDay.Depth = Power.Depth;
            Monitor.Depth = BaseDepth;
            cursor.Depth = BaseDepth - 6;
            Border.Depth = BaseDepth - 7;
            Machine.Play("idle");
            #endregion
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            #region Icon Setup

            scene.Add(Icon = new DIcon("Destruct", "DESTRUCT", "NDESTRUCT"));
            Icon.Sprite.Visible = false;

            #endregion
        }
        public override void Update()
        {
            base.Update();

            Collider = new Hitbox(8, 10, Monitor.Position.X - Position.X + MousePosition.X / 6, Monitor.Position.Y - Position.Y + MousePosition.Y / 6);

            #region Scene and Player check
            player = Scene.Tracker.GetEntity<Player>();
            if (Scene as Level == null || player == null)
            {
                return;
            }
            MachineEntity.Depth = player.Depth + 2;
            l = Scene as Level;
            player.Light.Visible = true;
            #endregion
            #region Button Sequence Picker
            foreach (DWindowButton button in DWindow.ButtonsUsed)
            {
                if (button.Waiting && !LeftClicked && CollideCheck(button)) //TODO does button close the window?
                {
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
                    if (!DWindow.Drawing)
                    {
                        OnClicked(); //if DWindow isn't being drawn, run OnClicked
                    }
                    if (window != null) //if the window is valid...
                    {
                        if (CollideRect(window.TabArea) || DraggingWindow)
                        {
                            DraggingWindow = true;
                        }
                        if (CollideCheck(window.x) && !DraggingWindow)
                        {
                            DWindow.Drawing = false;
                        }
                    }
                }
                else
                {
                    DraggingWindow = false;
                    SetDragOffset = false;
                    cursorSprite.Play("idle"); //revert cursor sprite if not being clicked
                }
                if (DraggingWindow)
                {
                    DWindow.DrawPosition = Collider.Position + GetDragOffset();
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
            scene.Remove(Icon);
            player = scene.Tracker.GetEntity<Player>();

            if (player != null)
            {
                player.Light.Alpha = 1;
            }
            base.Removed(scene);
        }
        private Vector2 GetDragOffset()
        {
            if (!SetDragOffset)
            {
                //Get the distance from the tab's position to the cursor's position
                DragOffset = new Vector2(-(Collider.Position.X - DWindow.DrawPosition.X), (window.TabArea.Y - Collider.Position.Y) + DWindow.tabHeight);
            }
            SetDragOffset = true;
            return DragOffset;
        }
        public void RemoveWindow([Optional] DWindowButton remove)
        {
            DIconText.CurrentIcon = null;
            window.x.Collider = null;
            DWindow.Drawing = false;
            CanClickIcons = true;
            DWindow.RemoveButtons(Scene);
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
                    foreach (DWindowButton button in DWindow.ButtonsUsed)
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
                if (CollideCheck(Icon)) //if mouse is colliding with an icon...
                {
                    DWindow.CaseWidth = DWindow.WindowWidth;
                    DWindow.CaseHeight = DWindow.WindowHeight;
                    //prevent icons from reacting to clicks, set window type, and start drawing window
                    CurrentIconName = "Destruct";
                    DIconText.CurrentIcon = Icon;
                    CanClickIcons = false;
                    window.Name = Icon.Name; //send the type of window to draw to DWindow.cs
                    DTextWindow.CurrentID = Icon.Text;
                    DWindow.Drawing = true;
                    return;
                }
            }
            #endregion
        }
        private void Interact(Player player)
        {
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
            #region Set Icon Positions

            Vector2 iconPosition = Monitor.Position + new Vector2(18, 12);
            float iconPositionX = 0;
            bool change = false;
            int row = 0;

            //set the icon positions

            iconPositionX = change ? 0 : iconPositionX + Icon.Width + 8;
            change = iconPositionX > MonitorSprite.Width - 32;
            if (change)
            {
                row++;
                iconPosition = Monitor.Position + new Vector2(18, 12 + 8 * row);
            }
            Icon.Position = iconPosition;
            Icon.Sprite.Play("idle");
            if (!change)
            {
                iconPosition += new Vector2(Icon.Width + 8, 0);
            }
            whirringSfx = Audio.Play("event:/PianoBoy/interface/Whirring", Position, "Computer State", 0);
            #endregion
            BorderSprite.Visible = true;
            MonitorSprite.Play("boot");
            BorderSprite.Play("fadeIn");

            //ScreenCoords collider
            Collider = new Hitbox(8, 10, Monitor.Position.X - Position.X + MousePosition.X / 6, Monitor.Position.Y - Position.Y + MousePosition.Y / 6);

            player.StateMachine.State = 11; //Disable player movement
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
                Icon.Sprite.Visible = count % 8 == 0;
                PowerSprite.Visible = count % 8 == 0;
                SunMoon.Visible = count % 8 == 0;
                MonitorSprite.SetColor(Color.Lerp(startColor, Color.Green, Ease.SineInOut(i)));
                count++;
                yield return null;
            }

            cursorSprite.Visible = true;
            SunMoon.Visible = true;
            Icon.Sprite.Visible = true;
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
                Icon.Sprite.Visible = count % 8 == 0;
                cursorSprite.Visible = count % 8 == 0;
                PowerSprite.Visible = count % 8 == 0;
                SunMoon.Visible = count % 8 == 0;
                count++;
                yield return null;
            }
            cursorSprite.Visible = false;
            PowerSprite.Visible = false;
            SunMoon.Visible = false;
            Icon.Sprite.Visible = false;
            whirringSfx.setParameterValue("Computer State", 1);
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
            yield return null;
        }
        private class Cursor : Entity
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
    public class DIcon : Entity
    {
        public Sprite Sprite;
        public string Name;
        public string Text;
        public string TabText;
        public bool Open = false;
        public DIcon(string name, string text, [Optional] string tabText)
        {
            Depth = Destruct.BaseDepth - 1;
            Name = name;
            Text = text;
            if (!string.IsNullOrEmpty(tabText))
            {
                TabText = tabText;
            }
            Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/icons/"));
            Sprite.AddLoop("idle", "destruct", 0.1f);
            Sprite.SetColor(Color.White);
            Collider = new Hitbox(Sprite.Width, Sprite.Height);
        }
    }
    public class DTextWindow : Entity
    {
        #region Variables
        private bool Access
        {
            get
            {
                return CurrentID.ToUpper() == "DESTRUCT" || CurrentID.ToUpper() == "DESTRUCTDENIED";
            }
        }
        private static readonly Dictionary<string, List<string>> fontPaths;
        public static bool Drawing = false;
        public static string CurrentID = "";
        private static string fontName = "alarm clock";
        static DTextWindow()
        {
            // Fonts.paths is private static and never instantiated besides in the static constructor, so we only need to get the reference to it once.
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        private Level l;
        private FancyText.Text activeText;
        public List<FancyText.Node> Nodes => activeText.Nodes;

        public static Vector2 TextPosition;
        public static int TextWidth = 0;
        private float textScale = 0.7f;
        #endregion
        public DTextWindow()
        {
            Tag = TagsExt.SubHUD;
            Depth = Destruct.BaseDepth - 5;
        }
        public override void Render()
        {
            base.Render();
            if (Scene as Level == null)
            {
                return;
            }
            l = Scene as Level;

            //if the text is being drawn
            if (Drawing)
            {
                /*                if (Access && LoadSequence.HasArtifact && Interface.Loading)
                                {
                                    return;
                                }*/
                activeText = FancyText.Parse(Dialog.Get(CurrentID.ToUpper()),
                                            (int)DWindow.CaseWidth * 8,
                                            15, 1, Interface.NightMode ? Color.White : Color.Black);
                DWindow.TextDimensions.Y = (activeText.BaseSize * activeText.Lines / 6 * textScale) + activeText.BaseSize / 6 + 3;
                activeText.Font = ActiveFont.Font;
                activeText.Draw(ToInt(l.Camera.CameraToScreen(TextPosition)) * 6, Vector2.Zero, Vector2.One * textScale, 1f);

            }
        }
        public override void Update()
        {
            base.Update();
            if (!DWindow.Drawing)
            {
                Drawing = false;
            }
        }
        #region Finished
        private Vector2 ToInt(Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Depth = Interface.BaseDepth - 5;
            ensureCustomFontIsLoaded();
        }
        private void ensureCustomFontIsLoaded()
        {
            if (Fonts.Get(fontName) == null)
            {
                // this is a font we need to load for the cutscene specifically!
                if (!fontPaths.ContainsKey(fontName))
                {
                    // the font isn't in the list... so we need to list fonts again first.
                    Logger.Log(LogLevel.Warn, "PuzzleIslandHelper/EscapeTimer", $"We need to list fonts again, {fontName} does not exist!");
                    Fonts.Prepare();
                }

                Fonts.Load(fontName);
                Engine.Scene.Add(new FontHolderEntity());
            }
        }

        // a small entity that just ensures the font loaded by the timer unloads upon leaving the map.
        private class FontHolderEntity : Entity
        {
            public FontHolderEntity()
            {
                Tag = Tags.Global;
            }

            public override void SceneEnd(Scene scene)
            {
                base.SceneEnd(scene);
                Fonts.Unload(fontName);
            }
        }
        #endregion
    }
    [Tracked]
    public class DWindow : Entity
    {
        public static bool AccessLoad = false;
        public Sprite Sprite;
        public Sprite xSprite;
        public Sprite button;
        public DWindowButton ok;
        public DWindowButton cancel;
        public static bool KeysSwitched = false;
        public Entity x;
        public Collider backupCollider;
        public Collider windowCollider;
        public Collider header = new Hitbox(WindowWidth, 9);
        public string Name = "";
        public bool WasDrawing = false;
        public static float WindowWidth = 200;
        public static float WindowHeight = 120;
        public static float CaseWidth = 1;
        public static float CaseHeight = 1;
        public static float TextCaseWidth = 1;
        public static int tabHeight = 9;
        private static int _Depth;
        private Vector2 PicoDimensions = new Vector2(160, 90);
        public static Vector2 TextDimensions = new Vector2(WindowWidth, 0);
        public static Vector2 DrawPosition;
        public static readonly Vector2 TextOffset = Vector2.One * 3;
        public static bool Drawing = false;
        public Rectangle TabArea;
        private Color TabColor = Color.Blue;
        public static List<DWindowButton> ButtonsUsed = new List<DWindowButton>();
        private DTextWindow textWindow;
        private bool inRoutine = false;

        public DWindow([Optional] Vector2 position)
        {
            Depth = Destruct.BaseDepth - 4;
            if (position != null)
            {
                Position = position;
            }
        }
        private void SetButtonPosition(int maxButtons)
        {
            for (int i = 0; i < maxButtons; i++)
            {
                Vector2 position = ToInt(ToInt(DrawPosition) + new Vector2(CaseWidth, CaseHeight)
                                - new Vector2(DWindowButton.ButtonWidth / 3, DWindowButton.ButtonHeight / 4)
                                - Vector2.One - (Vector2.UnitX * DWindowButton.ButtonWidth * i / 2) - Vector2.UnitX * DWindowButton.ButtonWidth / 2);
                ButtonsUsed[i].Position = position - Vector2.UnitX * 3 * i;
            }
        }

        public override void Render()
        {
            base.Render();
            if (Drawing)
            {
                DrawWindow(Name, DrawPosition, WindowWidth, WindowHeight);
            }
        }
        private IEnumerator WaitBeforeAdd()
        {
            inRoutine = true;
            if (ButtonsUsed.Count == 0)
            {
                AddButtons(Scene);
            }
            while (Drawing)
            {
                yield return null;
            }

            yield return null;
            inRoutine = false;
        }
        public void DrawWindow(string name, Vector2 position, float width, float height)
        {

            /*           
                            #region Access
                            case "access":
                                CaseWidth = width;
                                CaseHeight = height;
                                TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                                WindowButton.Buttons.Clear();
                                WindowButton.Buttons.Add(WindowButton.ButtonType.Start);
                                break;
                            #endregion*/

            TabArea.Width = (int)CaseWidth - (int)xSprite.Width;
            TabArea.X = (int)position.X;
            TabArea.Y = (int)position.Y - tabHeight;
            Draw.Rect(position, (int)CaseWidth, (int)CaseHeight, Destruct.NightMode ? Color.DarkSlateGray : Color.White);
            Draw.HollowRect(position, (int)CaseWidth, (int)CaseHeight, Color.Gray);
            Draw.Rect(new Vector2((int)position.X, (int)position.Y - tabHeight), (int)CaseWidth, tabHeight, TabColor);
            for (int i = 0; i < ButtonsUsed.Count; i++)
            {
                ButtonsUsed[i].Render();
            }
            x.Position = new Vector2(TabArea.X + (int)CaseWidth - 8, TabArea.Y + 1); //must be adjusted after rectangles are drawn
            xSprite.Play("idle");
            xSprite.Render();
            DTextWindow.TextWidth = (int)TextCaseWidth;
            DTextWindow.Drawing = true;
        }
        public override void Update()
        {
            base.Update();
            x.Visible = Drawing;
            _Depth = Depth;
            if (Drawing)
            {
                SetButtonPosition(ButtonsUsed.Count);
                if (!inRoutine)
                {
                    Add(new Coroutine(WaitBeforeAdd(), true));
                }
                x.Collider = backupCollider;
            }
            DTextWindow.TextPosition = ToInt(DrawPosition) + ToInt(TextOffset);
            WasDrawing = Drawing;
        }
        private Vector2 ToInt(Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(x = new Entity(Position));

            x.Add(xSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/icons/"));
            xSprite.AddLoop("idle", "x", 0.1f);
            x.Collider = new Hitbox(xSprite.Width, xSprite.Height);
            backupCollider = x.Collider;
            TabArea = new Rectangle((int)Position.X, (int)Position.Y - tabHeight, (int)WindowWidth, tabHeight);
            DrawPosition = Position;
            scene.Add(textWindow = new DTextWindow());
            AddButtons(scene);
        }
        public void AddButtons(Scene scene)
        {
            if (scene is null)
            {
                return;
            }
            for (int i = 0; i < ButtonsUsed.Count; i++)
            {
                scene.Remove(ButtonsUsed[i]);
            }
            ButtonsUsed.Clear();


            foreach (DWindowButton button in DWindowButton.Buttons)
            {
                ButtonsUsed.Add(new DWindowButton(ToInt(Position), Vector2.One));
            }

            for (int i = 0; i < ButtonsUsed.Count; i++)
            {
                scene.Add(ButtonsUsed[i]);
            }
        }
        public static void RemoveButtons(Scene scene)
        {
            if (scene is null)
            {
                return;
            }
            for (int i = 0; i < ButtonsUsed.Count; i++)
            {
                scene.Remove(ButtonsUsed[i]);
            }
            ButtonsUsed.Clear();
        }
    }

    [Tracked]
    public class DWindowButton : Entity
    {
        private static readonly Dictionary<string, List<string>> fontPaths;
        private Sprite sprite;
        static DWindowButton()
        {
            // Fonts.paths is private static and never instantiated besides in the static constructor, so we only need to get the reference to it once.
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }
        public static float ButtonWidth = 30;
        public static float ButtonHeight = 30;
        private static string fontName = "Tahoma Regular font";
        public DestructButtonText BT;
        public bool Drawing = false;
        public static float Size = 40f;
        public bool Waiting = false;
        public Vector2 WindowPosition;
        public static List<DWindowButton> Buttons = new();
        public Action OnClicked;
        public Vector2 Scale;
        public DWindowButton(Vector2 position, string Text, Action OnClicked, Vector2 Scale)
            : this(position, Scale, Text)
        {
            this.OnClicked = OnClicked;
        }
        public DWindowButton(Vector2 position, Vector2 Scale, string Text = null)
        {
            if (Scale != Vector2.Zero)
            {
                this.Scale = Scale;
            }
            else
            {
                this.Scale = Vector2.One;
            }

            Depth = Interface.BaseDepth - 2;
            BT = new DestructButtonText(Scale, Text);
            Position = ToInt(position);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(BT);
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/icons/"));
            sprite.AddLoop("idle", "button", 1f);
            sprite.AddLoop("pressed", "buttonPressed", 1f);
            sprite.Play("idle");
            sprite.Scale = Scale;
            Collider = new Hitbox(sprite.Width * Scale.X, sprite.Height * Scale.Y);
            WindowPosition = new Vector2(Window.CaseWidth / 2, Window.CaseHeight);
        }
        public override void Update()
        {
            base.Update();
            //CloseWindow = true;
            Position = ToInt(Position);
            if (CollideCheck<Destruct>() && Destruct.LeftClicked)
            {
                Waiting = true;
                sprite.Play("pressed");
            }
            else
            {
                sprite.Play("idle");
            }
        }
        private Vector2 ToInt(Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }
        public Vector2 ButtonDrawPosition()
        {
            return ToInt(ToInt(DWindow.DrawPosition) + WindowPosition - new Vector2(ButtonWidth / 3, ButtonHeight / 4) - Vector2.One);
        }

        public class DestructButtonText : Entity
        {
            private Level l;
            public string Text;
            public Vector2 Scale;
            public bool IsDestruct;
            public DestructButtonText(Vector2 Scale, string Text = null)
            {
                Tag = TagsExt.SubHUD;
                this.Text = Text;

                this.Scale = Scale;
            }
            public override void Render()
            {
                base.Render();
                if (Scene as Level is null || !DWindow.Drawing)
                {
                    return;
                }
                l = Scene as Level;
                if (DWindow.Drawing)
                {
                    for (int i = 0; i < DWindow.ButtonsUsed.Count; i++)
                    {
                        Fonts.Get(fontName).Draw(Size, Text, AbsoluteDrawPosition(i), Vector2.Zero, Scale, Color.Black);
                    }
                }
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                ensureCustomFontIsLoaded();
            }
            private Vector2 ToInt(Vector2 vector)
            {
                return new Vector2((int)vector.X, (int)vector.Y);
            }
            private void ensureCustomFontIsLoaded()
            {
                if (Fonts.Get(fontName) == null)
                {
                    // this is a font we need to load for the cutscene specifically!
                    if (!fontPaths.ContainsKey(fontName))
                    {
                        // the font isn't in the list... so we need to list fonts again first.
                        Logger.Log(LogLevel.Warn, "PuzzleIslandHelper/EscapeTimer", $"We need to list fonts again, {fontName} does not exist!");
                        Fonts.Prepare();
                    }

                    Fonts.Load(fontName);
                    Engine.Scene.Add(new FontHolderEntity());
                }
            }
            public Vector2 AbsoluteDrawPosition(int i, bool destruct = false)
            {
                Vector2 vec1 = ToInt(l.Camera.CameraToScreen(DWindow.ButtonsUsed[i].Position)) * 6;
                if (destruct)
                {
                    vec1 = ToInt(l.Camera.CameraToScreen(Destruct.Buttons[i].Position)) * 6;
                }
                Vector2 adjust = ToInt(new Vector2(ButtonWidth / 2, 6));
                return vec1 + adjust;
            }
        }

        // a small entity that just ensures the font loaded by the timer unloads upon leaving the map.
        private class FontHolderEntity : Entity
        {
            public FontHolderEntity()
            {
                Tag = Tags.Global;
            }

            public override void SceneEnd(Scene scene)
            {
                base.SceneEnd(scene);
                Fonts.Unload(fontName);
            }
        }

    }
    public class DIconText : Entity
    {
        private static readonly Dictionary<string, List<string>> fontPaths;
        private Level l;
        static DIconText()
        {
            // Fonts.paths is private static and never instantiated besides in the static constructor, so we only need to get the reference to it once.
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }
        public static float ButtonWidth;
        public static float ButtonHeight;
        private static string fontName = "alarm clock";
        public Vector2 DrawPosition;
        public float IconWidth = 0;
        public FancyText.Text ActiveText;
        public static readonly float TextScale = 0.8f;
        public static DIcon CurrentIcon;
        public List<FancyText.Node> Nodes => ActiveText.Nodes;
        public DIconText()
        {
            Tag = TagsExt.SubHUD;
        }
        private Vector2 ToInt(Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }
        public float WidestLine()
        {
            return ActiveText.WidestLine() / 6 * TextScale;
        }
        public override void Render()
        {
            base.Render();
            if (Scene as Level is null || !DWindow.Drawing)
            {
                return;
            }
            l = Scene as Level;

            ActiveText = FancyText.Parse(Dialog.Get(CurrentIcon.TabText), (int)DWindow.WindowWidth * 10, 20);
            ActiveText.Font = ActiveFont.Font;
            ActiveText.Draw(TabTextPosition(), Vector2.Zero, Vector2.One * TextScale, 1);
        }
        public Vector2 TabTextPosition()
        {
            return (ToInt(l.Camera.CameraToScreen(DWindow.DrawPosition)) * 6) +
                    ToInt(new Vector2(1, -DWindow.tabHeight) * 6);
        }
        private void ensureCustomFontIsLoaded()
        {
            if (Fonts.Get(fontName) == null)
            {
                // this is a font we need to load for the cutscene specifically!
                if (!fontPaths.ContainsKey(fontName))
                {
                    // the font isn't in the list... so we need to list fonts again first.
                    Logger.Log(LogLevel.Warn, "PuzzleIslandHelper/DIconText", $"We need to list fonts again, {fontName} does not exist!");
                    Fonts.Prepare();
                }

                Fonts.Load(fontName);
                Engine.Scene.Add(new FontHolderEntity());
            }
        }

        // a small entity that just ensures the font loaded by the timer unloads upon leaving the map.
        private class FontHolderEntity : Entity
        {
            public FontHolderEntity()
            {
                Tag = Tags.Global;
            }

            public override void SceneEnd(Scene scene)
            {
                base.SceneEnd(scene);
                Fonts.Unload(fontName);
            }
        }

    }
}
