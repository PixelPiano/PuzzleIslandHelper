using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.PuzzleIslandHelper.Entities.WARP.WARPData;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    public class UI : Entity
    {
        public class Node : Component
        {
            public static VirtualMap<int?> NodeMap;
            [OnLoad]
            public static void Load()
            {
                NodeMap = new(7, 3);
                NodeMap[1, 0] = 0;
                NodeMap[3, 0] = 1;
                NodeMap[5, 0] = 2;

                NodeMap[0, 1] = 3;
                NodeMap[2, 1] = 4;
                NodeMap[4, 1] = 5;
                NodeMap[6, 1] = 6;

                NodeMap[1, 2] = 7;
                NodeMap[3, 2] = 8;
                NodeMap[5, 2] = 9;
            }
            public static (int x, int y) NextInNodeMap(int current, int moveX, int moveY)
            {
                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (NodeMap[i, j].HasValue && NodeMap[i, j] == current)
                        {
                            int x = i.Wrap(0, NodeMap.Columns - 1, moveX);
                            int y = j.Wrap(0, NodeMap.Rows - 1, moveY);
                            while (NodeMap[x, y] == null)
                            {
                                x = x.Wrap(0, NodeMap.Columns - 1, moveX);
                                y = y.Wrap(0, NodeMap.Rows - 1, moveY);
                            }
                            if (NodeMap[x, y].HasValue)
                            {
                                return (x, y);
                            }
                            else
                            {
                                return (i, j);
                            }
                        }
                    }
                }
                return (0, 0);
                // 0 1 2
                //3 4 5 6
                // 7 8 9
            }
            public bool Held;
            public bool DrawBounds;
            public static bool Lined => UI.Lined;
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
            public int Index;
            public MTexture HeadTex => GFX.Game[UI.Path + "nodeHead"];
            public MTexture BodyTex => GFX.Game[UI.Path + "nodeBody"];
            public Color OnColor = Color.Orange;
            public Color OffColor = Color.Red;
            public Color IncorrectColor = Color.DarkRed;
            public Color FlashColor;
            public Color FinalColor => Color.Lerp(Color.Lerp(OffColor, OnColor, HeadColorLerp), FlashColor, FlashLerp);
            public float HeadColorLerp;
            public float FlashLerp
            {
                get
                {
                    if (flashTime == 0) return 0;
                    return flashTimer / flashTime;
                }
            }
            public bool Obtained => Inv.HasNode(Type);
            private Tween tween;
            private float flashTimer;
            private float flashTime = 1;
            private Ease.Easer flashEase = Ease.Linear;
            public Dictionary<NodeTypes, List<NodeTypes>> ImpliedConnections = [];
            public Node(Vector2 center, NodeTypes node) : base(true, true)
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
                if (DrawBounds)
                {
                    Draw.Rect(pos - Vector2.One * 8, Math.Max(BodyTex.Width, HeadTex.Width) + 16, Math.Max(BodyTex.Height, HeadTex.Height) + 16, Color.Magenta);
                }

                if (Obtained)
                {
                    BodyTex.DrawOutline(pos);
                    HeadTex.DrawOutline(pos);
                    BodyTex.Draw(pos, Vector2.Zero, Held ? Color.Yellow : Color.White);
                    HeadTex.Draw(pos, Vector2.Zero, FinalColor);
                }
                else
                {
                    BodyTex.Draw(pos, Vector2.Zero, Color.Gray);
                    HeadTex.Draw(pos, Vector2.Zero, Color.DarkGray);
                }
            }
            public override void Update()
            {
                base.Update();

                if (flashTimer > 0)
                {
                    flashTimer -= Engine.DeltaTime;
                }
                else
                {
                    flashTimer = 0;
                }
            }
            public void TurnOn()
            {
                HeadColorLerp = 1;
                tween?.Stop();
                tween?.RemoveSelf();
                if (!Lined)
                {
                    Flash(OnColor, 1);
                }
            }
            public void Flash(Color color, float time = 1)
            {
                FlashColor = color;
                flashTime = time;
                flashTimer = time;
                flashEase = Ease.SineIn;
            }
            public void IncorrectFlash(float time = 1)
            {
                Flash(Color.DarkRed, time);
            }
            public void TurnOff()
            {
                flashTime = flashTimer = 0;
                flashEase = Ease.Linear;
                HeadColorLerp = 0;
                FlashColor = Color.Transparent;
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
        public class RuneIndent
        {
            public WarpRune Rune;
            public ConnectionList List;
            public Color Color;
            public RuneIndent(WarpRune rune, List<Node> nodes, Color color)
            {
                Rune = rune;
                List = new ConnectionList(nodes);
                List.TransferFromRune(rune);
                Color = color;
            }
            public void Render(Vector2 position)
            {
                foreach (Connection c in List.Connections)
                {
                    c.RenderNodeLine(3, Color);
                }
            }
        }
        public const string Path = "objects/PuzzleIslandHelper/runeUI/";
        public static bool Lined => true;//PianoModule.Settings.WarpCapsuleDisplayMode == PianoModuleSettings.WCDM.Lined;
        public enum Control
        {
            None,
            Pad,
            Mouse
        }
        public Control ControlMode;
        public static char?[,] map = new char?[3, 5]
        {
            {'h', '0','1','2',null},
            {'e','3','4','5','6'},
            {'s', '7','8','9',null},
        };
        public int MapX;
        public int MapY;
        public class Connection
        {
            public override string ToString()
            {
                string output = "";
                if (!Empty)
                {
                    foreach (Node n in Nodes.OrderBy(item => item.Index))
                    {
                        output += n.Index;
                    }
                }
                return output;
            }
            public Node[] Nodes = new Node[2];
            public bool Incomplete => Nodes[0] == null || Nodes[1] == null;
            public bool Empty => Nodes[0] == null && Nodes[1] == null;
            public bool Naive;
            public bool IsForbidden => Nodes[0] != null && Nodes[1] != null && !Naive && ((!Nodes[0].Obtained && !Nodes[1].Obtained) || Nodes[0].ImpliedConnections.ContainsKey(Nodes[1].Type));
            public Connection(Node a, Node b)
            {
                Nodes = [a, b];
            }
            public Node GetPartner(Node exclude) => Nodes[0] == exclude ? Nodes[1] : Nodes[0];
            public bool CollideHead(Vector2 pos)
            {
                bool a = false, b = false;
                if (Nodes[0] != null)
                {
                    a = Nodes[0].Check(pos);
                }
                if (Nodes[1] != null)
                {
                    b = Nodes[1].Check(pos);
                }
                return a || b;

            }
            public bool CollideLine(Vector2 pos)
            {
                if (Nodes[0] != null && Nodes[1] != null)
                {
                    return Collide.RectToLine(pos.X, pos.Y, 30, 30, Nodes[0].Head, Nodes[1].Head);
                }
                return false;
            }
            public bool TryGetTuple(out (int, int) tuple)
            {
                tuple = (0, 0);
                if (Incomplete)
                {
                    return false;
                }
                tuple = (Nodes[0].Index, Nodes[1].Index);
                return true;
            }
            public Node FirstNodeNotNull => Nodes[0] ?? Nodes[1];
            public bool Contains(Node node)
            {
                return node != null && Nodes != null && Nodes.Contains(node);
            }
            public bool Match(Node a, Node b)
            {
                return a != b && Contains(a) && Contains(b);
            }
            public void Fill(Node fill)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (Nodes[i] == null)
                    {
                        Nodes[i] = fill;
                        Nodes[i].TurnOn();
                        return;
                    }
                }
            }
            public void Hold(Node clicked)
            {
                if (clicked != null)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (Nodes[i] == clicked)
                        {
                            Nodes[i].TurnOff();
                            Nodes[i] = null;
                            return;
                        }
                    }
                }
            }
            public const int DefaultThickness = 50;
            public void RenderNodeLine(int? thickness = null, Color? color = null)
            {
                Draw.Line(Nodes[0].Head, Nodes[1].Head, color ?? Color.White, thickness ?? DefaultThickness);
            }
            public void RenderNodeLine(Vector2 position, Vector2 scale, Color color, int thickness)
            {
                Draw.Line(position + Nodes[0].Head * scale, position + Nodes[1].Head * scale, Color.White, thickness);
            }
            public void RenderMouseLine(Vector2 mouse)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (Nodes[i] != null)
                    {
                        Draw.Line(Nodes[i].Head, mouse, Color.White, DefaultThickness);
                    }
                }
            }
            public bool Match(Connection other)
            {
                return Match(other.Nodes[0], other.Nodes[1]);
            }
        }
        public class ConnectionList
        {
            public List<Connection> Connections = new();
            public List<Connection> Held = new();
            public List<int> NodeConnects = new();
            public Connection LeftHeld
            {
                get => _leftHeld;
                set
                {
                    if (_leftHeld != null)
                    {
                        foreach (Node n in _leftHeld.Nodes)
                        {
                            if (n != null && (value == null || !value.Nodes.Contains(n)))
                            {
                                n.Held = false;
                            }
                        }
                    }
                    _leftHeld = value;
                }
            }
            private Connection _leftHeld;
            public List<Node> Nodes = new();
            public ConnectionList(List<Node> validNodes)
            {
                RegisterNodes(validNodes);
            }
            public void TransferFromRune(WarpRune rune)
            {
                ClearHeld();
                RemoveAll();
                List<(Node, Node)> list = [];

                foreach (var rf in WarpRune.ToFragments(rune))
                {
                    int item1 = (int)rf.NodeA;
                    int item2 = (int)rf.NodeB;
                    list.Add((Nodes[item1], Nodes[item2]));
                }
                foreach (var pair in list)
                {
                    AddValidConnections(pair.Item1, pair.Item2, out _, true);
                }
            }
            public static List<Connection> GetAllUnique(List<Connection> a)
            {
                List<Connection> unique = new();
                foreach (Connection c in a)
                {
                    bool duplicate = false;
                    foreach (Connection c2 in unique)
                    {
                        if (c.Match(c2))
                        {
                            duplicate = true;
                            break;
                        }
                    }
                    if (!duplicate)
                    {
                        unique.Add(c);
                    }
                }
                return unique;
            }
            public override string ToString()
            {
                string o = "";
                foreach (Connection c in Connections)
                {
                    o += c.ToString();
                }
                return o;
            }
            private IEnumerator chainTurnOn(Entity e, HashSet<Node> nodes, Node start, Node end)
            {
                foreach (Node n in nodes.Except([start, end]))
                {
                    n?.TurnOn();
                    yield return 0.1f;
                }
                end?.TurnOn();
                e?.RemoveSelf();
            }
            public void OnLeftClick(Node node)
            {
                if (!Lined)
                {
                    if (LeftHeld == null)
                    {
                        if (node != null)
                        {
                            LeftHeld = new Connection(node, null);
                            node.Flash(node.OnColor);
                        }

                    }
                    else
                    {
                        if (node != null && LeftHeld.FirstNodeNotNull is Node held)
                        {
                            AddValidConnections(held, node, out HashSet<Node> nodes);
                            foreach (var n in nodes)
                            {
                                if (n != held && n != node)
                                {
                                    n?.Flash(n.OnColor);
                                }
                            }
                            LeftHeld = new Connection(node, null);
                            node.Flash(node.OnColor);
                        }
                    }
                }
                else
                {
                    if (Held.Count == 0 && node != null && LeftHeld == null)
                    {
                        LeftHeld = new Connection(node, null);
                    }
                }
            }
            public static WarpRune.RuneNodeInventory NodeInventory => PianoModule.SaveData.RuneNodeInventory;
            public bool TryAddConnection(Node from, Node to)
            {
                if (!HasPair(from, to) && NodeInventory.HasNodes(from.Type, to.Type))
                {
                    AddConnection(from, to);
                    return true;
                }
                return false;
            }
            public void AddValidConnections(Node from, Node to, out HashSet<Node> nodes, bool naive = false)
            {
                nodes = [];
                void add(HashSet<Node> hash, Node f, Node t, bool naive)
                {
                    bool added = false;
                    if (naive)
                    {
                        if (!HasPair(f, t))
                        {
                            Connection c = AddConnection(f, t);
                            c.Naive = true;
                            added = true;
                        }
                    }
                    else if (TryAddConnection(f, t))
                    {
                        added = true;
                    }
                    if (added || !Lined)
                    {
                        hash.Add(f);
                        hash.Add(t);
                    }
                }
                if (from != to)
                {
                    if (from.ImpliedConnections.TryGetValue(to.Type, out List<NodeTypes> list))
                    {
                        NodeTypes start = from.Type;
                        foreach (NodeTypes node in list)
                        {
                            Node fromNode = Nodes[(int)start];
                            Node connect = Nodes[(int)node];
                            add(nodes, fromNode, connect, naive);
                            start = node;
                        }
                    }
                    else add(nodes, from, to, naive);
                }

            }

            public void OnLeftRelease(Node node)
            {
                if (Lined)
                {
                    if (node != null && LeftHeld != null && LeftHeld.FirstNodeNotNull is Node held)
                    {
                        AddValidConnections(held, node, out _);
                    }
                    LeftHeld = null;
                }
            }

            public void OnLeftHeld(Vector2 mouse, Node node)
            {
                if (!Lined)
                {
                    if (LeftHeld == null && Held.Count == 0)
                    {
                        List<Connection> toRemove = new();

                        foreach (Connection c in Connections)
                        {
                            if (c.IsForbidden || (c.CollideLine(mouse) && !c.CollideHead(mouse)))
                            {
                                toRemove.Add(c);
                            }
                        }
                        foreach (var c in toRemove)
                        {
                            RemoveConnection(c);
                        }
                    }
                }
            }
            public void OnRightClick(Node node)
            {
                if (Lined)
                {
                    if (LeftHeld == null)
                    {
                        if (node == null)
                        {
                            ClearHeld();
                        }
                        else if (Held.Count > 0)
                        {
                            TransferHeldTo(node);
                        }
                        else
                        {
                            HoldAllConnectionsFrom(node);
                        }
                    }
                }
                else
                {
                    LeftHeld = null;
                }
            }
            public void OnRightRelease(Node node)
            {
                if (Lined)
                {
                    if (node != null && Held.Count > 0)
                    {
                        TransferHeldTo(node);
                    }
                    ClearHeld();
                }
            }
            public void RegisterNodes(List<Node> nodes)
            {
                foreach (Node node in nodes)
                {
                    NodeConnects.Add(0);
                }
                Nodes = nodes;
            }
            public void AddConnection(NodeTypes a, NodeTypes b)
            {
                AddConnection(Nodes[(int)a], Nodes[(int)b]);
            }
            public void AddConnection(Connection c)
            {
                Connections.Add(c);
            }
            public Connection AddConnection(Node a, Node b)
            {
                Connection c = new Connection(a, b);
                AddConnection(c);
                return c;
            }
            public void RemoveConnection(Connection c)
            {
                Connections.Remove(c);
            }
            public void TransferHeldTo(Node node)
            {
                foreach (Connection c in Held)
                {
                    if (!c.Contains(node))
                    {
                        Node check = c.FirstNodeNotNull;
                        if (!HasPair(node, check))
                        {
                            c.Fill(node);
                        }
                        AddValidConnections(node, check, out _);
                    }
                }
                ClearHeld();
            }
            public void HoldAllConnectionsFrom(Node node)
            {
                List<Connection> connections = GetConnectionsFrom(node);
                foreach (Connection c in connections)
                {
                    RemoveConnection(c);
                    c.Hold(node);
                    Held.Add(c);
                }
            }
            public List<Connection> GetConnectionsFrom(Node node)
            {
                List<Connection> output = new();
                foreach (Connection c in Connections)
                {
                    if (c.Contains(node))
                    {
                        output.Add(c);
                    }
                }
                return output;
            }
            public bool HasPair(Node a, Node b)
            {
                foreach (Connection c in Connections)
                {
                    if (c.Match(a, b))
                    {
                        return true;
                    }
                }
                return false;
            }
            public bool HasPair(NodeTypes a, NodeTypes b)
            {
                return HasPair(Nodes[(int)a], Nodes[(int)b]);
            }
            public void RemoveAll()
            {
                List<Connection> toRemove = [.. Connections];
                foreach (Connection c in toRemove)
                {
                    RemoveConnection(c);
                }
            }
            public void ClearHeld()
            {
                Held.Clear();
            }
            public void Update()
            {

                foreach (Connection c in Connections)
                {
                    foreach (Node node in c.Nodes)
                    {
                        if (node != null)
                        {
                            NodeConnects[node.Index] += 1;
                        }
                    }
                }
                if (Lined)
                {
                    foreach (Node node in Nodes)
                    {
                        if (NodeConnects[node.Index] <= 0)
                        {
                            node.TurnOff();
                        }
                        else
                        {
                            node.TurnOn();
                        }

                    }
                }
                NodeConnects = NodeConnects.Select(item => item = 0).ToList();
            }
            public void Render(Vector2 mouse)
            {
                foreach (Connection c in Connections)
                {
                    c.RenderNodeLine();
                }
                if (LeftHeld != null)
                {
                    LeftHeld.RenderMouseLine(mouse);
                }
                else
                {
                    foreach (Connection c in Held)
                    {
                        c.RenderMouseLine(mouse);
                    }
                }
            }
        }
        public class Button
        {
            public bool DrawBounds;
            public bool Disabled;
            public Vector2 Offset;
            public bool ClickedFirstFrame;
            public bool Colliding;
            public bool CursorOver;
            public MTexture IdleTex => GFX.Game[Path + TexturePath];
            public MTexture PressedTex => GFX.Game[Path + TexturePath + "Pressed"];
            public MTexture Texture => ClickedFirstFrame && Colliding ? PressedTex : IdleTex;
            public string TexturePath;
            public Action OnClicked;
            public char ID;
            public Button(Vector2 offset, string path, Action onClicked, char id)
            {
                ID = id;
                Offset = offset;
                TexturePath = path;
                OnClicked = onClicked;
            }
            public void Render()
            {
                if (DrawBounds)
                {
                    Draw.Rect(Offset - Vector2.One * 8, Texture.Width + 16, Texture.Height + 16, Color.Magenta);
                }
                Texture.Draw(Offset);
            }
            public bool Check(Vector2 pos)
            {
                if (Disabled)
                {
                    return false;
                }
                Vector2 check = Offset;
                float width = Texture.Width;
                float height = Texture.Height;
                if (pos.X > check.X && pos.Y > check.Y && pos.X < check.X + width)
                {
                    return pos.Y < check.Y + height;
                }
                return false;
            }
        }
        public VirtualRenderTarget Buffer, Buffer2;
        public List<Node> Nodes = new();
        public List<Button> Buttons = [];
        public ConnectionList Connections;
        public Button Submit;
        public Button Home;
        public Button Exit;
        public MouseComponent Mouse;
        public Inventory Inventory;
        public float Alpha = 1;
        public bool Standby
        {
            get => standby;
            set
            {
                standby = value;
                foreach(Button b in Buttons)
                {
                    b.Disabled = standby;
                }
                if (Mouse != null) Mouse.Active = !standby;

            }
        }
        private bool standby;
        public bool Finished;
        public WarpCapsule Parent;
        public AreaKey AreaKey => SceneAs<Level>().Session.Area;

        public UI(WarpCapsule parent) : base()
        {
            Parent = parent;
            Tag |= TagsExt.SubHUD;
            Buffer = VirtualContent.CreateRenderTarget("RuneUIBuffer", 1920, 1080);
            Buffer2 = VirtualContent.CreateRenderTarget("RuneUIBuffer", 1920, 1080);
            Add(new BeforeRenderHook(BeforeRender));
            Add(Mouse = new MouseComponent(OnLeftClick, OnRightClick, OnLeftRelease, OnRightRelease, OnLeftIdle, OnRightIdle, OnLeftHeld, OnRightHeld));
            Mouse.MethodsEnabled = false;

            Submit = AddButton(new Vector2(50, 1080 - 209), "button", CheckRune, 's');
            Home = AddButton(new Vector2(50, 209), "homeButton", GoHome, 'h');
            Exit = AddButton(new Vector2(50, 418), "exitButton", () => FadeOut(), 'e');

        }
        public Button AddButton(Vector2 position, string path, Action action, char id)
        {
            Button button = new Button(position, path, action, id);
            Buttons.Add(button);
            return button;
        }
        public void FadeIn()
        {
            Standby = true;
            Alpha = 0;
            Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineOut, t => Alpha = t.Eased, t => { Standby = false; Alpha = 1; });
        }
        public void FadeOut(bool removeSelf = false)
        {
            Standby = true;
            Alpha = 1;
            Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineOut, t => Alpha = 1 - t.Eased, t => { Standby = false; Alpha = 0; Finished = true; if (removeSelf) RemoveSelf(); });
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            Vector2 center = new Vector2(1920, 1080) / 2;
            float spacing = 300;
            for (int i = 0; i < 3; i++)
            {
                Node n = new Node(center + new Vector2(-spacing + spacing * i, -spacing), (NodeTypes)i);
                Add(n);
                Nodes.Add(n);
            }
            for (int i = 0; i < 4; i++)
            {
                Node n = new Node(center + (Vector2.UnitX * (spacing * -1.5f + (i * spacing))), (NodeTypes)(3 + i));
                Add(n);
                Nodes.Add(n);
            }
            for (int i = 0; i < 3; i++)
            {
                Node n = new Node(center + new Vector2(-spacing + spacing * i, spacing), (NodeTypes)(7 + i));
                Add(n);
                Nodes.Add(n);
            }
            Connections = new(Nodes);
            Home.Offset += Home.Texture.HalfSize();
            Exit.Offset += Exit.Texture.HalfSize();
            Inventory = new(Connections, 60);
            FadeIn();
        }
        public float MouseIconAlpha = 0;
        private float padBuffer;
        public override void Update()
        {
            base.Update();
            if (!Standby)
            {
                Connections.Update();
                if (ControlMode == Control.None)
                {
                    Mouse.MethodsEnabled = false;
                    if (Mouse.PrevMousePosition != Mouse.MousePosition)
                    {
                        ControlMode = Control.Mouse;
                    }
                    else if (Input.MoveX.Value != 0 || Input.MoveY.Value != 0)
                    {
                        ControlMode = Control.Pad;
                    }
                }
                switch (ControlMode)
                {
                    case Control.Pad:

                        Mouse.MethodsEnabled = false;
                        if (padBuffer <= 0)
                        {
                            if (Input.MoveX.Value != 0)
                            {
                                padBuffer = 0.2f;
                                int mapX = MapX.Wrap(0, 4, Input.MoveX.Value);
                                int mapY = MapY;
                                int origX = mapX;
                                int origY = mapY;
                                while (!map[mapY, mapX].HasValue)
                                {
                                    mapX = mapX.Wrap(0, 4, Input.MoveX.Value);
                                    if (mapX == origX)
                                    {
                                        mapY = mapY.Wrap(0, 2, 1);
                                        if (mapY == origY)
                                        {
                                            break;
                                        }
                                    }
                                }
                                MapX = mapX;
                                MapY = mapY;
                            }
                            else if (Input.MoveY.Value != 0)
                            {
                                padBuffer = 0.2f;
                                int mapX = MapX;
                                int mapY = MapY.Wrap(0, 2, Input.MoveY.Value);
                                int origX = mapX;
                                int origY = mapY;
                                while (!map[mapY, mapX].HasValue)
                                {
                                    mapY = mapY.Wrap(0, 2, Input.MoveY.Value);
                                    if (mapY == origY)
                                    {
                                        mapX = mapX.Wrap(0, 4, Input.MoveX.Value);
                                        if (mapX == origX)
                                        {
                                            break;
                                        }
                                    }
                                }
                                MapX = mapX;
                                MapY = mapY;
                            }
                        }
                        int mX = MapX;
                        int mY = MapY;
                        char? selected = map[MapY, MapX];

                        foreach(Button b in Buttons)
                        {
                            b.CursorOver = b.DrawBounds = selected == b.ID;
                        }
                        int? index = selected.HasValue && int.TryParse(selected.Value.ToString(), out int result) ? result : null;
                        for (int i = 0; i < Nodes.Count; i++)
                        {
                            Nodes[i].DrawBounds = index.HasValue && index.Value == i;
                        }
                        if (Input.Dash.Check)
                        {
                            if (Input.DashPressed)
                            {
                                OnLeftClick();
                            }
                            else
                            {
                                OnLeftHeld();
                            }
                        }
                        else if (Input.Dash.Released)
                        {
                            OnLeftRelease();
                        }
                        else
                        {
                            OnLeftIdle();
                        }
                        if (Mouse.PrevMousePosition != Mouse.MousePosition)
                        {
                            ControlMode = Control.Mouse;
                        }
                        break;
                    case Control.Mouse:
                        Mouse.MethodsEnabled = true;
                        Vector2 pos = Mouse.MousePosition;
                        foreach(Button b in Buttons)
                        {
                            b.CursorOver = b.Check(pos);
                            b.DrawBounds = false;
                        }
                        for (int i = 0; i < Nodes.Count; i++)
                        {
                            Nodes[i].DrawBounds = false;
                        }
                        if (Input.MoveX.Value != 0 || Input.MoveY.Value != 0 || Input.Jump.Pressed || Input.DashPressed)
                        {
                            ControlMode = Control.Pad;
                        }
                        break;
                }
                MouseIconAlpha = Calc.Approach(MouseIconAlpha, ControlMode == Control.Mouse ? 1f : 0f, Engine.DeltaTime);
                padBuffer = Math.Max(0, padBuffer - Engine.DeltaTime);

                Inventory.Update(Mouse);
            }
        }
        public void OnLeftClick()
        {
            foreach(Button b in Buttons)
            {
                b.ClickedFirstFrame = b.CursorOver;
            }
            Connections.OnLeftClick(GetFirstCollided());
        }
        public void OnLeftRelease()
        {
            foreach(Button b in Buttons)
            {
                if(b.ClickedFirstFrame && b.CursorOver)
                {
                    b.OnClicked.Invoke();
                    break;
                }
            }
            foreach(Button b in Buttons)
            {
                b.ClickedFirstFrame = false;
            }
/*            Home.ClickedFirstFrame = false;
            Submit.ClickedFirstFrame = false;
            Exit.ClickedFirstFrame = false;*/
            Connections.OnLeftRelease(GetFirstCollided());
        }
        public void GoHome()
        {
            FadeOut(true);
            SetRune(WarpRune.Default);
        }
        public void OnLeftIdle()
        {
            ResetButton();
        }
        public void OnLeftHeld()
        {
            foreach(Button b in Buttons)
            {
                b.Colliding = b.ClickedFirstFrame && b.CursorOver;
            }
            Connections.OnLeftHeld(Mouse.MousePosition, GetFirstCollided());
        }
        public Node GetFirstCollided()
        {
            if (ControlMode == Control.Pad)
            {
                foreach (Node n in Nodes)
                {
                    if (n.DrawBounds)
                    {
                        return n;
                    }
                }
                return null;
            }
            else
            {
                Vector2 m = Mouse.MousePosition;
                foreach (Node n in Nodes)
                {
                    if (n.Check(m))
                    {
                        return n;
                    }
                }
                return null;
            }
        }
        public void OnRightClick()
        {
            Connections.OnRightClick(GetFirstCollided());

        }
        public void OnRightRelease()
        {
            Connections.OnRightRelease(GetFirstCollided());
        }
        public void OnRightHeld()
        {

        }
        public void OnRightIdle()
        {

        }
        public void ResetButton()
        {
            foreach(Button b in Buttons)
            {
                b.Colliding = false;
                b.ClickedFirstFrame = false;
            }
        }
        public WarpRune CreateRune()
        {
            string input = "";
            foreach (Connection c in Connections.Connections)
            {
                if (c.TryGetTuple(out var tuple))
                {
                    input += tuple.Item1;
                    input += tuple.Item2;
                    input += " ";
                }
            }

            return new WarpRune("", input);
        }
        private IEnumerator lockedRoutine()
        {
            Standby = true;
            yield return Textbox.Say("WarpRestricted");
            Standby = false;
        }
        private IEnumerator noRuneFoundRoutine()
        {
            Standby = true;
            foreach (Node n in Nodes)
            {
                n.IncorrectFlash(0.4f);
            }
            yield return 0.4f;
            foreach (Node n in Nodes)
            {
                n.IncorrectFlash(0.4f);
            }
            yield return 0.4f;
            foreach (Node n in Nodes)
            {
                n.IncorrectFlash(0.4f);
            }
            yield return 0.4f;
            Standby = false;
        }
        public void CheckRune()
        {
            ResetButton();
            WarpRune rune = CreateRune();
            if (RuneExists(rune, out WarpData data))
            {
                SetRune(data.Rune);
            }
            /*            else if (Connections.Connections.Count == 0)
                        {
                            SetRune(WarpRune.Default);
                        }*/
            else
            {
                foreach (Node n in Nodes)
                {
                    n.TurnOff();
                }
                Add(new Coroutine(noRuneFoundRoutine()));
            }
            Connections.LeftHeld = null;
            Connections.RemoveAll();
        }
        public void SetRune(WarpRune rune)
        {
            if (rune == null || (Parent.OwnWarpData != null && Parent.OwnWarpData.HasRune && Parent.OwnWarpData.Rune.Match(rune)))
            {
                return;
            }
            ObtainedRunes.Add(rune);
            Connections.RemoveAll();
            FadeOut(true);
            Parent.TargetWarpRune = rune;
        }
        public void DrawLine(Node a, Node b, Color color, int thickness)
        {
            Draw.Line(a.Center, b.Center, color, thickness);
        }
        public MTexture Cursor => GFX.Game["objects/PuzzleIslandHelper/runeUI/cursor"];

        public void BeforeRender()
        {
            if (Alpha > 0)
            {
                Vector2 p = Mouse.MousePosition;
                Buffer2.SetAsTarget(true);
                if (Lined || PianoModule.Session.DEBUGBOOL4)
                {
                    Draw.SpriteBatch.StandardBegin(Matrix.Identity, BlendState.AlphaBlend, null);
                    Connections.Render(p);
                    Draw.SpriteBatch.End();
                }

                Buffer.SetAsTarget(Color.Black);
                Draw.SpriteBatch.StandardBegin(Matrix.Identity);
                foreach (Node node in Nodes)
                {
                    node.DrawTexture();
                }
                foreach(Button b in Buttons)
                {
                    b.Render();
                }
                Draw.SpriteBatch.Draw(Buffer2, Vector2.Zero, Color.White);
                Inventory.Render();
                Cursor.DrawCentered(p, Color.White * MouseIconAlpha);

                Draw.SpriteBatch.End();
            }
        }
        public override void Render()
        {
            base.Render();
            if (Alpha > 0)
            {
                Draw.SpriteBatch.Draw(Buffer, Vector2.Zero, Color.White * Alpha);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Buffer?.Dispose();
            Buffer = null;
            Buffer2?.Dispose();
            Buffer2 = null;
        }
    }
}
