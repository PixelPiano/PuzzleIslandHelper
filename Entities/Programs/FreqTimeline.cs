using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    public class Freq : Component
    {
        public Vector2 RenderPosition
        {
            get
            {
                return (Entity as FreqTimeline).Position + Position;
            }
        }
        public Vector2 Position;
        public Freq() : base(true, true)
        {

        }
    }


    [Tracked]
    public class Dial : Image
    {
        public FreqTimeline FreqTimeline => Entity as FreqTimeline;
        public Dial(Vector2 position) : base(GFX.Game["objects/PuzzleIslandHelper/interface/dial00"], true)
        {
            Position = position;
            Visible = false;
        }
        public override void Update()
        {
            Position.Y = (int)(BetterWindow.CaseHeight - 8 - BetterWindow.CaseHeight / 4);
            if (FreqTimeline.Holding)
            {
                Position.X = (int)(FreqTimeline.Interface.Collider.AbsoluteX - BetterWindow.DrawPosition.X) - (int)(Width / 2);
                Position.X = (int)MathHelper.Clamp(Position.X, 0, BetterWindow.CaseWidth - Width);
            }
            base.Update();
        }

    }
    [Tracked]
    public class FreqTimeline : WindowContent
    {
        private readonly int TickSpace = 6;
        private readonly int TickHeight = 8;
        public bool Holding;
        private readonly Collider DialCollider;
        private readonly Dial Dial;

        public FreqTimeline(BetterWindow window) : base(window)
        {
            Name = "freq";
            DialCollider = new Hitbox(8, 8);
            Add(Dial = new Dial(Vector2.Zero));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
        public override void Update()
        {
            if (!IsActive)
            {
                base.Update();
                return;
            }
            if (Interface.LeftClicked)
            {
                if (DialCollider.Collide(Interface))
                {
                    Holding = true;
                }
            }
            else
            {
                Holding = false;
            }
            DialCollider.Position = Dial.RenderPosition.ToInt();
            base.Update();
        }
        public override void Render()
        {
            if (!IsActive)
            {
                Dial.Visible = false;
                base.Render();
                return;
            }
            Dial.Visible = true;
            DrawTicks();
            base.Render();
        }
        public void DrawTicks()
        {
            int y = (int)Dial.RenderPosition.Y + (int)Dial.Height;
            Vector2 offset = new Vector2(2, -1);
            for (int i = (int)Position.X; i < (int)BetterWindow.DrawPosition.X + (int)BetterWindow.CaseWidth - 4; i += TickSpace)
            {
                Vector2 start = new Vector2(i, y).ToInt();

                Vector2 end = new Vector2(i, start.Y + TickHeight).ToInt();
                if (i + TickSpace < (int)BetterWindow.DrawPosition.X + (int)BetterWindow.CaseWidth - 1)
                {
                    for (int j = 1; j <= 3; j++)
                    {
                        Vector2 shortStart = start + new Vector2(j * 2, TickHeight - 3).ToInt();
                        Draw.Line(shortStart, shortStart - Vector2.UnitY * 3, Color.DarkGray);
                    }
                }
                Draw.Line(start + offset, end + offset, Color.Black);
            }
            Vector2 longStart = new Vector2(Position.X - 3, y + TickHeight) + offset;

            Draw.Line(longStart.ToInt() + offset, longStart.ToInt() + offset + Vector2.UnitX * ((int)BetterWindow.CaseWidth - 2), Color.Black, 2);
        }

    }
}