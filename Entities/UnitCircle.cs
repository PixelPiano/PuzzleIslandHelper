using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;


namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class UnitCircle : Entity
    {
        public int Degrees;
        public float Radians;
        public float Angle;
        public float Radius = 80;
        public UnitCircle() : base()
        {
            Tag |= TagsExt.SubHUD;
            Add(new Coroutine(RotateRoutine()));
        }
        public IEnumerator RotateRoutine()
        {
            while (true)
            {
                Degrees = (Degrees + 1) % 360;
                Angle = ((float)Degrees).ToRad();
                yield return null;
            }
        }
        public override void Render()
        {
            base.Render();
            Vector2 offset = Vector2.One * 40;
            Vector2 center = offset + Vector2.One * Radius;
            Draw.Line(center, center + Calc.AngleToVector(Angle, Radius), Color.White, 5);
            Draw.Circle(offset + Vector2.One * Radius, Radius, Color.Red, 200);
            float yOffset = ActiveFont.LineHeight * 2;
            string text = Degrees + ", " + ((float)Degrees).ToRad();
            ActiveFont.Draw(text, offset - Vector2.UnitY * yOffset, Color.White);
        }
        [Command("unit_circle", "i cannot rember")]
        public static void AddUnitCircle()
        {
            if (Engine.Scene is Level level)
            {
                level.Add(new UnitCircle());
            }
        }
    }
}
