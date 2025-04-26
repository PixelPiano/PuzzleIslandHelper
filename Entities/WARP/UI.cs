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
        public const string Path = "objects/PuzzleIslandHelper/runeUI/";
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
            public Node FirstValid()
            {
                return Nodes.First(item => item != null);
            }
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
            public const int LineThickness = 50;
            public void DrawLine(Vector2 from, Vector2 to, Color color)
            {
                Draw.Line(from, to, color, LineThickness);
            }
            public void RenderNodeLine()
            {
                DrawLine(Nodes[0].Head, Nodes[1].Head, Color.White);
            }
            public void RenderNodeLine(Vector2 position, Vector2 scale, int thickness)
            {
                Draw.Line(position + Nodes[0].Head * scale, position + Nodes[1].Head * scale, Color.White, thickness);
            }
            public void RenderMouseLine(Vector2 mouse)
            {
                for (int i = 0; i < Nodes.Length; i++)
                {
                    if (Nodes[i] != null)
                    {
                        DrawLine(Nodes[i].Head, mouse, Color.White);
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
            public Connection LeftHeld;
            public List<Node> Nodes = new();
            public ConnectionList(List<Node> validNodes)
            {
                RegisterNodes(validNodes);
            }
            public void TransferFromSlot(Inventory.RuneInventorySlot slot)
            {
                ClearHeld();
                ClearMain();
                List<(Node, Node)> list = [];

                foreach (var rf in WarpRune.ToFragments(slot.Rune))
                {
                    int item1 = (int)rf.NodeA;
                    int item2 = (int)rf.NodeB;
                    list.Add((Nodes[item1], Nodes[item2]));
                }
                foreach (var pair in list)
                {
                    AddValidConnections(pair.Item1, pair.Item2, true);
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
            public void OnLeftClick(Node node)
            {
                if (Held.Count == 0 && node != null && LeftHeld == null)
                {
                    LeftHeld = new Connection(node, null);
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
            public void AddValidConnections(Node from, Node to, bool naive = false)
            {
                void add(Node f, Node t, bool naive)
                {
                    if (naive)
                    {
                        if (!HasPair(f, t))
                        {
                            Connection c = AddConnection(f, t);
                            c.Naive = true;
                        }
                    }
                    else
                    {
                        TryAddConnection(f, t);
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
                            add(fromNode, connect, naive);
                            start = node;
                        }
                    }
                    else add(from, to, naive);
                }

            }

            public void OnLeftRelease(Node node)
            {
                if (node != null && LeftHeld != null && LeftHeld.FirstValid() is Node node2)
                {
                    AddValidConnections(node, node2);
                }
                LeftHeld = null;
            }

            public void OnLeftHeld(Vector2 mouse, Node node)
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
            public void OnRightClick(Node node)
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
            public void OnRightRelease(Node node)
            {
                if (node != null && Held.Count > 0)
                {
                    TransferHeldTo(node);
                }
                ClearHeld();
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
                        Node check = c.FirstValid();
                        if (!HasPair(node, check))
                        {
                            c.Fill(node);
                        }
                        AddValidConnections(node, check);
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
            public void ClearMain()
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
        public abstract class Button
        {
            public Vector2 Offset;
            public bool ClickedFirstFrame;
            public bool Colliding;
            public MTexture IdleTex => GFX.Game[Path + TexturePath];
            public MTexture PressedTex => GFX.Game[Path + TexturePath + "Pressed"];
            public MTexture Texture => ClickedFirstFrame && Colliding ? PressedTex : IdleTex;
            public string TexturePath;
            public Button(Vector2 offset, string path)
            {
                Offset = offset;
                TexturePath = path;
            }
            public void Render()
            {
                Texture.Draw(Offset);
            }
            public bool Check(Vector2 pos)
            {
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
        public class SubmitButton(Vector2 offset) : Button(offset, "button") { }
        public class HomeButton(Vector2 offset) : Button(offset, "homeButton") { }
        public VirtualRenderTarget Buffer, Buffer2;
        public List<Node> Nodes = new();
        public ConnectionList Connections;
        public SubmitButton Submit;
        public HomeButton Home;
        public MouseComponent Mouse;
        public Inventory Inventory;
        public float Alpha = 1;
        public bool Standby;
        public bool Finished;
        public bool CollidingWithSubmit;
        public bool CollidingWithHome;
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
            FadeIn();
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
                Nodes.Add(new Node(center + new Vector2(-spacing + spacing * i, -spacing), (NodeTypes)i));
            }
            for (int i = 0; i < 4; i++)
            {
                Nodes.Add(new Node(center + (Vector2.UnitX * (spacing * -1.5f + (i * spacing))), (NodeTypes)(3 + i)));
            }
            for (int i = 0; i < 3; i++)
            {
                Nodes.Add(new Node(center + new Vector2(-spacing + spacing * i, spacing), (NodeTypes)(7 + i)));
            }
            Connections = new(Nodes);
            Submit = new SubmitButton(new Vector2(50, 1080 - 209));
            Home = new HomeButton(new Vector2(50, 209));
            Home.Offset += Home.Texture.HalfSize();
            Inventory = new(Connections, 60);
        }
        public override void Update()
        {
            base.Update();
            if (!Standby)
            {
                if (Input.DashPressed)
                {
                    FadeOut(true);
                }
                Vector2 pos = Mouse.MousePosition;
                Connections.Update();
                CollidingWithSubmit = Submit.Check(pos);
                CollidingWithHome = Home.Check(pos);
                Inventory.Update(Mouse);
            }
        }
        public void OnLeftClick()
        {
            Submit.ClickedFirstFrame = CollidingWithSubmit;
            Home.ClickedFirstFrame = CollidingWithHome;
            Connections.OnLeftClick(GetFirstCollided());
        }
        public void OnLeftRelease()
        {
            if (Submit.ClickedFirstFrame && CollidingWithSubmit)
            {
                CheckRune();
            }
            else if (Home.ClickedFirstFrame && CollidingWithHome)
            {
                GoHome();
            }
            Home.ClickedFirstFrame = false;
            Submit.ClickedFirstFrame = false;
            Connections.OnLeftRelease(GetFirstCollided());
        }
        public void GoHome()
        {
            SetRune(WarpRune.Default);
        }
        public void OnLeftIdle()
        {
            ResetButton();
        }
        public void OnLeftHeld()
        {
            Submit.Colliding = Submit.ClickedFirstFrame && CollidingWithSubmit;
            Home.Colliding = Home.ClickedFirstFrame && CollidingWithHome;
            Connections.OnLeftHeld(Mouse.MousePosition, GetFirstCollided());
        }
        public Node GetFirstCollided()
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
            Submit.Colliding = false;
            Submit.ClickedFirstFrame = false;
            Home.Colliding = false;
            Home.ClickedFirstFrame = false;
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
        public void CheckRune()
        {
            ResetButton();
            WarpRune rune = CreateRune();
            if (WARPData.RuneExists(rune, out AlphaWarpData data))
            {
                if (PianoModule.SaveData.WarpLockedToLab && !data.Lab)
                {
                    Add(new Coroutine(lockedRoutine()));
                    return;
                }
                SetRune(data.Rune);
            }
        }
        public void SetRune(WarpRune rune)
        {
            WARPData.ObtainedRunes.TryAdd(rune);
            ClearAll();
            FadeOut(true);
            Parent.WarpRune = rune;
        }
        public void ClearAll()
        {
            Connections.ClearMain();
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
                Draw.SpriteBatch.StandardBegin(Matrix.Identity, BlendState.AlphaBlend, null);
                Connections.Render(p);
                Draw.SpriteBatch.End();

                Buffer.SetAsTarget(Color.Black);
                Draw.SpriteBatch.StandardBegin(Matrix.Identity);
                foreach (Node node in Nodes)
                {
                    node.DrawTexture();
                }
                Submit.Render();
                Home.Render();
                Draw.SpriteBatch.Draw(Buffer2, Vector2.Zero, Color.White);
                Inventory.Render();
                Cursor.DrawCentered(p);

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
