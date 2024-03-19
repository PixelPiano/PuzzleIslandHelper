using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LabGeneratorPuzzle")]
    [Tracked]
    public class LabGeneratorPuzzle : Entity
    {
        private Player player;
        private Image Texture;
        public const int BaseDepth = -13000;
        public static int PuzzlesCompleted
        {
            get
            {
                return PianoModule.StageData.CurrentStage;
            }
            set
            {
                PianoModule.StageData.CurrentStage = value;
            }
        }
        public static bool Completed;
        public static bool Reset;
        private CustomTalkComponent Talk;
        public LabGeneratorPuzzle(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            PianoModule.Session.GeneratorStarted = false;
            Reset = false;
            Add(Texture = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/puzzleMachine"]));
            Collider = new Hitbox(Texture.Width, Texture.Height);
            Depth = 2;
            Add(Talk = new DotX3(0, 0, Width, Height, Vector2.UnitX * Width/2, Interact));
            Talk.Enabled = !LabGenerator.Laser;
            Position +=  Vector2.UnitY * 8;
        }
        public override void Update()
        {
            base.Update();
            if (LabGenerator.InSequence)
            {
                Talk.Enabled = false;
                return;
            }
            Talk.Enabled = !LabGenerator.Laser;

        }
        private void Interact(Player player)
        {
            this.player = player;
            Completed = false;
            Reset = false;
            player.StateMachine.State = Player.StDummy;
            SceneAs<Level>().Add(new LGPOverlay(SceneAs<Level>().Camera.Position));
            Add(new Coroutine(WaitForCompleteOrRemoved()));
        }
        private IEnumerator WaitForCompleteOrRemoved()
        {
            while (!Completed && !Reset)
            {
                yield return null;
            }
            player.StateMachine.State = Player.StNormal;
        }


        public class LGPOverlay : Entity
        {

            public class Node : Entity
            {
                public bool Lit;
                public bool Lead;

                public enum Type
                {
                    Default,
                    Still,
                    Spin, //same as static but spins
                    Danger
                }

                public Type NodeType;
                public bool Static;
                public bool Visited;
                public bool Selected;
                public int Connections;
                public List<Side> Sides;
                public int Col;
                public int Row;
                public string Path;
                public Image CellLight;
                public Image WhiteTexture;
                public Image Handles;
                public Image Texture;
                public Image Circuits;
                public Image CoolDesign;
                public static float DangerColorAmount = 1;

                public Color HandlesColor = Color.White;
                public Color BaseColor = Color.Black;
                private float buffer;
                private float waitTime;

                public float Rotation
                {
                    get
                    {
                        return Texture.Rotation;
                    }
                    set
                    {
                        for (int i = 0; i < Components.Count; i++)
                        {
                            if (Components[i] is Image && Components[i] != CellLight)
                            {

                                (Components[i] as Image).Rotation = value;
                            }
                        }
                    }
                }

                public bool Resetting;
                public int SpecialNum;
                public Image Number;

                public Node(Vector2 position, string type, Type nodeType) : base(position)
                {
                    string path = "objects/PuzzleIslandHelper/decisionMachine/puzzle/";
                    DangerColorAmount = 1;
                    NodeType = nodeType;
                    Sides = GetSides(type).ToList();
                    Path = type;
                    Depth = BaseDepth;
                    Add(CellLight = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/cellLight"]));
                    Add(WhiteTexture = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/" + type + "White"]));
                    Add(Handles = new Image(GFX.Game[path + Path + "Handles"]));
                    Add(Texture = GetTexture(type));
                    Add(Circuits = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/" + type + "Circuits"]));
                    Add(CoolDesign = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/" + type + "Lead"]));
                    CoolDesign.Color = Color.Red;
                    if (type == "null")
                    {
                        Circuits.Visible = false;
                        CoolDesign.Visible = false;
                    }
                    Collider = new Hitbox(Texture.Width, Texture.Height, -Texture.Width / 2, -Texture.Height / 2);
                    WhiteTexture.Visible = false;
                    Circuits.CenterOrigin();
                    CoolDesign.CenterOrigin();
                    WhiteTexture.CenterOrigin();
                    Handles.CenterOrigin();
                    Texture.CenterOrigin();
                    CellLight.CenterOrigin();
                    CellLight.Visible = false;
                    Visible = false;

                }

                public Node(Vector2 position, string type) : this(position, type, Type.Default)
                {
                }

                public override void Update()
                {
                    base.Update();
                    buffer += Engine.DeltaTime;
                    for (int i = 0; i < Columns; i++)
                    {
                        for (int j = 0; j < Rows; j++)
                        {
                            if (buffer > waitTime && NodeType == Type.Spin)
                            {
                                Rotate(true);
                            }
                        }
                    }

                    if (buffer > waitTime)
                    {
                        buffer = 0;
                    }
                }

                private static Side[] GetSides(string type)
                {
                    List<Side> Sides = type switch
                    {
                        "t" => new List<Side> { Side.Left, Side.Up, Side.Right },
                        "line" => new List<Side> { Side.Up, Side.Down },
                        "corner" => new List<Side> { Side.Up, Side.Right },
                        _ => new()
                    };
                    return Sides.ToArray();
                }

                private Image GetTexture(string type)
                {
                    string path = "objects/PuzzleIslandHelper/decisionMachine/puzzle/";
                    return type == "t" || type == "line" || type == "corner" ? new Image(GFX.Game[path + type]) : new Image(GFX.Game[path + "null"]);
                }

                public void Rotate(bool automatic = false)
                {
                    if (!automatic && NodeType == Type.Spin)
                    {
                        return;
                    }
                    if (NodeType != Type.Still)
                    {
                        for (int i = 0; i < Sides.Count; i++)
                        {
                            Sides[i] = Sides[i] switch
                            {
                                Side.Up => Side.Right,
                                Side.Right => Side.Down,
                                Side.Down => Side.Left,
                                Side.Left => Side.Up,
                                _ => Sides[i]
                            };
                        }
                        Rotation += 90f.ToRad();
                    }
                }

                public void Rotate(int turns)
                {
                    for (int i = 0; i < turns; i++)
                    {
                        Rotate();
                    }
                }

                public override void Render()
                {


                    Color color3 = Lead ? Color.Red : Lit ? Color.Green : Color.Gray;
                    CellLight.Color = Selected ? Color.Yellow : Lead ? Color.Red : Color.White;
                    CellLight.Color *= 0.2f;
                    CellLight.Render();

                    Circuits.Color = color3;

                    if (WhiteTexture != null && Selected && !Loading)
                    {

                        //color2 *= Opacity;
                        WhiteTexture.DrawOutline(Color.Yellow * 0.2f);
                    }
                    Handles.Color = Color.White;
                    Texture.Color = NodeType switch
                    {
                        Type.Danger => Color.Lerp(Color.Black, Color.Red, DangerColorAmount),
                        Type.Still => Color.Gray,
                        Type.Spin => Color.Gray,
                        _ => Color.Black
                    };

                    if (!Lead)
                    {
                        CoolDesign.Visible = false;
                    }

                    base.Render();
                }
            }

            public class Goal : Entity
            {
                public Image B;
                public Image Battery;
                public Image Handles;
                public Side Side;
                public Image CellLight;
                public float Percent;
                public float FillTime;

                public Side Opposite;
                public int CatalystX;
                public int CatalystY;
                public Goal(Vector2 position, Side side, float fillTime = 2) : this(0, 0, position, side, fillTime)
                {
                }



                public Goal(int fromX, int fromY, Vector2 position, Side side, float fillTime = 2) : base(position)
                {
                    CatalystX = fromX;
                    CatalystY = fromY;
                    Add(CellLight = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/cellLight"]));
                    CellLight.Color = Color.White * 0.4f;
                    CellLight.Visible = false;
                    Add(Battery = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/battery"]));
                    Add(B = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/B"]));
                    Collider = new Hitbox(Battery.Width, Battery.Height);
                    FillTime = fillTime;
                    Side = side;
                    Opposite = Side switch
                    {
                        Side.Up => Side.Down,
                        Side.Down => Side.Up,
                        Side.Left => Side.Right,
                        Side.Right => Side.Left,
                        _ => Side.Up
                    };
                    string nameside = Side.ToString();
                    Add(Handles = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/battery" + nameside]));
                    Depth = BaseDepth;
                    Visible = false;
                }

                public override void Update()
                {
                    base.Update();
                    B.Color = Color.White;
                    Battery.Color = Color.White;
                }

                public override void Render()
                {
                    Vector2 offset = new Vector2(8, 7);
                    int width = 8; int height = 10;
                    CellLight.Render();
                    Draw.Rect(Position + offset, width, height, Color.Green);
                    Draw.Rect(Position + offset, width, height - (Percent * height), Color.Gray);
                    base.Render();
                }
            }

            public class Counter : Entity
            {
                [Tracked]
                public class Bead : Actor
                {
                    public float Speed;
                    public float Accel = 0.5f;
                    private Image Image;
                    public bool Warped;
                    public bool Fallen;
                    public float Start;
                    public float End;
                    public bool AtTarget;
                    public bool Released;
                    public bool CanRender;
                    public const float MaxFallSpeed = 280f;
                    public const float Gravity = 220f;


                    public Bead(float x, float start, float end, bool ended) : base(new Vector2(x, ended ? end : start))
                    {
                        Depth = BaseDepth - 1;
                        Start = start;
                        End = end;
                        Warped = ended;
                        Visible = false;

                        Add(Image = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/bead"]));
                        Collider = new Hitbox(Image.Width, Image.Height);
                    }
                    public override void Added(Scene scene)
                    {
                        base.Added(scene);
                        if (!Warped)
                        {
                            Position.Y = (scene as Level).Camera.Position.Y;
                            Speed = 10;
                        }
                    }
                    public override void Render()
                    {
                        base.Render();
                    }

                    public override void Update()
                    {
                        base.Update();

                        if (Fallen || Warped)
                        {
                            return;
                        }
                        if (!AtTarget)
                        {
                            Speed = Calc.Approach(Speed, MaxFallSpeed, Gravity * Engine.DeltaTime);
                        }
                        Position.Y += Speed * Engine.DeltaTime;

                        if (Position.Y >= (Released ? End : Start))
                        {
                            Position.Y = (Released ? End : Start);
                            AtTarget = true;
                            Speed = 0;

                            Fallen = Released ? true : Fallen;
                        }
                        else
                        {
                            AtTarget = false;
                        }
                        if (Speed != 0f)
                        {
                            Speed += Engine.DeltaTime * Gravity;
                        }
                    }
                    public void Shift()
                    {
                        Start += 4;
                    }
                }
                public int Max;
                public int Fallen;
                public Sprite Support;
                public Sprite Barrier;
                public Image Case;
                public Image Bg;
                public float Opacity;
                public bool AllFallen
                {
                    get
                    {
                        foreach (Bead bead in Beads)
                        {
                            if (!bead.Fallen)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                public bool InMotion
                {
                    get
                    {
                        foreach (Bead bead in Beads)
                        {
                            if (bead.Speed != 0)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                }
                private Bead[] Beads;
                public int BeadsToReset;
                public static float SafeHeight;
                public static float Floor;
                private static VirtualRenderTarget _Target;
                public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("GeneratorPuzzleCounterTarget", 320, 180);
                public override void Removed(Scene scene)
                {
                    base.Removed(scene);
                    for (int i = 0; i < Beads.Length; i++)
                    {
                        scene.Remove(Beads[i]);
                    }
                    _Target?.Dispose();
                    _Target = null;
                }
                public Counter(Vector2 Position, float height, int max, int beadsToReset = 0) : base(Position + new Vector2(24, 20))
                {
                    Support = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/decisionMachine/puzzle/");
                    Barrier = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/decisionMachine/puzzle/");
                    Case = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/counterCase"]);
                    Bg = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/counterBg"]);

                    Support.AddLoop("forwardIdle", "supportMoveBack", 0.1f, 0);
                    Support.AddLoop("backIdle", "supportMove", 0.1f, 0);
                    Support.Add("forward", "supportMove", 0.1f, "forwardIdle");
                    Support.Add("back", "supportMoveBack", 0.1f, "backIdle");

                    Barrier.AddLoop("forwardIdle", "barrierMoveBack", 0.1f, 0);
                    Barrier.AddLoop("backIdle", "barrierMove", 0.1f, 0);
                    Barrier.Add("forward", "barrierMove", 0.05f, "forwardIdle");
                    Barrier.Add("back", "barrierMoveBack", 0.05f, "backIdle");
                    Add(Bg);
                    Add(Barrier, Support);
                    Add(Case);
                    Barrier.Visible = false;
                    Support.Visible = false;
                    Barrier.Play("forwardIdle");
                    Support.Play("backIdle");
                    Fallen = beadsToReset;
                    Collidable = false;
                    Depth = BaseDepth;
                    Max = max;
                    SafeHeight = base.Position.Y + height / 8;
                    Floor = base.Position.Y + height;
                    Beads = new Bead[Max];
                    Collider = new Hitbox(6, height);
                    BeadsToReset = beadsToReset;
                    int adjust = 16;
                    Support.X -= adjust;
                    Barrier.Position = new Vector2(Barrier.Width - adjust, 0);
                    Case.Position = Bg.Position = new Vector2(X - Position.X - Case.Width, -5);
                    Bg.Visible = false;
                    Bg.Color = Calc.HexToColor("16005A");
                    Case.Visible = false;
                    Add(new BeforeRenderHook(BeforeRender));
                    Visible = false;

                }
                public override void DebugRender(Camera camera)
                {
                    base.DebugRender(camera);
                    Draw.Line(new Vector2(Position.X - 8, SafeHeight), new Vector2(Position.X + Width + 8, SafeHeight), Color.Green);
                    Draw.Line(new Vector2(Position.X - 8, Floor), new Vector2(Position.X + Width + 8, Floor), Color.Orange);
                }
                private void BeforeRender()
                {
                    if (Scene is not Level level)
                    {
                        return;
                    }
                    Bg.Color = Color.White;
                    Target.DrawThenMask(Bg.Render, Drawing, level.Camera.Matrix);
                    Target.DrawToObject(Case.Render, level.Camera.Matrix);
                }
                private void DrawBeads()
                {
                    foreach (Bead bead in Beads)
                    {
                        if (bead.CanRender)
                        {
                            bead.Render();
                        }
                    }
                }
                private void Drawing()
                {
                    Bg.Color = Calc.HexToColor("16005A");
                    Bg.Render();
                    Support.DrawSimpleOutline();
                    Barrier.DrawSimpleOutline();
                    Support.Render();
                    Barrier.Render();

                    Draw.Line(new Vector2(Center.X - 1, Position.Y), new Vector2(Center.X - 1, Position.Y + Height), Color.Black);
                    Draw.Line(new Vector2(Center.X, Position.Y), new Vector2(Center.X, Position.Y + Height), Color.DimGray);
                    Draw.Line(new Vector2(Center.X + 1, Position.Y), new Vector2(Center.X + 1, Position.Y + Height), Color.Black);
                    DrawBeads();
                }
                public override void Render()
                {
                    base.Render();
                    Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position, Color.White * Opacity);
                }
                private IEnumerator Intro(Scene scene)
                {
                    for (int i = 0; i < Max; i++)
                    {
                        Beads[i] = new Bead(x: Position.X - 1,
                                            start: Position.Y - (4 * i) + Height / 8,
                                            end: Position.Y + Height - 4 - (4 * i),
                                            ended: i < BeadsToReset);
                    }

                    Beads = Beads.OrderByDescending(item => item.Y).ToArray();
                    for (int i = 0; i < Beads.Length; i++)
                    {
                        if (!Beads[i].Warped)
                        {
                            for (int j = 0; j < BeadsToReset; j++)
                            {
                                Beads[i].Shift();
                            }
                        }
                        scene.Add(Beads[i]);
                        Beads[i].CanRender = true;
                        if (!Beads[i].Warped)
                        {
                            yield return 0.3f;
                        }
                    }
                    yield return null;
                }
                public override void Awake(Scene scene)
                {
                    base.Awake(scene);
                    Add(new Coroutine(Intro(scene)));
                }
                public void Increment()
                {
                    Add(new Coroutine(IncrementRoutine()));
                }
                private IEnumerator IncrementRoutine()
                {
                    yield return null;
                    Support.Play("forward");
                    while (Support.CurrentAnimationID == "forward")
                    {
                        yield return null;
                    }
                    Barrier.Play("back");
                    yield return null;
                    Beads[Fallen].Released = true;

                    while (Barrier.CurrentAnimationID == "back")
                    {
                        yield return null;
                    }
                    yield return null;
                    Barrier.Play("forward");
                    while (Barrier.CurrentAnimationID == "forward")
                    {
                        yield return null;
                    }
                    Support.Play("back");
                    yield return null;
                    for (int i = 0; i < Beads.Length; i++)
                    {
                        if (!Beads[i].Fallen)
                        {
                            Beads[i].Shift();
                        }
                    }
                    yield return 0.5f;
                    Fallen++;

                }
            }
            
            public const int Size = 24;
            public static int Columns = 8;
            public static int Rows = 5;
            private const float MoveDelay = 0.15f;
            private const float RotateDelay = 0.16f;
            public static float Opacity;
            public static bool Loading;
            private int CurrentRow;
            private int CurrentCol;
            private int NodeSpace = 4;
            private int GraceMoves;
            private float SavedAlpha;
            private float MoveTimer = MoveDelay;
            private float RotateTimer = RotateDelay;
            private Vector2 GoalPosition;
            public Collider NodeBounds;
            public bool Completed;
            private bool Removing;
            private bool Resetting;
            private bool InDanger;
            public Image Background;
            private Node[,] Nodes = new Node[Columns, Rows];
            private Goal Battery;
            private Vector2 TopRightPosition;
            public float BackgroundOpacity;
            private float ProgressOpacity;
            private static VirtualRenderTarget _Target;
            public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("GeneratorPuzzleTarget", 320, 180);
            private Counter Aba;
            public static readonly Color CellColor = Color.White * 0.4f;
            public enum Side { Right, Down, Left, Up }
            public LGPOverlay(Vector2 Position) : base(Position)
            {
                Depth = BaseDepth;
                Loading = true;
                Collider = new Hitbox(320, 180);
                Collidable = false;
                Add(Background = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/background"]));
                Opacity = 0;
                Add(new BeforeRenderHook(BeforeRender));
            }
            public GeneratorStage GetStage(string stageName)
            {
                return PianoModule.StageData.Stages.TryGetValue(stageName, out GeneratorStage stage) ? stage : null;
            }
            public GeneratorStage GetStage(int index)
            {
                StageData data = PianoModule.StageData;
                return data.Stages.TryGetValue(data.StageSequence[index], out GeneratorStage stage) ? stage : null;
            }
            public GeneratorStage GetCurrentStage()
            {
                StageData data = PianoModule.StageData;
                return data.Stages.TryGetValue(data.StageSequence[data.CurrentStage], out GeneratorStage stage) ? stage : null;
            }


            public override void Added(Scene scene)
            {
                base.Added(scene);
                Player player = scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    SavedAlpha = player.Light.Alpha;
                    Add(new Coroutine(LightAdjust(player)));
                    player.Light.Alpha = 0;
                }


                bool random = false;
                if (!random)
                {
                    if (PianoModule.StageData.Completed)
                    {
                        Removed(scene);
                    }
                    GeneratorStage myData = GetCurrentStage();
                    if (myData is not null)
                    {
                        CreateNodesFromStageData(myData);
                        scene.Add(Aba = new Counter(Position, 141, PianoModule.StageData.StageSequence.Count,
                                                    PianoModule.StageData.CurrentStage));
                    }
                    else
                    {
                        Removed(scene);
                    }
                }
                else
                {
                    CreateRandomizedNodes(scene);
                }

                Add(new Coroutine(Setup()));
            }
            private void CreateNodesFromStageData(GeneratorStage data)
            {
                TopRightPosition = Position + new Vector2(65, 22);
                Rows = data.Rows;

                Columns = data.Columns;
                Nodes = new Node[Columns, Rows];

                for (int i = 0; i < Columns; i++)
                {
                    for (int k = 0; k < Rows; k++)
                    {
                        Scene.Add(Nodes[i, k] = CreateNode(data.PieceData[i, k], i, k, data.RotateData[i, k], data.TypeData[i, k]));
                    }
                }
                string direction = data.Direction;
                Nodes[0, 0].Lead = true;
                Nodes[0, 0].Selected = true;
                CurrentCol = 0;
                CurrentRow = 0;
                int goalX, goalY;

                goalX = Calc.Clamp(data.GoalX, 0, Columns - 1);
                goalY = Calc.Clamp(data.GoalY, 0, Rows - 1);
                Scene.Add(Battery = new Goal(goalX, goalY, Vector2.Zero, data.Side));
                Battery.Side = direction.ToLower() switch
                {
                    "up" => Side.Up,
                    "down" => Side.Down,
                    "left" => Side.Left,
                    "right" => Side.Right,
                    _ => Side.Up
                };

                int colAdjust = data.GoalX <= 0 ? 1 : 0;
                int rowAdjust = data.GoalY <= 0 ? 1 : 0;

                for (int i = 0; i < Columns; i++)
                {
                    for (int j = 0; j < Rows; j++)
                    {
                        Nodes[i, j].Position = TopRightPosition + new Vector2(i * (Size + NodeSpace), j * (Size + NodeSpace)) + new Vector2(Nodes[i, j].Width / 2, Nodes[i, j].Height / 2) + (new Vector2(Size + NodeSpace) * new Vector2(colAdjust, rowAdjust));
                    }
                }
                Vector2 position = TopRightPosition /*- new Vector2(Size + NodeSpace)*/ + new Vector2(data.GoalX * (Size + NodeSpace), data.GoalY * (Size + NodeSpace));
                Vector2 offset = Vector2.Zero/*One * (Size + NodeSpace)*/;
                Battery.Position = position - offset;
                GoalPosition = position - offset;
            }

            private Node CreateNode(string type, int col, int row, int rotations, Node.Type nodeType = Node.Type.Default)
            {
                string[] types = new string[] { "t", "l", "c" };
                Vector2 position = Center + new Vector2(col * (Size + NodeSpace), row * (Size + NodeSpace));
                Vector2 offset = new Vector2(Columns / 2f, Rows / 2f) * (Size + NodeSpace);
                Vector2 justify = new Vector2(Size / 2, Size / 2);
                string t = type;
                if (type == "r")
                {
                    t = Calc.Random.Choose(types);
                }
                t = t switch
                {
                    "t" => "t",
                    "l" => "line",
                    "c" => "corner",
                    _ => "null"
                };
                Node n = new Node(position - offset + justify, t, nodeType)
                {
                    Col = col,
                    Row = row
                };
                int rot = rotations;
                if (rotations == -1)
                {
                    rot = Calc.Random.Range(0, 4);
                }
                n.Rotate(rot);
                return n;

            }
            private void CreateRandomizedNodes(Scene scene)
            {
                for (int i = 0; i < Columns; i++)
                {
                    for (int j = 0; j < Rows; j++)
                    {
                        scene.Add(Nodes[i, j] = CreateNode("r", i, j, -1));
                    }
                }
                Nodes[0, 0].Lead = true;
                GoalPosition = BottomRightNode().Position + new Vector2(-Size / 3, Size / 2 + NodeSpace);
                scene.Add(Battery = new Goal(Columns - 1, Rows - 1, GoalPosition, Side.Up));

            }
            private Node BottomRightNode()
            {
                return Nodes[Nodes.GetLength(0) - 1, Nodes.GetLength(1) - 1];
            }
            public override void Update()
            {
                base.Update();
                Aba.Opacity = BackgroundOpacity;
                Background.Color = Color.White * BackgroundOpacity;
                if (Scene is not Level level || Resetting || Completed || Removing) return;
                Position = level.Camera.Position;
                if (Nodes[Battery.CatalystX, Battery.CatalystY].Lit && Nodes[Battery.CatalystX, Battery.CatalystY].Sides.Contains(Battery.Opposite))
                {
                    Battery.Percent += Engine.DeltaTime / Battery.FillTime;
                }
                else
                {
                    Battery.Percent = 0;
                }

                if (Battery.Percent >= 1)
                {
                    OnClear();
                }
                CheckInputs();
                UpdateConnections();

            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Draw.Circle(Center, 10, Color.Red, 20);
            }

            private IEnumerator SetupNextStage(GeneratorStage next)
            {
                Resetting = true;
                for (float i = 1; i > 0; i -= Engine.DeltaTime)
                {
                    Opacity = i;
                    yield return null;
                }
                Opacity = 0;
                Level level = SceneAs<Level>();
                foreach (Node node in Nodes)
                {
                    level.Remove(node);
                }
                level.Remove(Battery);
                yield return null;

                CreateNodesFromStageData(next);
                yield return null;
                Nodes[0, 0].Selected = true;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    Opacity = i;
                    yield return null;
                }
                Opacity = 1;
                Resetting = false;
            }
            private void OnClear()
            {
                Battery.Percent = 1;
                if (Completed || Resetting)
                {
                    return;
                }
                Aba.Increment();
                if (!PianoModule.StageData.Completed && PianoModule.StageData.GetNextStage(out GeneratorStage stage))
                {
                    Add(new Coroutine(SetupNextStage(stage)));
                }
                else
                {
                    Completed = true;
                    Add(new Coroutine(EndRoutine(true)));
                }
            }

            private void CheckInputs()
            {
                if (Loading)
                {
                    return;
                }
                if (Input.Jump && !Resetting)
                {
                    Reset(false);
                    return;
                }
                MoveTimer += Engine.DeltaTime;
                RotateTimer += Engine.DeltaTime;
                if (Input.Jump || Input.Dash || Input.MoveX.Value != 0 || Input.MoveY.Value != 0)
                {
                    if (MoveTimer > MoveDelay)
                    {
                        MoveTimer = 0;
                        if (Input.MoveX.Value != 0 || Input.MoveY.Value != 0)
                        {
                            CurrentCol = Calc.Clamp(CurrentCol + Input.MoveX.Value, 0, Columns - 1);
                            CurrentRow = Calc.Clamp(CurrentRow + Input.MoveY.Value, 0, Rows - 1);
                        }

                    }
                    UpdateSelected();
                }
            }
            private void Reset(bool random)
            {
                GraceMoves = 0;
                Resetting = true;
                if (random)
                {
                    Add(new Coroutine(RandomReset()));
                }
                else
                {
                    Add(new Coroutine(EndRoutine(false)));
                }
            }
            private void UpdateSelected()
            {
                for (int i = 0; i < Columns; i++)
                {
                    for (int j = 0; j < Rows; j++)
                    {
                        bool selected = i == CurrentCol && j == CurrentRow;
                        Nodes[i, j].Selected = selected;
                        if (selected)
                        {
                            if (Input.Dash && RotateTimer > RotateDelay)
                            {
                                RotateTimer = 0;
                                Nodes[i, j].Rotate(1);
                                GraceMoves = InDanger ? GraceMoves + 1 : 0;
                                if (GraceMoves >= 3) Reset(true);
                            }
                        }
                    }
                }
            }
            private void UpdateConnections()
            {//Clear lit & visited flag of all lineColors
                bool hitDanger = false;
                for (int x = 0; x < Columns; x++)
                {
                    for (int y = 0; y < Rows; y++)
                    {
                        Nodes[x, y].Lit = false;
                        Nodes[x, y].Visited = false;
                        Nodes[x, y].Connections = 0;
                    }
                }
                if (Resetting)
                {
                    return;
                }
                //Run a DFS (depth first search)
                Stack<Tuple<int, int>> dfsStack = new Stack<Tuple<int, int>>();
                dfsStack.Push(new Tuple<int, int>(0, 0));
                while (dfsStack.Count > 0)
                {
                    Tuple<int, int> nodeCoords = dfsStack.Pop();
                    int x = nodeCoords.Item1, y = nodeCoords.Item2;

                    //Check if this node has already been visited
                    if (Nodes[x, y].Visited) continue;
                    Nodes[x, y].Visited = true;
                    Nodes[x, y].Lit = true;

                    //Traverse all connections and push them onto the stack
                    if (x > 0 && Nodes[x, y].Sides.Contains(Side.Left) && Nodes[x - 1, y].Sides.Contains(Side.Right))
                    {
                        Nodes[x, y].Connections++;
                        if (Nodes[x - 1, y].NodeType == Node.Type.Danger)
                        {
                            hitDanger = true;
                        }
                        dfsStack.Push(new Tuple<int, int>(x - 1, y));
                    }
                    if (x < Columns - 1 && Nodes[x, y].Sides.Contains(Side.Right) && Nodes[x + 1, y].Sides.Contains(Side.Left))
                    {
                        Nodes[x, y].Connections++;
                        if (Nodes[x + 1, y].NodeType == Node.Type.Danger)
                        {
                            hitDanger = true;
                        }
                        dfsStack.Push(new Tuple<int, int>(x + 1, y));
                    }
                    if (y > 0 && Nodes[x, y].Sides.Contains(Side.Up) && Nodes[x, y - 1].Sides.Contains(Side.Down))
                    {
                        Nodes[x, y].Connections++;
                        if (Nodes[x, y - 1].NodeType == Node.Type.Danger)
                        {
                            hitDanger = true;
                        }
                        dfsStack.Push(new Tuple<int, int>(x, y - 1));
                    }
                    if (y < Rows - 1 && Nodes[x, y].Sides.Contains(Side.Down) && Nodes[x, y + 1].Sides.Contains(Side.Up))
                    {
                        Nodes[x, y].Connections++;
                        if (Nodes[x, y + 1].NodeType == Node.Type.Danger)
                        {
                            hitDanger = true;
                        }
                        dfsStack.Push(new Tuple<int, int>(x, y + 1));
                    }
                    InDanger = hitDanger;
                }
            }

            #region Routines
            private IEnumerator EndRoutine(bool complete)
            {
                Removing = true;
                yield return null;
                while (Aba.InMotion)
                {
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    Opacity = 1 - i;
                    ProgressOpacity = Opacity;
                    yield return null;
                }
                Opacity = 0;
                ProgressOpacity = 0;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    BackgroundOpacity = 1 - i;
                    yield return null;
                }
                BackgroundOpacity = 0;

                LabGeneratorPuzzle.Completed = complete;
                if (!complete)
                {
                    LabGeneratorPuzzle.Reset = true;
                }
                RemoveSelf();
            }

            private IEnumerator Setup()
            {
                Loading = true;

                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    BackgroundOpacity = i;
                    yield return null;
                }
                BackgroundOpacity = 1;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    Opacity = i;
                    ProgressOpacity = i;
                    yield return null;
                }
                Opacity = 1;
                ProgressOpacity = 1;
                Loading = false;
            }
            private IEnumerator DangerFlash()
            {
                bool on = true;
                while (Resetting)
                {
                    Node.DangerColorAmount = on ? 1f : 0.5f;
                    on = !on;
                    yield return 0.2f;
                }
                Node.DangerColorAmount = 1;
            }
            private IEnumerator RandomReset()
            {
                Resetting = true;
                Add(new Coroutine(DangerFlash()));
                for (int i = 0; i < Columns; i++)
                {
                    for (int j = 0; j < Rows; j++)
                    {
                        Nodes[i, j].Lit = false;
                    }
                }
                int x = 0, y = 0;
                while (true)
                {
                    List<int> xPoints = new();
                    List<int> yPoints = new();
                    int x2 = x, y2 = y;
                    while (x2 < Columns && y2 > -1)
                    {
                        xPoints.Add(x2);
                        yPoints.Add(y2);
                        x2++;
                        y2--;
                    }
                    for (int i = 0; i < xPoints.Count; i++)
                    {
                        Nodes[xPoints[i], yPoints[i]].Rotate();
                    }
                    yield return 0.1f;
                    for (int i = 0; i < xPoints.Count; i++)
                    {
                        Nodes[xPoints[i], yPoints[i]].Rotate();
                    }
                    yield return 0.1f;
                    if (y + 1 < Rows)
                    {
                        y++;
                    }
                    else if (x + 1 < Columns)
                    {
                        x++;
                    }
                    else
                    {
                        Resetting = false;
                        break;
                    }
                }
                yield return null;
            }
            private IEnumerator LightAdjust(Player player)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    player.Light.Alpha = Calc.LerpClamp(1, 0, i);
                    yield return null;
                }
                yield return null;
            }
            #endregion
            public override void Removed(Scene scene)
            {
                Player player = scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    player.Light.Alpha = SavedAlpha;
                }
                foreach (Node node in Nodes)
                {
                    if (node is not null)
                    {
                        scene.Remove(node);
                    }
                }
                if (Aba is not null)
                {
                    scene.Remove(Aba);
                }
                if (Battery is not null)
                {
                    scene.Remove(Battery);
                }
                base.Removed(scene);
            }
            private void Drawing()
            {
                foreach (Node node in Nodes) //Nodes
                {
                    node.Render();
                }
                Battery.Render(); //Goal
            }
            public void BeforeRender()
            {
                Target.DrawToObject(Drawing, SceneAs<Level>().Camera.Matrix, true);
            }
            public override void Render()
            {
                base.Render();
                Aba.Render();
                Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position, Color.White * Opacity);
            }
        }
    }
}