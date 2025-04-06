using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Entities.InvertAuth;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LabGenerator")]
    [Tracked]
    public class LabGenerator : Entity
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

                if (range == -1)
                {
                    range = xRange;
                }
                int x = (int)start.X, y = (int)start.Y;
                int height = (int)MathHelper.Distance(start.Y, end.Y);
                int totalHeight = 0;
                for (int i = 0; i < height / points; i++)
                {
                    Points.Add(new Point(x, y));
                    int added = Calc.Random.Range(5, 20);
                    int xVariation = Calc.Random.Range(-range, range + 1);

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
                foreach (Point point in Points)
                {
                    Draw.Point(point.ToVector2(), Color.Red);
                }
            }
            public void DrawBeamLine(Color color, bool randomShades, float thickness = 1)
            {
                Color trueColor = !randomShades ? color : Color.Lerp(color, Calc.Random.Choose(Color.White, Color.Black), Calc.Random.Range(0f, 0.4f)) * Calc.Random.Range(0f, 1f);
                for (int i = 0; i < Points.Count - 1; i++)
                {
                    PointLine(Points[i], Points[i + 1], Color.Lerp(trueColor, Color.Black, Calc.Random.Range(0, 0.5f)) * 0.4f, thickness + 6);
                    PointLine(Points[i], Points[i + 1], trueColor, thickness);
                }
            }
        }
        private Sprite Outside;
        private Sprite Lights;
        private MTexture Inside;
        private MTexture Glass;
        private MTexture Dim;
        private MTexture FrontLines;
        private MTexture Pipes;
        private Image Glow;
        private Entity glowEntity;
        private float timer;
        private int xRange = 12;
        private readonly float WaitTime = 0.3f;
        private float colorTimer;
        private List<BeamLine> BeamsList = new();
        private int Beams = 15;
        private int DrawNumber;
        private Color Color = Color.Blue;
        private VertexLight Light;
        private BloomPoint Bloom;
        public FlagData Laser = new FlagData("LabGeneratorLaser");
        public bool InSequence;
        public Collider LightBox;
        public Collider ConnectorBox;

        private float lightDim = 0.2f;
        private LabGeneratorPuzzle puzzle;


        public LabGenerator(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
        }

        private void DrawBloom()
        {
            Glow.Render();
        }
        public override void Update()
        {
            base.Update();
            Lights.Color = Color.Lerp(Color.White, Color.Black, lightDim);
            if (puzzle.Completed.State && PianoModule.Session.HasFixedPipes && !PianoModule.Session.GeneratorStarted) //If fixed
            {
                PianoModule.Session.GeneratorStarted = true;
                Add(new Coroutine(StartSequenceOrSomething())); //activate machine
            }

            MissingPartsUpdate();
            LaserUpdate();
        }
        private void MissingPartsUpdate()
        {
            //currentlly unused, wasn't necessary.
            /*            if (powerLight != null)
                        {
                            if (LightBox.Collide(powerLight) && powerLight.InAir && !LightFixed)
                            {
                                if (Math.Abs(LightBox.AbsoluteLeft + LightBox.Width / 2 - powerLight.CenterX) <= 2)
                                {
                                    LightFixed = true;
                                    //play click sound or something
                                    SceneAs<Level>().Remove(powerLight);
                                }
                                powerLight.DialogueOnHitGround = true;
                            }
                        }
                        if (connector != null)
                        {
                            if (ConnectorBox.Collide(connector) && connector.InAir && !PortFixed)
                            {
                                if (Math.Abs(ConnectorBox.AbsoluteLeft + ConnectorBox.Width / 2 - connector.CenterX) <= 2)
                                {
                                    PortFixed = true;
                                    //play click sound or something
                                    SceneAs<Level>().Remove(connector);
                                }
                            }
                        }*/
        }
        private void LaserUpdate()
        {
            if (Laser.State)
            {
                if (Scene.OnInterval(5f / 60f))
                {
                    Glow.Color = Color.White * Calc.Random.Range(0.3f, 0.5f);
                }
                Glow.Visible = true;

                Light.Visible = true;
                Bloom.Visible = true;
                timer -= Engine.DeltaTime;
                colorTimer -= Engine.DeltaTime;
                if (timer <= 0)
                {

                    foreach (BeamLine line in BeamsList)
                    {

                        line.GeneratePoints(line.Start, line.End, 4, Calc.Random.Range(5, 10));
                    }
                    timer = WaitTime;

                }
            }
            else
            {
                Glow.Visible = false;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            puzzle = level.Tracker.GetEntity<LabGeneratorPuzzle>();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            string path = "objects/PuzzleIslandHelper/decisionMachine/";
            Outside = new Sprite(GFX.Game, path);
            Outside.AddLoop("idle", "outside", 0.1f);
            Lights = new Sprite(GFX.Game, path);
            Lights.AddLoop("offIdle", "lightsOffFix", 0.1f);
            Lights.AddLoop("onIdle", "lightsLoop", 0.1f);
            Lights.Add("activate", "lightsStart", 0.1f, "onIdle");
            Outside.Visible = Lights.Visible = false;
            Outside.X = Lights.X = 43;
            Outside.Play("idle");
            Lights.Play("offIdle");
            Add(Outside);
            Add(Lights);
            Inside = GFX.Game[path + "inside"];
            Glass = GFX.Game[path + "glass"];
            Dim = GFX.Game[path + "dim"];
            FrontLines = GFX.Game[path + "whiteLines"];
            Pipes = GFX.Game[path + "pipes"];
            Glow = new Image(GFX.Game[path + "glow"]);
            Color = Color.Lerp(Color.White, Color.Black, 0.1f);
            LightBox = new Hitbox(7, 16, Outside.X + 84, Outside.Y + 37);
            ConnectorBox = new Hitbox(7, 6, Outside.X + 84, Outside.Y + 51);
            Collider = new ColliderList(new Hitbox(Pipes.Width, Pipes.Height), LightBox, ConnectorBox);

            Bloom = new BloomPoint(Center, 1, Outside.Width / 2);
            Light = new VertexLight(Center, Color, 0.5f, 16, 30);
            Light.Visible = false;
            Bloom.Visible = false;
            Depth = 3;
            InSequence = false;
            for (int i = 0; i < Beams; i++)
            {
                BeamsList.Add(new BeamLine(Position + new Vector2(94, Height + 8), Position + new Vector2(94, 8), xRange));
            }
            foreach (BeamLine line in BeamsList)
            {
                line.GeneratePoints(line.Start, line.End, 3);
            }
            timer = WaitTime;
            DrawNumber = BeamsList.Count;
            Add(Light);
            Add(new CustomBloom(DrawBloom));
            Add(Bloom);

            scene.Add(glowEntity = new Entity(Center));
            glowEntity.Add(Glow);
            glowEntity.Depth = -1;
            Glow.CenterOrigin();
            if (PianoModule.Session.GeneratorStarted)
            {
                Laser.State = true;
                Lights.Play("onIdle");
            }
            else
            {
                Glow.Visible = false;
            }
            Glow.Color = Color.White * 0.4f;
        }

        private IEnumerator StartSequenceOrSomething()
        {
            InSequence = true;
            Lights.Play("activate");

            yield return null;
            while (Lights.CurrentAnimationID == "activate")
            {
                yield return null;
            }
            for (int i = 0; i < 3; i++)
            {
                yield return Calc.Random.Range(0.05f, 0.2f);
                Laser.State = true;
                yield return 0.1f;
                Laser.State = false;
            }
            InSequence = false;
            Laser.State = true;
            //todo: play machine sounds or something mechanical

        }
        public override void Render()
        {
            if (Scene is not Level level) return;
            Vector2 machinePosition = Position + Vector2.UnitX * 43;
            Inside.Draw(machinePosition, Vector2.Zero, Color.White);
            Dim.Draw(machinePosition, Vector2.Zero, Color.White * 0.4f);
            FrontLines.Draw(machinePosition, Vector2.Zero, Color.Lerp(Color.White, Color.Black, 0.1f) * 0.5f);
            if (Laser.State)
            {
                for (int i = 0; i < DrawNumber; i++)
                {
                    BeamsList[i].DrawBeamLine(Color.Blue, !level.Paused, !level.Paused ? Calc.Random.Choose(1, 3) : 2);
                }
            }

            Glass.Draw(machinePosition, Vector2.Zero, Color.White * 0.8f);
            Outside.RenderPosition = Lights.RenderPosition = machinePosition;
            Outside.Render();
            Lights.Render();
            Pipes.Draw(Position, Vector2.Zero, Color.White);
            base.Render();
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
