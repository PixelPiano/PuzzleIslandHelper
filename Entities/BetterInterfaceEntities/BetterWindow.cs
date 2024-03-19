using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{
    [Tracked]
    public class BetterWindow : Entity
    {
        public Sprite Sprite;
        public Sprite xSprite;
        public Sprite button;
        public bool PressingButton;
        public Entity x;
        public Vector2 DrawPosition;
        public Collider backupCollider;
        public Collider windowCollider;
        public Collider header = new Hitbox(BaseWidth, 9);
        public string Name = "";
        public bool WasDrawing = false;
        public const float BaseWidth = 200;
        public const float BaseHeight = 120;
        public float WindowWidth = 200;
        public float WindowHeight = 120;
        public float CaseWidth = 1;
        public float CaseHeight = 1;
        public float TextCaseWidth = 1;
        public int tabHeight = 9;
        private Vector2 PicoDimensions = new Vector2(160, 90);
        public const int TextOffset = 3;
        public bool Drawing = false;
        public Rectangle TabArea;
        public TextWindow TextWindow;
        private bool WaitForClickRelease;
        private bool CanCloseWindow;
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
            float xSpace = 8;
            foreach (BetterWindowButton button in Components.GetAll<BetterWindowButton>())
            {
                button.Position.X = x - (button.Width + xSpace);
                button.Position.Y = y - button.Height;
                x -= button.Width + xSpace;
            }
        }
        public void ChangeWindowText(string dialog)
        {
            TextWindow?.ChangeCurrentID(dialog);
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
            TextWindow.TextWidth = (int)TextCaseWidth;
            TextWindow.Drawing = true;
            /*            if (ComputerIcon.TextDictionary.Contains(Name) || ComputerIcon.StaticText.Contains(Name))
                        {
                            TextWindow.Drawing = true;
                        }*/
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
            TextWindow.TextPosition = Position.ToInt() + TextOffset * Vector2.One;

            WasDrawing = Drawing;
            base.Update();
            if (Drawing)
            {
                SetButtonPosition();
            }
            bool pressed = false;
            foreach (BetterButton b in Components.GetAll<BetterButton>())
            {
                if (b.Pressing)
                {
                    pressed = true;
                    break;
                }
            }
            PressingButton = pressed;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(x = new Entity());
            x.Add(xSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/"));
            xSprite.AddLoop("idle", "x", 0.1f);
            x.Collider = new Hitbox(xSprite.Width, xSprite.Height);
            backupCollider = x.Collider;
            TabArea = new Rectangle((int)Position.X, (int)Position.Y - tabHeight, (int)WindowWidth, tabHeight);
            Visible = Drawing;
            x.Visible = Drawing;
            scene.Add(TextWindow = new TextWindow(Interface, Interface.CurrentIconName));
        }
        public void UpdateWindow()
        {
            string lower = Name.ToLower();
            if (lower == "text" || lower == "info")
            {
                if (TextWindow is not null && TextWindow.activeText is not null)
                {
                    CaseHeight = (int)((TextWindow.activeText.BaseSize * TextWindow.activeText.Lines / 6 * TextWindow.textScale) + TextWindow.activeText.BaseSize / 6 + 3);
                }
            }
            TabColor = Color.Lerp(lower == "unknown" ? Color.Red : Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
        }
        public void ParseData(string iconName)
        {
            string name = iconName.ToLower();
            Color color = Color.Blue;

            switch (name) //set variables based on iconName
            {
                case "unknown" or "invalid":
                    color = Color.Red;
                    CaseWidth *= 0.5f;
                    CaseHeight *= 0.25f;
                    TextCaseWidth *= 0.7f;
                    break;
                case "access":
                    Add(new InputBox(Interface, CaseWidth / 2, CaseHeight / 2, Interface.AccessProgram.CheckIfValidPass));
                    break;
                case "ram":
                    Add(new StartButton(Interface));
                    Add(new QuitButton(Interface));
                    break;
                case "memory":
                    Add(new StartButton(Interface));
                    break;
                case "pipe":
                    Add(new CustomButton(Interface, "Switch", 35f, Vector2.Zero, Interface.PipeProgram.Switch));
                    break;
                case "life" or "gameoflife":
                    CaseWidth++;
                    CaseHeight++;
                    Add(new StartButton(Interface, delegate { Interface.GameOfLifeProgram.Simulating = true; }));
                    Add(new CustomButton(Interface, "StopEvent", 35f, Vector2.Zero, Interface.GameOfLifeProgram.Stop));
                    Add(new CustomButton(Interface, "Clear", 35f, Vector2.Zero, Interface.GameOfLifeProgram.Clear));
                    Add(new CustomButton(Interface, "Rand", 35f, Vector2.Zero, Interface.GameOfLifeProgram.Randomize));
                    Add(new CustomButton(Interface, "Store", 35f, Vector2.Zero, Interface.GameOfLifeProgram.Store));
                    Add(new CustomButton(Interface, "Load", 35f, Vector2.Zero, Interface.GameOfLifeProgram.LoadPreset));
                    break;
            }
            TabColor = Color.Lerp(color, Color.Black, Interface.NightMode ? 0.5f : 0);
        }
        public void PrepareWindow()
        {
            if (Drawing) return;
            RemoveComponents();

            CaseWidth = WindowWidth;
            CaseHeight = WindowHeight;
            TextCaseWidth = WindowWidth;
            TextWindow.Initialize(Interface.CurrentIconName);
            ParseData(Name.ToLower());
        }
        public void Close()
        {
            RemoveComponents();
            x.Collider = null;
            Drawing = false;
        }
        public void RemoveComponents()
        {
            List<Component> toRemove = new();
            foreach (Component c in Components)
            {
                if (c is BetterWindowButton)
                {
                    toRemove.Add(c);
                }
                if (c is InputBox)
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