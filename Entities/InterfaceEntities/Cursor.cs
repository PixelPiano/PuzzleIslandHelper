using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.IO;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class Cursor : Entity
    {
        public Sprite Sprite;
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                Sprite.Color = value * Alpha;
            }
        }
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = value;
                Sprite.Color = Color * value;
            }
        }
        private Color color;
        private float alpha;
        public Vector2 WorldPosition
        {
            get
            {
                if (Scene is not Level level)
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
        public enum States
        {
            Idle,
            Pressed,
            Buffering
        }
        public States State;
        public void Idle()
        {
            Sprite.Play("idle");
            State = States.Idle;
        }
        public void Pressed()
        {
            Sprite.Play("pressed");
            State = States.Pressed;
        }
        public void Buffering()
        {
            Sprite.Play("buffering");
            State = States.Buffering;
        }
        public Cursor()
        {
            Tag = TagsExt.SubHUD;
            Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/"));
            Sprite.AddLoop("idle", "Cursor", 1f);
            Sprite.AddLoop("pressed", "cursorPress", 1f);
            Sprite.AddLoop("buffering", "buffering", 0.1f);
            Idle();
            Visible = false;
        }
        public override void Update()
        {
            base.Update();
        }

    }
}