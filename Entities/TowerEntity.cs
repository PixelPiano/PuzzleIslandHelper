using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Tower")]
    [Tracked]
    public class TowerEntity : Entity
    {
        private class back : Entity
        {
            private Color color = Color.White.Shade(-0.7f);
            public TowerEntity Tower;
            public Rectangle DoorBounds;
            private VirtualRenderTarget target;
            public back(TowerEntity tower, Rectangle doorBounds) : base(tower.Position)
            {
                Tower = tower;
                Collider = new Hitbox(tower.Width, tower.Height);
                DoorBounds = doorBounds;
                target = VirtualContent.CreateRenderTarget("towerBack", (int)tower.Width, (int)tower.Height);
                bool rendered = false;
                Add(new BeforeRenderHook(() =>
                {
                    if (!rendered)
                    {
                        target.SetAsTarget(color);
                    }
                    rendered = true;
                }));
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                target?.Dispose();
            }
            public override void Render()
            {
                base.Render();
                if (Tower.Front.Opacity < 1)
                {
                    Draw.SpriteBatch.Draw(target, Position, Color.White);
                }
                Draw.Rect(X + DoorBounds.X, Y + DoorBounds.Y, DoorBounds.Width, DoorBounds.Height, Color.Black);
            }
        }
        private class front : Entity
        {
            private Color color = Color.White.Shade(-0.2f);
            public bool Fading;
            public TowerEntity Tower;
            public TalkComponent Enter;
            private Rectangle talkBounds;
            private VirtualRenderTarget target;
            private bool rendered;
            private Tween fadeTween;
            public float Opacity = 1;
            public void FadeTo(float opacity)
            {
                if (Opacity == opacity) return;
                float from = Opacity;
                fadeTween?.RemoveSelf();
                Fading = true;
                fadeTween = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t =>
                {
                    Opacity = Calc.LerpClamp(from, opacity, t.Eased);
                }, t =>
                {
                    Opacity = opacity;
                    Fading = false;
                });
            }
            public void Snap(float opacity)
            {
                fadeTween?.RemoveSelf();
                Fading = false;
                Opacity = opacity;
            }
            public front(TowerEntity tower, Rectangle doorBounds) : base(tower.Position)
            {
                Tower = tower;
                Collider = new Hitbox(tower.Width, tower.Height);
                target = VirtualContent.CreateRenderTarget("frontTarget", (int)Width, (int)Height);
                talkBounds = doorBounds;
                Add(Enter = new TalkComponent(talkBounds, talkBounds.TopCenter(), (p) =>
                {
                    if (!Tower.Inside)
                    {
                        Tower.Enter();
                    }
                    else
                    {
                        Tower.Exit();
                    }
                }));
                Enter.PlayerMustBeFacing = false;
                Add(new BeforeRenderHook(() =>
                {
                    if (!rendered)
                    {
                        target.SetAsTarget();
                        Draw.SpriteBatch.StandardBegin();
                        Draw.Rect(0, 0, talkBounds.X, Height, color);
                        Draw.Rect(talkBounds.X, 0, talkBounds.Right - talkBounds.Left, Height - talkBounds.Height, color);
                        Draw.Rect(talkBounds.Right, 0, Width - talkBounds.Right, Height, color);
                        Draw.SpriteBatch.End();
                        rendered = true;
                    }
                }));
            }
            public override void Render()
            {
                base.Render();
                if (Opacity > 0 && rendered)
                {
                    Draw.SpriteBatch.Draw(target, Position, Color.White * Opacity);
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                target?.Dispose();
            }
        }
        [CustomEntity("PuzzleIslandHelper/TowerInsideSegment")]
        [Tracked]
        private class insideSegment : Entity
        {
            private VertexPositionColor[] Vertices = new VertexPositionColor[6];
            private static Vector2[] points = [new(0), new(0.5f, 0), new(1, 0),
                                              new(0, 1),new(0.5f, 1), new(1, 1)];
            private static int[] indices = [0, 1, 3, 3, 1, 4, 1, 2, 4, 4, 2, 5];
            private Color color = Color.White.Shade(-0.4f);
            private TowerEntity Tower;
            public int OutsideDepth = 2;
            public int InsideDepth;
            public insideSegment(EntityData data, Vector2 offset) : base(data.Position + offset)
            {
                Depth = InsideDepth = data.Int("depth");
                Collider = new Hitbox(data.Width, data.Height);
                for (int i = 0; i < points.Length; i++)
                {
                    Vertices[i].Position = new Vector3(Position + points[i] * Collider.Size, 0);
                    Vertices[i].Color = (i == 1 || i == 4) ? Color.White : Color.Black;
                    if (Depth > 0)
                    {
                        Vertices[i].Color *= 0.5f;
                    }
                }
            }
            public override void Update()
            {
                base.Update();
                Depth = Tower.Inside && !Tower.Front.Fading ? InsideDepth : OutsideDepth;
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Tower = scene.Tracker.GetEntity<TowerEntity>();
                if (Tower == null)
                {
                    RemoveSelf();
                }
            }
            public override void Render()
            {
                base.Render();
                if (Tower.Front.Opacity < 1)
                {
                    //Draw.Rect(Collider, color);
                    Draw.SpriteBatch.End();
                    GFX.DrawIndexedVertices(SceneAs<Level>().Camera.Matrix, Vertices, 6, indices, 4);
                    GameplayRenderer.Begin();
                }
            }
        }
        [CustomEntity("PuzzleIslandHelper/TowerPlatform")]
        [Tracked]
        public class insidePlatform : JumpThru
        {
            private float collisionTimer;
            private Ladder ladder;
            public TowerEntity Tower;
            private Entity ladderChecker;
            private int outsideDepth = 10;
            private int insideDepth = -1;
            public insidePlatform(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, true)
            {
                Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/segment"]);
                Add(image);
                image.Scale.X = Width / 8;
                Depth = outsideDepth;
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                ladderChecker?.RemoveSelf();
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Tower = scene.Tracker.GetEntity<TowerEntity>();
                if (Tower == null)
                {
                    RemoveSelf();
                    return;
                }
                ladderChecker = new Entity(Position + Vector2.UnitY * 8);
                ladderChecker.Collider = new Hitbox(16, 8);
                Scene.Add(ladderChecker);
            }
            public override void Update()
            {
                base.Update();
                Depth = Tower.Inside && !Tower.Front.Fading ? insideDepth : outsideDepth;
                if (GetPlayerRider() is Player player)
                {
                    ladderChecker.CenterX = Calc.Clamp(player.CenterX, X + 8, Right - 8);
                    collisionTimer = 0.2f;
                    if (Input.MoveY == 1 && Input.MoveX == 0)
                    {
                        if (ladderChecker.CollideFirst<Ladder>() is Ladder ladder)
                        {
                            this.ladder = ladder;
                            Collidable = false;
                            ladder.CollidableWhileHoldingDown = true;
                        }

                    }
                }
                else
                {
                    if (collisionTimer > 0)
                    {
                        collisionTimer -= Engine.DeltaTime;
                        if (collisionTimer <= 0)
                        {
                            Collidable = true;
                            if (ladder != null)
                            {
                                ladder.CollidableWhileHoldingDown = false;
                                ladder = null;
                            }
                        }
                    }
                }
            }
        }
        private front Front;
        private InvisibleBarrier[] Barriers = new InvisibleBarrier[2];
        private back Back;
        private List<Entity> Ladders = [];
        private List<Entity> platforms = [];
        public bool Inside;

        public TowerEntity(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Collider = new Hitbox(data.Width, data.Height);
        }

        public void Enter(bool instant = false)
        {
            if (instant)
            {
                Front.Snap(0.1f);
                Front.Depth = -5;
            }
            else if (!Inside)
            {
                Front.FadeTo(0.1f);
            }
            Front.Depth = -5;
            Inside = true;
            foreach (InvisibleBarrier barrier in Barriers)
            {
                barrier.Collidable = true;
            }
            foreach (Ladder ladder in Ladders)
            {
                ladder.Collidable = true;
            }
            foreach (insidePlatform p in platforms)
            {
                p.Collidable = true;
            }
        }
        public void Exit(bool instant = false)
        {
            if (instant)
            {
                Front.Snap(1);
            }
            else if (Inside)
            {
                Front.FadeTo(1);
            }
            Front.Depth = 1;
            Inside = false;
            foreach (InvisibleBarrier barrier in Barriers)
            {
                barrier.Collidable = false;
            }
            foreach (Ladder ladder in Ladders)
            {
                ladder.Collidable = false;
            }
            foreach (insidePlatform p in platforms)
            {
                p.Collidable = false;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            float talkWidth = Width / 4;
            float talkHeight = 32;
            float talkX = Width / 2 - talkWidth / 2;
            float talkY = Height - talkHeight;
            Rectangle doorBounds = new Rectangle((int)talkX, (int)talkY, (int)talkWidth, (int)talkHeight);
            Front = new front(this, doorBounds);
            Back = new back(this, doorBounds);
            Front.Depth = Depth;
            Back.Depth = Depth + 4;
            Scene.Add(Front, Back);
            Barriers[0] = new InvisibleBarrier(TopLeft - Vector2.UnitX * 8, 8, Height);
            Barriers[1] = new InvisibleBarrier(TopRight, 8, Height);
            Scene.Add(Barriers);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Ladders = CollideAll<Ladder>();
            platforms = CollideAll<insidePlatform>();
            if (CollideCheck<Player>())
            {
                Enter(true);
            }
            else
            {
                Exit(true);
            }
        }
        public override void Update()
        {
            base.Update();
            foreach (InvisibleBarrier barrier in Barriers)
            {
                barrier.Collidable = barrier.Active = Inside;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Front?.RemoveSelf();
            Back?.RemoveSelf();
            foreach (var b in Barriers)
            {
                b.RemoveSelf();
            }
        }
    }
}