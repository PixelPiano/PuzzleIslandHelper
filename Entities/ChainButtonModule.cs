using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ChainButtonModule")]
    [Tracked]
    public class ChainButtonModule : Entity
    {
        public class ButtonNode : GraphicsComponent
        {
            public bool On => Flag.GetFlag();
            public string Flag;
            public Color NodeColor => On ? Color.Red : Color.Lime;
            public MTexture Texture;
            public ButtonNode(Vector2 position, string flag) : base(true)
            {
                Flag = flag;
                Position = position;
                Texture = GFX.Game["objects/PuzzleIslandHelper/puzzles/square"];
            }
            public override void Render()
            {
                base.Render();
                Texture.Draw(RenderPosition, Origin, NodeColor, Scale, Rotation, Effects);
            }

        }
        public ButtonNode[] Nodes = new ButtonNode[10];

        public bool AllNodesActivate
        {
            get
            {
                foreach(ButtonNode node in Nodes)
                {
                    if(!node.On) return false;
                }
                return true;
            }
        }
        public string Prefix;
        private string[] suffixes = ["TL", "TM", "TR", "MLL", "ML", "MR", "MRR", "BL", "BM", "BR"];
        public ChainButtonModule(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Prefix = data.Attr("buttonFlagPrefix");
            Collider = new Hitbox(data.Width, data.Height);
            float ypad = 1;
            float xpad = 1;
            float h = (Width - xpad) / 5;
            float h2 = (Width - xpad) / 4;
            float v = (Height - ypad) / 3;
            for (int i = 0; i < 3; i++)
            {
                ButtonNode node = new ButtonNode(new Vector2(xpad + h * (1 + i), ypad), Prefix + suffixes[i]);
                Nodes[i] = node;
                Add(node);
            }
            for (int i = 0; i < 4; i++)
            {
                ButtonNode node = new ButtonNode(new Vector2(xpad + h2 * i, ypad + v), Prefix + suffixes[3 + i]);
                Nodes[3 + i] = node;
                Add(node);
            }
            for (int i = 0; i < 3; i++)
            {
                ButtonNode node = new ButtonNode(new Vector2(xpad + h * (1 + i), ypad + v * 2), Prefix + suffixes[7 + i]);
                Nodes[7 + i] = node;
                Add(node);
            }
            Add(Nodes);
        }
        public override void Render()
        {
            Draw.Rect(Collider, Color.Black);
            Draw.HollowRect(Position - Vector2.One, Width + 2, Height + 2, Color.White);
            base.Render();
        }
    }
}