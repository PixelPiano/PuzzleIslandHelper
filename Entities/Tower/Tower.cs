using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TransitTower")]
    [Tracked]
    public class Tower : Entity
    {
        [CustomEntity("PuzzleIslandHelper/TowerEntrance")]
        [Tracked]
        public class Entrance : Entity
        {
            private VirtualRenderTarget leftTarget;
            public TalkComponent Talk;
            public Action<Player> OnInteract;
            public MTexture Texture;
            private MTexture halfTexture;
            public FlagList Flag;
            public Rectangle Bounds => Talk.Bounds;
            public float Alpha = 1;
            private float openPercent;
            public bool CanInteract = true;
            public bool Locked => !Flag;
            public ImageShine Shine;
            private Color iconColor = Color.Black;
            private Color doorColor = Color.White;
            public Entrance(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height)
            {
                Flag = data.FlagList("lockFlag");
            }
            public Entrance(Vector2 position, int width, int height) : base(position)
            {
                Collider = new Hitbox(width, height);
                Depth = -1;
                Add(new BeforeRenderHook(() =>
                {
                    float halfWidth = width / 2;
                    if (openPercent < 1) //if door not completely open
                    {
                        Vector2 center = new Vector2(width, height) / 2;
                        float xOffset = halfWidth * openPercent;

                        leftTarget.SetAsTarget(true);
                        Draw.SpriteBatch.Begin();
                        Draw.Rect(-xOffset, 0, halfWidth, height, doorColor);
                        Draw.Line(-xOffset + halfWidth + 1, 0, -xOffset + halfWidth + 1, height, Color.Black);
                        Vector2 leftCenter = center - Vector2.UnitX * xOffset;
                        halfTexture.DrawJustified(leftCenter, new Vector2(1, 0.5f), iconColor);
                        Draw.SpriteBatch.End();
                    }
                    else
                    {
                        leftTarget.SetAsTarget(true);
                    }

                }));
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Texture = GFX.Game["objects/PuzzleIslandHelper/tower/doorIcon"];
                halfTexture = Texture.GetSubtexture(0, 0, Texture.Width / 2, Texture.Height);
                leftTarget = VirtualContent.CreateRenderTarget("entranceDoorLeftTarget", (int)(Width / 2), (int)Height);
                Add(Shine = new ImageShine(Texture, 0)
                {
                    Color = Color.Black,
                    Position = Collider.HalfSize,
                    PrePulseFrameOffset = -4,
                    OnPrePulse = () =>
                    {
                        if (!Flag && PianoModule.Session.CanUseKey)
                        {
                            pulse(Shine.PulseDelay * 0.6f, 1);
                        }
                    },
                });
            }
            private void pulse(float time, float doorMult)
            {
                iconColor = Color.Lerp(Color.Black, Color.White, doorMult);
                doorColor = Color.Lerp(Color.White, Color.Black, doorMult);
                Shine.Color = Color.White;
                Color iconFrom = iconColor, doorFrom = doorColor;

                Tween.Set(this, Tween.TweenMode.Oneshot, time, Ease.CubeOut, t =>
                {
                    iconColor = Color.Lerp(iconFrom, Color.Black, t.Eased);
                    doorColor = Color.Lerp(doorFrom, Color.White, t.Eased);
                    Shine.Color = Color.Lerp(Color.White, Color.Black, t.Eased);
                }, t =>
                {
                    iconColor = Color.Black;
                    doorColor = Color.White;
                    Shine.Color = Color.Black;
                });
            }
            public override void Update()
            {
                base.Update();
                if (!Flag && PianoModule.Session.CanUseKey)
                {
                    Shine.Alpha = Calc.Approach(Shine.Alpha, (float)(Math.Sin(Scene.TimeActive * 0.9f) + 1) / 2, 15f * Engine.DeltaTime);
                }
                else
                {
                    Shine.Alpha = Calc.Approach(Shine.Alpha, 0, Engine.DeltaTime * 10);
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                leftTarget.Dispose();
            }
            public void Initialize(Action<Player> interact)
            {
                OnInteract = interact;
                Talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), Vector2.UnitX * Width / 2, Interact);
                Talk.PlayerMustBeFacing = false;
                Add(Talk);
            }
            public void Interact(Player player)
            {
                Add(new Coroutine(routine(player)));
            }
            private IEnumerator routine(Player player)
            {
                player.DisableMovement();
                if (Locked)
                {
                    if (PianoModule.Session.CanUseKey)
                    {
                        PianoModule.Session.KeysUsed++;
                        Flag.State = true;
                        yield return new SwapImmediately(swap(player));
                    }
                    else
                    {
                        SceneAs<Level>().Session.SetFlag("TriedToOpenLockedTower");
                        yield return new SwapImmediately(Textbox.Say("it's locked."));
                    }
                }
                else
                {
                    yield return new SwapImmediately(swap(player));
                }
                player.EnableMovement();
            }
            private IEnumerator swap(Player player)
            {
                yield return 0.2f;
                yield return new SwapImmediately(open());
                yield return 0.3f;
                OnInteract.Invoke(player);
                yield return 0.7f;
                yield return new SwapImmediately(close());
            }
            private IEnumerator open()
            {
                yield return new SwapImmediately(PianoUtils.Lerp(Ease.Linear, 1, f => openPercent = f, true));
                openPercent = 1;
            }
            private IEnumerator close()
            {
                yield return new SwapImmediately(PianoUtils.Lerp(Ease.Linear, 1, f => openPercent = 1 - f, true));
            }
            public override void Render()
            {
                base.Render();
                if (Alpha < 1)
                {
                    DrawDoor(Position, Width, Height, (1 - Alpha) * 0.5f);
                }
            }
            public void DrawDoor(Vector2 position, float width, float height, float alpha)
            {
                Draw.Rect(position, width, height, Color.Black * alpha);
                Draw.SpriteBatch.Draw(leftTarget, position, Color.White * alpha);
                Draw.SpriteBatch.Draw(leftTarget, position + Vector2.UnitX * ((int)(width / 2)), null, Color.White * alpha,
                    0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
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
        public class RodEntity : Solid
        {
            public Tower Parent;
            public float Lerp;
            public RodEntity(Tower parent, Vector2 position, float width, float height) : base(position, width, height, true)
            {
                Parent = parent;
            }
            public override void Render()
            {
                base.Render();
                Draw.Rect(Collider, Color.Lerp(Color.White, Color.Red, Lerp));
            }
        }
        public class PortalEntity : Entity
        {
            public Image Circle;
            public float Scale = 0;
            public PortalEntity(Vector2 position) : base(position)
            {
                Add(Circle = new Image(GFX.Game["objects/PuzzleIslandHelper/circle"]));
                Circle.CenterOrigin();
                Circle.Scale = Vector2.Zero;
                Depth = 1;
            }
            public override void Update()
            {
                base.Update();
                Circle.Scale = Vector2.One * Scale;
            }

        }
        public class EndingEntity : Entity
        {
            public class Cutscene : CutsceneEntity
            {
                public Tower Parent;
                public Cutscene(Tower tower) : base(true, true)
                {
                    Parent = tower;
                }
                public override void OnBegin(Level level)
                {
                    if (level.GetPlayer() is Player player)
                    {
                        player.DisableMovement();
                        Add(new Coroutine(cutscene(player)));
                    }
                }
                private IEnumerator cutscene(Player player)
                {
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        foreach (RodEntity rod in Parent.Rods)
                        {
                            rod.Lerp = i;
                        }
                        yield return null;
                    }
                    yield return 0.2f;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        Parent.Portal.Scale = i * 4;
                        yield return null;
                    }
                    yield return 0.5f;
                    player.DummyGravity = false;
                    float mult = 0;
                    while (player.Center != Parent.Portal.Center)
                    {
                        player.Center = Calc.Approach(player.Center, Parent.Portal.Center, 40f * Engine.DeltaTime * mult);
                        mult = Calc.Approach(mult, 1, Engine.DeltaTime);
                        yield return null;
                    }
                    yield return 1;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        player.Sprite.Scale = Vector2.One * Ease.BigBackIn(1 - i);
                        yield return null;
                    }
                    player.Visible = false;
                    Level.Flash(Color.White);
                    var e = PulseEntity.Circle(Parent.Portal.Position, 2, Pulse.Fade.Linear, Pulse.Mode.Oneshot, 0, 54, 2, true, Color.White, default, null, Ease.CubeOut);
                    e.Pulse.Thickness = 6;
                    yield return 1;
                    EndCutscene(Level);
                }
                public override void OnEnd(Level level)
                {
                    level.CompleteArea(true, false, false);
                }
            }
            public TalkComponent Talk;
            public Pulse Pulse;
            public bool InCutscene;
            public EndingEntity(Tower tower, float yOffset) : base(tower.Position + Vector2.UnitY * yOffset)
            {
                Collider = new Hitbox(tower.Width, Math.Abs(yOffset));
                Rectangle bounds = new Rectangle(0, 0, (int)Width, (int)Height);
                Pulse = Pulse.Diamond(this, Width - 16, Color.White, default, 0.7f, false, null, Ease.CubeOut);
                Pulse.PulseMode = Pulse.Mode.Persist;
                Pulse.Position = Collider.HalfSize;
                Add(Talk = new TalkComponent(bounds, Collider.HalfSize, p =>
                {
                    Scene.Add(new Cutscene(tower));
                    InCutscene = true;
                }));
                Pulse.Thickness = 8;
                Add(new Coroutine(pulseRoutine()));
            }
            public override void Update()
            {
                base.Update();
                if (!InCutscene)
                {
                    Pulse.Alpha = Calc.Approach(Pulse.Alpha, 1, Engine.DeltaTime);
                }
                else
                {
                    Pulse.Alpha = Calc.Approach(Pulse.Alpha, 0, Engine.DeltaTime);
                }
            }
            private IEnumerator pulseRoutine()
            {
                while (true)
                {
                    Pulse.Start();
                    while (Pulse.Active)
                    {
                        yield return null;
                    }
                    yield return 0.8f;
                }
            }
        }
        public EndingEntity Ending;
        public RodEntity[] Rods = new RodEntity[2];
        public PortalEntity Portal;
        public List<Stairs> Stairs = [];
        public Entrance entrance;
        public Entity BgEntity;
        public Column.VertexGradient BackWall;
        public List<Stairs.CustomPlatform> Floors = [];
        private VirtualRenderTarget outsideTarget;
        public Column Col;
        public PlayerShade PlayerShade;
        public InvisibleBarrier[] Safeguards = new InvisibleBarrier[2];
        public float OutsideAlpha = 1;
        private bool outsideRendered;
        public FlagList InsideFlag = new FlagList("insideTower");
        public bool Inside => InsideFlag;
        public bool WasInside;
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
            Depth = 3;
            Add(new Column.Elevator.InteractComponent(OnEnterColumn, OnExitColumn));
        }
        public void OnEnterColumn(Player p)
        {
            foreach (var s in Stairs)
            {
                s.DisablePlatform = true;
            }
            foreach (var f in Floors)
            {
                f.Disabled = true;
            }
        }
        public void OnExitColumn(Player p)
        {
            foreach (var s in Stairs)
            {
                s.DisablePlatform = false;
            }
            foreach (var f in Floors)
            {
                f.Disabled = false;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            int height = 50;
            Rods[0] = new RodEntity(this, Position - Vector2.UnitY * height, 16, height);
            Rods[1] = new RodEntity(this, TopRight - new Vector2(16, height), 16, height);
            Portal = new PortalEntity(TopCenter - Vector2.UnitY * 50);
            Ending = new EndingEntity(this, -50);
            scene.Add(Rods);
            scene.Add(Portal, Ending);

            BgEntity = new Entity(Position);
            BackWall = new Column.VertexGradient(Vector2.Zero, (int)Width, (int)Height, Color.Gray, Color.Black);
            BgEntity.Add(BackWall);
            BgEntity.Depth = Depth + 3;
            scene.Add(BgEntity);
            Rectangle b = new Rectangle((int)(Width / 2) - (int)Width / 6, (int)Height - 40, (int)Width / 3, 40);
            entrance = PianoUtils.SeekController(scene, () =>
            {
                return new Entrance(Position + new Vector2(b.X, b.Y), b.Width, b.Height);
            });
            entrance.Initialize(OnEnterOrExit);
            foreach (Stairs stairs in PianoUtils.SeekControllers<Stairs>(scene)) /// 
            {
                Stairs.Add(stairs);
                stairs.Parent = this;
                stairs.Depth = Depth + 1;
            }
            bool needsTopFloor = true;
            foreach (Stairs stairs in Stairs)
            {
                Stairs.CustomPlatform floor = new Stairs.CustomPlatform(new Vector2(X, stairs.Bottom), (int)(Width));
                scene.Add(floor);
                if (stairs.Y == Top)
                {
                    needsTopFloor = false;
                }
            }
            if (needsTopFloor)
            {
                Stairs.CustomPlatform topFloor = new Stairs.CustomPlatform(new Vector2(X, Top), (int)(Width));
                scene.Add(topFloor);
            }
        }
        public void AddColumn(Scene scene, float height)
        {
            float halfColWidth = 40;
            if (Stairs.Count > 0) ///
            {
                Stairs = [.. Stairs.OrderByDescending(item => item.Bottom)];
                halfColWidth = (int)(Stairs[0].Width * 0.3f);
            }
            Col = new Column(this, Position + Vector2.UnitX * (Width / 2 - halfColWidth), halfColWidth * 2, height);
            scene.Add(Col);

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

        }
        public void FinalAwake(Scene scene)
        {
            scene.Add(PlayerShade = new PlayerShade(0));
            foreach (Stairs stairs in Stairs) ///
            {
                stairs.Initialize(scene);
            }
            foreach (Stairs stairs in Stairs) ///
            {
                stairs.Disable();
            }
            int barrierWidth = 8;
            if (Stairs.Count > 0) ///
            {
                Col.Depth = Stairs[0].Depth + 1;
                barrierWidth = (int)(Stairs[0].Platform.Width / 2f);
            }
            Col.HidesPlayer = false;
            Safeguards[0] = new InvisibleBarrier(TopLeft, 8, Height);
            Safeguards[1] = new InvisibleBarrier(TopRight - Vector2.UnitX * 8, 8, Height);
            scene.Add(Safeguards);
            foreach (Stairs.CustomPlatform a in scene.Tracker.GetEntities<Stairs.CustomPlatform>())
            {
                Floors.Add(a);
            }
            Floors = [.. Floors.OrderBy(item => item.Y)];
        }
        public override void Update()
        {
            base.Update();
            if (Scene.GetPlayer() is not Player player) return;
            bool prevState = InsideFlag;
            if (player.Right > Right || player.Left < Left || (player.Bottom <= Top))
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
                Enable();
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
                Disable();
                OutsideAlpha = Calc.Approach(OutsideAlpha, 1, Engine.DeltaTime * 3f);
                PlayerShade.Alpha = Calc.Approach(PlayerShade.Alpha, 0, Engine.DeltaTime * 3f);
            }
            foreach (Stairs.CustomPlatform p in Scene.Tracker.GetEntities<Stairs.CustomPlatform>())
            {
                p.InElevator = Col.InElevator;
            }
            foreach (Stairs stairs in Scene.Tracker.GetEntities<Stairs>())
            {
                stairs.InElevator = Col.InElevator;
            }
            WasInside = inside;
        }
        public void Enable()
        {
            Col.Collidable = true;
            Safeguards[0].Collidable = Safeguards[0].Active = Safeguards[1].Collidable = Safeguards[1].Active = true;
            foreach (Stairs stairs in Stairs) ///
            {
                if (!stairs.Enabled)
                {
                    stairs.Enable();
                }
            }
        }
        public void Disable()
        {
            Col.HidesPlayer = false;
            Safeguards[0].Collidable = Safeguards[0].Active = Safeguards[1].Collidable = Safeguards[1].Active = false;
            foreach (Stairs stairs in Stairs) ///
            {
                if (stairs.Enabled)
                {
                    stairs.Disable();
                }
            }
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
                entrance.DrawDoor(entrance.Position, entrance.Width, entrance.Height, OutsideAlpha);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            outsideTarget?.Dispose();
            Stairs.RemoveSelves();
            entrance.RemoveSelf();
            Col.RemoveSelf();
            Safeguards.RemoveSelves();
            Rods.RemoveSelves();
            Ending.RemoveSelf();
            Portal.RemoveSelf();
        }
        public void OnEnterOrExit(Player player)
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