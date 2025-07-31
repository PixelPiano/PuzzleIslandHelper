using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
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
    public class Inventory
    {
        public class RuneInventorySlot
        {
            public RuneInventorySlot(Inventory parent, WarpRune rune, float x, float xOffset, float y, float size, float scrollSize)
            {
                Rune = rune;
                this.scrollSize = scrollSize;
                Inventory = parent;
                XOffset = xOffset;
                X = x;
                Y = y;
                Size = size;
                Connections = new(parent.Connections.Nodes);
                Connections.TransferFromRune(rune);
            }
            public bool OnScreen
            {
                get
                {
                    float y = RenderPosition.Y;
                    return y < 1920 && y + Size > 0;
                }
            }
            public Inventory Inventory;

            public float XOffset;
            public float ScrollOffset;
            public Vector2 RenderPosition => new Vector2(X + XOffset, Y + ScrollOffset);
            public float X;
            public float Y;
            public float Size;
            public Color Color;
            private float scrollSize;

            public WarpRune Rune;
            public UI.ConnectionList Connections;

            public void Render()
            {
                if (OnScreen)
                {
                    Vector2 p = RenderPosition;
                    Draw.Rect(p, Size, Size, Color.Black);
                    Draw.HollowRect(p, Size, Size, Color);
                    foreach (UI.Connection c in Connections.Connections)
                    {
                        c.RenderNodeLine(p, new Vector2(Size) / new Vector2(1920, 1080), Color.White, 3);
                    }
                }
            }
            public void Update(bool open, float xOffset, int scrollOffset, MouseComponent mouse)
            {
                XOffset = xOffset;
                ScrollOffset = scrollOffset * scrollSize;
                bool colliding = Check(mouse.MousePosition);
                if (colliding)
                {
                    Inventory.DebugText = Connections.ToString();
                }
                Color = colliding && open ? Color.White : Color.Green;
                if (colliding && mouse.JustLeftClicked && Inventory.State == States.Open)
                {
                    Inventory.Connections.TransferFromRune(Rune);
                }
            }
            public bool Check(Vector2 pos)
            {
                Vector2 p = RenderPosition;
                return pos.X >= p.X && pos.X <= p.X + Size && pos.Y >= p.Y && pos.Y <= p.Y + Size;
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
        public string DebugText;
        private int scroll;
        private int lastScroll;
        private int currentScroll;
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
            foreach (WarpRune rune in AllRunes)
            {
                Slots.Add(new RuneInventorySlot(this, rune, slotSpace, TabRight, y, slotSize, slotSize + slotSpace));
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
            ActiveFont.Draw(scroll.ToString(), Vector2.Zero, Color.White);
        }
        public void UpdateSlots(MouseComponent mouse)
        {
            lastScroll = currentScroll;
            currentScroll = mouse.State.ScrollWheelValue;
            scroll = Calc.Clamp(scroll + (currentScroll - lastScroll) / 120, Math.Max(-8, -Slots.Count), 0);
            foreach (RuneInventorySlot slot in Slots)
            {
                slot.Update(State == States.Open, TabRight, scroll, mouse);
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
                        Slots = Slots.OrderBy(item => item.Rune.ID).ThenBy(item => item.Rune.Segments.Count).ToList();
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
}
