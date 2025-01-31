using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [TrackedAs(typeof(InterfaceEntity))]
    public class Window : InterfaceEntity
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
        public const int TextOffset = 3;
        public bool Drawing = false;
        public Rectangle TabArea;
        public TextWindow TextWindow;
        public WindowContent CurrentProgram;
        public bool RunningProgram => CurrentProgram != null;
        public string ProgramName => CurrentProgram != null ? CurrentProgram.Name : "";
        public bool DraggingEnabled = true;
        public bool ClosingEnabled = true;
        public List<Button> Buttons => Components.GetAll<Button>().ToList();
        public List<WindowComponent> CustomComponents = new();
        private Color TabColor = Color.Blue;
        public Interface Interface;
        public float Alpha = 1;

        public Window(Vector2 position, Interface inter)
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
            foreach (Button button in Buttons)
            {
                if (button.AutoPosition)
                {
                    button.Position.X = x - (button.Width + xSpace);
                    button.Position.Y = y - button.Height;
                    x -= button.Width + xSpace;
                }
            }
        }
        public void ChangeWindowText(string dialog)
        {
            TextWindow?.ChangeCurrentID(dialog);
        }
        public void Add(WindowComponent component)
        {
            Components.Add(component);
            CustomComponents.Add(component);
        }
        public void Remove(WindowComponent component)
        {
            Components.Remove(component);
            CustomComponents.Remove(component);
        }
        public override void InterfaceRender()
        {
            if (!Drawing)
            {
                base.InterfaceRender();
                return;
            }
            else
            {
                TabArea.Width = (int)CaseWidth - (int)xSprite.Width;
                TabArea.X = (int)Position.X;
                TabArea.Y = (int)Position.Y - tabHeight;

                Draw.Rect(Position, (int)CaseWidth, (int)CaseHeight, Interface.NightMode ? Color.DarkSlateGray * Alpha : Color.White * Alpha);
                Draw.HollowRect(Position, (int)CaseWidth, (int)CaseHeight, Color.Gray * Alpha);

                CurrentProgram?.WindowRender();

                Draw.Rect(new Vector2(TabArea.X, TabArea.Y), (int)CaseWidth, tabHeight, TabColor * Alpha);
                base.InterfaceRender();
                if (ClosingEnabled)
                {
                    x.Position = new Vector2(TabArea.X + (int)CaseWidth - 8, TabArea.Y + 1); //must be adjusted after rectangles are drawn
                    xSprite.Color = Color.White * Alpha;
                    xSprite.Play("idle");
                    xSprite.Render();
                }
                TextWindow.TextWidth = (int)TextCaseWidth;
                TextWindow.Drawing = true;
            }
        }
        public void DisableButtons()
        {
            foreach (Button button in Buttons)
            {
                button.Disabled = true;
            }
        }
        public void EnableButtons()
        {
            foreach (Button button in Buttons)
            {
                button.Disabled = false;
            }
        }
        public override void Update()
        {
            if (CurrentProgram != null)
            {
                ClosingEnabled = CurrentProgram.ClosingEnabled;
                DraggingEnabled = CurrentProgram.DraggingEnabled;
            }

            string lower = Name.ToLower();

            if (lower == "text" || lower == "info")
            {
                if (TextWindow is not null && TextWindow.activeText is not null)
                {
                    CaseHeight = (int)((TextWindow.activeText.BaseSize * TextWindow.activeText.Lines / 6 * TextWindow.textScale) + TextWindow.activeText.BaseSize / 6 + 3);
                }
            }
            TabColor = Color.Lerp(lower == "unknown" ? Color.Red : Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
            foreach (WindowComponent c in CustomComponents)
            {
                c.Active = Drawing;
            }

            CenterX = Calc.Clamp(CenterX, Interface.Monitor.X, Interface.Monitor.Right);
            Position.Y = Calc.Clamp(Position.Y, Interface.Monitor.Y + TabArea.Height, Interface.Monitor.Bottom);
            Position = Position.Floor();
            DrawPosition = Position;
            Visible = Drawing;
            x.Visible = Drawing && ClosingEnabled;
            if (x.Visible)
            {
                x.Collider = backupCollider;
            }
            TextWindow.TextPosition = Position.Floor() + TextOffset * Vector2.One;
            WasDrawing = Drawing;
            base.Update();
            if (Drawing)
            {
                SetButtonPosition();
            }
            foreach (Button b in Buttons)
            {
                if (b.Pressing)
                {
                    PressingButton = true;
                    break;
                }
                PressingButton = false;
            }
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
            x.Visible = Drawing && ClosingEnabled;
            scene.Add(TextWindow = new TextWindow(Interface, Interface.CurrentIconName));
        }


        public void OpenWindow(Icon icon)
        {
            OpenWindow(icon.Name);
        }
        public void OpenWindow(string name)
        {
            if (Drawing || Scene is not Level level) return;
            CurrentProgram = null;

            CaseWidth = WindowWidth;
            CaseHeight = WindowHeight;
            TextCaseWidth = WindowWidth;
            Position = level.Camera.Position + new Vector2(160, 90) - new Vector2(CaseWidth / 2, CaseHeight / 2);

            TextWindow.Initialize(name);

            Color color = Color.Blue;

            removeComponents();
            WindowContent content = Interface.GetProgram(name);
            if (content is not null)
            {
                content.OnOpened(this);
                CurrentProgram = content;
                addProgramComponents();
            }
            TabColor = Color.Lerp(color, Color.Black, Interface.NightMode ? 0.5f : 0);
            Collider = new Hitbox(CaseWidth, CaseHeight);
        }
        private void addProgramComponents()
        {
            if (CurrentProgram is null) return;
            foreach (WindowComponent c in CurrentProgram.ProgramComponents)
            {
                Add(c);
                c.OnOpened(Scene);
            }
        }
        private void removeComponents()
        {
            bool programValid = CurrentProgram is not null;
            List<WindowComponent> toRemove = new();
            foreach (WindowComponent c in CustomComponents)
            {
                toRemove.Add(c);
            }
            foreach (WindowComponent c in toRemove)
            {
                c.OnClosed(Scene);
                Remove(c);
            }
        }
        public void Close()
        {
            removeComponents();
            CurrentProgram?.OnClosed(this);
            x.Collider = null;
            Drawing = false;
        }
    }
}