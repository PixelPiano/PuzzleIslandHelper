using Celeste.Mod.PuzzleIslandHelper.Entities.Windows;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class MemoryWindowContent : Entity
    {
        public Entity loader;
        public bool ButtonPressed = false;
        private int TickSpace = 6;
        private int TickHeight = 8;
        private float DrawY;
        private Entity Dial;
        private Vector2 DialPosition;

        public MemoryWindowContent(int Depth, Vector2 Position)
        {
            this.Position = Position;
            this.Depth = Depth - 1;

        }
        public override void Update()
        {
            base.Update();
            Dial.Visible = Window.Drawing;
            //If the Interface initiates a loading sequence

            //if player does not have item
            Position = Window.DrawPosition.ToInt();
            DrawY = Position.Y + Window.CaseHeight - 8 - Window.CaseHeight / 4;
            if (Dial.CollideCheck<Interface.Cursor>() && Interface.LeftClicked)
            {
                Dial.Position.X = (int)(Interface.MousePosition.X/6) + SceneAs<Level>().Camera.Position.X;
            }
        }
        public override void Render()
        {
            base.Render();
            if (Window.Drawing && Interface.CurrentIconName == "memory")
            {
                Vector2 offset = new Vector2(2, -1);
                for (int i = (int)Position.X; i < (int)Window.DrawPosition.X + (int)Window.CaseWidth - 4; i += TickSpace)
                {
                    Vector2 start = new Vector2(i, DrawY).ToInt();

                    Vector2 end = new Vector2(i, start.Y + TickHeight).ToInt();
                    if (i + TickSpace < (int)Window.DrawPosition.X + (int)Window.CaseWidth - 1)
                    {
                        for (int j = 1; j <= 3; j++)
                        {
                            Vector2 shortStart = start + new Vector2(j * 2, TickHeight - 3).ToInt();
                            Draw.Line(shortStart, shortStart - Vector2.UnitY * 3, Color.DarkGray);
                        }
                    }
                    Draw.Line(start + offset, end + offset, Color.Black);
                }
                Vector2 longStart = new Vector2(Position.X - 3, DrawY + TickHeight) + offset;

                Draw.Line(longStart.ToInt() + offset, longStart.ToInt() + offset + Vector2.UnitX * ((int)Window.CaseWidth - 2), Color.Black, 2);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Dial = new Entity());
            Dial.Depth = Depth - 1;
            Sprite sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/");
            sprite.AddLoop("idle", "dial", 0.1f);
            Dial.Add(sprite);
            sprite.Play("idle");
            Dial.Collider = new Hitbox(8, 8);
            Dial.Position = new Vector2(Position.X, Position.Y).ToInt();
            DrawY = Position.Y + Window.CaseHeight - 8 - Window.CaseHeight / 4;
        }

    }
    public static class Vector2Ext
    {
        public static Vector2 ToInt(this Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }
    }
}