using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using VivHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class BitrailTransporter : Entity
    {
        public static bool Transporting;
        public bool Transitioning;
        public Vector2 TransportPosition;
        public Player player;
        private int Count => Nodes.Count;
        public VirtualMap<BitrailNode> Map => BitrailNode.Map;
        public Grid Grid => BitrailNode.Grid;
        public const float Speed = 70;
        public static void Load()
        {
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
            Everest.Events.Level.OnTransitionTo += Level_OnTransitionTo;
        }

        private static void Level_OnTransitionTo(Level level, LevelData next, Vector2 direction)
        {
            Transporting = false;
        }

        public static void Unload()
        {
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
            Everest.Events.Level.OnTransitionTo -= Level_OnTransitionTo;
        }
        private static void LevelLoader_OnLoadingThread(Level level)
        {
            level.Add(new BitrailTransporter());
        }
        public List<BitrailNode> Nodes = new();
        public BitrailTransporter() : base()
        {
            Depth = 1;
            Tag |= Tags.Global | Tags.TransitionUpdate;
            BitrailNode.InitializeGrids();
            Collider = new Hitbox(Grid.Width, Grid.Height);
            Position = BitrailNode.MapPosition;
            for (int i = 0; i < Grid.CellsX; i++)
            {
                for (int j = 0; j < Grid.CellsY; j++)
                {
                    if (Grid[i, j])
                    {
                        BitrailNode node = new BitrailNode(new Vector2(i * 8, j * 8));
                        Nodes.Add(node);
                        Add(node);
                    }
                }
            }

            DashListener listener = new DashListener();
            listener.OnDash = (Vector2 dir) =>
            {
                /*if (!Transporting && player is not null && player.CollideFirst<BitrailNode>() is BitrailNode node)
                {
                    Add(new Coroutine(TransportRoutine(player, node)));
                }*/
            };
            //Add(listener);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = scene.GetPlayer();
            foreach (BitrailNode node in Nodes)
            {
                node.AddToGrid();
            }

            foreach (BitrailNode node in Nodes)
            {
                node.InitializeDirections();
            }
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Transporting = false;
        }
        public override void Render()
        {
            foreach (BitrailNode node in Nodes)
            {
                if (node.OnScreen)
                {
                    node.Render();
                }
            }
            Draw.Rect(TransportPosition, 8, 8, Color.Red);
        }
        public override void Update()
        {
            base.Update();
            if (Scene.GetPlayer() is Player player && Transporting)
            {
                player.MoveToX(TransportPosition.X + player.Width / 2);
                player.MoveToY(TransportPosition.Y + player.Height / 2);
            }
        }

        private IEnumerator TransportRoutine(Player player, BitrailNode startingNode)
        {
            Transporting = true;
            TransportPosition = startingNode.Position;
            player.Visible = false;
            player.DummyGravity = false;
            player.DummyFriction = false;
            BitrailNode node = startingNode;
            Vector2 dir = Vector2.Zero;
            while (Transporting)
            {
                bool isSingle = node.IsSingle;
                if (isSingle || node.IsIntersection)
                {
                    Vector2 prevDir = dir;
                    if (Input.MoveX.Value != 0)
                    {
                        dir = Vector2.UnitX * Input.MoveX.Value;
                    }
                    else if (Input.MoveY.Value != 0)
                    {
                        dir = Vector2.UnitY * Input.MoveY.Value;
                    }
                    if ((!isSingle && (prevDir.X == -dir.X || prevDir.Y == -dir.Y)) || !node.NextDirectionValid(dir))
                    {
                        dir = prevDir;
                    }
                    else if (node.Directions.Count < 4)
                    {
                        //automatically move to the left if at a 3 way intersection
                        dir = node.NextIntersectionDirection(dir);
                    }
                }
                if (dir.X != 0)
                {
                    if (dir.X > 0)
                    {
                        if (node.NodeRight != null) node = node.NodeRight;
                    }
                    else
                    {
                        if (node.NodeLeft != null) node = node.NodeLeft;
                    }
                }
                else if (dir.Y != 0)
                {
                    if (dir.Y > 0)
                    {
                        if (node.NodeBelow != null) node = node.NodeBelow;
                    }
                    else
                    {
                        if (node.NodeAbove != null) node = node.NodeAbove;

                    }
                }

                if (TransportPosition != node.Position)
                {
                    while (TransportPosition != node.Position)
                    {
                        TransportPosition = Calc.Approach(TransportPosition, node.Position, Speed * Engine.DeltaTime);
                        yield return null;
                    }
                }
                else
                {
                    if (node.IsCorner)
                    {
                        dir = node.NextCornerDirection(dir);
                    }
                    yield return null;
                }
            }
        }
    }
    [Tracked]
    public class BitrailNode : Image
    {
        public static Grid Grid;
        public static VirtualMap<BitrailNode> Map;
        public static Vector2 MapPosition;
        public BitrailNode NodeLeft;
        public BitrailNode NodeRight;
        public BitrailNode NodeAbove;
        public BitrailNode NodeBelow;
        public bool Intersection;
        public int CellX;
        public int CellY;
        public bool OnScreen;
        public enum NodeType
        {
            None,
            Up,
            Down,
            Left,
            Right,
            UpDown,
            LeftRight,
            UpLeft,
            DownLeft,
            UpRight,
            DownRight,
            UpDownLeft,
            UpDownRight,
            UpLeftRight,
            DownLeftRight,
            UpDownLeftRight
        }
        public NodeType Node;
        public bool IsCorner => (int)Node > 6 && (int)Node < 11;
        public bool IsIntersection => (int)Node > 10;
        public bool IsSingle => (int)Node < 5;
        public bool IsExit;
        public List<Direction> Directions = new();
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right,
            None
        }
        public static MTexture railTexture => GFX.Game["objects/PuzzleIslandHelper/bitRail/nodeTypes"];

        public static MTexture GetTextureQuad(MTexture texture, Vector2 position)
        {
            return texture.GetSubtexture((int)position.X * 8, (int)position.Y * 8, 8, 8);
        }
        public static void InitializeGrids()
        {
            float left = int.MaxValue, right = int.MinValue, top = int.MaxValue, bottom = int.MinValue;
            foreach (BitrailData d in PianoMapDataProcessor.Bitrails)
            {
                Vector2 pos = d.LevelData.Position + d.Offset;
                left = Calc.Min(pos.X, left);
                right = Calc.Max(pos.X, right);
                top = Calc.Min(pos.Y, top);
                bottom = Calc.Max(pos.Y, bottom);
            }
            int cellsX = (int)(MathHelper.Distance(right, left)) / 8;
            int cellsY = (int)(MathHelper.Distance(top, bottom)) / 8;
            Grid = new Grid(cellsX + 1, cellsY + 1, 8, 8);
            MapPosition = new Vector2(left, top);
            foreach (BitrailData d in PianoMapDataProcessor.Bitrails)
            {
                Vector2 pos = d.LevelData.Position + d.Offset - MapPosition;
                Grid[(int)pos.X / 8, (int)pos.Y / 8] = true;
            }
            Map = new VirtualMap<BitrailNode>(cellsX + 1, cellsY + 1, null);
        }
        public BitrailNode(Vector2 position) : base(null, true)
        {
            Position = position;
            Visible = false;
        }
        public static void Unload()
        {
            Grid = null;
            Map = null;
            MapPosition = Vector2.Zero;
        }
        public void AddToGrid()
        {
            Vector2 gridPos = Position;
            CellY = (int)(gridPos.Y) / 8;
            CellX = (int)(gridPos.X) / 8;
            Grid[CellX, CellY] = true;
            Map[CellX, CellY] = this;
        }
        public void InitializeDirections()
        {

            if (Grid[CellX, CellY - 1]) Directions.Add(Direction.Up);
            if (Grid[CellX, CellY + 1]) Directions.Add(Direction.Down);
            if (Grid[CellX - 1, CellY]) Directions.Add(Direction.Left);
            if (Grid[CellX + 1, CellY]) Directions.Add(Direction.Right);
            if (Directions.Count == 0) Directions.Add(Direction.None);

            NodeAbove = Map[CellX, CellY - 1];
            NodeBelow = Map[CellX, CellY + 1];
            NodeLeft = Map[CellX - 1, CellY];
            NodeRight = Map[CellX + 1, CellY];
            Intersection = Directions.Count > 2;
            addVisual(Directions);
        }
        private string directionToWord(Vector2 direction)
        {
            if (direction.X > 0) return "Right";
            else if (direction.X < 0) return "Left";
            else if (direction.Y > 0) return "Down";
            else if (direction.Y < 0) return "Up";
            return "None";
        }
        public Vector2 NextCornerDirection(Vector2 direction)
        {
            string word = Node.ToString().Replace(directionToWord(-direction), "");
            if (Enum.TryParse(word, out Direction nextDirection))
            {
                return DirectionToVector(nextDirection);
            }
            return direction;
        }
        public bool HasSameDirection(Vector2 direction)
        {
            if (direction.X > 0 && Directions.Contains(Direction.Right)) return true;
            if (direction.X < 0 && Directions.Contains(Direction.Left)) return true;
            if (direction.Y > 0 && Directions.Contains(Direction.Down)) return true;
            if (direction.Y < 0 && Directions.Contains(Direction.Up)) return true;
            return false;
        }
        public Vector2 NextIntersectionDirection(Vector2 direction)
        {
            if (HasSameDirection(direction)) return direction;
            if (direction.X > 0 && Directions.Contains(Direction.Up))
            {
                return -Vector2.UnitY;
            }
            else if (direction.X < 0 && Directions.Contains(Direction.Down))
            {
                return Vector2.UnitY;
            }
            else if (direction.Y > 0 && Directions.Contains(Direction.Right))
            {
                return Vector2.UnitX;
            }
            else if (direction.Y < 0 && Directions.Contains(Direction.Left))
            {
                return -Vector2.UnitX;
            }
            return direction;
        }
        public static Dictionary<Vector2, Direction> VectorDirections = new()
        {
            {Vector2.UnitX,Direction.Right},
            {-Vector2.UnitX,Direction.Left},
            {Vector2.UnitY,Direction.Down},
            {-Vector2.UnitY,Direction.Up},
            {Vector2.Zero, Direction.None}
        };
        public static Dictionary<Direction, Vector2> DirectionVectors = new()
        {
            {Direction.Right ,Vector2.UnitX},
            {Direction.Left ,-Vector2.UnitX},
            {Direction.Down,Vector2.UnitY},
            {Direction.Up,-Vector2.UnitY},
            {Direction.None ,Vector2.Zero}
        };
        public Vector2 DirectionToVector(Direction direction)
        {
            return DirectionVectors[direction];
        }
        public Direction VectorToDirection(Vector2 dir)
        {
            return VectorDirections[dir];
        }
        public bool NextDirectionValid(Vector2 dir)
        {
            return Directions.Contains(VectorToDirection(dir));
        }
        public string GetOrderedName(List<Direction> directions)
        {
            string result = "";
            foreach (Direction d in directions)
            {
                result += d.ToString();
            }
            return result;
        }
        public void addVisual(List<Direction> directions)
        {
            if (Enum.TryParse(GetOrderedName(directions), false, out NodeType type))
            {
                Node = type;
                IsExit = Node != NodeType.None && (int)Node < 5;
                Texture = GetTextureQuad(railTexture, new Vector2((int)type, 0));
            }
        }
        public override void Render()
        {
            base.Render();
            //Draw.Rect(RenderPosition, 8, 8, Color.Blue);
        }
        public override void Update()
        {
            base.Update();
            if (Scene is Level level)
            {
                Vector2 p = RenderPosition;
                OnScreen = p.X + 16 >= level.Camera.Position.X && p.X - 16 < level.Camera.Position.X + 320
                    && p.Y + 16 >= level.Camera.Position.Y && p.Y - 16 < level.Camera.Position.Y + 180;
            }
        }
    }
}
