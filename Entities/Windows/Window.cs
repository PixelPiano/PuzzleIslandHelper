using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
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
        public Interface Interface;
        public Window(Vector2 position, Interface inter)
        {
            Interface = inter;
            Depth = Interface.BaseDepth - 4;
            Position = position;
        }
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
            if (!Drawing)
            {
                return;
            }
            TabArea.Width = (int)CaseWidth - (int)xSprite.Width;
            TabArea.X = (int)DrawPosition.X;
            TabArea.Y = (int)DrawPosition.Y - tabHeight;
            Draw.Rect(DrawPosition, (int)CaseWidth, (int)CaseHeight, Interface.NightMode ? Color.DarkSlateGray : Color.White);
            Draw.HollowRect(DrawPosition, (int)CaseWidth, (int)CaseHeight, Color.Gray);
            Draw.Rect(new Vector2((int)DrawPosition.X, (int)DrawPosition.Y - tabHeight), (int)CaseWidth, tabHeight, TabColor);
            for (int i = 0; i < ButtonsUsed.Count; i++)
            {
                ButtonsUsed[i].Render();
            }
            x.Position = new Vector2(TabArea.X + (int)CaseWidth - 8, TabArea.Y + 1); //must be adjusted after rectangles are drawn
            xSprite.Play("idle");
            xSprite.Render();
            if (ComputerIcon.TextDictionary.Contains(Name) || ComputerIcon.StaticText.Contains(Name))
            {
                TextWindow.TextWidth = (int)TextCaseWidth;
                TextWindow.Drawing = true;
            }
        }
        private IEnumerator WaitBeforeAdd()
        {
            inRoutine = true;
            if (ButtonsUsed.Count == 0)
            {
                PrepareWindow(Scene);
            }
            while (Drawing)
            {
                yield return null;
            }

            yield return null;
            inRoutine = false;
        }
        public override void Update()
        {
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
            base.Update();
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
            PrepareWindow(scene);
        }
        public void PrepareWindow(Scene scene)
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
            CaseWidth = WindowWidth;
            CaseHeight = WindowHeight;
            TextCaseWidth = 1;
            switch (Name) //set variables based on name
            {
                case "text":
                    CaseHeight = (int)TextDimensions.Y;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                #region Unknown
                case "unknown":
                    CaseWidth *= 0.5f;
                    CaseHeight *= 0.25f;
                    TextCaseWidth = 0.7f;
                    TabColor = Color.Lerp(Color.Red, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                #endregion
                #region Invalid
                case "invalid":
                    CaseWidth *= 0.5f;
                    CaseHeight *= 0.25f;
                    TextCaseWidth = 0.7f;
                    TabColor = Color.Lerp(Color.Red, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                #endregion
                #region Folder
                case "folder":
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                #endregion
                #region Access
                case "access":
                    ButtonsUsed.Add(new WindowButton(WindowButton.ButtonType.Start, ToInt(Position), Vector2.One));
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                #endregion
                #region Ram
                case "ram":
                    CaseHeight = 50;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    ButtonsUsed.Add(new WindowButton(WindowButton.ButtonType.Start, ToInt(Position), Vector2.One));
                    ButtonsUsed.Add(new WindowButton(WindowButton.ButtonType.Quit, ToInt(Position), Vector2.One));
                    break;
                #endregion
                #region Sus
                case "sus":
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                #endregion
                #region Info
                case "info":
                    CaseHeight = (int)TextDimensions.Y;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    break;
                #endregion
                #region Memory
                case "memory":
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    ButtonsUsed.Add(new WindowButton(WindowButton.ButtonType.Start, ToInt(Position), Vector2.One));
                    break;
                #endregion
                #region Pico
                case "pico":
                    CaseWidth = PicoDimensions.X;
                    CaseHeight = PicoDimensions.Y;
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    ButtonsUsed.Add(new WindowButton(WindowButton.ButtonType.Start, ToInt(Position), Vector2.One));
                    break;
                #endregion
                #region Pipes
                case "pipe":
                    TabColor = Color.Lerp(Color.Blue, Color.Black, Interface.NightMode ? 0.5f : 0);
                    ButtonsUsed.Add(new WindowButton(ToInt(Position), "Switch", Interface.pipeContent.Switch, Vector2.One, 39f));
                    break;
                    #endregion

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
}