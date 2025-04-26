using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.PuzzleIslandHelper.Entities.WARP.WARPData;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    public class Node
    {
        public NodeTypes Type;
        public Vector2 Center;
        public Vector2 TopLeft => new Vector2(Left, Top);
        public Vector2 Head => Center - Vector2.UnitY * 77;
        public float Top => Center.Y - Height / 2;
        public float Bottom => Center.Y + Height / 2;
        public float Left => Center.X - Width / 2;
        public float Right => Center.X + Width / 2;
        public Vector2 RenderPosition => Center - new Vector2(Width, Height) / 2f;
        public float Width => HeadTex.Width;
        public float Height => HeadTex.Height;
        public float HeadHeight = 142;
        public bool Lit;
        public int Index;
        public MTexture HeadTex => GFX.Game[UI.Path + "nodeHead"];
        public MTexture BodyTex => GFX.Game[UI.Path + "nodeBody"];
        public bool Obtained => Inv.HasNode(Type);
        public Dictionary<NodeTypes, List<NodeTypes>> ImpliedConnections = [];
        public Node(Vector2 center, NodeTypes node)
        {
            Center = center;
            Type = node;
            Index = (int)node;
            switch (node)
            {
                case NodeTypes.TL:
                    ImpliedConnections.Add(NodeTypes.TR, [NodeTypes.TM]);
                    ImpliedConnections.Add(NodeTypes.BM, [NodeTypes.ML]);
                    break;
                case NodeTypes.TM:
                    ImpliedConnections.Add(NodeTypes.BL, [NodeTypes.ML]);
                    ImpliedConnections.Add(NodeTypes.BR, [NodeTypes.MR]);
                    break;
                case NodeTypes.TR:
                    ImpliedConnections.Add(NodeTypes.TL, [NodeTypes.TM]);
                    ImpliedConnections.Add(NodeTypes.BM, [NodeTypes.MR]);
                    break;
                case NodeTypes.MLL:
                    ImpliedConnections.Add(NodeTypes.MR, [NodeTypes.ML]);
                    ImpliedConnections.Add(NodeTypes.MRR, [NodeTypes.ML, NodeTypes.MR]);
                    break;
                case NodeTypes.ML:
                    ImpliedConnections.Add(NodeTypes.MRR, [NodeTypes.MR]);
                    break;
                case NodeTypes.MR:
                    ImpliedConnections.Add(NodeTypes.MLL, [NodeTypes.ML]);
                    break;
                case NodeTypes.MRR:
                    ImpliedConnections.Add(NodeTypes.ML, [NodeTypes.MR]);
                    ImpliedConnections.Add(NodeTypes.MLL, [NodeTypes.MR, NodeTypes.ML]);
                    break;
                case NodeTypes.BL:
                    ImpliedConnections.Add(NodeTypes.TM, [NodeTypes.ML]);
                    ImpliedConnections.Add(NodeTypes.BR, [NodeTypes.BM]);
                    break;
                case NodeTypes.BM:
                    ImpliedConnections.Add(NodeTypes.TL, [NodeTypes.ML]);
                    ImpliedConnections.Add(NodeTypes.TR, [NodeTypes.MR]);
                    break;
                case NodeTypes.BR:
                    ImpliedConnections.Add(NodeTypes.BL, [NodeTypes.BM]);
                    ImpliedConnections.Add(NodeTypes.TM, [NodeTypes.MR]);
                    break;
            }

            foreach (KeyValuePair<NodeTypes, List<NodeTypes>> pair in ImpliedConnections)
            {
                ImpliedConnections[pair.Key].Add(pair.Key);
            }
        }
        public void DrawTexture()
        {
            Vector2 pos = RenderPosition;
            if (Obtained)
            {
                BodyTex.DrawOutline(pos);
                HeadTex.DrawOutline(pos);
                BodyTex.Draw(pos);
                HeadTex.Draw(pos, Vector2.Zero, Lit ? Color.Orange : Color.Red);
            }
            else
            {
                BodyTex.Draw(pos, Vector2.Zero, Color.Gray);
                HeadTex.Draw(pos, Vector2.Zero, Color.DarkGray);
            }
        }
        public void TurnOn()
        {
            Lit = true;
        }
        public void TurnOff()
        {
            Lit = false;
        }
        public bool Check(Vector2 pos)
        {
            if (!Obtained) return false;
            Vector2 position = RenderPosition;
            float pad = 0;
            if (pos.X > position.X + pad && pos.Y > position.Y + pad && pos.X < position.X + Width - pad)
            {
                return pos.Y < position.Y + HeadHeight - pad;
            }
            return false;
        }
        public bool CanConnectTo(Node node)
        {
            return Obtained && node != this;
        }
    }
}
