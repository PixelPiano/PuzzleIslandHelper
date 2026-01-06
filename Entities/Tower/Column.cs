using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [Tracked]
    public class Column : Entity
    {

        [CustomEntity("PuzzleIslandHelper/TowerElevator")]
        [Tracked]
        public class Elevator : Entity
        {
            [Tracked]
            public class InteractComponent : Component
            {
                public Action<Player> OnExit;
                public Action<Player> OnEnter;
                public InteractComponent(Action<Player> onEnter, Action<Player> onExit) : base(true, false)
                {
                    OnEnter = onEnter;
                    OnExit = onExit;
                }
            }
            [Tracked]
            internal class Entrance : Entity
            {
                public bool Enabled;
                public void Enable()
                {
                    Enabled = true;
                }
                internal class DoorsComponent : GraphicsComponent
                {
                    private int offset;
                    public float PercentOpened;
                    public float Width, Height;
                    public DoorsComponent(float width, float height) : base(true)
                    {
                        Width = width;
                        Height = height;
                    }
                    public override void Render()
                    {
                        base.Render();
                        int maxWidth = (int)(Width / 2);
                        int width = (int)(maxWidth * (1 - PercentOpened));
                        Vector2 pos = RenderPosition;
                        int x = (int)pos.X;
                        int y = (int)pos.Y;
                        int height = (int)Height;
                        int centerX = (int)(x + Width / 2);
                        if (width > 2)
                        {
                            Rectangle left = new Rectangle(x, y, width - offset, height);
                            Rectangle right = new Rectangle(centerX + maxWidth - width + offset, y, width - offset, height);
                            Draw.Rect(left, Color.Gray);
                            Draw.Rect(right, Color.Gray);
                            Draw.HollowRect(left, Color.DarkGray);
                            Draw.HollowRect(right, Color.DarkGray);
                        }
                    }
                    public IEnumerator OpenRoutine()
                    {
                        int sign = 1;
                        for (int i = 0; i < 10; i++)
                        {
                            offset = Math.Max(0, sign);
                            sign *= -1;
                            yield return null;
                        }
                        offset = 0;
                        yield return 0.2f;
                        for (float i = 0; i < 1; i += Engine.DeltaTime)
                        {
                            PercentOpened = Ease.SineInOut(i);
                            yield return null;
                        }
                        PercentOpened = 1;
                    }
                    public IEnumerator CloseRoutine()
                    {
                        for (float i = 0; i < 1; i += Engine.DeltaTime)
                        {
                            PercentOpened = Ease.SineInOut(1 - i);
                            yield return null;
                        }
                        PercentOpened = 0;
                    }
                }
                public TalkComponent Talk;
                internal DoorsComponent Doors;
                public Action<Player> OnInteract;
                public Elevator Parent;
                public Entrance(Elevator parent, Vector2 position, float width, float height, Action<Player> interact) : base(position)
                {
                    Collider = new Hitbox(width, height);
                    OnInteract = interact;
                    Rectangle bounds = new Rectangle(0, 0, (int)Width, (int)Height);
                    Add(Talk = new TalkComponent(bounds, Collider.HalfSize.XComp(), Interact));
                    Talk.PlayerMustBeFacing = false;
                    Depth = 1;
                    Add(Doors = new DoorsComponent(Width, Height));
                    Parent = parent;
                }
                public override void Update()
                {
                    base.Update();
                    Talk.Enabled = Enabled;
                }
                public void SnapClosed()
                {
                    Doors.PercentOpened = 0;
                }
                public void PrepareForExit()
                {
                    Depth = -1;
                    SnapClosed();
                }
                public void PrepareForEntrance()
                {
                    Depth = 1;
                    SnapClosed();
                }
                public void Interact(Player player)
                {
                    OnInteract.Invoke(player);
                }
                public IEnumerator EnterRoutine(Player player, Entrance other)
                {
                    other.PrepareForExit();
                    PrepareForEntrance();
                    yield return new SwapImmediately(Doors.OpenRoutine());
                    yield return 0.4f;
                    yield return player.DummyWalkToExact((int)CenterX);
                    Parent.Shade.Fade(1, 1);
                    yield return 0.2f;
                    Depth = -1;
                    yield return new SwapImmediately(Doors.CloseRoutine());
                }
                public IEnumerator ExitRoutine(Player player, Entrance other)
                {
                    other.PrepareForEntrance();
                    PrepareForExit();
                    yield return new SwapImmediately(Doors.OpenRoutine());
                    yield return 0.4f;
                    Parent.Shade.Fade(Parent.Shade.Alpha, 1);
                    yield return 0.4f;
                    Depth = 1;
                    yield return new SwapImmediately(Doors.CloseRoutine());
                }
            }

            public class UI : Entity
            {
                public class Arrow : Entity
                {
                    private VirtualRenderTarget target, outline;
                    public Sprite Sprite;
                    public bool Selected;
                    public float Alpha;
                    public float OutlineAlpha = 0;
                    private Vector2 offset;
                    public bool Flip
                    {
                        get => flip;
                        set
                        {
                            Sprite.Effects = value ? SpriteEffects.FlipVertically : SpriteEffects.None;
                            flip = value;
                        }
                    }
                    private bool flip;
                    public Arrow(Vector2 position) : base(position)
                    {
                        Depth = int.MinValue;
                        Position = position;
                        target = VirtualContent.CreateRenderTarget("arrow", 16, 16);
                        outline = VirtualContent.CreateRenderTarget("outline", 16, 16);
                        Add(new BeforeRenderHook(() =>
                        {
                            outline.SetAsTarget(true);
                            if (OutlineAlpha > 0)
                            {
                                Draw.SpriteBatch.Begin();
                                Vector2 prev = Sprite.RenderPosition;
                                Sprite.RenderPosition = Vector2.Zero;
                                Sprite.DrawSimpleOutline();
                                Sprite.RenderPosition = prev;
                                Draw.SpriteBatch.End();
                            }

                            target.SetAsTarget(true);
                            Draw.SpriteBatch.Begin();
                            Draw.SpriteBatch.Draw(outline, Vector2.Zero, Color.Black * OutlineAlpha);
                            Sprite.RenderAt(Vector2.Zero);
                            Draw.SpriteBatch.End();
                        }));
                    }
                    public override void Added(Scene scene)
                    {
                        base.Added(scene);
                        Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/tower/button/normal/");
                        Sprite.AddLoop("idleFrozen", "idle", 0.1f, 0);
                        Sprite.AddLoop("idle", "idle", 0.1f);
                        Sprite.AddLoop("pressed", "pressed", 0.1f);
                        Sprite.Add("press", "press", 0.1f, "pressed");
                        Add(Sprite);
                        Sprite.Play("idle");
                    }
                    public IEnumerator OffsetRoutine(float offsetFrom, float offsetTo, float time, bool alphaIn)
                    {
                        for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                        {
                            offset.X = (Calc.LerpClamp(offsetFrom, offsetTo, Ease.SineInOut(i)));
                            Alpha = alphaIn ? Ease.SineInOut(i) : Ease.SineInOut(1 - i);
                            yield return null;
                        }
                        offset = Vector2.UnitX * offsetTo;
                        Alpha = alphaIn ? 1 : 0;
                    }

                    public void Select()
                    {
                        Selected = true;
                        Sprite.Play("idle");
                    }
                    public void Deselect()
                    {
                        Selected = false;
                        Sprite.Play("idleFrozen");
                    }
                    public void Confirm()
                    {
                        Selected = false;
                        Sprite.Play("press");
                    }
                    public override void Render()
                    {
                        if (Alpha > 0)
                        {
                            Draw.SpriteBatch.Draw(target, Position + offset, Color.White * Alpha);
                        }
                    }
                    public override void Removed(Scene scene)
                    {
                        base.Removed(scene);
                        target.Dispose();
                        outline.Dispose();
                    }
                }
                public Arrow Button;
                public bool Finished;
                public bool MoveConfirmed;
                public Elevator Parent;
                public UI(Elevator parent, Vector2 position, float height) : base(position)
                {
                    Parent = parent;
                    Collider = new Hitbox(16, height);
                }
                public override void Added(Scene scene)
                {
                    base.Added(scene);
                    Button = new Arrow(Position);
                    scene.Add(Button);
                    Button.Alpha = 0;
                }
                public override void Removed(Scene scene)
                {
                    base.Removed(scene);
                    Button.RemoveSelf();
                }
                public IEnumerator Routine(bool atTop)
                {
                    Entrance e = atTop ? Parent.Entrances[0] : Parent.Entrances[1];
                    Position.Y = e.Top;
                    Position.X = e.Left - 32;
                    Button.Position = Position;
                    Finished = false;
                    MoveConfirmed = false;
                    Button.Flip = atTop;
                    yield return Button.OffsetRoutine(-32, 0, 1, true);
                    Button.Select();
                    while (true)
                    {
                        if (Input.MenuConfirm.Pressed)
                        {
                            Button.Confirm();
                            MoveConfirmed = true;
                            yield return 1f;
                            break;
                        }
                        if (Input.MenuCancel.Pressed)
                        {
                            Button.Deselect();
                            break;
                        }
                        yield return null;
                    }
                    yield return Button.OffsetRoutine(0, -32, 1, false);
                    Finished = true;
                }
            }
            private bool InRoutine;
            public PlayerShade Shade;
            public Vector2 Start, End;
            public JumpThru Platform;
            public bool Inside;
            private UI ui;
            internal Entrance[] Entrances = new Entrance[2];
            public InvisibleBarrier[] Barriers = new InvisibleBarrier[3];
            public Elevator(EntityData data, Vector2 offset) : base(data.Position + offset)
            {
                Vector2[] nodes = data.NodesWithPosition(offset);
                Start = nodes[0];
                End = nodes[1];

                Entrances[0] = new Entrance(this, Start, data.Width, data.Height, TopInteract);
                Entrances[1] = new Entrance(this, End, data.Width, data.Height, BottomInteract);
            }
            public void Enable()
            {
                Entrances[0].Enable();
                Entrances[1].Enable();
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);

                if (scene.Tracker.GetEntity<Tower>() is Tower t)
                {
                    Tower = t;
                    scene.Add(Entrances);
                    ui = new UI(this, Position, 6 * 8);
                    scene.Add(ui);
                    t.AddColumn(scene, (End.Y - t.Y));
                    float height = End.Y - Start.Y + Entrances[0].Height + Entrances[1].Height;
                    Barriers[0] = new InvisibleBarrier(new Vector2(t.Col.Left - 8, Entrances[0].Top), 8, height);
                    Barriers[1] = new InvisibleBarrier(new Vector2(t.Col.Right, Entrances[0].Top), 8, height);
                    Barriers[2] = new InvisibleBarrier(Barriers[0].TopLeft - Vector2.UnitY * 8, Barriers[1].Right - Barriers[0].Left, 8);
                    scene.Add(Barriers);
                    Platform = new JumpThru(new Vector2(t.Col.X, Entrances[0].Bottom), (int)t.Col.Width, true);
                    scene.Add(Platform);
                    Platform.Collidable = false;
                    t.FinalAwake(scene);
                }
                else
                {
                    RemoveSelf();
                }
            }
            public override void Update()
            {
                base.Update();
                BarrierState(Tower.Col.InElevator);

                if (!Tower.CollideCheck<Player>() || !InRoutine)
                {
                    Shade.Alpha = Calc.Approach(Shade.Alpha, 0, Engine.DeltaTime);
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Platform.RemoveSelf();
                Entrances.RemoveSelves();
                Barriers.RemoveSelves();
                ui.RemoveSelf();
                Shade.RemoveSelf();
            }
            public void TopInteract(Player player)
            {
                Interact(true, player);
            }
            public void BottomInteract(Player player)
            {
                Interact(false, player);
            }
            public void Interact(bool top, Player player)
            {
                Add(new Coroutine(Routine(top, player)));
            }
            public IEnumerator EnterRoutine(bool top, Player player)
            {
                Entrances[0].PrepareForEntrance();
                Entrances[1].PrepareForEntrance();
                Platform.Collidable = true;
                if (player.CollideFirst<Entrance>() is var e)
                {
                    player.Bottom = e.Bottom;
                    Platform.MoveToY(e.Bottom);
                }
                foreach (InteractComponent c in Scene.Tracker.GetComponents<InteractComponent>())
                {
                    c.OnEnter?.Invoke(player);
                }
                Entrance entrance = top ? Entrances[0] : Entrances[1];
                Entrance other = top ? Entrances[1] : Entrances[0];
                yield return entrance.EnterRoutine(player, other);
            }
            public IEnumerator ExitRoutine(bool top, Player player)
            {
                Entrances[0].PrepareForExit();
                Entrances[1].PrepareForExit();
                Platform.Collidable = false;
                if (player.CollideFirst<Entrance>() is var e)
                {
                    player.Bottom = e.Bottom;
                    Platform.MoveToY(e.Bottom);
                }
                foreach (InteractComponent c in Scene.Tracker.GetComponents<InteractComponent>())
                {
                    c.OnExit?.Invoke(player);
                }
                Entrance entrance = top ? Entrances[0] : Entrances[1];
                Entrance other = top ? Entrances[1] : Entrances[0];
                yield return entrance.ExitRoutine(player, other);
            }
            public IEnumerator MoveRoutine(bool moveToTop, Player player)
            {
                player.DisableMovement();
                player.ForceCameraUpdate = true;
                float y = moveToTop ? Entrances[0].Bottom : Entrances[1].Bottom;
                while ((int)Platform.Top != (int)y)
                {
                    float prev = Platform.Top;
                    Platform.MoveTowardsY(y, 70f * Engine.DeltaTime);
                    player.Hair.MoveHairBy(Vector2.UnitY * (Platform.Top - prev));
                    yield return null;
                }
                yield return 1;
            }
            public IEnumerator Routine(bool top, Player player)
            {
                InRoutine = true;
                player.DisableMovement();
                player.ForceCameraUpdate = true;
                Inside = false;
                yield return new SwapImmediately(EnterRoutine(top, player));
                Inside = true;
                while (true)
                {
                    yield return new SwapImmediately(ui.Routine(top));
                    if (ui.MoveConfirmed)
                    {
                        yield return new SwapImmediately(MoveRoutine(!top, player));
                        yield return ExitRoutine(!top, player);
                        break;
                    }
                    else
                    {
                        yield return ExitRoutine(top, player);
                        break;
                    }
                }
                player.EnableMovement();
                Inside = false;
                InRoutine = false;
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                scene.Add(Shade = new PlayerShade(0));
            }
            public Tower Tower;
            public void BarrierState(bool value)
            {
                foreach (var b in Barriers)
                {
                    b.Active = b.Collidable = value;
                }
            }
        }

        [Tracked]
        public class VertexGradient : GraphicsComponent
        {
            private VertexPositionColor[] ColVerts = new VertexPositionColor[6];
            private static readonly int[] columnIndices = [0, 1, 3, 3, 1, 4, 1, 2, 4, 4, 2, 5];
            public int Width
            {
                get => Target.Width;
                set
                {
                    Target.Width = value;
                    rendered = false;
                }
            }
            public int Height
            {
                get => Target.Height;
                set
                {
                    Target.Height = value;
                    rendered = false;
                }
            }
            public Color Edge = Color.Black;
            public Color Middle = Color.White;
            public VirtualRenderTarget Target;
            private bool rendered;
            public bool RenderOnce;
            public float EdgeLerp = 0; //
            public float MiddleLerp = 0; //
            public Color Edge2; //
            public Color Middle2; //
            public float MiddleDist; //
            public float Alpha = 1;

            private BeforeRenderHook Hook;
            public VertexGradient(Vector2 position, int width, int height, Color edgeColor, Color middleColor) : base(true)
            {
                Position = position;
                Target = VirtualContent.CreateRenderTarget("ColumnVertexComponentTarget", width, height);
                Edge = edgeColor;
                Middle = middleColor;
            }
            public VertexGradient(Vector2 position, int width, int height) : base(true)
            {
                Position = position;
                Target = VirtualContent.CreateRenderTarget("ColumnVertexComponentTarget", width, height);
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                UpdateVertices(ColVerts, Vector2.Zero, Width, Height, Edge, Middle, Alpha);
                entity.Add(Hook = new BeforeRenderHook(() =>
                {
                    if (!RenderOnce || !rendered)
                    {
                        Target.SetAsTarget(Color.White);
                        GFX.DrawIndexedVertices(Matrix.Identity, ColVerts, 6, columnIndices, 4);
                        rendered = true;
                    }
                }));
            }
            public override void Removed(Entity entity)
            {
                base.Removed(entity);
                Hook.RemoveSelf();
                Target?.Dispose();
            }
            public override void Update()
            {
                base.Update();
                UpdateVertices(ColVerts, Vector2.Zero, Width, Height, Edge, Middle, Alpha);
            }
            public override void Render()
            {
                base.Render();
                if (Alpha <= 0) return;
                if (!RenderOnce || rendered)
                {
                    Draw.SpriteBatch.Draw(Target, RenderPosition, Color.White * Alpha);
                }
            }
            public static void CreateData(Vector2 position, float width, float height, Color edge, Color middle, out VertexPositionColor[] vertices, out int[] indices)
            {
                vertices = new VertexPositionColor[6];
                UpdateVertices(vertices, position, width, height, edge, middle, 1);
                indices = [.. columnIndices];
            }
            public static void DrawGradient(Matrix matrix, Vector2 position, float width, float height, Color edge, Color middle)
            {
                CreateData(position, width, height, edge, middle, out var verts, out var ind);
                GFX.DrawIndexedVertices(matrix, verts, 6, ind, 4);
            }
            public void DrawVertices(Matrix matrix)
            {
                GFX.DrawIndexedVertices(matrix, ColVerts, 6, columnIndices, 4);
            }
            public static void UpdateVertices(VertexPositionColor[] vertices, Vector2 offset, float width, float height, Color edge, Color middle, float alpha)
            {
                vertices[0].Position.X = vertices[3].Position.X = offset.X;
                vertices[1].Position.X = vertices[4].Position.X = offset.X + width / 2;
                vertices[2].Position.X = vertices[5].Position.X = offset.X + width;
                vertices[0].Position.Y = vertices[1].Position.Y = vertices[2].Position.Y = offset.Y;
                vertices[3].Position.Y = vertices[4].Position.Y = vertices[5].Position.Y = offset.Y + height;
                vertices[0].Color = vertices[2].Color = vertices[3].Color = vertices[5].Color = edge * alpha;
                vertices[1].Color = vertices[4].Color = middle * alpha;
            }
        }
        [Tracked]
        public class SigilCode : Entity
        {
            public string Code;
            public string Input = "";
            public Action OnValid;
            public SigilCode(string code, Action onValid) : base()
            {
                Code = code;
                OnValid = onValid;
            }
            public void AddInput(char key)
            {
                Input += key;
                if (Input.Length >= Code.Length)
                {
                    string check = Input.Substring(Math.Max(Input.Length - Code.Length, 0), Code.Length);
                    if (check == Code)
                    {
                        OnValid.Invoke();
                    }
                }
            }
        }
        private class Cutscene : CutsceneEntity
        {
            public Tower Tower;
            public Vector2 CameraSave;
            public Cutscene(Tower tower) : base()
            {
                Tower = tower;
            }
            public override void OnBegin(Level level)
            {
                Add(new Coroutine(cutscene()));
                CameraSave = level.Camera.Position;
            }
            private IEnumerator cutscene()
            {
                Level.GetPlayer()?.DisableMovement();
                Elevator.Entrance e = Tower.Col.elevator.Entrances[0];
                yield return CameraTo(e.Center - new Vector2(160, 90), 1, Ease.CubeOut, 1);
                yield return 1;
                Tower.Col.elevator.Enable();
                yield return CameraTo(CameraSave, 1, Ease.CubeOut, 1);
                yield return null;
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                Level.GetPlayer()?.EnableMovement();
                Tower.Col.elevator.Enable();
                level.Camera.Position = CameraSave;
            }
        }
        public FlagData ColumnSolved = new FlagData("ColumnPuzzleSolved");
        public LightOcclude Occlude;
        public Tower Parent;
        public Elevator elevator;

        public PlayerHider PlayerHider;
        public VertexGradient Graphic, CoverGraphic;
        public Entity Cover;
        public SigilCode Code;
        public float FadeAlpha;
        public bool InElevator;
        public float HeightExtend;
        public bool HidesPlayer
        {
            get => !PlayerHider.Disabled;
            set
            {
                Occlude.Alpha = value ? 1 : 0;
                PlayerHider.Disabled = !value;
            }
        }
        public bool HidingDisabled => PlayerHider.Disabled;
        private float prevPlayerLightAlpha = 1;
        public float TrueBottom;

        public Column(Tower parent, Vector2 position, float width, float height) : base(position)
        {
            AddTag(Tags.TransitionUpdate);
            Parent = parent;
            Collider = new Hitbox(width, height);
            Add(Occlude = new LightOcclude());
            Occlude.Alpha = 0;
            Add(Graphic = new VertexGradient(-Vector2.UnitY * 40, (int)width, (int)height + 40));
            PlayerHider = new PlayerHider(Vector2.Zero, (int)Width, (int)Height);
            Add(new Elevator.InteractComponent(OnEnterColumn, OnExitColumn));
        }
        public void OnEnterColumn(Player p)
        {
            SceneAs<Level>().SolidTiles.Collidable = false;
            Audio.Play("event:/PianoBoy/Machines/ButtonPressC");
            InElevator = true;
            CoverGraphic.Alpha = 0;
            CoverGraphic.Visible = true;
            HidesPlayer = false;
            prevPlayerLightAlpha = p.Light.Alpha;
        }
        public void OnExitColumn(Player p)
        {
            SceneAs<Level>().SolidTiles.Collidable = true;
            InElevator = false;
            CoverGraphic.Alpha = 0.5f;
            HidesPlayer = true;
        }


        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            float height = Height + 40;
            if (scene.Tracker.GetEntity<Elevator>() is Elevator elevator)
            {
                height = Height + 40 + elevator.Entrances[1].Bottom - Bottom;
            }
            Cover = new Entity(Position);
            Cover.Depth = -3;
            CoverGraphic = new VertexGradient(Vector2.UnitY * -40, (int)Width, (int)height);
            CoverGraphic.Alpha = 0;
            scene.Add(Cover);
            Cover.Add(CoverGraphic);

            Add(PlayerHider);
            scene.Add(Code = new SigilCode("111", OnCode));
            HidesPlayer = !Parent.Inside;

            if (scene.GetPlayer() is Player player)
            {
                prevPlayerLightAlpha = player.Light.Alpha;
            }
            foreach (Sigil sigil in Scene.Tracker.GetEntities<Sigil>())
            {
                if (CollideCheck(sigil))
                {
                    sigil.Depth = sigil.Behind ? Depth + 1 : Depth - 1;
                }
            }
        }
        public override void Render()
        {
            base.Render();
            /*            if (ColumnSolved)
                        {
                            Rectangle r = Talk.Bounds;
                            r.X += (int)X;
                            r.Y += (int)Y;
                            Draw.Rect(r, Color.Black);
                            if (Talk2 != null)
                            {
                                Rectangle r2 = Talk2.Bounds;
                                r2.X += (int)X;
                                r2.Y += (int)Y;
                                Draw.Rect(r2, Color.Black);
                            }
                        }*/
            if (FadeAlpha > 0)
            {
                Camera c = SceneAs<Level>().Camera;
                float top = Math.Max(c.Y, Top);
                float bottom = Math.Min(c.Y + 180, Bottom);
                float height = bottom - top;

                Draw.Rect(new Vector2(c.X, top), Left - c.X, height, Color.Black * FadeAlpha);
                Draw.Rect(new Vector2(Right, top), (c.X + 320) - Right, height, Color.Black * FadeAlpha);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Cover.RemoveSelf();
            if (scene.GetPlayer() is Player player)
            {
                player.Light.Alpha = prevPlayerLightAlpha;
            }
            (scene as Level).SolidTiles.Collidable = true;
        }
        public override void Update()
        {
            if (HidesPlayer)
            {
                PlayerHider.Disabled = false;

            }
            base.Update();
            foreach (Sigil key in Scene.Tracker.GetEntities<Sigil>())
            {
                key.Collidable = key.Behind == HidesPlayer && Parent.Inside;
            }
            if (Scene.GetPlayer() is Player player)
            {
                if (InElevator)
                {
                    CoverGraphic.Alpha = Calc.Approach(CoverGraphic.Alpha, 0.5f, Engine.DeltaTime);
                    player.Light.Alpha = Calc.Approach(player.Light.Alpha, 0, Engine.DeltaTime);
                }
                else
                {
                    CoverGraphic.Alpha = Calc.Approach(CoverGraphic.Alpha, 0, Engine.DeltaTime);
                    player.Light.Alpha = Calc.Approach(player.Light.Alpha, prevPlayerLightAlpha, Engine.DeltaTime);
                }
                Occlude.Visible = HidesPlayer && Parent.Inside;
                /*                Talk.Enabled = Parent.Inside && ColumnSolved;
                                if (Talk2 != null)
                                {
                                    Talk2.Enabled = ColumnSolved && player.Bottom <= TrueBottom;
                                }*/

                FadeAlpha = Calc.Approach(FadeAlpha, InElevator ? 1 : 0, Engine.DeltaTime);
            }
        }
        public void OnCode()
        {
            Scene.Add(new Cutscene(Parent));
            Code.Input = "";
            foreach (Sigil key in Scene.Tracker.GetEntities<Sigil>())
            {
                key.Deactivate(false);
            }
        }

    }
}