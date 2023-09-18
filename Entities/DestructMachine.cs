using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

using System.Collections.Generic;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
// PuzzleIslandHelper.DestructMachine
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DestructMachine")]
    [Tracked]
    public class DestructMachine : Entity
    {
        private struct BeamLine
        {
            public List<Point> Points = new();
            public Vector2 Start, End;
            public int xRange;
            public BeamLine(Vector2 start, Vector2 end, int xRange)
            {
                Start = start;
                End = end;
                this.xRange = xRange;
            }
            public void GeneratePoints(Vector2 start, Vector2 end, int points, int range = -1)
            {
                Points.Clear();

                if(range == -1)
                {
                    range = xRange;
                }
                int x = (int)start.X, y = (int)start.Y;
                int height = (int)MathHelper.Distance(start.Y, end.Y);
                int totalHeight = 0;
                for(int i = 0; i < height/points; i++)
                {
                    Points.Add(new Point(x, y));
                    int added = (int)Calc.Random.Range(5,20);
                    int xVariation = Calc.Random.Range(-range, range+1);
 
                    totalHeight += added;
                    if (totalHeight > height)
                    {
                        added -= (int)MathHelper.Distance(height, totalHeight);
                        Points.Add(new Point((int)start.X, y - added));
                        break;
                    }
                    x = (int)start.X + xVariation;
                    y -= added;
                }
            }
            public void PointLine(Point one, Point two, Color color, float Thickness = 1)
            {
                Draw.Line(one.ToVector2(), two.ToVector2(), color, Thickness);
            }
            public void DrawPoints()
            {
                foreach(Point point in Points)
                {
                    Draw.Point(point.ToVector2(), Color.Red);
                }
            }
            public void DrawBeamLine(Color color, bool randomShades, float thickness = 1)
            {
                Color trueColor = !randomShades ? color : Color.Lerp(color, Calc.Random.Choose(Color.White, Color.Black), Calc.Random.Range(0f, 0.4f)) * Calc.Random.Range(0f,1f);
                for (int i = 0; i<Points.Count - 1; i++)
                {
                    PointLine(Points[i], Points[i + 1], Color.Lerp(trueColor,Color.Black, Calc.Random.Range(0,0.5f))*0.4f, thickness + 6);
                    PointLine(Points[i], Points[i + 1], trueColor, thickness);
                }
            }
        }

        private Sprite Machine;
        private float timer;
        private int xRange = 10;
        private readonly float WaitTime = 0.3f;
        private float colorTimer;
        private List<BeamLine> BeamsList = new();
        private int Beams;
        private int DrawNumber;
        public DestructMachine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Machine = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/decisionMachine/");
            Machine.AddLoop("idle", "base", 0.15f);
            Add(Machine);
            Machine.Play("idle");
            Sprite streaks = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/decisionMachine/");
            streaks.AddLoop("idle", "streaks", 0.1f);
            Add(streaks);
            streaks.Play("idle");
            Sprite sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/decisionMachine/");
            sprite.AddLoop("idle", "guard", 0.1f);
            sprite.X -= 2;
            Add(sprite);
            sprite.Play("idle");
            Beams = 30;
            Depth = 3;
            for (int i = 0; i < Beams; i++)
            {
                BeamsList.Add(new BeamLine(Position + new Vector2(62, Machine.Height), Position + new Vector2(62, 0), xRange));
            }
            foreach (BeamLine line in BeamsList)
            {
                line.GeneratePoints(line.Start, line.End, 3);
            }
            timer = WaitTime;
            DrawNumber = BeamsList.Count;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Entity entity = new Entity(Position);
            Sprite sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/decisionMachine/");
            sprite.AddLoop("idle", "glass", 0.1f);
            sprite.Play("idle");
            sprite.Color = Color.White * 0.8f;
            entity.Add(sprite);
            entity.Depth = Depth - 1;
            scene.Add(entity);
        }
        public override void Update()
        {
            base.Update();
            timer -= Engine.DeltaTime;
            colorTimer -= Engine.DeltaTime;
            if (timer <= 0)
            {
                foreach(BeamLine line in BeamsList)
                {
                    line.GeneratePoints(line.Start, line.End, 4, Calc.Random.Range(5, 12));
                }
                timer = WaitTime;

            }
        }
        public override void Render()
        {
            base.Render();
            if(Scene as Level is null)
            {
                return;
            }
            bool Paused = (Scene as Level).Paused;
            for(int i = 0; i<DrawNumber; i++) 
            {
                BeamsList[i].DrawBeamLine(Color.Blue, !Paused, !Paused ? Calc.Random.Choose(1, 3) : 2);
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            foreach (BeamLine line in BeamsList)
            {
                line.DrawPoints();
            }
        }

    }
}
