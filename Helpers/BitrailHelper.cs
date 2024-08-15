using System;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using Direction = Celeste.Mod.PuzzleIslandHelper.Components.BitrailNode.Direction;
using DirectionCombos = Celeste.Mod.PuzzleIslandHelper.Components.BitrailNode.DirectionCombos;
using Nodes = Celeste.Mod.PuzzleIslandHelper.Components.BitrailNode.Nodes;
namespace Celeste.Mod.PuzzleIslandHelper.Helpers
{
    public static class BitrailHelper
    {
        public static Grid Grid;
        public static VirtualMap<BitrailNode> Map;
        public static Vector2 MapPosition;
        public class NodeFamily
        {
            public BitrailNode Parent;
            public HashSet<BitrailNode> Nodes = new();
            public HashSet<BitrailNode> EntryPoints = new();
            public NodeFamily(BitrailNode from)
            {
                Parent = from;
                from.ParentOfFamily = true;
                Nodes = from.AddMissingConnectedNodes(Nodes);
                foreach (BitrailNode node in Nodes)
                {
                    if (node.IsEntryPoint) EntryPoints.Add(node);
                    node.Family = this;
                }
            }
            public void Render()
            {
                foreach (BitrailNode node in Nodes)
                {
                    Draw.Line(Parent.RenderPosition, node.RenderPosition, Color.Red * 0.5f);
                }
                foreach (BitrailNode node in EntryPoints)
                {
                    Draw.Line(Parent.RenderPosition, node.RenderPosition, Color.Magenta);
                }
            }
        }
        public static void Unload()
        {
            Grid = null;
            Map = null;
            MapPosition = Vector2.Zero;
        }
        public static Dictionary<Vector2, Direction> VectorDirections = new()
        {
            {Vector2.UnitX,Direction.Right},
            {Vector2.UnitY,Direction.Down},
            {-Vector2.UnitY,Direction.Up},
            {-Vector2.UnitX,Direction.Left},
            {Vector2.Zero, Direction.None}
        };
        public static Dictionary<Direction, Vector2> DirectionVectors = new()
        {
            {Direction.Right ,Vector2.UnitX},
            {Direction.Down,Vector2.UnitY},
            {Direction.Up,-Vector2.UnitY},
            {Direction.Left ,-Vector2.UnitX},
            {Direction.None ,Vector2.Zero}
        };
        public static MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/bitRail/nodeTypes"];
        public static MTexture GetTexture(int num)
        {
            return GFX.Game["objects/PuzzleIslandHelper/bitRail/node" + num];
        }
        public static void AssignFamilies(HashSet<BitrailNode> nodes)
        {
            foreach (BitrailNode node in nodes)
            {
                if (node.IsEntryPoint)
                {
                    node.CreateFamily();
                }
            }
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
            int cellsX = (int)MathHelper.Distance(right, left) / 8;
            int cellsY = (int)MathHelper.Distance(top, bottom) / 8;
            Grid = new Grid(cellsX + 1, cellsY + 1, 8, 8);
            MapPosition = new Vector2(left, top);

            Map = new VirtualMap<BitrailNode>(cellsX + 1, cellsY + 1, null);
        }
        public static HashSet<BitrailNode> CreateNodes()
        {
            HashSet<BitrailNode> nodes = new();
            foreach (BitrailData d in PianoMapDataProcessor.Bitrails)
            {
                Vector2 pos = d.LevelData.Position + d.Offset - MapPosition;
                int x = (int)pos.X / 8;
                int y = (int)pos.Y / 8;
                Grid[x, y] = true;
                BitrailNode node = new BitrailNode(pos.Floor(), d);
                nodes.Add(node);
            }
            return nodes;
        }
        public static Dictionary<BitrailNode, Collider> CreateExitColliders(HashSet<BitrailNode> nodes)
        {
            Dictionary<BitrailNode, Collider> dict = new();
            foreach (BitrailNode node in nodes)
            {
                if (node.Node is Nodes.DeadEnd or Nodes.Single)
                {
                    dict.Add(node, new Hitbox(8,8));
                }
            }
            return dict;
        }
        public static Dictionary<string, HashSet<BitrailNode>> ExitNodesByLevel(HashSet<BitrailNode> nodes, Level level)
        {
            Dictionary<string, HashSet<BitrailNode>> dict = new();
            MapData data = level.Session.MapData;
            foreach (BitrailNode node in nodes)
            {
                if (node.Node is Nodes.DeadEnd or Nodes.Single)
                {
                    LevelData lData = data.GetAt(node.RenderPosition);
                    if (lData != null)
                    {
                        string levelName = lData.Name;
                        if (!dict.ContainsKey(levelName))
                        {
                            dict.Add(levelName, new());
                        }
                        dict[levelName].Add(node);
                    }
                }
            }
            return dict;
        }
        public static void PrepareNodes(HashSet<BitrailNode> nodes)
        {
            foreach (BitrailNode node in nodes)
            {
                node.AddToGrid();
            }
            foreach (BitrailNode node in nodes)
            {
                node.InitializeDirections();
            }
        }
        public static BitrailNode GetNeighbor(this BitrailNode from, Vector2 direction)
        {
            if (from == null || direction == Vector2.Zero) return from;
            //if x is not 0, check left or right. otherwise, if y is not 0, check above or below
            BitrailNode result = direction.X != 0 ? direction.X > 0 ? from.NodeRight : from.NodeLeft : direction.Y > 0 ? from.NodeBelow : from.NodeAbove;
            return result ?? from;
        }
        public static Vector2 NextCornerDirection(this BitrailNode node, Vector2 direction)
        {
            DirectionCombos type = node.Combo;
            return type switch
            {
                DirectionCombos.UpLeft => direction.Y > 0 ? -Vector2.UnitX : -Vector2.UnitY,
                DirectionCombos.DownLeft => direction.Y < 0 ? -Vector2.UnitX : Vector2.UnitY,
                DirectionCombos.UpRight => direction.Y > 0 ? Vector2.UnitX : -Vector2.UnitY,
                DirectionCombos.DownRight => direction.Y < 0 ? Vector2.UnitX : Vector2.UnitY,
                _ => direction
            };
        }
        public static Vector2 NextDirection(this BitrailNode node, Vector2 currentDirection)
        {
            return node.Node switch
            {
                Nodes.DeadEnd => NextSingleDirection(node),
                Nodes.Single => Vector2.Zero,
                Nodes.Corner => NextCornerDirection(node, currentDirection),
                Nodes.ThreeWay => NextIntersectionDirection(node, currentDirection),
                _ => currentDirection
            };
        }

        public static Vector2 NextSingleDirection(this BitrailNode node)
        {
            if (Enum.TryParse(node.Combo.ToString(), out Direction result))
            {
                return DirectionVectors[result];
            }
            return Vector2.Zero;
        }
        public static bool HasSameDirection(this BitrailNode node, Vector2 direction)
        {
            if (direction == Vector2.Zero && node.Control is BitrailNode.ControlTypes.Full) return true;
            if (direction.X > 0 && node.Directions.Contains(Direction.Right)) return true;
            if (direction.X < 0 && node.Directions.Contains(Direction.Left)) return true;
            if (direction.Y > 0 && node.Directions.Contains(Direction.Down)) return true;
            if (direction.Y < 0 && node.Directions.Contains(Direction.Up)) return true;
            return false;
        }
        public static Vector2 NextIntersectionDirection(this BitrailNode node, Vector2 direction)
        {
            if (HasSameDirection(node, direction)) return direction;
            if (direction.X > 0 && node.Directions.Contains(Direction.Up))
            {
                return -Vector2.UnitY;
            }
            else if (direction.X < 0 && node.Directions.Contains(Direction.Down))
            {
                return Vector2.UnitY;
            }
            else if (direction.Y > 0 && node.Directions.Contains(Direction.Right))
            {
                return Vector2.UnitX;
            }
            else if (direction.Y < 0 && node.Directions.Contains(Direction.Left))
            {
                return -Vector2.UnitX;
            }
            return direction;
        }
        public static Vector2 DirectionToVector(Direction direction)
        {
            return DirectionVectors[direction];
        }
        public static Direction VectorToDirection(Vector2 dir)
        {
            return VectorDirections[dir];
        }
        public static bool NextDirectionValid(this BitrailNode node, Vector2 dir)
        {
            if (node.Control == BitrailNode.ControlTypes.Full && dir == Vector2.Zero) return true;
            return node.Directions.Contains(VectorToDirection(dir));
        }
        public static string GetOrderedName(List<Direction> directions)
        {
            string result = "";
            foreach (Direction d in directions)
            {
                result += d.ToString();
            }
            return result;
        }
    }
}
