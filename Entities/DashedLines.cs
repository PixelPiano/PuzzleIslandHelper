using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked(false)]
    public class DashedLines : Entity
    {
        public class Line : Component
        {
            public Vector2 From;
            public Vector2 To;
            public Color Color;
            public float Duration;
            private float d;

            public Line(Vector2 from, Vector2 to, Color color, float duration) : base(true, true)
            {
                From = from;
                To = to;
                d = Duration = duration;
                Color = color;
            }
            public override void Update()
            {
                base.Update();
                if (Duration <= 0)
                {
                    RemoveSelf();
                }
                Duration -= Engine.DeltaTime;
            }
            public override void Render()
            {
                base.Render();
                Draw.Line(From, To, Color * (Duration / d));
            }
        }
        public List<Line> Lines = new();
        public DashedLines(Vector2 from, Vector2 to, float interval, float duration, Color color) : base()
        {
            //for()
        }
    }
}