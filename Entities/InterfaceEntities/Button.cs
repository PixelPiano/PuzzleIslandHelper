using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{

    [TrackedAs(typeof(Button))]
    public class Button : WindowImage
    {
        public bool AutoPosition;
        public Action OnClicked;
        public bool Disabled;
        public IEnumerator Routine;
        public string Text;
        private bool pressing;

        public bool Pressing
        {
            get
            {
                return ForcePressed || pressing;
            }
            set
            {
                pressing = value;
                ForcePressed = false;
            }
        }
        public Collider ButtonCollider;
        public const string DefaultPath = "objects/PuzzleIslandHelper/interface/buttons/";

        public string Path;
        public Vector2 HalfArea
        {
            get
            {
                return new Vector2(Width / 2, Height / 2);
            }
        }
        public float Bottom
        {
            get
            {
                return Position.Y + Height;
            }
        }
        public float Top
        {
            get
            {
                return Position.Y;
            }
        }
        private string customPath;
        public bool InFocus;
        public float TextSize;
        public ButtonText TextRenderer;
        public Vector2 TextOffset = Vector2.Zero;
        public bool UsesCircleCollider;
        public Circle Circle;
        public bool ForcePressed;
        public static string GetPath(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                string result = DefaultPath + s;
                if (result[^1] != '/')
                {
                    result += "/";
                }
                return result;
            }
            else
            {
                return DefaultPath + "default/";
            }
        }
        public Button(Window window, Circle circle, string customPath = null, Action OnClicked = null, IEnumerator Routine = null) : this(window, customPath, OnClicked, Routine)
        {
            Circle = circle;
            UsesCircleCollider = true;
        }
        public Button(Window window, string customPath = null, Action OnClicked = null, IEnumerator Routine = null) : base(window, GFX.Game[GetPath(customPath) + "button00"])
        {
            this.customPath = customPath;
            Path = GetPath(customPath);
            this.OnClicked = OnClicked;
            this.Routine = Routine;
            ButtonCollider = new Hitbox(Texture.Width, Texture.Height);
        }
        public void Hide()
        {
            Disabled = true;
            Visible = false;
        }
        public void Show()
        {
            Disabled = false;
            Visible = true;
        }
        public virtual void RunActions()
        {
            OnClicked?.Invoke();
            if (Routine is not null)
            {
                Entity?.Add(new Coroutine(Routine));
            }
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            TextRenderer = new ButtonText(this, Text, TextSize, Vector2.One, TextOffset);
            entity.Scene.Add(TextRenderer);
        }
        public void Dispose(Scene scene)
        {
            if (TextRenderer is not null)
            {
                scene.Remove(TextRenderer);
                TextRenderer = null;
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Visible)
            {
                Color color = Color.Lerp(Color.Aqua, Color.Gray, Disabled ? 0.3f : 0);
                if (Window.Drawing && Entity.Visible)
                {
                    if (UsesCircleCollider)
                    {
                        Draw.Circle(Circle.Position, Circle.Radius, color, (int)Circle.Radius * 2);
                    }
                    else
                    {
                        Draw.HollowRect(ButtonCollider, color);
                    }
                }
            }
        }
        public override void Update()
        {
            base.Update();
            TextRenderer.Text = Text;
            Color = Color.Lerp(Color.White, Disabled ? Color.LightGray : Color.White, 0.5f) * Alpha;
            if (UsesCircleCollider)
            {
                Circle.Position = RenderPosition + HalfArea;
            }
            else
            {
                ButtonCollider.Position = RenderPosition;
            }
            if (Scene is Level level && TextRenderer is not null)
            {
                TextRenderer.RenderPosition = (level.Camera.CameraToScreen(RenderPosition + new Vector2(2, 1))).Floor() * 6;
            }
            if (ForcePressed)
            {
                if (!Pressing)
                {
                    RunActions();
                    Press();
                }
                Color = Color.Blue * Alpha;
                return;
            }
            if (Disabled)
            {
                pressing = false;
                Texture = GFX.Game[Path + "button00"];
                return;
            }
            bool collidingWithMouse;
            if (UsesCircleCollider)
            {
                collidingWithMouse = Interface.MouseOver(Circle);
            }
            else
            {
                collidingWithMouse = Interface.MouseOver(ButtonCollider);
            }

            if (collidingWithMouse && Interface.LeftPressed)
            {
                Press();
            }
            else
            {
                if (Pressing && collidingWithMouse)
                {
                    RunActions();
                }
                Unpress();
            }
        }
        public void Press()
        {
            pressing = true;
            Texture = GFX.Game[Path + "button01"];
        }
        public void Unpress()
        {
            pressing = false;
            Texture = GFX.Game[Path + "button00"];
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose(scene);
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            Dispose(entity.Scene);
        }

    }
}