using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities
{
    [Tracked]
    public class GrassMazeOverlay : Entity
    {
        public class Node : Entity
        {
            public bool Lead;
            public bool Safe;
            public Type NodeType;
            public bool Selected;
            public List<Side> Sides;
            public int Col;
            public int Row;
            public string Path;
            public Image Texture;

            public Color BaseColor = Color.Black;
            private float buffer;
            private float waitTime;
            public Node(Vector2 position, bool safe) : base(position)
            {
                Safe = safe;
                Add(Texture = GetTexture(""));
                Collider = new Hitbox(Texture.Width, Texture.Height, -Texture.Width / 2, -Texture.Height / 2);
                Texture.CenterOrigin();
                Visible = false;
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

            private Image GetTexture(string type)
            {
                string path = "objects/PuzzleIslandHelper/decisionMachine/puzzle/";
                return type == "t" || type == "line" || type == "corner" ? new Image(GFX.Game[path + type]) : new Image(GFX.Game[path + "null"]);
            }

            public override void Render()
            {
                base.Render();
            }
        }

        public class Goal : Entity
        {
            public Image Battery;
            public int CatalystX;
            public int CatalystY;
            public Goal(Vector2 position, Side side, float fillTime = 2) : this(0, 0, position, side, fillTime)
            {
            }
            public Goal(int fromX, int fromY, Vector2 position, Side side, float fillTime = 2) : base(position)
            {
                CatalystX = fromX;
                CatalystY = fromY;

                Add(Battery = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/battery"]));

                Collider = new Hitbox(Battery.Width, Battery.Height);
                Visible = false;
            }

            public override void Update()
            {
                base.Update();
                Battery.Color = Color.White;
            }

            public override void Render()
            {
                base.Render();
            }
        }

        public const int Size = 24;
        public static int Columns = 8;
        public static int Rows = 5;
        private const float MoveDelay = 0.15f;
        public static float Opacity;
        public static bool Loading;
        private int CurrentRow;
        private int CurrentCol;
        private int NodeSpace = 4;
        private float SavedAlpha;
        private float MoveTimer = MoveDelay;
        public Collider NodeBounds;
        public bool Completed;
        private bool Removing;
        private bool Resetting;
        public Image Background;
        private Node[,] Nodes = new Node[Columns, Rows];
        private Goal Battery;
        public bool HitDanger;
        private Vector2 TopRightPosition;
        public float BackgroundOpacity;
        private static VirtualRenderTarget _Target;

        public Image Icon;

        public bool AboveExit;
        public bool InCompleteRoutine;
        public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("GrassMazeTarget", 320, 180);
        public static readonly Color CellColor = Color.White * 0.4f;
        public enum Side { Right, Down, Left, Up }
        public GrassMazeOverlay(Vector2 Position) : base(Position)
        {
            Loading = true;
            Collider = new Hitbox(320, 180);
            Collidable = false;
            Add(Background = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/puzzle/background"]));
            Opacity = 0;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public MazeData GetMaze()
        {
            return PianoModule.MazeData;
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
            MazeData myData = GetMaze();
            if (myData is not null)
            {
                CreateNodesFromMazeData(myData);
            }
            else
            {
                Removed(scene);
            }
            Add(new Coroutine(Setup()));
        }
        private void CreateNodesFromMazeData(MazeData data)
        {
            TopRightPosition = Position + new Vector2(65, 22);
            Rows = data.Rows;

            Columns = data.Columns;
            Nodes = new Node[Columns, Rows];

            for (int i = 0; i < Columns; i++)
            {
                for (int k = 0; k < Rows; k++)
                {
                    Scene.Add(Nodes[i, k] = CreateNode(data.Data[i, k] == 1, i, k));
                }
            }
            Nodes[0, 0].Selected = true;
            CurrentCol = 0;
            CurrentRow = 0;
            Vector2 offset = new Vector2(Nodes[0, 0].Width / 2, Nodes[0, 0].Height / 2);

            for (int i = 0; i < Columns; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    Nodes[i, j].Position = TopRightPosition + new Vector2(i * (Size + NodeSpace), j * (Size + NodeSpace)) + offset;
                }
            }
            Vector2 position = TopRightPosition;
            Battery.Position = position;
        }

        private Node CreateNode(bool safe, int col, int row)
        {
            Vector2 position = Center + new Vector2(col * (Size + NodeSpace), row * (Size + NodeSpace));
            Vector2 offset = new Vector2(Columns / 2f, Rows / 2f) * (Size + NodeSpace);
            Vector2 justify = new Vector2(Size / 2, Size / 2);
            Node n = new Node(position - offset + justify, safe)
            {
                Col = col,
                Row = row
            };
            return n;

        }
        public override void Update()
        {
            base.Update();
            Background.Color = Color.White * BackgroundOpacity;
            if (Scene is not Level level || Resetting || Completed || Removing || InCompleteRoutine) return;
            Position = level.Camera.Position;
            CheckInputs();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Circle(Center, 10, Color.Red, 20);
        }
        public virtual void OnClear()
        {
            if (Completed || Resetting)
            {
                return;
            }
            Completed = true;
            Add(new Coroutine(EndRoutine(true)));
        }

        private void CheckInputs()
        {
            if (Loading)
            {
                return;
            }
            if (Input.Jump)
            {
                Exit();
                return;
            }
            MoveTimer += Engine.DeltaTime;
            if (Input.Dash || Input.MoveX.Value != 0 || Input.MoveY.Value != 0)
            {
                if (MoveTimer > MoveDelay)
                {
                    MoveTimer = 0;
                    if (Input.MoveX.Value != 0 || Input.MoveY.Value != 0)
                    {
                        CurrentCol = Calc.Clamp(CurrentCol + Input.MoveX.Value, 0, Columns - 1);
                        CurrentRow = Calc.Clamp(CurrentRow + Input.MoveY.Value, 0, Rows - 1);
                        if (AboveExit && Input.MoveY.Value == 1)
                        {
                            InCompleteRoutine = true;
                            Add(new Coroutine(CompleteRoutine()));
                            return;
                        }
                        CheckCurrentNode();
                    }

                }
            }
        }
        private IEnumerator CompleteRoutine()
        {
            /*
             * todo: make icon texture, make icon subclass, make new background texture, make machine texture
             * Vector2 prev = Icon.Position;
             * for(float i = 0; i<1; i+=Engine.DeltaTime / moveTime)
             * {
             *      Icon.Position = Vector2.Lerp(prev, exitPosition, i);
             *      yield return null;
             * }
             * Audio.Play("event:/PianoBoy/GrassMazeComplete");
             * for(int i = 0; i<Columns; i++)
             * {
             *      int row = 0;
             *      int col = i;
             *      while(row < Rows && col >= 0)
             *      {
             *          Add(new Coroutine(FlashCell(col, row, Color.Green, 0.8f, 0.2f)));
             *          row++;
             *          col--;
             *      }
             *      yield return 0.1f;
             * }
             * yield return 0.5f;
             * SceneAs<Level>().SetFlag("CompletedGrassMaze");
             * Exit();
             */
            yield return null;
        }
        private IEnumerator FlashCell(int column, int row, Color color, float time, float hold)
        {
            if (column < Columns && column >= 0 && row < Rows && row >= 0 && Nodes[column, row] is not null)
            {
                Color orig = Nodes[column, row].BaseColor;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    Nodes[column, row].BaseColor = Color.Lerp(orig, color, i);
                    yield return null;
                }
                Nodes[column, row].BaseColor = color;
                yield return hold;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    Nodes[column, row].BaseColor = Color.Lerp(color, orig, i);
                    yield return null;
                }
                Nodes[column, row].BaseColor = orig;
            }
            yield return null;
        }
        public void CheckCurrentNode()
        {
            if (!Nodes[CurrentCol, CurrentRow].Safe)
            {
                Reset();
            }
        }
        private void Reset()
        {
            if (!Resetting)
            {
                Resetting = true;
                Add(new Coroutine(ResetRoutine()));
            }
        }
        private IEnumerator ResetRoutine()
        {
            //todo
            //fade selector out
            //do BEEP BOOP YOU WRONG
            //fade selector in at start position

            yield return null;
        }
        private void Exit()
        {
            if (!Removing)
            {
                Removing = true;
                Add(new Coroutine(ExitRoutine()));
            }
        }
        private IEnumerator ExitRoutine()
        {
            yield return FadeOut();
            yield return null;
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
        #region Routines
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
            Battery?.Render(); //Goal
        }
        public void BeforeRender()
        {
            Target.DrawToObject(Drawing, SceneAs<Level>().Camera.Matrix, true);
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position, Color.White * Opacity);
        }
    }
}