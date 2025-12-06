using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TransitTower")]
    [Tracked]
    public class Tower : Entity
    {
        public List<Stairs> Stairs;
        public class Entrance : Entity
        {
            public TalkComponent Talk;
            public Rectangle Bounds => Talk.Bounds;
            public float Alpha = 1;
            public Entrance(Vector2 position, float width, float height, Action<Player> interact) : base(position)
            {
                Collider = new Hitbox(width, height);
                Talk = new TalkComponent(new Rectangle(0, 0, (int)width, (int)height), Vector2.UnitX * width / 2, interact);
                Talk.PlayerMustBeFacing = false;
                Add(Talk);
                Depth = -1;
            }
            public override void Render()
            {
                base.Render();
                if (Alpha < 1)
                {
                    DrawDoor(Position, Width, Height, (1 - Alpha) * 0.5f);
                }
            }
            public static void DrawDoor(Vector2 position, float width, float height, float alpha)
            {
                Draw.Rect(position, width, height, Color.Black * alpha);
                Draw.Rect(position.X - 1, position.Y + 1, 2, height - 1, Color.Gray * alpha);
                Draw.Rect(position.X + width - 1, position.Y + 1, 2, height - 1, Color.Gray * alpha);
                Draw.Rect(position - Vector2.One, width + 2, 2, Color.Gray * alpha);
            }
        }
        public Entrance entrance;
        public Entity BackWallEntity;
        public Column.VertexGradient BackWall;
        private VirtualRenderTarget outsideTarget;
        public float OutsideAlpha = 1;
        private bool outsideRendered;
        public FlagList InsideFlag = new FlagList("insideTower");
        public bool Inside => InsideFlag;
        public bool WasInside;
        public Entity Door;
        public Tower(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset - Vector2.UnitX * 8, data.Width + 16, data.Height)
        {
            InsideFlag = new FlagList("insideTower[" + id.Key + ']');
        }
        public Tower(Vector2 position, float width, float height) : base(position)
        {
            Collider = new Hitbox(width, height);
            outsideTarget = VirtualContent.CreateRenderTarget("outsideOfTower", (int)Width, (int)Height);
            Add(new BeforeRenderHook(() =>
            {
                if (outsideRendered) return;
                outsideRendered = true;
                outsideTarget.SetAsTarget();
                outsideTarget.DrawThenMask(
                    mask: () => { Draw.Rect(entrance.X - X, entrance.Y - Y, entrance.Width, entrance.Height, Color.White); },
                    render: () => { Column.VertexGradient.DrawGradient(Matrix.Identity, Vector2.Zero, Width, Height, Color.Black, Color.White); },
                    Matrix.Identity);
            }));
            Depth = 1;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            //Stairs = new Stairs(this, Position + Vector2.UnitX * 8, Width - 16, Height, Floors);
            //scene.Add(Stairs);
            //Stairs.Depth = Depth + 1;
            BackWallEntity = new Entity(Position);
            BackWall = new Column.VertexGradient(Vector2.Zero, (int)Width, (int)Height, Color.Gray, Color.Black);
            BackWallEntity.Add(BackWall);
            BackWallEntity.Depth = Depth + 3;
            scene.Add(BackWallEntity);
            Rectangle b = new Rectangle((int)(Width / 2) - (int)Width / 6, (int)Height - 40, (int)Width / 3, 40);
            entrance = new Entrance(Position + new Vector2(b.X, b.Y), b.Width, b.Height, OnEnter);
            scene.Add(entrance);
        }
        public Column Col;
        public PlayerShade PlayerShade;
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (Stairs stairs in scene.Tracker.GetEntities<Stairs>())
            {
                Stairs.Add(stairs);
                stairs.Parent = this;
                stairs.Depth = Depth + 1;
            }
            Stairs = [.. Stairs.OrderByDescending(item => item.Bottom)];
            float halfColWidth = Stairs[0].Width * 0.3f;
            Col = new Column(this, Position + Vector2.UnitX * ((Width / 2) - halfColWidth), halfWidth * 2, bottom - Top);
            scene.Add(Col);
            scene.Add(PlayerShade = new PlayerShade(0));
        }
        public void OnEnter(Player player)
        {
            Input.Dash.ConsumePress();
            bool wasInside = InsideFlag;
            InsideFlag.State = !wasInside;
            CanEnter = !wasInside;
            if (!wasInside)
            {

                Stairs[0].PlatformTo(Bottom, true);
                Stairs[0].WaitForUpInput = true;
            }
            else
            {
                Stairs[0].WaitForUpInput = false;
            }
        }
        public bool CanEnter;
        public override void Update()
        {
            base.Update();
            if (Scene.GetPlayer() is not Player player) return;
            if (player.Right > Right || player.Left < Left || player.Bottom <= Top)
            {
                InsideFlag.State = false;
            }
            else if (CanEnter)
            {
                InsideFlag.State = true;
            }
            bool inside = InsideFlag;
            entrance.Alpha = OutsideAlpha;
            if (inside)
            {
                if (!WasInside)
                {
                    foreach (Stairs stairs in Stairs)
                    {
                        if (!stairs.Enabled)
                        {
                            stairs.Enable(false);
                        }
                    }
                }
                OutsideAlpha = Calc.Approach(OutsideAlpha, 0, Engine.DeltaTime * 3f);
            }
            else
            {
                if (WasInside)
                {
                    foreach (Stairs stairs in Stairs)
                    {
                        if (stairs.Enabled)
                        {
                            stairs.Disable(false);
                        }
                    }
                }
                OutsideAlpha = Calc.Approach(OutsideAlpha, 1, Engine.DeltaTime * 3f);
            }
            WasInside = inside;
        }
        public override void Render()
        {
            base.Render();
            if (OutsideAlpha > 0)
            {
                Draw.SpriteBatch.Draw(outsideTarget, Position, Color.White * OutsideAlpha);
                Entrance.DrawDoor(entrance.Position, entrance.Width, entrance.Height, OutsideAlpha);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            outsideTarget?.Dispose();
            Stairs.RemoveSelf();
            entrance.RemoveSelf();
        }
    }
}