using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
/*using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;*/

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Windows
{
    [Tracked]
    public class Window : Entity
    {
        public static bool AccessLoad = false;
        public Sprite Sprite;
        public Sprite xSprite;
        public Sprite button;
        public WindowButton ok;
        public WindowButton cancel;
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
        public static List<WindowButton> ButtonsUsed = new List<WindowButton>();
        private TextWindow textWindow;
        private bool inRoutine = false;

        private void SetButtonPosition(int maxButtons)
        {
            for (int i = 0; i < maxButtons; i++)
            {
                Vector2 position = ToInt(ToInt(DrawPosition) + new Vector2(CaseWidth, CaseHeight)
                                - new Vector2(WindowButton.ButtonWidth / 3, WindowButton.ButtonHeight / 4)
                                - Vector2.One - Vector2.UnitX * WindowButton.ButtonWidth * i / 2 - Vector2.UnitX * WindowButton.ButtonWidth / 2);
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

            switch (name) //set variables based on name
            {
                #region Text
                case "text":
                    CaseWidth = width;
                    CaseHeight = (int)TextDimensions.Y;
                    TextCaseWidth = 1;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    WindowButton.Buttons.Clear();
                    break;
                #endregion
                #region Unknown
                case "unknown":
                    CaseWidth = width * 0.5f;
                    CaseHeight = height * 0.25f;
                    TextCaseWidth = 0.7f;
                    TabColor = Color.Lerp(Color.Red, Color.Black, Interface.NightMode ? 0.5f : 0);
                    WindowButton.Buttons.Clear();
                    break;
                #endregion
                #region Invalid
                case "invalid":
                    CaseWidth = width * 0.5f;
                    CaseHeight = height * 0.25f;
                    TextCaseWidth = 0.7f;
                    TabColor = Color.Lerp(Color.Red, Color.Black, Interface.NightMode ? 0.5f : 0);
                    WindowButton.Buttons.Clear();
                    break;
                #endregion
                #region Pico
                case "pico":
                    CaseWidth = PicoDimensions.X;
                    CaseHeight = PicoDimensions.Y;
                    TextCaseWidth = 1;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    WindowButton.Buttons.Clear();
                    WindowButton.Buttons.Add(WindowButton.ButtonType.Start);
                    break;
                #endregion
                #region Folder
                case "folder":
                    CaseWidth = width;
                    CaseHeight = height;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    WindowButton.Buttons.Clear();
                    break;
                #endregion
                #region Access
                case "access":
                    CaseWidth = width;
                    CaseHeight = height;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    WindowButton.Buttons.Clear();
                    WindowButton.Buttons.Add(WindowButton.ButtonType.Start);
                    break;
                #endregion
                #region Ram
                case "ram":
                    CaseWidth = width;
                    CaseHeight = 50;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    WindowButton.Buttons.Clear();
                    WindowButton.Buttons.Add(WindowButton.ButtonType.Start);
                    WindowButton.Buttons.Add(WindowButton.ButtonType.Quit);
                    break;
                #endregion
                #region Sus
                case "sus":
                    CaseWidth = width;
                    CaseHeight = height;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    WindowButton.Buttons.Clear();
                    break;
                #endregion
                #region Info
                case "info":
                    CaseWidth = width;
                    CaseHeight = (int)TextDimensions.Y;
                    TextCaseWidth = 1;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    WindowButton.Buttons.Clear();
                    break;
                #endregion
                #region Memory
                case "memory":
                    CaseWidth = width;
                    CaseHeight = height;
                    TextCaseWidth = 1;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    WindowButton.Buttons.Clear();
                    WindowButton.Buttons.Add(WindowButton.ButtonType.Start);
                    break;
                    #endregion

            }

            TabArea.Width = (int)CaseWidth - (int)xSprite.Width;
            TabArea.X = (int)position.X;
            TabArea.Y = (int)position.Y - tabHeight;
            Draw.Rect(position, (int)CaseWidth, (int)CaseHeight, Interface.NightMode ? Color.DarkSlateGray : Color.White);
            Draw.HollowRect(position, (int)CaseWidth, (int)CaseHeight, Color.Gray);
            Draw.Rect(new Vector2((int)position.X, (int)position.Y - tabHeight), (int)CaseWidth, tabHeight, TabColor);
            for (int i = 0; i < ButtonsUsed.Count; i++)
            {
                ButtonsUsed[i].Render();
            }
            x.Position = new Vector2(TabArea.X + (int)CaseWidth - 8, TabArea.Y + 1); //must be adjusted after rectangles are drawn
            xSprite.Play("idle");
            xSprite.Render();
            if (ComputerIcon.TextDictionary.Contains(name) || ComputerIcon.StaticText.Contains(name))
            {
                TextWindow.TextWidth = (int)TextCaseWidth;
                TextWindow.Drawing = true;
            }
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
            TextWindow.TextPosition = ToInt(DrawPosition) + ToInt(TextOffset);
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
            scene.Add(textWindow = new TextWindow());
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


            foreach (WindowButton.ButtonType button in WindowButton.Buttons)
            {
                ButtonsUsed.Add(new WindowButton(button, ToInt(Position), Vector2.One));
            }
            foreach (WindowButton customButton in WindowButton.CustomButtons)
            {
                ButtonsUsed.Add(customButton);
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
        public Window([Optional] Vector2 position)
        {
            Depth = Interface.BaseDepth - 4;
            if (position != null)
            {
                Position = position;
            }
        }
    }
}