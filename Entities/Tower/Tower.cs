using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TransitTower")]
    [Tracked]
    public class Tower : Entity
    {
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
        public class Bg : Entity
        {
            public Column.VertexGradient Gradient;
            public Tower Parent;
            public Bg(Tower parent, Vector2 position) : base(position)
            {
                Parent = parent;
            }
            public override void Render()
            {
                base.Render();
                for (int i = 1; i < Parent.Stairs.Count; i++)
                {
                    Stairs a = Parent.Stairs[i - 1];
                    Stairs b = Parent.Stairs[i];
                    Draw.Rect(Parent.Left, a.Bottom, Parent.Width, b.Top - a.Bottom, Color.Gray * (1 - Parent.OutsideAlpha));
                }
            }
        }
        public List<Stairs> Stairs = [];
        public Entrance entrance;
        public Entity BgEntity;
        public Column.VertexGradient BackWall;
        private VirtualRenderTarget outsideTarget;
        public float OutsideAlpha = 1;
        private bool outsideRendered;
        public FlagList InsideFlag = new FlagList("insideTower");
        public bool Inside => InsideFlag;
        public bool WasInside;
        public Entity Door;
        public Column Col;
        public PlayerShade PlayerShade;
        public bool CanEnter;
        public Tower(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset - Vector2.UnitX * 8, data.Width + 16, data.Height)
        {
            InsideFlag = new FlagList("insideTower[" + id.Key + ']');
        }
        public Tower(Vector2 position, float width, float height) : base(position)
        {
            Collider = new Hitbox(width, height);
            outsideTarget = VirtualContent.CreateRenderTarget("outsideOfTower", (int)Width, (int)Height);
            Add(new BeforeRenderHook(BeforeRender));
            AddTag(Tags.TransitionUpdate);
            Depth = 1;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            BgEntity = new Entity(Position);
            BackWall = new Column.VertexGradient(Vector2.Zero, (int)Width, (int)Height, Color.Gray, Color.Black);
            BgEntity.Add(BackWall);
            BgEntity.Depth = Depth + 3;
            scene.Add(BgEntity);
            Rectangle b = new Rectangle((int)(Width / 2) - (int)Width / 6, (int)Height - 40, (int)Width / 3, 40);
            entrance = new Entrance(Position + new Vector2(b.X, b.Y), b.Width, b.Height, OnEnter);
            scene.Add(entrance);
            foreach (Stairs stairs in scene.Tracker.GetEntities<Stairs>()) /// 
            {
                Stairs.Add(stairs);
                stairs.Parent = this;
                stairs.Depth = Depth + 1;
            }
            float halfColWidth = 40;
            if (Stairs.Count > 0) ///
            {
                Stairs = [.. Stairs.OrderByDescending(item => item.Bottom)];
                halfColWidth = (int)(Stairs[0].Width * 0.3f);
            }
            float levelBottom = (scene as Level).Bounds.Bottom;
            Col = new Column(this, Position + Vector2.UnitX * (Width / 2 - halfColWidth), halfColWidth * 2, levelBottom - Top);
            scene.Add(PlayerShade = new PlayerShade(0));
            foreach (Stairs stairs in Stairs) ///
            {
                stairs.Initialize(scene);
            }
            scene.Add(Col);
            foreach (Stairs stairs in Stairs) ///
            {
                stairs.Disable();
            }
            if (Stairs.Count > 0) ///
            {
                Col.Depth = Stairs[0].Depth + 1;
            }
            Col.HidesPlayer = false;
        }
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
            //if ((Input.MoveY == 1 && TopPlatform.HasPlayerRider() && player.CenterX >= CenterX && player.CenterX < CenterX + Platform.Width * 2))
            //{
            //   TopPlatformCollisionTimer = 0.3f;
            //  Parent.CanEnter = true;
            //}
            if (inside)
            {
                Col.Collidable = true;
                foreach (Stairs stairs in Stairs) ///
                {
                    if (!stairs.Enabled)
                    {
                        stairs.Enable();
                    }
                }
                bool hidePlayer = false;
                float shade = 0;
                foreach (Stairs s in Stairs) ///
                {
                    if (player.CollideCheck(s))
                    {
                        hidePlayer = s.HidingEnabled;
                        shade = s.ShadeValue;
                        break;
                    }
                }
                Col.HidesPlayer = hidePlayer;
                PlayerShade.Alpha = Calc.Approach(PlayerShade.Alpha, shade, Engine.DeltaTime * 3f);
                OutsideAlpha = Calc.Approach(OutsideAlpha, 0, Engine.DeltaTime * 3f);
            }
            else
            {
                Col.HidesPlayer = false;
                foreach (Stairs stairs in Stairs) ///
                {
                    if (stairs.Enabled)
                    {
                        stairs.Disable();
                    }
                }
                OutsideAlpha = Calc.Approach(OutsideAlpha, 1, Engine.DeltaTime * 3f);
                PlayerShade.Alpha = Calc.Approach(PlayerShade.Alpha, 0, Engine.DeltaTime * 3f);
            }
            WasInside = inside;
        }
        private void BeforeRender()
        {
            if (outsideRendered) return;
            outsideRendered = true;
            outsideTarget.SetAsTarget();
            outsideTarget.DrawThenMask(
                mask: () => { Draw.Rect(entrance.X - X, entrance.Y - Y, entrance.Width, entrance.Height, Color.White); },
                render: () => { Column.VertexGradient.DrawGradient(Matrix.Identity, Vector2.Zero, Width, Height, Color.Black, Color.White); },
                Matrix.Identity);
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
            Col.RemoveSelf();
        }
        public void OnEnter(Player player)
        {
            Input.Dash.ConsumePress();
            bool wasInside = InsideFlag;
            InsideFlag.State = !wasInside;
            CanEnter = !wasInside;
            if (Stairs.Count > 0) ///
            {
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
        }
    }
}