using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{

    [TrackedAs(typeof(BetterButton))]
    public class BetterButton : Image
    {
        public Action OnClicked;
        public bool Disabled;

        public IEnumerator Routine;
        public string Text;
        public bool Pressing;
        public Collider ButtonCollider;
        public const string DefaultPath = "objects/PuzzleIslandHelper/interface/icons/";
        public string Path
        {
            get
            {
                if (!string.IsNullOrEmpty(customPath))
                {
                    return customPath;
                }
                else
                {
                    return DefaultPath;
                }
            }
        }
        public override float Height
        {
            get
            {
                if (UsesCircleCollider)
                {
                    if (ButtonCollider is not null)
                    {
                        return Circle.Height;
                    }
                }
                else
                {
                    if (ButtonCollider is not null)
                    {
                        return ButtonCollider.Height;
                    }
                }

                return Texture is null ? 0 : Texture.Height;
            }
        }
        public override float Width
        {
            get
            {
                if (UsesCircleCollider)
                {
                    if (ButtonCollider is not null)
                    {
                        return Circle.Width;
                    }
                }
                else
                {
                    if (ButtonCollider is not null)
                    {
                        return ButtonCollider.Width;
                    }
                }
                return Texture is null ? 0 : Texture.Width;
            }
        }

        public Vector2 HalfArea
        {
            get
            {
                return new Vector2(Width / 2, Height / 2);
            }
        }

        private string customPath;
        public bool InFocus;
        public float TextSize;
        public ButtonText TextRenderer;
        public Vector2 TextOffset = Vector2.Zero;
        public bool UsesCircleCollider;
        public Circle Circle;
        public static string GetPath(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return s;
            }
            else
            {
                return DefaultPath;
            }
        }
        public BetterButton(Circle circle, string customPath = null, Action OnClicked = null, IEnumerator Routine = null) : this(customPath, OnClicked, Routine)
        {
            Circle = circle;
            UsesCircleCollider = true;
        }
        public BetterButton(string customPath = null, Action OnClicked = null, IEnumerator Routine = null) : base(GFX.Game[GetPath(customPath) + "button"], true)
        {
            this.customPath = customPath;
            this.OnClicked = OnClicked;
            this.Routine = Routine;
            ButtonCollider = new Hitbox(Texture.Width, Texture.Height);
        }
        public void RunActions()
        {
            OnClicked?.Invoke();
            if (Routine is not null)
            {
                Entity.Add(new Coroutine(Routine));
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
            if (Visible && !Disabled)
            {
                if (BetterWindow.Drawing && Entity.Visible)
                {
                    if (UsesCircleCollider)
                    {
                        Draw.Circle(Circle.Position, Circle.Radius, Color.Aqua, (int)Circle.Radius * 2);
                    }
                    else
                    {
                        Draw.HollowRect(ButtonCollider, Color.Aqua);
                    }
                }
            }
        }
        public override void Update()
        {
            base.Update();
            Color = Color.Lerp(Color.White, Disabled ? Color.LightGray : Color.White, 0.5f);
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
                TextRenderer.RenderPosition = (level.Camera.CameraToScreen(RenderPosition + new Vector2(2, 1))).ToInt() * 6;
            }
            if (Disabled)
            {
                Pressing = false;
                Texture = GFX.Game[Path + "button"];
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

            if (collidingWithMouse && Interface.LeftClicked)
            {
                Pressing = true;
                Texture = GFX.Game[Path + "buttonPressed"];
            }
            else
            {
                if (Pressing && collidingWithMouse)
                {
                    RunActions();
                }
                Pressing = false;
                Texture = GFX.Game[Path + "button"];
            }
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