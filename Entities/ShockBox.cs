using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class RandomShock : Entity
    {
        public struct Strand
        {
            public List<Vector2> Points = new();
            public Vector2 Start;
            public Vector2 End;
            public int Range;
            public Strand(Vector2 start, Vector2 end, int range)
            {
                Start = start;
                End = end;
                Range = range;
                Points = GetPoints(Start, End, Range);
            }
            private List<Vector2> GetPoints(Vector2 start, Vector2 end, int range)
            {
                List<Vector2> list = new();
                list.Add(start);
                int points = (int)Vector2.Distance(start, end);
                for (int i = 0; i < points / 4; i++)
                {
                    Vector2 position = Vector2.Lerp(start, end, i / (float)points * 4);
                    int xVar = Calc.Random.Range(-range, range + 1);
                    int yVar = Calc.Random.Range(-range, range + 1);
                    list.Add(new Vector2(position.X + xVar, position.Y + yVar));
                }
                list.Add(end);
                return list;
            }
            public void Update(Vector2 end)
            {
                End = end;
                Points = GetPoints(Start, End, Range);
            }
            public void Render(Color color)
            {
                for (int i = 1; i < Points.Count; i++)
                {
                    Draw.Line(Points[i - 1], Points[i], color);
                }
            }
        }
        public float Size;
        public Entity Track;
        private int generations;
        private int timesRegenerated;
        public List<Strand> Strands = new();
        private float interval;
        private float intervalTimer;
        private Color color;
        private Vector2 trackOffset;
        public RandomShock(Vector2 position, int strands, int generations, float interval, Entity track, Color color, Vector2 trackOffset = default) : base(position)
        {
            this.trackOffset = trackOffset;
            Depth = 1;
            this.color = color;
            this.interval = interval;
            this.generations = generations;
            for (int i = 0; i < strands; i++)
            {
                Strands.Add(new Strand(position, track.Position + trackOffset, 8));
            }
            Track = track;
        }
        public void Regenerate()
        {
            for (int i = 0; i < Strands.Count; i++)
            {
                Strands[i].Update(Track.Position + trackOffset);
            }
            timesRegenerated++;
        }
        public override void Update()
        {
            base.Update();
            intervalTimer -= Engine.DeltaTime;
            if (intervalTimer <= 0)
            {
                if (timesRegenerated >= generations)
                {
                    RemoveSelf();
                }
                else
                {
                    Regenerate();
                    intervalTimer = interval;
                }
            }
        }
        public override void Render()
        {
            base.Render();
            foreach (Strand s in Strands)
            {
                s.Render(color);
            }
        }
    }
}