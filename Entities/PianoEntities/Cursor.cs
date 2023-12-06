using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.PianoEntities
{
    [Tracked]
    public class CursorHelper : Entity
    {
        private Sprite Sprite;
        public bool Clicking;
        public CursorHelper(string path, float? width = null, float? height = null, float x = 0, float y = 0) : base(Vector2.Zero)
        {
            Sprite = new Sprite(GFX.Game, path);
            Sprite.AddLoop("idle", "idle", 0.1f);
            Sprite.AddLoop("click", "click", 0.1f);
            Add(Sprite);
            Sprite.Play("idle");
            float w = width.HasValue ? width.Value : Sprite.Width;
            float h = height.HasValue ? height.Value : Sprite.Height;
            Collider = new Hitbox(w, h, x, y);
        }
        public void Click()
        {
            if (!Clicking)
            {
                Clicking = true;
                Sprite.Play("click");
            }
        }
        public void Release()
        {
            if (Clicking)
            {
                Clicking = false;
                Sprite.Play("idle");
            }
        }
    }

    [Tracked]
    public class Cursor : Entity
    {

        private string path;
        public static bool LeftClicked => Mouse.GetState().LeftButton == ButtonState.Pressed;
        public static bool MouseOnBounds
        {
            get
            {
                MouseState mouseState = Mouse.GetState();
                float mouseX = Calc.Clamp(mouseState.X, 0, Engine.ViewWidth);
                float mouseY = Calc.Clamp(mouseState.Y, 0, Engine.ViewHeight);
                return mouseX == 0 || mouseX == Engine.ViewWidth || mouseY == 0 || mouseY == Engine.ViewHeight;
            }
        }
        public const int BorderX = 16;
        public const int BorderY = 10;
        public float TimeHeld;
        public Vector2 WorldPosition
        {
            get
            {
                if(Scene is not Level level)
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

        public Cursor(string path)
        {
            this.path = path;
            Tag = TagsExt.SubHUD;
        }
        public CursorHelper Helper;
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Helper = new CursorHelper(path);
            scene.Add(Helper);
            
        }
        
        public bool ClickCollide(Entity entity, bool acceptHeld = false)
        {
            return LeftClicked && CollideCheck(entity) && (acceptHeld || TimeHeld == 0);
        }
        public bool ClickCollide<T>(bool acceptHeld = false) where T : Entity
        {
            return LeftClicked && CollideCheck<T>() && (acceptHeld || TimeHeld == 0);
        }
        public new bool CollideCheck(Entity entity)
        {
            if (Helper is null)
            {
                return false;
            }
            return Helper.CollideCheck(entity);
        }
        public new bool CollideCheck<T>() where T : Entity
        {
            if (Helper is null)
            {
                return false;
            }
            return Helper.CollideCheck<T>();
        }
        public void Click()
        {
            Helper.Click();
        }
        public void Release()
        {
            Helper.Release();
        }
        public override void Update()
        {
            if (Scene is not Level level)
            {
                base.Update();
                return;
            }
            if (Helper != null)
            {
                TimeHeld = Helper.Clicking ? TimeHeld + Engine.DeltaTime : 0;
                Helper.Position = (level.Camera.Position + MousePosition / 6).ToInt();
            }
            if (!MouseOnBounds)
            {
                Position = MousePosition.ToInt();
            }
            base.Update();
            if (LeftClicked && !MouseOnBounds) //if mouse is clicked
            {
                Click();
            }
            else
            {
                Release();
            }
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose(scene);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose(scene);
        }
        private void Dispose(Scene scene)
        {
            if (Helper is not null)
            {
                scene.Remove(Helper);
            }
            Helper = null;
        }
    }
}