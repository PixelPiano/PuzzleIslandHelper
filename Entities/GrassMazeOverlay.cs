using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities
{
    [Tracked]
    public class GrassMazeOverlay : Entity
    {
        public class Icon : Component
        {
            public Vector2 RenderPosition
            {
                get
                {
                    return ((Entity == null) ? Vector2.Zero : Entity.Position) + Position + GridOffset - Vector2.UnitX;
                }
                set
                {
                    Position = value - ((Entity == null) ? Vector2.Zero : Entity.Position);
                }
            }
            public Vector2 Position;
            public Vector2 GridOffset;
            public int CellSize;
            public float MaxSpeed = 60f;
            public const float ControlSpeed = 60f;
            public const float MoveBackSpeed = 100f;
            public Vector2 Target;
            public float Alpha = 1;
            public float Alpha2 = 1;
            public bool MovingBack;
            public bool MovingToUnsafe;
            public Node Start;
            public Node Current;
            public Node Previous;
            public bool InControl => !MovingToUnsafe && AtTarget && !MovingBack;
            public bool AtTarget => GridOffset == Target;
            public static MTexture Texture = GFX.Game["objects/PuzzleIslandHelper/grassMaze/icon"];
            public Icon(int cellSize, Node start) : base(true, true)
            {
                CellSize = cellSize;
                Start = start;
                Current = start;
                Previous = start;
                MoveToCell(start.Col, start.Row, true);
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                if (Previous is not null && Current is not null)
                {
                    Draw.Line(Current.Center, Previous.Center, Color.Magenta);
                }
            }
            public void MoveToCell(int x, int y, bool instant = false)
            {
                Target = new Vector2(x, y) * CellSize;
                if (instant) GridOffset = Target;
            }
            public Vector2 GetDirectionalVector(Node.Direction direction)
            {
                return direction switch
                {
                    Node.Direction.Up => -Vector2.UnitY,
                    Node.Direction.Down => Vector2.UnitY,
                    Node.Direction.Left => -Vector2.UnitX,
                    Node.Direction.Right => Vector2.UnitX,
                    _ => Vector2.Zero
                };
            }
            public void Complete()
            {
                if (Current.IsSubEnd)
                {
                    MaxSpeed = ControlSpeed / 2;
                    Target += GetDirectionalVector(Current.EndDir) * CellSize;
                }
            }
            public void MoveTo(int col, int row)
            {
                Target = new Vector2(col, row) * CellSize;
            }
            public void MoveToNode(Node node)
            {
                if (node is null) return;
                Previous = Current;
                Current = node;
                Target = new Vector2(node.Col, node.Row) * CellSize;

            }
            public void MoveBack(Node node)
            {
                MoveToNode(Current.Connect);
            }
            public void BeginMoveBack()
            {
                MovingBack = true;
                MaxSpeed = MoveBackSpeed;
                MovingToUnsafe = false;
                MoveToNode(Previous);
            }
            public override void Update()
            {
                base.Update();
                if (MovingToUnsafe)
                {
                    if (AtTarget)
                    {
                        BeginMoveBack();
                    }
                }
                else
                {
                    if (MovingBack && AtTarget)
                    {
                        if (Current == Start)
                        {
                            Reset();
                            return;
                        }
                        else
                        {
                            if (Current.Connect is not null)
                            {
                                MoveToNode(Current.Connect);
                            }
                        }
                    }
                }
                if (GridOffset.X != Target.X) GridOffset.X = Calc.Approach(GridOffset.X, Target.X, MaxSpeed * Engine.DeltaTime);
                if (GridOffset.Y != Target.Y) GridOffset.Y = Calc.Approach(GridOffset.Y, Target.Y, MaxSpeed * Engine.DeltaTime);

            }
            public void Reset()
            {
                MovingBack = false;
                MaxSpeed = ControlSpeed;
                GridOffset = Target;
                Previous = Start;
            }
            public override void Render()
            {
                if (Texture != null)
                {
                    Texture.Draw(RenderPosition, Vector2.Zero, Color.White * Alpha * Alpha2);
                }
            }
        }
        public float DebugOpacity;
        public class Node : Entity
        {
            public bool Lead;
            public bool IsStart;
            public bool IsSubEnd;
            public string Data;
            public bool Safe;
            public Type NodeType;
            public bool Selected;
            public int Col;
            public int Row;
            public string Path;
            public Image Texture;
            public Color BaseColor = Color.Black;
            public float ColorAlpha = 0;
            private float buffer;
            private float waitTime;
            public Node Connect;
            public bool Flashing => ColorAlpha > 0;
            public char move;
            public enum Direction
            {
                None, Up = 'U', Down = 'D', Left = 'L', Right = 'R'
            }
            public Direction Dir;
            public Direction EndDir;
            public Node(Vector2 position, string data) : base(position)
            {
                Data = data;
                Safe = data[0].Equals('1');
                if (Safe)
                {
                    IsStart = data.Contains("S");
                    Dir = (Direction)data[1];
                    if (data.Contains("E") && data.IndexOf('E') + 1 < data.Length)
                    {
                        IsSubEnd = true;
                        EndDir = (Direction)data[data.IndexOf('E') + 1];
                    }
                }
                Texture = GetTexture("");
                Collider = new Hitbox(Texture.Width, Texture.Height, -Texture.Width / 2, -Texture.Height / 2);
                Texture.CenterOrigin();
                Visible = false;
            }

            public void Flash(Color color, float alphaFrom, float alphaTo, float fadeTime, float holdTime)
            {
                Add(new Coroutine(FlashRoutine(color, alphaFrom, alphaTo, fadeTime, holdTime)));
            }
            public IEnumerator FlashRoutine(Color color, float alphaFrom, float alphaTo, float fadeTime, float holdTime)
            {
                Color c = BaseColor;
                BaseColor = color;
                for (float i = 0; i < 1; i += Engine.DeltaTime / fadeTime)
                {
                    ColorAlpha = Calc.LerpClamp(alphaFrom, alphaTo, i);
                    yield return null;
                }
                ColorAlpha = alphaTo;
                yield return holdTime;
                for (float i = 0; i < 1; i += Engine.DeltaTime / fadeTime)
                {
                    ColorAlpha = Calc.LerpClamp(alphaTo, alphaFrom, i);
                    yield return null;
                }
                ColorAlpha = alphaFrom;
                BaseColor = c;
            }
            public override void Update()
            {
                base.Update();
                buffer += Engine.DeltaTime;

                if (buffer > waitTime)
                {
                    buffer = 0;
                }
            }
            public override void DebugRender(Camera camera)
            {
                Color color = Safe ? Color.Green : Color.Red;
                Draw.HollowRect(Collider, color);
                if (Connect != null)
                {
                    Draw.Line(Center, Connect.Center, Color.White);
                }
            }

            private Image GetTexture(string type)
            {
                string path = "objects/PuzzleIslandHelper/decisionMachine/puzzle/";
                return type == "t" || type == "line" || type == "corner" ? new Image(GFX.Game[path + type]) : new Image(GFX.Game[path + "null"]);
            }

            public override void Render()
            {
                base.Render();
                if (ColorAlpha > 0) Draw.Rect(Collider, BaseColor * ColorAlpha);
            }
        }

        public const int Size = 24;
        public int Columns = 8;
        public int Rows = 5;

        public int StartRow;
        public int StartCol;
        public int EndRow;
        public int EndCol;
        public static float Opacity;
        public static bool Loading;
        private int CurrentRow;
        private int CurrentCol;
        private int NodeSpace = 4;
        private float SavedAlpha;
        public Collider NodeBounds;
        public bool Completed;
        private bool Removing;
        private bool Resetting;
        public Image Background;
        private Node[,] Nodes;
        private Vector2 TopRightPosition;
        public float BackgroundOpacity;
        private static VirtualRenderTarget _Target;
        public Node Start;

        public bool AboveExit;
        public bool InCompleteRoutine;
        public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("GrassMazeTarget", 320, 180);
        public static readonly Color CellColor = Color.White * 0.4f;
        private Icon icon;
        public Node.Direction EndDirection;
        public GrassMazeOverlay(Vector2 Position) : base(Position)
        {
            Depth = -100000;
            Loading = true;
            Collider = new Hitbox(320, 180);
            Collidable = false;
            Add(Background = new Image(GFX.Game["objects/PuzzleIslandHelper/grassMaze/background"]));
            Opacity = 0;
            Add(new BeforeRenderHook(BeforeRender));
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

            if (GrassMaze.Completed)
            {
                Removed(scene);
            }
            CreateNodes(PianoModule.MazeData);
            Add(icon = new Icon(Size + NodeSpace, Nodes[CurrentRow, CurrentCol]));
            icon.Position = (TopRightPosition - Position).Round();
            Add(new Coroutine(Setup()));
        }
        private void CreateNodes(MazeData data)
        {
            TopRightPosition = Position + new Vector2(65, 22);
            Columns = data.Columns;
            Rows = data.Rows;
            Nodes = new Node[data.Rows, data.Columns];
            for (int i = 0; i < Rows; i++)
            {
                for (int k = 0; k < Columns; k++)
                {
                    Node node = Nodes[i, k] = CreateNode(data.Grid[i, k], i, k);

                    if (node.IsStart)
                    {
                        StartRow = i;
                        StartCol = k;
                    }
                    if (node.IsSubEnd)
                    {
                        EndRow = i;
                        EndCol = k;
                        EndDirection = node.EndDir;
                    }
                    Scene.Add(node);
                }
            }

            CurrentCol = StartCol;
            CurrentRow = StartRow;
            Vector2 offset = new Vector2(Nodes[0, 0].Width / 2, Nodes[0, 0].Height / 2);

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Nodes[i, j].Position = TopRightPosition.Round() + new Vector2(j * (Size + NodeSpace), i * (Size + NodeSpace)) + offset;
                    if (Nodes[i, j].Dir is not Node.Direction.None)
                    {
                        int x = 0, y = 0;
                        switch ((char)Nodes[i, j].Dir)
                        {
                            case 'U': y = -1; break;
                            case 'D': y = 1; break;
                            case 'L': x = -1; break;
                            case 'R': x = 1; break;
                        }

                        Nodes[i, j].Connect = Nodes[i + y, j + x];

                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            icon.Alpha2 = Opacity;
            Background.Color = Color.White * BackgroundOpacity;
            if (Scene is not Level level || Resetting || Completed || Removing || InCompleteRoutine) return;
            Position = level.Camera.Position.Round();
            CheckInputs();
        }
        public void BeforeRender()
        {
            if (Scene is not Level level) return;
            Target.DrawToObject(Drawing, level.Camera.Matrix, true);
        }
        public override void Render()
        {
            base.Render();
            if (Target is null || Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White * (1 - DebugOpacity) * Opacity);
        }

        private void CheckInputs()
        {
            if (!Loading)
            {
                if (Input.Jump)
                {
                    Exit();
                    return;
                }
                if (Input.Dash || Input.MoveX.Value != 0 || Input.MoveY.Value != 0)
                {
                    if (icon.InControl)
                    {
                        if (Input.MoveX.Value != 0)
                        {
                            CurrentCol = Calc.Clamp(CurrentCol + Input.MoveX.Value, 0, Columns - 1);
                            icon.MoveToNode(Nodes[CurrentRow, CurrentCol]);
                        }
                        else if (Input.MoveY.Value != 0)
                        {
                            CurrentRow = Calc.Clamp(CurrentRow + Input.MoveY.Value, 0, Rows - 1);
                            icon.MoveToNode(Nodes[CurrentRow, CurrentCol]);
                        }
                        CheckCurrentNode();
                    }
                }
            }

        }
        public void Complete()
        {
            InCompleteRoutine = true;
            Add(new Coroutine(CompleteRoutine()));
        }
        public void OnClear()
        {
            if (Completed || Resetting)
            {
                return;
            }
            Completed = true;
            Add(new Coroutine(EndRoutine(true)));
        }
        public void CheckCurrentNode()
        {
            if (!Nodes[CurrentRow, CurrentCol].Safe)
            {
                Reset();
            }
            else if (Nodes[CurrentRow, CurrentCol].IsSubEnd && icon.AtTarget)
            {
                Complete();
            }
        }
        private void Reset()
        {
            icon.MovingToUnsafe = true;
            CurrentCol = StartCol;
            CurrentRow = StartRow;
            icon.Alpha = 1;
        }
        private void Exit()
        {
            if (!Removing)
            {
                Removing = true;
                Add(new Coroutine(ExitRoutine()));
            }
        }
        #region Routines
        private IEnumerator CompleteRoutine()
        {

            //todo: make new background image
            //Audio.Play("event:/PianoBoy/GrassMazeComplete");
            while (!icon.AtTarget)
            {
                yield return null;
            }
            icon.Complete();
            while (!icon.AtTarget)
            {
                yield return null;
            }
            yield return 0.05f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.2f)
            {
                icon.Alpha = 1 - i;
                yield return null;
            }
            icon.Alpha = 0;
            yield return FlashBoard(Color.Green, 0, 0.8f, 0.4f, 0.2f, 0.1f);
            yield return 0.4f;
            yield return FlashBoard(Color.Green, 0, 0.8f, 0.4f, 0.2f, 0.1f);
            yield return 0.4f;
            foreach (Node node in Nodes)
            {
                while (node.Flashing)
                {
                    yield return null;
                }
            }
            yield return 0.1f;

            //SceneAs<Level>().Session.SetFlag("CompletedGrassMaze");
            Exit();

            yield return null;
        }
        private IEnumerator FlashBoard(Color color, float fromAlpha, float toAlpha, float time, float hold, float delay)
        {
            for (int i = 0; i < Rows; i++)
            {
                Add(new Coroutine(FlashRow(i, color, fromAlpha, toAlpha, time, hold, delay)));
                yield return delay;
            }
        }
        private IEnumerator FlashRow(int row, Color color, float fromAlpha, float toAlpha, float time, float hold, float delay)
        {
            if (row >= Rows) yield break;
            for (int i = 0; i < Columns; i++)
            {
                Add(new Coroutine(FlashCell(i, row, color, fromAlpha, toAlpha, time, hold)));
                yield return delay;
            }
        }
        private IEnumerator FlashCell(int column, int row, Color color, float fromAlpha, float toAlpha, float time, float hold)
        {
            if (column < Columns && column >= 0 && row < Rows && row >= 0 && Nodes[row, column] is not null)
            {
                Node node = Nodes[row, column];
                Color orig = node.BaseColor;
                node.BaseColor = color;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    node.ColorAlpha = Calc.LerpClamp(fromAlpha, toAlpha, i);
                    yield return null;
                }
                node.ColorAlpha = toAlpha;
                yield return hold;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    node.ColorAlpha = Calc.LerpClamp(toAlpha, fromAlpha, i);
                    yield return null;
                }
                node.ColorAlpha = fromAlpha;
                node.BaseColor = orig;
            }
            yield return null;
        }
        private IEnumerator ExitRoutine()
        {
            yield return FadeOut();
            yield return null;
            if (Scene is Level level && level.GetPlayer() is Player player)
            {
                player.StateMachine.State = Player.StNormal;
            }
            RemoveSelf();
        }
        public IEnumerator FadeOut()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Opacity = 1 - i;
                yield return null;
            }
            Opacity = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                BackgroundOpacity = 1 - i;
                yield return null;
            }
            BackgroundOpacity = 0;
        }
        public IEnumerator FadeIn()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                BackgroundOpacity = i;
                yield return null;
            }
            BackgroundOpacity = 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Opacity = i;
                yield return null;
            }
            Opacity = 1;
        }

        private IEnumerator EndRoutine(bool complete)
        {
            Removing = true;
            yield return FadeOut();
            GrassMaze.Completed = complete;
            if (!complete)
            {
                GrassMaze.Reset = true;
            }
            RemoveSelf();
        }

        private IEnumerator Setup()
        {
            Loading = true;
            yield return FadeIn();
            Loading = false;
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
        private Node CreateNode(string data, int row, int col)
        {
            Vector2 position = Center + new Vector2(col * (Size + NodeSpace), row * (Size + NodeSpace));
            Vector2 offset = new Vector2(Columns / 2f, Rows / 2f) * (Size + NodeSpace);
            Vector2 justify = new Vector2(Size / 2, Size / 2);
            Node n = new Node(position.Round() - offset + justify, data)
            {
                Col = col,
                Row = row
            };
            return n;

        }
        public static void Unload()
        {
            _Target?.Dispose();
            _Target = null;
        }
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
            base.Removed(scene);
        }
        private void Drawing()
        {
            foreach (Node node in Nodes) //Nodes
            {
                node.Render();
            }
            icon.Render();
        }
    }
}