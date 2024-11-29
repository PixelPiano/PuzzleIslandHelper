using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.WIP.PolygonDrawing;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DigiWarpReceiver")]
    [Tracked]
    public class WarpCapsule : Entity
    {
        public class RuneSegment
        {
            public Tuple<int, int> Pair;
            public RuneSegment(int a, int b) : this(new(a, b)) { }
            public RuneSegment(Tuple<int, int> pair)
            {
                Pair = pair;
            }
            public bool Match(int a, int b)
            {
                return a != b && (Pair.Item1 == a || Pair.Item1 == b) && (Pair.Item2 == a || Pair.Item2 == b);
            }
            public bool Match(Tuple<int, int> pair)
            {
                return Match(pair.Item1, pair.Item2);
            }
        }
        public class Rune
        {
            public List<Tuple<int, int>> Segments = new();
            public string ID;
            public Rune(string id, params int[] pattern)
            {
                ID = id;
                for (int i = 1; i < pattern.Length; i += 2)
                {
                    int a = pattern[i - 1];
                    int b = pattern[i];
                    Segments.Add(new(a, b));
                }
            }
            public bool Match(Rune rune)
            {
                return Match(rune.Segments);
            }
            public bool Match(List<Tuple<int, int>> pairs)
            {
                foreach (Tuple<int, int> pair in pairs)
                {
                    if (!ContainsSegment(pair))
                    {
                        return false;
                    }
                }
                return true;
            }
            public bool ContainsSegment(Tuple<int, int> segment)
            {
                foreach (Tuple<int, int> pair in Segments)
                {
                    if ((segment.Item1 == pair.Item1 || segment.Item1 == pair.Item2) &&
                        (segment.Item2 == pair.Item1 || segment.Item2 == pair.Item2))
                    {
                        return true;
                    }
                }
                return false;
            }
            public bool MatchSegment(Tuple<int, int> input, Tuple<int, int> compare)
            {
                if (input.Item1 == compare.Item1 || input.Item1 == compare.Item2)
                {
                    return input.Item2 == compare.Item1 || input.Item2 == compare.Item2;
                }
                return false;
            }
        }

        public class RuneUI : Entity
        {
            public const string Path = "objects/PuzzleIslandHelper/runeUI/";
            public class SubmitButton
            {
                public Vector2 Offset;
                public bool ClickedFirstFrame;
                public bool Colliding;
                public MTexture IdleTex => GFX.Game[Path + "button"];
                public MTexture PressedTex => GFX.Game[Path + "buttonPressed"];

                public MTexture Texture => ClickedFirstFrame && Colliding ? PressedTex : IdleTex;
                public SubmitButton(Vector2 offset)
                {
                    Offset = offset;
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
            public class Connection
            {
                public RuneNode NodeA;
                public RuneNode NodeB;
                public bool Partial => (NodeA != null && NodeB == null) || (NodeA == null && NodeB != null);
                public bool Full => NodeA != null && NodeB != null;
                public Connection(RuneNode a, RuneNode b)
                {
                    NodeA = a;
                    NodeB = b;
                }
                public RuneNode GetPartner(RuneNode exclude)
                {
                    return NodeA == exclude ? NodeB : NodeA;
                }
                public RuneNode FirstValid()
                {
                    return NodeA != null ? NodeA : NodeB;
                }
                public bool Contains(RuneNode node)
                {
                    return NodeA == node || NodeB == node;
                }
                public bool Contains(RuneNode a, RuneNode b)
                {
                    return a != b && Contains(a) && Contains(b);
                }
                public void Fill(RuneNode fill)
                {
                    if (NodeA == null)
                    {
                        NodeA = fill;
                        NodeA.TurnOn();
                    }
                    else if (NodeB == null)
                    {
                        NodeB = fill;
                        NodeB.TurnOn();
                    }
                }
                public void Hold(RuneNode clicked)
                {
                    if (NodeA == clicked)
                    {
                        NodeA.TurnOff();
                        NodeA = null;
                    }
                    else if (NodeB == clicked)
                    {
                        NodeB.TurnOff();
                        NodeB = null;
                    }
                }
                public void RenderNodeLine()
                {
                    Draw.Line(NodeA.Head, NodeB.Head, Color.Red, 10);
                }
                public void RenderMouseLine(Vector2 mouse)
                {
                    if (NodeA != null)
                    {
                        Draw.Line(NodeA.Head, mouse, Color.Yellow, 10);
                    }
                    else if (NodeB != null)
                    {
                        Draw.Line(NodeB.Head, mouse, Color.Yellow, 10);
                    }
                }
            }
            public class ConnectionList
            {
                public List<Connection> Connections = new();
                public List<Connection> Held = new();
                public Dictionary<RuneNode, int> ConnectCount = new();
                public Connection LeftHeld;
                public List<RuneNode> Nodes = new();
                public ConnectionList(List<RuneNode> validNodes)
                {
                    RegisterNodes(validNodes);
                }
                public void OnLeftClick(RuneNode node)
                {
                    if (Held.Count > 0 && node != null)
                    {
                        TransferHeldTo(node);
                        return;
                    }
                    if (node == null)
                    {
                        LeftHeld = null;
                        return;
                    }
                    if (LeftHeld != null)
                    {
                        if (LeftHeld.FirstValid() is RuneNode node2)
                        {
                            if (node != node2 && !HasPair(node, node2))
                            {
                                Connections.Add(new Connection(node, node2));
                                node.TurnOn();
                                node2.TurnOn();
                            }
                            LeftHeld = null;
                        }
                    }
                    else
                    {
                        LeftHeld = new Connection(node, null);
                    }
                }
                public void RegisterNodes(List<RuneNode> nodes)
                {
                    foreach (RuneNode node in nodes)
                    {
                        ConnectCount.Add(node, 0);
                    }
                    Nodes = nodes;
                }
                public void AddConnection(Connection c)
                {
                    Connections.Add(c);
                    if (c.NodeA != null)
                    {
                        ConnectCount[c.NodeA]++;
                    }
                    if (c.NodeB != null)
                    {
                        ConnectCount[c.NodeB]++;
                    }
                }
                public void RemoveConnection(Connection c)
                {
                    Connections.Remove(c);
                    if (c.NodeA != null)
                    {
                        ConnectCount[c.NodeA] = (int)Calc.Max(0, ConnectCount[c.NodeA] - 1);
                    }
                    if (c.NodeB != null)
                    {
                        ConnectCount[c.NodeB] = (int)Calc.Max(0, ConnectCount[c.NodeB] - 1);
                    }
                }
                public void OnRightClick(RuneNode node)
                {
                    if (LeftHeld != null)
                    {
                        LeftHeld = null;
                        return;
                    }
                    if (node == null)
                    {
                        ClearHeld();
                        return;
                    }
                    if (Held.Count > 0)
                    {
                        TransferHeldTo(node);
                    }
                    else
                    {
                        HoldAllConnectionsFrom(node);
                    }
                }
                public void TransferHeldTo(RuneNode node)
                {
                    foreach (Connection c in Held)
                    {
                        if (!c.Contains(node))
                        {
                            RuneNode check = c.FirstValid();
                            if (!HasPair(node, check))
                            {
                                c.Fill(node);
                                AddConnection(c);
                            }
                        }
                    }
                    ClearHeld();
                }
                public void HoldAllConnectionsFrom(RuneNode node)
                {
                    List<Connection> connections = GetConnectionsFrom(node);
                    foreach (Connection c in connections)
                    {
                        RemoveConnection(c);
                        c.Hold(node);
                        Held.Add(c);
                    }
                }
                public List<Connection> GetConnectionsFrom(RuneNode node)
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
                public bool HasPair(RuneNode a, RuneNode b)
                {
                    foreach (Connection c in Connections)
                    {
                        if (c.Contains(a, b))
                        {
                            return true;
                        }
                    }
                    return false;
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
                    foreach (RuneNode node in Nodes)
                    {
                        if (ConnectCount[node] <= 0)
                        {
                            node.TurnOff();
                        }
                        else
                        {
                            node.TurnOn();
                        }
                    }
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
            public class RuneNode
            {
                public Vector2 Center;
                public Vector2 Head => Center - Vector2.UnitY * 77;
                public Vector2 RenderPosition => Center - new Vector2(Width, Height) / 2f;
                public float Width => HeadTex.Width;
                public float Height => HeadTex.Height;
                public bool Lit;
                public int Index;
                public MTexture HeadTex => GFX.Game[Path + "nodeHead"];
                public MTexture BodyTex => GFX.Game[Path + "nodeBody"];
                public RuneNode(Vector2 center, float width, float height, int index)
                {
                    Center = center * 6f;
                    Index = index;
                }
                public void DrawTexture()
                {
                    Vector2 pos = RenderPosition;
                    BodyTex.DrawOutline(pos);
                    HeadTex.DrawOutline(pos);
                    BodyTex.Draw(pos);
                    HeadTex.Draw(pos, Vector2.Zero, Lit ? Color.Orange : Color.Red);
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
                    Vector2 position = RenderPosition;
                    if (pos.X > position.X && pos.Y > position.Y && pos.X < position.X + Width)
                    {
                        return pos.Y < position.Y + Height;
                    }
                    return false;
                }
                public bool CanConnectTo(RuneNode node)
                {
                    return node != this;
                }
            }
            public VirtualRenderTarget Buffer;
            public List<RuneNode> Nodes = new();
            public ConnectionList Connections;
            public SubmitButton Button;
            public MouseComponent Mouse;
            public float Alpha = 1;
            public bool Standby;
            public bool Finished;
            public bool CollidingWithButton;
            public RuneUI() : base()
            {
                Tag |= TagsExt.SubHUD;
                Buffer = VirtualContent.CreateRenderTarget("RuneUIBuffer", 1920, 1080);
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
                Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineOut, t => Alpha = t.Eased, t => { Standby = false; Alpha = 0; Finished = true; if (removeSelf) RemoveSelf(); });
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Vector2 topLeft = new Vector2(100, 50);
                float spacing = 50;

                Nodes.Add(new RuneNode(topLeft + Vector2.UnitX * spacing / 2, 30, 30, 0));
                Nodes.Add(new RuneNode(topLeft + Vector2.UnitX * spacing * 1.5f, 30, 30, 1));

                Nodes.Add(new RuneNode(topLeft + new Vector2(0, spacing), 30, 30, 2));
                Nodes.Add(new RuneNode(topLeft + new Vector2(spacing, spacing), 30, 30, 3));
                Nodes.Add(new RuneNode(topLeft + new Vector2(spacing * 2, spacing), 30, 30, 4));

                Nodes.Add(new RuneNode(topLeft + new Vector2(spacing / 2, spacing * 2), 30, 30, 5));
                Nodes.Add(new RuneNode(topLeft + new Vector2(spacing * 1.5f, spacing * 2), 30, 30, 6));
                Connections = new(Nodes);
                Button = new SubmitButton(new Vector2(1920, 1080) - new Vector2(357, 209));
            }
            public override void Update()
            {
                base.Update();
                if (Standby) return;
                CollidingWithButton = Button.Check(Mouse.MousePosition);
            }
            public void OnLeftClick()
            {
                Button.ClickedFirstFrame = CollidingWithButton;
                RuneNode clicked = null;
                foreach (RuneNode node in Nodes)
                {
                    if (node.Check(Mouse.MousePosition))
                    {
                        clicked = node;
                        break;
                    }
                }
                Connections.OnLeftClick(clicked);
            }
            public void OnLeftRelease()
            {
                if (Button.ClickedFirstFrame && CollidingWithButton)
                {
                    CheckRune();
                }
                Button.ClickedFirstFrame = false;
            }
            public void OnLeftIdle()
            {
                ResetButton();
            }
            public void OnLeftHeld()
            {
                Button.Colliding = Button.ClickedFirstFrame && CollidingWithButton;
            }
            public void OnRightClick()
            {
                Vector2 m = Mouse.MousePosition;
                RuneNode clicked = null;
                foreach (RuneNode n in Nodes)
                {
                    if (n.Check(m))
                    {
                        clicked = n;
                        break;
                    }
                }
                Connections.OnRightClick(clicked);
            }
            public void OnRightRelease()
            {

            }
            public void OnRightHeld()
            {

            }
            public void OnRightIdle()
            {

            }
            public void ResetButton()
            {
                Button.Colliding = false;
                Button.ClickedFirstFrame = false;
            }
            public void CheckRune()
            {
                Button.ClickedFirstFrame = false;
                Button.Colliding = false;
                Add(new Coroutine(CheckRuneRoutine()));
            }
            public IEnumerator CheckRuneRoutine()
            {
                ClearAll();
                FadeOut(true);
                yield return null;
            }
            public void ClearAll()
            {
                /*                foreach (RuneNode node in Nodes)
                                {
                                    node.CancelAllConnections();
                                }*/

            }
            public void DrawLine(RuneNode a, RuneNode b, Color color, int thickness)
            {
                Draw.Line(a.Center, b.Center, color, thickness);
            }
            public void BeforeRender()
            {
                Buffer.SetAsTarget(Color.Black);
                Draw.SpriteBatch.StandardBegin(Matrix.Identity);


                foreach (RuneNode node in Nodes)
                {
                    node.DrawTexture();
                }
                Button.Render();
                Connections.Render(Mouse.MousePosition);

                foreach (RuneNode node in Nodes)
                {
                    Draw.HollowRect(node.RenderPosition, node.Width, node.Height, node.Lit ? Color.Yellow : Color.Magenta);
                }
                Draw.HollowRect(Button.Offset, Button.Texture.Width, Button.Texture.Height, Color.Cyan);
                Draw.HollowRect(Mouse.MousePosition, 30, 30, Color.Red);

                ActiveFont.Draw("MousePos: " + Mouse.MousePosition, Vector2.One * 10, Color.White);
                ActiveFont.Draw("LeftClicking: " + Mouse.LeftClicked, Vector2.One * 10 + Vector2.UnitY * 80, Color.White);
                ActiveFont.Draw("RightClicking: " + Mouse.RightClicked, Vector2.One * 10 + Vector2.UnitY * 120, Color.White);
                ActiveFont.Draw("JustLeftClicked: " + (Mouse.JustLeftClicked), Vector2.One * 10 + Vector2.UnitY * 160, Color.White);
                ActiveFont.Draw("JustRightClicked: " + (Mouse.JustRightClicked), Vector2.One * 10 + Vector2.UnitY * 200, Color.White);
                string text = "Not colliding with node.";
                RuneNode colliding = null;
                foreach (RuneNode node in Nodes)
                {
                    if (node.Check(Mouse.MousePosition))
                    {
                        colliding = node;
                        break;
                    }
                }
                if (colliding != null)
                {
                    text = "Colliding with node " + colliding.Index;
                    text += "\n\tConnections:";
                    bool hasConnection = false;
                    foreach (Connection c in Connections.Connections)
                    {
                        if (c.Contains(colliding) && c.GetPartner(colliding) is RuneNode node)
                        {
                            text += "\n\t\t" + node.Index;
                            hasConnection = true;
                        }
                    }
                    if (!hasConnection)
                    {
                        text += "\n\t\t None.";
                    }

                }
                ActiveFont.Draw(text, Vector2.One * 10 + Vector2.UnitY * 240, Color.White);

                Draw.SpriteBatch.End();
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
            }
        }
        [Tracked]
        public class Machine : Entity
        {
            public static MTexture Texture => GFX.Game[Path + "terminal"];
            public Image Image;
            public WarpCapsule Parent;
            public DotX3 Talk;
            public bool Blocked;
            public void Block()
            {
                Blocked = true;
            }
            public void Unblock()
            {
                Alarm.Set(this, 0.3f, delegate { Blocked = false; });
            }
            public Machine(WarpCapsule parent, Vector2 position) : base(position)
            {
                Depth = 10;
                Parent = parent;
                Collider = new Hitbox(Texture.Width, Texture.Height);
                Add(Image = new Image(Texture, true));
                Talk = new DotX3(Collider, Interact);
                Add(Talk);
                Add(new BathroomStallComponent(null, Block, Unblock));
            }
            public override void Update()
            {
                base.Update();
                Talk.Enabled = Parent.Accessible && !Blocked;
            }
            public override void Render()
            {
                Image.DrawSimpleOutline();
                base.Render();
            }
            public void Interact(Player player)
            {
                Add(new Coroutine(SecondCutscene(player)));
            }
            private IEnumerator pressButton()
            {
                Parent.Enabled = true;
                Parent.TargetID = DefaultID;
                //play beep sound
                yield return null;
            }
            public IEnumerator SecondCutscene(Player player)
            {
                Level level = Scene as Level;
                player.DisableMovement();
                RuneUI ui = new RuneUI();
                level.Add(ui);
                while (!ui.Finished)
                {
                    yield return null;
                }
                player.EnableMovement();
                yield return null;
            }
            public IEnumerator Cutscene(Player player)
            {
                Level level = Scene as Level;
                player.StateMachine.State = Player.StDummy;
                if (PianoModule.Session.TimesUsedCapsuleWarp < 1 && Marker.TryFind("isStartingWarpRoom", out _))
                {
                    yield return Textbox.Say("capsuleWelcome", pressButton);
                    player.StateMachine.State = Player.StNormal;
                    yield break;
                }
                /*                Vector2 from = level.Camera.Position;
                                Vector2 to = level.Camera.Position + Vector2.UnitX * 80;
                                for (float i = 0; i < 1; i += Engine.DeltaTime)
                                {
                                    level.Camera.Position = Vector2.Lerp(from, to, Ease.CubeOut(i));
                                    yield return null;
                                }*/
                float width = 150;
                float height = 80;
                FakeTerminal t = new FakeTerminal(level.Camera.Position + new Vector2(160, 90) - new Vector2(width / 2, height / 2), width, height);
                Scene.Add(t);
                while (t.TransitionAmount < 1)
                {
                    yield return null;
                }
                WarpProgram program = new WarpProgram(Parent, t);
                Scene.Add(program);

                while (t.TransitionAmount > 0)
                {
                    yield return null;
                }

                /*                to = from;
                                from = level.Camera.Position;
                                for (float i = 0; i < 1; i += Engine.DeltaTime)
                                {
                                    level.Camera.Position = Vector2.Lerp(from, to, Ease.CubeOut(i));
                                    yield return null;
                                }*/
                yield return 0.1f;
                player.StateMachine.State = Player.StNormal;
                yield return null;
            }
        }
        public const string DefaultID = "beginning";
        public const int XOffset = 10;
        public const string Path = "objects/PuzzleIslandHelper/digiWarpReceiver/";
        public static MTexture LonnTexture => GFX.Game[Path + "lonn"];
        public static Vector2 TargetScale = new Vector2(0.4f, 2f);
        public static Vector2 Scale = Vector2.One;
        public float DoorPercent;
        public float ShineAmount;
        public bool InCutscene;
        public bool ReadyForBeam;
        public bool Primary;
        public bool Enabled = true;
        public string WarpID;
        public string WarpPassword;
        public Door LeftDoor, RightDoor;
        public Image Bg, Fg, ShineTex;
        public SnapSolid Floor;
        public DotX3 Talk;
        public Machine InputMachine;
        public WarpBeam Beam;
        private Entity Shine;
        public EntityID ID;
        public string TargetID;
        public float DoorStallTimer;
        public bool DoorsIdle = true;

        public string DisableFlag;
        public bool InvertFlag;
        public bool Accessible;
        public bool Blocked;
        public void Block()
        {
            Blocked = true;
        }
        public void Unblock()
        {
            Alarm.Set(this, 0.3f, delegate { Blocked = false; });
        }
        public WarpCapsule(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            ID = id;
            Primary = data.Bool("primary");
            DisableFlag = data.Attr("disableMachineFlag");
            InvertFlag = data.Bool("invertMachineFlag");
            InputMachine = new Machine(this, data.NodesOffset(offset)[0]);
            Tag |= Tags.TransitionUpdate;
            Depth = 10;
            WarpID = data.Attr("warpID").Replace(" ", "").ToLower();
            WarpPassword = data.Attr("password").Replace(" ", "").ToLower();
            Add(Bg = new Image(GFX.Game[Path + "bg"]));
            Collider = new Hitbox(Bg.Width, Bg.Height);
            Vector2 texoffset = new Vector2(Bg.Width / 2, Bg.Height);
            Bg.JustifyOrigin(0.5f, 1);
            Bg.Position += texoffset;

            Fg = new Image(GFX.Game[Path + "fg"]);
            ShineTex = new Image(GFX.Game[Path + "shine"]);
            ShineTex.Color = Color.White * 0;
            Add(Talk = new DotX3(Collider, Interact));
            if (Primary)
            {
                Talk.Enabled = Enabled = false;
            }
            Add(new BathroomStallComponent(null, Block, Unblock));
        }
        public void Interact(Player player)
        {
            /*            if (ValidateID(TargetID))
                        {
                            Scene.Add(new WarpBack(this, player));
                        }*/

        }
        public bool ValidateID(string id)
        {
            if (!string.IsNullOrEmpty(id) && id != WarpID && GetCapsuleData(id) != null)
            {
                return true;
            }
            return false;
        }
        public static bool ValidatePassword(string id, string password)
        {
            WarpCapsuleData data = GetCapsuleData(id);
            return data != null && (string.IsNullOrEmpty(data.Password) || data.Password.Equals(password));
        }
        public static WarpCapsuleData GetCapsuleData(string id)
        {
            if (!PianoMapDataProcessor.WarpLinks.ContainsKey(id))
            {
                return null;
            }
            return PianoMapDataProcessor.WarpLinks[id];
        }
        public void SetWarpTarget(string id)
        {
            if (ValidateID(id))
            {
                TargetID = id;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            Floor = new SnapSolid(Position + Vector2.UnitY * Height, Width, 2, true) { Fg };
            Shine = new Entity(Position) { ShineTex };
            Shine.Depth = Floor.Depth - 1;
            LeftDoor = new Door(Center, -1, XOffset);
            RightDoor = new Door(Center, 1, XOffset);
            scene.Add(Floor, Shine, LeftDoor, RightDoor);
            scene.Add(InputMachine);
            if (PianoModule.Session.PersistentWarpLinks.ContainsKey(ID))
            {
                TargetID = PianoModule.Session.PersistentWarpLinks[ID];
            }
            else
            {
                PianoModule.Session.PersistentWarpLinks.Add(ID, "");
            }
            if (ValidateID(TargetID))
            {
                InstantOpenDoors();
            }
            else
            {
                InstantCloseDoors();
            }
        }
        public override void Update()
        {
            base.Update();
            if (DoorStallTimer > 0)
            {
                DoorStallTimer = Calc.Approach(DoorStallTimer, 0, Engine.DeltaTime);
            }
            Accessible = (string.IsNullOrEmpty(DisableFlag) || SceneAs<Level>().Session.GetFlag(DisableFlag) == InvertFlag);
            Floor.Collidable = Accessible && !Blocked;
            PianoModule.Session.PersistentWarpLinks[ID] = TargetID;
            Talk.Enabled = !InCutscene && Enabled && Accessible && !Blocked;
            UpdateScale(InCutscene ? Scale : Vector2.One);
            ShineTex.Color = Color.White * ShineAmount;
            if (!InCutscene)
            {
                bool valid = ValidateID(TargetID);
                Enabled = valid;
                if (DoorStallTimer <= 0)
                {
                    MoveAlongTowards(valid);
                }
            }

        }
        public void MoveAlongTowards(bool open)
        {
            if (!open)
            {
                if (DoorPercent < 1)
                {
                    MoveAlong(Math.Min(DoorPercent + Engine.DeltaTime, 1));
                }
            }
            else if (DoorPercent > 0)
            {
                MoveAlong(Math.Max(DoorPercent - Engine.DeltaTime, 0));
            }
        }
        public override void Render()
        {
            LeftDoor.Image.DrawSimpleOutline();
            RightDoor.Image.DrawSimpleOutline();
            Bg.DrawSimpleOutline();
            Fg.DrawSimpleOutline();
            base.Render();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            InputMachine.RemoveSelf();
            Shine.RemoveSelf();
            Floor.RemoveSelf();
            RightDoor.RemoveSelf();
            LeftDoor.RemoveSelf();
        }
        public void MoveAlong(float percent)
        {
            DoorPercent = percent;
            LeftDoor.SetTo(percent);
            RightDoor.SetTo(percent);
        }
        public IEnumerator MoveTo(float from, float to, float time, Ease.Easer ease)
        {
            DoorsIdle = false;
            ease ??= Ease.Linear;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                MoveAlong(Calc.LerpClamp(from, to, ease(i)));
                yield return null;
            }
            MoveAlong(to);
            DoorsIdle = true;
        }
        public IEnumerator CloseAndOpen(float closeTime, float openTime)
        {
            Enabled = false;
            yield return new SwapImmediately(CloseDoorsRoutine(closeTime));
            DoorsIdle = false;
            yield return 0.1f;
            yield return new SwapImmediately(OpenDoorsRoutine(openTime));
            Enabled = true;
        }
        public IEnumerator OpenDoorsRoutine(float openTime)
        {
            float from = DoorPercent;
            yield return new SwapImmediately(MoveTo(from, 0, openTime, null));
        }
        public IEnumerator CloseDoorsRoutine(float closeTime)
        {
            float from = DoorPercent;
            yield return new SwapImmediately(MoveTo(from, 1, closeTime, null));
        }
        public void OpenDoors(float time)
        {
            Enabled = true;
            Add(new Coroutine(OpenDoorsRoutine(time)));
        }
        public void CloseDoors(float time)
        {
            Enabled = false;
            Add(new Coroutine(CloseDoorsRoutine(time)));
        }
        public void InstantOpenDoors()
        {
            Enabled = true;
            MoveAlong(0);
        }
        public void InstantCloseDoors()
        {
            Enabled = false;
            MoveAlong(1);
        }
        public void UpdateScale(Vector2 scale)
        {
            ShineTex.Scale = Bg.Scale = LeftDoor.Scale = RightDoor.Scale = scale;
        }
        public IEnumerator IntroRoutine(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            yield return 0.1f;
            yield return MoveTo(DoorPercent, 0, 1.2f, Ease.BigBackIn);
            LeftDoor.MoveToBg();
            RightDoor.MoveToBg();
            DoorStallTimer = 0.5f;
            /*            if (Primary)
                        {
                            yield return MoveTo(0, 1, 0.8f, null);
                            Enabled = false;
                        }
                        else
                        {
                            Enabled = true;
                        }*/
            player.StateMachine.State = Player.StNormal;
        }
        public IEnumerator OutroRoutine(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            InstantOpenDoors();

            LeftDoor.MoveToFg();
            RightDoor.MoveToFg();
            yield return MoveTo(0, 1, 0.8f, Ease.BigBackIn);
        }
        public IEnumerator ScaleFirst()
        {
            WarpBeam beam = Scene.Tracker.GetEntity<WarpBeam>();
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
            {
                Scale = Vector2.Lerp(Vector2.One, TargetScale, i);
                if (beam != null && beam.Parent == this)
                {
                    beam.YOffset = -Math.Max(0, LonnTexture.Height * (Scale.Y - 1));
                }
                yield return null;
            }
        }
        public IEnumerator ScaleSecond()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                Scale = Vector2.Lerp(TargetScale, Vector2.One, i);
                yield return null;
            }
            Scale = Vector2.One;
        }

        public class WarpBack : CutsceneEntity
        {
            public WarpCapsule Parent;
            public Player Player;
            public WarpBeam Beam;
            public bool Teleported;
            public Vector2 PlayerPosSave;
            public float DoorPercentSave;
            public EntityID FirstParentID;
            public WarpBack(WarpCapsule parent, Player player) : base()
            {
                FirstParentID = parent.ID;
                Parent = parent;
                Player = player;
            }
            public override void OnBegin(Level level)
            {

                Parent.InCutscene = true;
                Add(new Coroutine(Routine(Player)));
            }
            public override void OnEnd(Level level)
            {
                Parent.InCutscene = false;
                if (WasSkipped)
                {
                    if (Parent.ID.ID == FirstParentID.ID)
                    {
                        InstantTeleport(level, Player, Parent, CleanUp);
                    }
                    else
                    {
                        CleanUp(level, Player);
                    }
                }
                PianoModule.Session.TimesUsedCapsuleWarp++;
            }
            private void TeleportCleanUp(Level level, Player player)
            {
                Teleported = true;
                Level = Engine.Scene as Level;
                Player = Level.GetPlayer();
                Parent = Level.Tracker.GetEntity<WarpCapsule>();
                if (Parent != null)
                {
                    Player.StateMachine.State = Player.StDummy;
                    Player.Position = Parent.Position + PlayerPosSave;
                    Player.ForceCameraUpdate = true;
                }
                Level.Camera.Position = Player.CameraTarget;
                Level.Camera.Position.Clamp(Level.Bounds);
                Level.Flash(Color.White, false);

                if (Parent != null)
                {
                    Scale = Vector2.One;
                    Parent.ShineAmount = 1;
                    Parent.DoorPercent = DoorPercentSave;
                    Parent.MoveAlong(DoorPercentSave);
                    Parent.LeftDoor.MoveToFg();
                    Parent.RightDoor.MoveToFg();
                    Parent.InCutscene = true;
                    Parent.UpdateScale(Scale);

                    Beam.Parent = Parent;
                    Beam.Sending = false;
                    Beam.Position = Parent.Floor.TopCenter;
                    Beam.AddPulses();
                }
            }
            private IEnumerator Routine(Player player)
            {
                Player = player;
                yield return player.DummyWalkToExact((int)Parent.CenterX);
                yield return Parent.OutroRoutine(player);
                Beam = new WarpBeam(Parent);
                Scene.Add(Beam);
                while (!Beam.ReadyForScale)
                {
                    yield return null;
                }
                yield return Parent.ScaleFirst();
                AddTag(Tags.Global);
                Beam.AddTag(Tags.Global);
                PlayerPosSave = player.Position - Parent.Position;
                DoorPercentSave = Parent.DoorPercent;
                Beam.EmitBeam(10, (int)Parent.Width, this);
                yield return null;
                InstantTeleport(Level, player, Parent, TeleportCleanUp);
                yield return null;
                while (!Beam.Finished)
                {
                    yield return null;
                }
                yield return PianoUtils.Lerp(null, 0.4f, f => Parent.ShineAmount = 1 - f);
                Parent.ShineAmount = 0;
                yield return Parent.IntroRoutine(Player);

                CleanUp(Level, Player);
                EndCutscene(Level);
            }
            private void CleanUp(Level level, Player player)
            {
                Beam?.RemoveSelf();
                Parent = level.Tracker.GetEntity<WarpCapsule>();
                if (Parent != null)
                {
                    if (Parent.Primary)
                    {
                        Parent.InstantCloseDoors();
                    }
                    else
                    {
                        Parent.InstantOpenDoors();
                    }
                    Parent.ShineAmount = 0;
                    Parent.LeftDoor.MoveToBg();
                    Parent.RightDoor.MoveToBg();
                }
                player.StateMachine.State = Player.StNormal;

            }
            public static void InstantTeleport(Level level, Player player, WarpCapsule from, Action<Level, Player> onEnd = null)
            {
                string room = PianoMapDataProcessor.WarpLinks[from.TargetID].Room;
                if (string.IsNullOrEmpty(room)) return;
                level.OnEndOfFrame += delegate
                {
                    FirfilStorage.Release(false);
                    Vector2 levelOffset = level.LevelOffset;
                    Vector2 playerPosInLevel = player.Position - level.LevelOffset;
                    Vector2 camPos = level.Camera.Position - from.Position;
                    float flash = level.flash;
                    Color flashColor = level.flashColor;
                    bool flashDraw = level.flashDrawPlayer;
                    bool doFlash = level.doFlash;
                    float zoom = level.Zoom;
                    float zoomTarget = level.ZoomTarget;
                    Facings facing = player.Facing;
                    level.Remove(player);
                    level.UnloadLevel();

                    level.Session.Level = room;
                    Session session = level.Session;
                    Level level2 = level;
                    Rectangle bounds = level.Bounds;
                    float left = bounds.Left;
                    bounds = level.Bounds;
                    session.RespawnPoint = level2.GetSpawnPoint(new Vector2(left, bounds.Top));
                    level.Session.FirstLevel = false;
                    level.LoadLevel(Player.IntroTypes.None);


                    level.Zoom = zoom;
                    level.ZoomTarget = zoomTarget;
                    level.flash = flash;
                    level.flashColor = flashColor;
                    level.doFlash = doFlash;
                    level.flashDrawPlayer = flashDraw;
                    player.Position = level.LevelOffset + playerPosInLevel;
                    if (level.Tracker.GetEntity<WarpCapsule>() is var r)
                    {
                        level.Camera.Position = r.Position + camPos;
                    }
                    else
                    {
                        level.Camera.Position = level.LevelOffset + camPos;
                    }
                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                    if (level.Wipe != null)
                    {
                        level.Wipe.Cancel();
                    }

                    onEnd?.Invoke(level, player);
                };
            }
        }
        public class Door : Entity
        {
            private class Lock : Entity
            {
                public Image Image;
                public Door Door;
                public Lock(Door door) : base(door.Position)
                {
                    Door = door;
                    Depth = 7;
                    Image = new Image(GFX.Game[Path + "lock"]);
                    Image.JustifyOrigin(0.5f, 1);
                    Image.Scale.X = door.xScale;
                    Collider = new Hitbox(Image.Width, Image.Height, -Image.Width / 2, -Image.Height / 2);
                    Image.Position.Y += Height / 2;
                    Add(Image);
                }
                public override void Render()
                {
                    Position = Door.Position;
                    base.Render();
                }
            }
            public Image Image;
            private Lock LockPlate;
            public Vector2 Scale = Vector2.One;
            public float xScale;
            public Vector2 Orig;
            private float xOffset;
            public Door(Vector2 position, int xScale, float xOffset) : base(position)
            {
                Depth = 8;
                Orig = position;
                this.xScale = xScale;
                this.xOffset = xOffset * xScale;
                Image = new Image(GFX.Game[Path + "doorFill00"]);
                Image.JustifyOrigin(0.5f, 1);
                Image.Scale.X = xScale;
                Collider = new Hitbox(Image.Width, Image.Height, -Image.Width / 2, -Image.Height / 2);
                Image.Position.Y += Height / 2;
                Add(Image);
                LockPlate = new Lock(this);
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                scene.Add(LockPlate);
            }
            public override void Render()
            {
                ChangeTexture(Scale.Y >= 1.4f);
                Image.Scale = LockPlate.Image.Scale = new Vector2(Scale.X * xScale, Scale.Y);
                base.Render();
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                LockPlate.RemoveSelf();
            }
            public void ChangeTexture(bool extend)
            {
                Image.Texture = GFX.Game[Path + "doorFill0" + (extend ? 1 : 0)];
            }
            public void SetTo(float percent)
            {
                Position.X = (int)Math.Round(Orig.X + xOffset * (1 - percent));
            }
            public void MoveToFg()
            {
                Depth = -2;
                LockPlate.Depth = -3;
            }
            public void MoveToBg()
            {
                Depth = 8;
                LockPlate.Depth = 7;
            }
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Player.Render += Player_Render;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Player.Render -= Player_Render;
        }

        private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
        {
            Vector2 prevScale = self.Sprite.Scale;
            self.Sprite.Scale *= Scale;
            orig(self);
            self.Sprite.Scale = prevScale;
        }
    }

}