using System;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.PuzzleIslandHelper.Helpers.BitrailHelper;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class BitrailNode : Image
    {
        public Grid Grid => BitrailHelper.Grid;
        public VirtualMap<BitrailNode> Map => BitrailHelper.Map;
        public Vector2? MapPosition => BitrailHelper.MapPosition;
        public NodeFamily Family;
        public BitrailTransporter Parent;
        public BitrailNode NodeLeft;
        public BitrailNode NodeRight;
        public BitrailNode NodeAbove;
        public BitrailNode NodeBelow;
        public Vector2 RenderCenter => RenderPosition + new Vector2(Width, Height) / 2;
        public bool Intersection;
        public int CellX;
        public int CellY;
        public bool OnScreen;
        public float WarningAmount;
        public bool ParentOfFamily;

        public MTexture SecondaryTex;
        public enum DirectionCombos
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
        public DirectionCombos Combo;

        public enum Nodes
        {
            Single,
            DeadEnd,
            Straight,
            Corner,
            ThreeWay,
            FourWay,
            Exit
        }
        public Nodes Node;
        public enum ControlTypes
        {
            Default,
            None,
            Full
        }
        public ControlTypes Control;
        public List<Direction> Directions = new();
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right,
            None
        }
        public bool IsEntryPoint;
        public bool IsExit;
        public int Bounces;
        public readonly string GroupID;
        public readonly float TimeLimit;
        public BitrailNode(Vector2 position, BitrailData data) : base(null, true)
        {
            Position = position;
            Visible = false;
            IsExit = data.IsExit;
            Color = data.Color;
            Bounces = data.Bounces;
            GroupID = data.GroupID;
            TimeLimit = data.TimeLimit;
            Control = data.ControlType;
        }
        public void CreateFamily()
        {
            Family ??= new NodeFamily(this);
        }
        public void BounceBack()
        {
            Bounces = (int)Calc.Max(Bounces - 1, 0);
            //todo play bounce sound
        }
        public HashSet<BitrailNode> AddMissingConnectedNodes(HashSet<BitrailNode> list)
        {
            if (NodeAbove != null && !list.Contains(NodeAbove))
            {
                list.Add(NodeAbove);
                NodeAbove.AddMissingConnectedNodes(list);
            }
            if (NodeBelow != null && !list.Contains(NodeBelow))
            {
                list.Add(NodeBelow);
                NodeBelow.AddMissingConnectedNodes(list);
            }
            if (NodeLeft != null && !list.Contains(NodeLeft))
            {
                list.Add(NodeLeft);
                NodeLeft.AddMissingConnectedNodes(list);
            }
            if (NodeRight != null && !list.Contains(NodeRight))
            {
                list.Add(NodeRight);
                NodeRight.AddMissingConnectedNodes(list);
            }
            return list;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            Parent = entity as BitrailTransporter;
            Vector2 p = RenderPosition;
            Bounds = new Rectangle((int)p.X, (int)p.Y, 8, 8);
        }
        public void AddToGrid()
        {
            Vector2 gridPos = Position;
            CellY = (int)gridPos.Y / 8;
            CellX = (int)gridPos.X / 8;
            Grid[CellX, CellY] = true;
            Map[CellX, CellY] = this;
        }
        public bool CheckGrid(int cellX, int cellY)
        {
            return Map[cellX, cellY].GroupID == GroupID;
        }
        public void InitializeDirections()
        {
            //Save references to neighboring nodes
            BitrailNode nodeAbove = Map[CellX, CellY - 1];
            BitrailNode nodeBelow = Map[CellX, CellY + 1];
            BitrailNode nodeLeft = Map[CellX - 1, CellY];
            BitrailNode nodeRight = Map[CellX + 1, CellY];

            //Create connections to neighboring nodes
            if (nodeAbove != null && nodeAbove.GroupID == GroupID)
            {
                NodeAbove = nodeAbove;
                Directions.Add(Direction.Up);
            }
            if (nodeBelow != null && nodeBelow.GroupID == GroupID)
            {
                NodeBelow = nodeBelow;
                Directions.Add(Direction.Down);
            }
            if (nodeLeft != null && nodeLeft.GroupID == GroupID)
            {
                NodeLeft = nodeLeft;
                Directions.Add(Direction.Left);
            }
            if (nodeRight != null && nodeRight.GroupID == GroupID)
            {
                NodeRight = nodeRight;
                Directions.Add(Direction.Right);
            }
            if (Directions.Count == 0) Directions.Add(Direction.None);
            Intersection = Directions.Count > 2;

            //Add the texture
            if (Enum.TryParse(GetOrderedName(Directions), false, out DirectionCombos type))
            {
                Combo = type;
                int num = (int)Combo;
                Node = num switch
                {
                    15 => Nodes.FourWay,
                    > 10 => Nodes.ThreeWay,
                    > 6 => Nodes.Corner,
                    > 4 => Nodes.Straight,
                    > 0 => Nodes.DeadEnd,
                    _ => Nodes.Single
                };
                int frame = (int)type;
                if (IsExit && Node is Nodes.DeadEnd)
                {
                    Node = Nodes.Exit;
                    frame += 15;
                }
                IsEntryPoint = Node is Nodes.DeadEnd or Nodes.Single;
                Frame = frame;
                Texture = GetTexture(frame);
                SecondaryTex = Control switch
                {
                    ControlTypes.Full => GFX.Game["objects/PuzzleIslandHelper/bitRail/fullControl" + num],
                    ControlTypes.None => GFX.Game["objects/PuzzleIslandHelper/bitRail/noControl" + num],
                    _ => null
                };
            }
        }
        public void SwitchToInSolid()
        {
            Texture = GFX.Game["objects/PuzzleIslandHelper/bitRail/inSolidNode" + Frame];
            InSolid = true;
        }
        public int Frame;
        public bool InSolid;
        public bool InMask;
        public Rectangle Bounds;
        public void RenderAt(Vector2 position)
        {
            if (Texture != null)
            {
                Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, position, null, Color.Lerp(Color, Color.Red, WarningAmount), Rotation, Origin, Scale, Effects, 0);
            }
            if (SecondaryTex != null)
            {
                Draw.SpriteBatch.Draw(SecondaryTex.Texture.Texture_Safe, position, null, Color.Lerp(Color, Color.Red, WarningAmount), Rotation, Origin, Scale, Effects, 0);
            }
            if (Bounces > 0)
            {
                Color color = Bounces switch
                {
                    1 => Color.Cyan,
                    2 => Color.Red,
                    _ => Color.Purple,
                };
                Draw.Rect(position + Vector2.One * 2, 4, 4, color);
            }
        }
        public override void Render()
        {
            RenderAt(RenderPosition);
        }
        public override void Update()
        {
            base.Update();
            WarningAmount = Calc.Approach(WarningAmount, 0, Engine.DeltaTime);
            if (Scene is Level level)
            {
                Vector2 p = RenderPosition;
                OnScreen = InBounds(p, level.Camera.Position, 320, 180, 16);
            }
        }
        public bool InBounds(Vector2 check, Vector2 topLeft, float width, float height, float padding)
        {
            return check.X + padding >= topLeft.X && check.X - padding < topLeft.X + width
                && check.Y + padding >= topLeft.Y && check.Y - 16 < topLeft.Y + height;
        }
    }
}
