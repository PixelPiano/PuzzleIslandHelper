using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ChainButtonModule")]
    [Tracked]
    public class ChainButtonModule : Entity
    {
        public class ButtonNode : GraphicsComponent
        {
            public bool On
            {
                get => Flag.GetFlag();
                set => Flag.SetFlag(value);
            }
            public string Flag;
            public Color NodeColor => On ? Color.Lime : Color.Red;
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
                Texture.Draw(RenderPosition + Texture.Size(), Origin, NodeColor, Scale, Rotation, Effects);
            }

        }
        public ButtonNode[] Nodes = new ButtonNode[10];

        public bool AllNodesActivate
        {
            get
            {
                foreach (ButtonNode node in Nodes)
                {
                    if (!node.On) return false;
                }
                return true;
            }
        }
        public FlagData Flag;
        public override void Update()
        {
            base.Update();
            foreach (var node in Nodes)
            {
                if (!node.On)
                {
                    Flag.State = false;
                    return;
                }
            }
            Flag.State = true;
        }
        [Command("button_module_state", "")]
        public static void moduleState(bool state)
        {
            if (Engine.Scene is Level level && level.GetPlayer() is Player player)
            {
                foreach (ChainButtonModule m in level.Tracker.GetEntities<ChainButtonModule>())
                {
                    foreach (ButtonNode n in m.Nodes)
                    {
                        n.On = state;
                    }
                }
            }
        }
        public string Prefix;
        private string[] suffixes = ["TL", "TM", "TR", "MLL", "ML", "MR", "MRR", "BL", "BM", "BR"];
        public ChainButtonModule(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Prefix = data.Attr("buttonFlagPrefix");
            Flag = new FlagData(Prefix + ":Active");
            Collider = new Hitbox(data.Width, data.Height);
            Depth = 10;
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
            if (!Flag) base.Render();
        }
    }
}