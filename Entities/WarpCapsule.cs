using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.WarpCapsule.Runes;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/WarpCapsule")]
    [Tracked]
    public class WarpCapsule : Entity
    {
        public class Machine : Entity
        {
            public static MTexture Texture => GFX.Game[Path + "terminalStand"];
            public static MTexture ScreenTex => GFX.Game[Path + "screen"];
            public Image Image;
            public Image Screen;
            public WarpCapsule Parent;
            public DotX3 Talk;
            public Runes.UI UI;
            public bool Blocked;
            public bool On = true;
            public Machine(WarpCapsule parent, Vector2 position) : base(position)
            {
                Depth = 10;
                Parent = parent;
                Collider = new Hitbox(Texture.Width, Texture.Height);
                Add(Screen = new Image(ScreenTex, true));
                Add(Image = new Image(Texture, true));
                Talk = new DotX3(Collider, Interact);
                Add(Talk);
                Add(new BathroomStallComponent(null, Block, Unblock));
                TurnOn();
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
                Add(new Coroutine(Sequence(player)));
            }
            public void Block()
            {
                Blocked = true;
            }
            public void Unblock()
            {
                Alarm.Set(this, 0.3f, delegate { Blocked = false; });
            }
            public void TurnOn()
            {
                On = true;
                Screen.Color = Color.Blue;
                //Add(new Coroutine(TurnOnRoutine()));
            }
            public void TurnOff()
            {
                On = false;
                Screen.Color = Color.Black;
            }
            public IEnumerator Sequence(Player player)
            {
                player.DisableMovement();
                string dialog = IsFirstTime ? "WontTurnOn" : Inventory.ObtainedRunes.Count < 2 ? "WarpBroken" : null;
                if (!string.IsNullOrEmpty(dialog))
                {
                    yield return Textbox.Say(dialog);
                    player.EnableMovement();
                }
                else
                {
                    Scene.Add(UI = new Runes.UI(Parent));
                    while (!UI.Finished)
                    {
                        yield return null;
                    }
                    Inventory.ObtainedRunes.TryAdd(Parent.WarpRune, false);
                }
                player.EnableMovement();
            }
            public IEnumerator TurnOnRoutine()
            {
                yield return null;
            }
            /*public IEnumerator origCutscene(Player player)
            {
                Level level = Scene as Level;
                player.StateMachine.State = Player.StDummy;
                if (PianoModule.Session.TimesUsedCapsuleWarp < 1 && Marker.TryFind("isStartingWarpRoom", out _))
                {
                    yield return Textbox.Say("capsuleWelcome", pressButton);
                    player.StateMachine.State = Player.StNormal;
                    yield break;
                }
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
                to = from;
                from = level.Camera.Position;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    level.Camera.Position = Vector2.Lerp(from, to, Ease.CubeOut(i));
                    yield return null;
                }
                yield return 0.1f;
                player.StateMachine.State = Player.StNormal;
                yield return null;
            }*/
            /*            private IEnumerator pressButton()
                        {
                            Parent.Enabled = true;
                            Parent.TargetID = DefaultID;
                            //play beep sound
                            yield return null;
                        }*/
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
            public WarpCapsuleData Data;
            public WarpBack(WarpCapsule parent, Player player, WarpCapsuleData data) : base()
            {
                FirstParentID = parent.ID;
                Parent = parent;
                Player = player;
                Data = data;
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
                        InstantTeleport(level, Player, Data, CleanUp);
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
                InstantTeleport(Level, player, Data, TeleportCleanUp);
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
                    Parent.InstantOpenDoors();
                    Parent.ShineAmount = 0;
                    Parent.LeftDoor.MoveToBg();
                    Parent.RightDoor.MoveToBg();
                }
                player.StateMachine.State = Player.StNormal;

            }
            public static void InstantTeleport(Level level, Player player, WarpCapsuleData from, Action<Level, Player> onEnd = null)
            {
                string room = from.Room;
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
        public static class Runes
        {

            [CustomEntity("PuzzleIslandHelper/RuneFragment")]
            [Tracked]
            public class CollectableFragment : Entity
            {
                public string ID;
                public string Connections;
                public Image Image;
                public List<Fragment> Fragments = new();
                public CollectableFragment(EntityData data, Vector2 offset) : base(data.Position + offset)
                {
                    ID = data.Attr("runeID");
                    Connections = data.Attr("connections");
                    Fragments = Fragment.Parse(ID, Connections);
                    Image = new Image(GFX.Game["PuzzleIslandHelper/collectableRuneFragment"]);
                    Add(Image);
                    Collider = Image.Collider();
                    Add(new DotX3(Collider, Collect));
                }
                public override void Added(Scene scene)
                {
                    base.Added(scene);
                    if (GetFlag())
                    {
                        RemoveSelf();
                    }
                }
                public void SetFlag(bool value)
                {
                    SceneAs<Level>().Session.SetFlag(ToString(), value);
                }
                public bool GetFlag()
                {
                    return SceneAs<Level>().Session.GetFlag(ToString());
                }
                public override string ToString()
                {
                    string output = "RuneFragment: {ID: " + ID + "}, {Connections: ";
                    foreach (Fragment fragment in Fragments)
                    {
                        output += fragment;
                    }
                    return output;
                }

                public void Collect(Player player)
                {
                    SetFlag(true);
                    Fragment.AddFragments(Fragments);
                    RemoveSelf();
                }

            }
            public class Fragment(string id, Tuple<NodeTypes, NodeTypes> fragment)
            {
                public Tuple<NodeTypes, NodeTypes> Frag = fragment;
                public string ID = id;
                public override string ToString()
                {
                    return Frag.ToString();
                }
                public static Dictionary<string, List<Fragment>> Fragments = new();
                public static List<Fragment> Parse(string id, string value)
                {
                    List<Fragment> list = [];
                    string[] groups = value.Split(',');
                    for (int i = 0; i < groups.Length; i++)
                    {
                        string[] pair = groups[i].Split(' ');
                        for (int j = 1; j < pair.Length; j += 2)
                        {
                            if (Enum.TryParse(pair[j - 1], out NodeTypes type1) && Enum.TryParse(pair[j], out NodeTypes type2))
                            {
                                list.Add(new Fragment(id, new(type1, type2)));
                            }
                        }
                    }
                    return list;
                }
                public static void AddFragments(List<Fragment> fragments)
                {
                    foreach (Fragment rf in fragments)
                    {
                        AddFragment(rf);
                    }
                }
                public static void ParseAndAddFragments(string id, string value)
                {
                    AddFragments(Parse(id, value));
                }
                public static void AddFragment(string id, Tuple<NodeTypes, NodeTypes> fragment)
                {
                    if (!Fragments.TryGetValue(id, out List<Fragment> value))
                    {
                        Fragments.Add(id, value = []);
                    }
                    value.TryAdd(new Fragment(id, fragment));
                }
                public static void AddFragment(Fragment fragment)
                {
                    if (!Fragments.TryGetValue(fragment.ID, out List<Fragment> value))
                    {
                        Fragments.Add(fragment.ID, value = []);
                    }
                    value.TryAdd(fragment);
                }
                public static void Clear()
                {
                    Fragments.Clear();
                }
                [OnLoad]
                public static void Load()
                {
                    Clear();
                }
                [OnUnload]
                public static void Unload()
                {
                    Clear();
                }
            }
            public class Rune
            {
                public static Rune Default;
                public string ID;
                public List<Tuple<int, int>> Segments = new();
                public Rune(string id, string pattern) : this(id, Parse(pattern))
                {

                }
                public Rune(string id, List<Tuple<int, int>> pattern)
                {
                    Segments = GetSortedPattern(pattern);
                    ID = id;
                }
                public static List<Fragment> ToFragments(Rune rune)
                {
                    List<Fragment> fragments = [];
                    foreach (var a in rune.Segments)
                    {
                        fragments.Add(new(rune.ID, new((NodeTypes)a.Item1, (NodeTypes)a.Item2)));
                    }
                    return fragments;
                }
                public static List<Tuple<int, int>> GetSortedPattern(List<Tuple<int, int>> pattern)
                {
                    List<Tuple<int, int>> insideSorted = new();
                    foreach (var t in pattern)
                    {
                        int a = t.Item1;
                        int b = t.Item2;
                        if (a < b) insideSorted.Add(new(a, b));
                        else insideSorted.Add(new(b, a));
                    }
                    return insideSorted.OrderBy(item => item.Item1).ToList();
                }
                public override string ToString()
                {
                    string output = "{";
                    foreach (Tuple<int, int> pair in Segments)
                    {
                        output += pair.Item1;
                        output += pair.Item2;
                        output += ",";
                    }
                    if (output.Length == 1)
                    {
                        output += "Empty";
                    }
                    output += "}";
                    return output;
                }
                public static List<Tuple<int, int>> Parse(string pattern)
                {
                    pattern = pattern.Replace(" ", "");
                    List<Tuple<int, int>> tuples = new();
                    for (int i = 1; i < pattern.Length; i += 2)
                    {
                        int a = (int)char.GetNumericValue(pattern[i - 1]);
                        int b = (int)char.GetNumericValue(pattern[i]);
                        tuples.Add(new(a, b));
                    }
                    return tuples;
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
                    return pairs.Count == Segments.Count;
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
                [OnUnload]
                public static void Unload()
                {
                    Default = null;
                }
            }
            public class Inventory
            {
                public static List<Rune> ObtainedRunes = [];
                public class RuneInventorySlot
                {
                    public RuneInventorySlot(Inventory parent, List<Fragment> fragments, float x, float xOffset, float y, float size)
                    {
                        Inventory = parent;
                        Fragments = fragments;
                        XOffset = xOffset;
                        X = x;
                        Y = y;
                        Size = size;
                        Connections = new(parent.Connections.Nodes);
                        Connections.TransferFromSlot(this);
                    }
                    public Inventory Inventory;
                    public List<Fragment> Fragments;
                    public float XOffset;
                    public float X;
                    public float Y;
                    public float Size;
                    public Color Color;
                    public UI.ConnectionList Connections;

                    public void Render()
                    {
                        Draw.Rect(X + XOffset, Y, Size, Size, Color.Black);
                        Draw.HollowRect(X + XOffset, Y, Size, Size, Color);
                        foreach (UI.Connection c in Connections.Connections)
                        {
                            c.RenderNodeLine(new Vector2(X + XOffset, Y), new Vector2(Size) / new Vector2(1920, 1080), 2);
                        }
                    }
                    public void Update(bool open, float xOffset, MouseComponent mouse)
                    {
                        XOffset = xOffset;
                        bool colliding = Check(mouse.MousePosition);
                        Color = colliding && open ? Color.White : Color.Green;
                        if (colliding && mouse.JustLeftClicked && Inventory.State == States.Open)
                        {
                            Inventory.Connections.TransferFromSlot(this);
                        }
                    }
                    public bool Check(Vector2 pos)
                    {
                        float x = X + XOffset;
                        return pos.X >= x && pos.X <= x + Size && pos.Y >= Y && pos.Y <= Y + Size;
                    }
                }
                public enum States
                {
                    Closed,
                    Open,
                    Closing,
                    Opening
                }
                public States State;
                public float TabX => origX - TabOffset;
                public float TabRight => TabX + TabWidth;
                private float origX;
                public float TabWidth = 60, TabOffset, MaxOffset = 150;
                private int slotSize = 120, slotSpace = 15;
                public List<RuneInventorySlot> Slots = [];
                public UI.ConnectionList Connections;
                public Dictionary<string, List<Fragment>> DebugFragments = [];
                public Inventory(UI.ConnectionList connections, float tabWidth)
                {
                    /*                    DebugFragments.Add("hello",
                                            [new("hello",new(NodeTypes.MLL, NodeTypes.MRR)),
                                             new("hello",new(NodeTypes.TM, NodeTypes.BL)),
                                             new("hello",new(NodeTypes.TM, NodeTypes.BR))]);
                                        DebugFragments.Add("bye",
                                            [new("bye",new(NodeTypes.TL, NodeTypes.TM)),
                                             new("bye",new(NodeTypes.TL, NodeTypes.BM)),
                                             new("bye",new(NodeTypes.BM, NodeTypes.MR)),
                                             new("bye",new(NodeTypes.MR, NodeTypes.MLL))]);
                                        DebugFragments.Add("styoud",
                                            [new("styoud",new(NodeTypes.MLL, NodeTypes.TL)),
                                             new("styoud",new(NodeTypes.TL, NodeTypes.TR)),
                                             new("styoud",new(NodeTypes.TR, NodeTypes.MRR)),
                                             new("styoud",new(NodeTypes.MRR, NodeTypes.BR)),
                                             new("styoud",new(NodeTypes.BR, NodeTypes.BL)),
                                             new("styoud",new(NodeTypes.BL, NodeTypes.MLL))]);*/
                    Connections = connections;
                    TabWidth = tabWidth;
                    origX = 1920 - tabWidth;
                    float y = slotSpace;
                    foreach (Rune rune in ObtainedRunes)
                    {
                        if (y + slotSize >= 1080) break;
                        Slots.Add(new RuneInventorySlot(this, Rune.ToFragments(rune), slotSpace, TabRight, y, slotSize));
                        y += slotSize + slotSpace;
                    }
                }
                public void Render()
                {
                    Draw.Rect(new Vector2(TabX, 0), TabWidth, 1080, Color.Blue);
                    if (State != States.Closed)
                    {
                        Draw.Rect(new Vector2(TabRight, 0), MaxOffset, 1080, Color.Red);
                        foreach (RuneInventorySlot slot in Slots)
                        {
                            slot.Render();
                        }
                    }
                }
                public void UpdateSlots(MouseComponent mouse)
                {
                    foreach (RuneInventorySlot slot in Slots)
                    {
                        slot.Update(State == States.Open, TabRight, mouse);
                    }
                }
                public void Update(MouseComponent mouse)
                {
                    switch (State)
                    {
                        case States.Opening:
                            TabOffset = Calc.Approach(TabOffset, MaxOffset, 10);
                            if (TabOffset == MaxOffset) State = States.Open;
                            break;
                        case States.Closing:
                            TabOffset = Calc.Approach(TabOffset, 0, 10);
                            if (TabOffset == 0) State = States.Closed;
                            break;
                        case States.Open:
                            if (ClickedTab(mouse))
                            {
                                State = States.Closing;
                            }
                            break;
                        case States.Closed:
                            if (ClickedTab(mouse))
                            {
                                State = States.Opening;
                            }
                            break;
                    }
                    UpdateSlots(mouse);
                }
                public bool ClickedTab(MouseComponent component)
                {
                    float mouseX = component.MousePosition.X;
                    return component.JustLeftClicked && mouseX > TabX && mouseX < TabRight;
                }
                [OnLoad]
                public static void Load()
                {
                    ObtainedRunes.Clear();
                }
                [OnUnload]
                public static void Unload()
                {
                    ObtainedRunes.Clear();
                }
            }
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
                public MTexture HeadTex => GFX.Game[Path + "nodeHead"];
                public MTexture BodyTex => GFX.Game[Path + "nodeBody"];
                public bool Obtained = true;//PianoModule.Session.RuneNodes.Contains(Index);
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
                    if (Obtained)
                    {
                        Vector2 pos = RenderPosition;
                        BodyTex.DrawOutline(pos);
                        HeadTex.DrawOutline(pos);
                        BodyTex.Draw(pos);
                        HeadTex.Draw(pos, Vector2.Zero, Lit ? Color.Orange : Color.Red);
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
            public class UI : Entity
            {
                public const string Path = "objects/PuzzleIslandHelper/runeUI/";
                public class Connection
                {
                    public Runes.Node[] Nodes = new Runes.Node[2];
                    public Connection(Runes.Node a, Runes.Node b)
                    {
                        Nodes = [a, b];
                    }
                    public Runes.Node GetPartner(Runes.Node exclude)
                    {
                        return Nodes[0] == exclude ? Nodes[1] : Nodes[0];
                    }
                    public bool IsForbidden()
                    {
                        return Nodes[0] != null && Nodes[1] != null && Nodes[0].ImpliedConnections.ContainsKey(Nodes[1].Type);
                    }
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
                    public bool TryGetTuple(out Tuple<int, int> tuple)
                    {
                        if (Incomplete)
                        {
                            tuple = null;
                            return false;
                        }
                        tuple = new(Nodes[0].Index, Nodes[1].Index);
                        return true;
                    }
                    public bool Incomplete => Nodes[0] == null || Nodes[1] == null;
                    public bool Empty => Nodes[0] == null && Nodes[1] == null;
                    public bool Full => !Empty;
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
                        List<Tuple<Node, Node>> list = [];

                        foreach (var rf in slot.Fragments)
                        {
                            int item1 = (int)rf.Frag.Item1;
                            int item2 = (int)rf.Frag.Item2;
                            list.Add(new Tuple<Node, Node>(Nodes[item1], Nodes[item2]));
                        }
                        foreach (var pair in list)
                        {
                            AddValidConnections(pair.Item1, pair.Item2);
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
                    public void OnLeftClick(Node node)
                    {
                        if (Held.Count == 0 && node != null && LeftHeld == null)
                        {
                            LeftHeld = new Connection(node, null);
                        }
                    }
                    public bool TryAddConnection(Node from, Node to)
                    {
                        if (!HasPair(from, to))
                        {
                            AddConnection(from, to);
                            return true;
                        }
                        return false;
                    }
                    public void AddValidConnections(Node from, Node to)
                    {
                        if (from != to)
                        {
                            if (from.ImpliedConnections.TryGetValue(to.Type, out List<NodeTypes> list))
                            {
                                NodeTypes start = from.Type;
                                foreach (NodeTypes node in list)
                                {
                                    Node fromNode = Nodes[(int)start];
                                    Node connect = Nodes[(int)node];
                                    TryAddConnection(fromNode, connect);
                                    start = node;
                                }
                            }
                            else TryAddConnection(from, to);
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
                                if (c.IsForbidden() || (c.CollideLine(mouse) && !c.CollideHead(mouse)))
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
                    public void AddConnection(Node a, Node b)
                    {
                        Connections.Add(new Connection(a, b));
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
                public VirtualRenderTarget Buffer, Buffer2;
                public List<Runes.Node> Nodes = new();
                public ConnectionList Connections;
                public SubmitButton Button;
                public MouseComponent Mouse;
                public Inventory Inventory;
                public float Alpha = 1;
                public bool Standby;
                public bool Finished;
                public bool CollidingWithButton;
                public WarpCapsule Parent;


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
                    Button = new SubmitButton(new Vector2(50, 1080 - 209));
                    Inventory = new(Connections, 60);
                }
                public override void Update()
                {
                    base.Update();
                    if (Standby) return;
                    if (Input.DashPressed)
                    {
                        FadeOut(true);
                    }
                    Vector2 pos = Mouse.MousePosition;
                    Connections.Update();
                    CollidingWithButton = Button.Check(pos);
                    Inventory.Update(Mouse);
                }
                public void OnLeftClick()
                {
                    Button.ClickedFirstFrame = CollidingWithButton;
                    Connections.OnLeftClick(GetFirstCollided());
                }
                public void OnLeftRelease()
                {
                    if (Button.ClickedFirstFrame && CollidingWithButton)
                    {
                        CheckRune();
                    }
                    Button.ClickedFirstFrame = false;
                    Connections.OnLeftRelease(GetFirstCollided());
                }
                public void OnLeftIdle()
                {
                    ResetButton();
                }
                public void OnLeftHeld()
                {
                    Button.Colliding = Button.ClickedFirstFrame && CollidingWithButton;
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
                    Button.Colliding = false;
                    Button.ClickedFirstFrame = false;
                }
                public Rune CreateRune()
                {
                    List<Tuple<int, int>> list = new();
                    List<Connection> unique = Connections.Connections.Distinct().ToList();
                    foreach (Connection c in unique)
                    {
                        if (c.TryGetTuple(out var tuple))
                        {
                            list.Add(tuple);
                        }
                    }
                    return new Rune("", list);
                }
                public void CheckRune()
                {
                    Button.ClickedFirstFrame = false;
                    Button.Colliding = false;
                    Rune rune = CreateRune();
                    foreach (Rune r in PianoMapDataProcessor.WarpRunes.Keys)
                    {
                        if (r.Match(rune))
                        {
                            Inventory.ObtainedRunes.TryAdd(r);
                            ClearAll();
                            FadeOut(true);
                            Parent.WarpRune = r;
                            return;
                        }
                    }
                }
                public void ClearAll()
                {
                    Connections.ClearMain();
                }
                public void DrawLine(Node a, Node b, Color color, int thickness)
                {
                    Draw.Line(a.Center, b.Center, color, thickness);
                }
                public void BeforeRender()
                {
                    if (Alpha > 0)
                    {
                        Buffer2.SetAsTarget(true);
                        Draw.SpriteBatch.StandardBegin(Matrix.Identity, BlendState.AlphaBlend, null);
                        Connections.Render(Mouse.MousePosition);
                        Draw.SpriteBatch.End();

                        Buffer.SetAsTarget(Color.Black);
                        Draw.SpriteBatch.StandardBegin(Matrix.Identity);
                        foreach (Node node in Nodes)
                        {
                            node.DrawTexture();
                        }
                        Button.Render();
                        Draw.SpriteBatch.Draw(Buffer2, Vector2.Zero, Color.White);
                        Inventory.Render();
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

        public static bool IsFirstTime => PianoModule.Session.TimesUsedCapsuleWarp < 1 && Marker.TryFind("isStartingWarpRoom", out _);
        public const int XOffset = 10;
        public const string Path = "objects/PuzzleIslandHelper/digiWarpReceiver/";
        public static MTexture LonnTexture => GFX.Game[Path + "lonn"];
        public static Vector2 TargetScale = new Vector2(0.4f, 2f);
        public static Vector2 Scale = Vector2.One;
        public enum NodeTypes
        {
            TL, TM, TR, MLL, ML, MR, MRR, BL, BM, BR
        }

        public float DoorPercent, ShineAmount, DoorStallTimer;
        public bool InCutscene, ReadyForBeam, Enabled = true, DoorsIdle = true, InvertFlag, Accessible, Blocked;
        public string TargetID, DisableFlag;
        public EntityID ID;
        public Image Bg, Fg, ShineTex;
        public DotX3 Talk;
        private Entity Shine;
        public SnapSolid Floor;
        public Machine InputMachine;
        public Door LeftDoor, RightDoor;
        public WarpBeam Beam;
        public Rune WarpRune;
        public WarpCapsuleData RuneData
        {
            get
            {
                if (WarpRune != null && PianoMapDataProcessor.WarpRunes.TryGetValue(WarpRune, out WarpCapsuleData value))
                {
                    return value;
                }
                return null;

            }
        }
        public WarpCapsule(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            Depth = 10;
            ID = id;
            DisableFlag = data.Attr("disableMachineFlag");
            InvertFlag = data.Bool("invertMachineFlag");
            Add(Bg = new Image(GFX.Game[Path + "bg"]));
            Bg.JustifyOrigin(0.5f, 1);
            Collider = Bg.Collider();
            Vector2 texoffset = new Vector2(Bg.Width / 2, Bg.Height);
            Bg.Position += texoffset;
            InputMachine = new Machine(this, data.NodesOffset(offset)[0]);
            Fg = new Image(GFX.Game[Path + "fg"]);
            ShineTex = new Image(GFX.Game[Path + "shine"]);
            ShineTex.Color = Color.White * 0;
            Add(Talk = new DotX3(Collider, Interact));
            Add(new BathroomStallComponent(null, Block, Unblock));
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
            if (IsFirstTime || Inventory.ObtainedRunes.Count < 2)
            {
                InputMachine.TurnOff();
            }
            else
            {
                InputMachine.TurnOn();
            }
            if (PianoModule.Session.PersistentWarpLinks.TryGetValue(ID, out string value))
            {
                TargetID = value;
            }
            else
            {
                PianoModule.Session.PersistentWarpLinks.Add(ID, "");
            }
            if (IsFirstTime)
            {
                WarpRune = Rune.Default;
            }
            if (WarpIsValid())
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
                bool valid = WarpIsValid();
                Enabled = valid;
                if (DoorStallTimer <= 0)
                {
                    MoveAlongTowards(valid);
                }
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
        public void Interact(Player player)
        {
            WarpCapsuleData data = RuneData;
            if (data != null)
            {
                Scene.Add(new WarpBack(this, player, data));
            }

        }
        public bool WarpIsValid()
        {
            return RuneData != null;
        }
        public void Block()
        {
            Blocked = true;
        }
        public void Unblock()
        {
            Alarm.Set(this, 0.3f, delegate { Blocked = false; });
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
        public void MoveAlong(float percent)
        {
            DoorPercent = percent;
            LeftDoor.SetTo(percent);
            RightDoor.SetTo(percent);
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