using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{
    [Tracked]
    public class BetterWindow : Entity
    {
        public static bool AccessLoad = false;
        public Sprite Sprite;
        public Sprite xSprite;
        public Sprite button;
        public static bool KeysSwitched = false;
        public Entity x;
        public static Vector2 DrawPosition;
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
        private Vector2 PicoDimensions = new Vector2(160, 90);
        public static readonly Vector2 TextOffset = Vector2.One * 3;
        public static bool Drawing = false;
        public Rectangle TabArea;
        public TextWindow TextWindow;
        public List<BetterWindowButton> Buttons => Components.GetAll<BetterWindowButton>().ToList();
        private Color TabColor = Color.Blue;
        public Interface Interface;

        public BetterWindow(Vector2 position, Interface inter)
        {
            Interface = inter;
            Depth = Interface.BaseDepth - 4;
            Position = position;
            Tag |= Tags.TransitionUpdate;
        }
        private void SetButtonPosition()
        {
            float y = CaseHeight - 3;
            float x = CaseWidth - 3;
            float xSpace = 3;
            foreach (BetterWindowButton button in Components.GetAll<BetterWindowButton>())
            {
                button.Position.X = x - (button.Width + xSpace);
                button.Position.Y = CaseHeight - button.Height - 3;
                x -= (button.Width + xSpace);
            }
        }
        public void ChangeWindowText(string dialog)
        {
            TextWindow.ChangeCurrentID(dialog);
        }
        public override void Render()
        {
            if (!Drawing)
            {
                base.Render();
                return;
            }
            UpdateWindow();
            TabArea.Width = (int)CaseWidth - (int)xSprite.Width;
            TabArea.X = (int)Position.X;
            TabArea.Y = (int)Position.Y - tabHeight;
            Draw.Rect(Position, (int)CaseWidth, (int)CaseHeight, Interface.NightMode ? Color.DarkSlateGray : Color.White);
            Draw.HollowRect(Position, (int)CaseWidth, (int)CaseHeight, Color.Gray);
            Draw.Rect(new Vector2((int)Position.X, (int)Position.Y - tabHeight), (int)CaseWidth, tabHeight, TabColor);
            base.Render();
            x.Position = new Vector2(TabArea.X + (int)CaseWidth - 8, TabArea.Y + 1); //must be adjusted after rectangles are drawn
            xSprite.Play("idle");
            xSprite.Render();
            if (ComputerIcon.TextDictionary.Contains(Name) || ComputerIcon.StaticText.Contains(Name))
            {
                TextWindow.TextWidth = (int)TextCaseWidth;
                TextWindow.Drawing = true;
            }
        }
        public void DisableButtons()
        {
            foreach (BetterWindowButton button in Components.GetAll<BetterWindowButton>())
            {
                button.Disabled = true;
            }
        }
        public void EnableButtons()
        {
            foreach (BetterWindowButton button in Components.GetAll<BetterWindowButton>())
            {
                button.Disabled = false;
            }
        }
        public override void Update()
        {
            UpdateWindow();
            Position = Position.ToInt();
            DrawPosition = Position;
            Visible = Drawing;
            x.Visible = Drawing;

            if (Drawing)
            {
                x.Collider = backupCollider;
            }
            TextWindow.TextPosition = Position.ToInt() + TextOffset.ToInt();
            WasDrawing = Drawing;
            base.Update();
            if (Drawing)
            {
                SetButtonPosition();
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(x = new Entity());
            x.Add(xSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/icons/"));
            xSprite.AddLoop("idle", "x", 0.1f);
            x.Collider = new Hitbox(xSprite.Width, xSprite.Height);
            backupCollider = x.Collider;
            TabArea = new Rectangle((int)Position.X, (int)Position.Y - tabHeight, (int)WindowWidth, tabHeight);
            Visible = Drawing;
            x.Visible = Drawing;
            scene.Add(TextWindow = new TextWindow(Interface.CurrentIconName));
        }
        public void UpdateWindow()
        {
            if (Name == "text" || Name == "info")
            {
                if (TextWindow is not null && TextWindow.activeText is not null)
                {
                    CaseHeight = (int)((TextWindow.activeText.BaseSize * TextWindow.activeText.Lines / 6 * TextWindow.textScale) + TextWindow.activeText.BaseSize / 6 + 3);
                }
            }
        }
        public void PrepareWindow()
        {
            RemoveButtons();
            CaseWidth = WindowWidth;
            CaseHeight = WindowHeight;
            TextCaseWidth = 1;
            switch (Name) //set variables based on name
            {
                case "text":
                    //CaseHeight = (int)TextDimensions.Y;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                case "unknown":
                    CaseWidth *= 0.5f;
                    CaseHeight *= 0.25f;
                    TextCaseWidth = 0.7f;
                    TabColor = Color.Lerp(Color.Red, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                case "invalid":
                    CaseWidth *= 0.5f;
                    CaseHeight *= 0.25f;
                    TextCaseWidth = 0.7f;
                    TabColor = Color.Lerp(Color.Red, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                case "folder":
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                case "access":
                    Add(new StartButton(Interface, Interface.StartAccessEnding));
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                case "ram":
                    CaseHeight = 50;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    Add(new StartButton(Interface));
                    Add(new QuitButton(Interface));
                    break;
                case "sus":
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                case "info":
                    //CaseHeight = (int)TextDimensions.Y;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                case "memory":
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    Add(new StartButton(Interface));
                    break;
                case "pico":
                    CaseWidth = PicoDimensions.X;
                    CaseHeight = PicoDimensions.Y;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    Add(new StartButton(Interface));
                    break;
                case "pipe":
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    Add(new CustomButton(Interface, "Switch", 35f, Vector2.Zero, Interface.pipeContent.Switch));
                    break;
                case "life":
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    Add(new StartButton(Interface, delegate{Interface.GameOfLife.Simulating = true;}));
                    break;

            }
        }
        public void Close()
        {
            RemoveButtons();
            x.Collider = null;
            Drawing = false;
        }
        public void RemoveButtons()
        {
            List<Component> toRemove = new();
            foreach (Component c in Components)
            {
                if (c is BetterWindowButton)
                {
                    toRemove.Add(c);
                }
            }
            if (toRemove.Count > 0)
            {
                Components.Remove(toRemove);
            }
        }
    }
}