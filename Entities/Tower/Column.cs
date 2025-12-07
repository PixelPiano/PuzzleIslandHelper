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
    public class Column : Entity
    {
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
        public override void Render()
        {
            base.Render();
            if (ColumnSolved)
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
            }
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
            public Stairs Tower;
            public Cutscene(Stairs tower) : base()
            {
                Tower = tower;
            }
            public override void OnBegin(Level level)
            {
                Add(new Coroutine(cutscene()));
            }
            private IEnumerator cutscene()
            {
                yield return null;
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
            }
        }
        public FlagData ColumnSolved = new FlagData("ColumnPuzzleSolved");
        public LightOcclude Occlude;
        public JumpThru Elevator;
        public Tower Parent;
        public DotX3 Talk, Talk2;
        public bool InsideTower;
        public InvisibleBarrier[] Barriers = new InvisibleBarrier[3];
        public PlayerHider PlayerHider;
        public VertexGradient Graphic, CoverGraphic;
        private VertexGradient extend;
        public Entity Cover;
        public SigilCode Code;
        public float FadeAlpha;
        public bool InColumn;
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
        private float occludeAlpha => Occlude.Alpha;
        public bool hidingDisabled => PlayerHider.Disabled;
        private float hidingAlpha => PlayerHider.Alpha;
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

        }
        public void Interact(Player p)
        {
            Input.Dash.ConsumePress();
            if (!InColumn)
            {
                SceneAs<Level>().SolidTiles.Collidable = false;
                Audio.Play("event:/PianoBoy/Machines/ButtonPressC");
                InColumn = true;
                Elevator.MoveToY(p.Bottom);
                CoverGraphic.Alpha = 0;
                CoverGraphic.Visible = true;
                HidesPlayer = false;
                prevPlayerLightAlpha = p.Light.Alpha;
                EnableBarriers();
            }
            else
            {
                SceneAs<Level>().SolidTiles.Collidable = true;
                InColumn = false;
                CoverGraphic.Alpha = 0.5f;
                HidesPlayer = true;
                p.Bottom = Parent.Stairs[^1].TopPlatform.Top;
                DisableBarriers();
            }
            Elevator.Collidable = InColumn;
            foreach (var s in Parent.Stairs)
            {
                s.DisablePlatform = InColumn;
            }
        }
        public void EnableBarriers()
        {
            foreach (InvisibleBarrier barrier in Barriers)
            {
                barrier.Collidable = true;
                barrier.Active = true;
            }
        }
        public void DisableBarriers()
        {
            foreach (InvisibleBarrier barrier in Barriers)
            {
                barrier.Collidable = false;
                barrier.Active = false;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Elevator = new JumpThru(BottomLeft, (int)Width, true);
            Elevator.Collidable = false;
            scene.Add(Elevator);


            Cover = new Entity(Position);
            Cover.Depth = -3;
            CoverGraphic = new VertexGradient(Vector2.UnitY * -40, (int)Width, (int)Height + 40);
            CoverGraphic.Alpha = 0;
            Cover.Add(CoverGraphic);
            scene.Add(Cover);

            Add(PlayerHider);
            scene.Add(Code = new SigilCode("111", OnCode));
            HidesPlayer = !InsideTower;
        }

        public void OnCode()
        {
            //Scene.Add(new Cutscene(Parent));
            Code.Input = "";
            foreach (Sigil key in Scene.Tracker.GetEntities<Sigil>())
            {
                key.Deactivate(false);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add(Talk = new DotX3(Width / 2 - Width / 6, -40, Width / 3, 40, Vector2.UnitX * Width / 6, Interact));
            float height = Height;
            if (Marker.TryFind("tunnelStart", out Vector2 position))
            {
                SolidTiles tiles = SceneAs<Level>().SolidTiles;
                while (!tiles.CollidePoint(position + Vector2.UnitY))
                {
                    position.Y++;
                }
                Add(Talk2 = new DotX3(Width / 2 - Width / 6, position.Y - Y - 40, Width / 3, 40, Vector2.UnitX * Width / 6, Interact));
                height = position.Y - Top;
                TrueBottom = Top + height;
                while (!tiles.CollidePoint(position - Vector2.UnitY))
                {
                    position.Y--;
                }
                Add(extend = new VertexGradient(new Vector2(X, position.Y), (int)Width, (int)Height));
            }
            Barriers[0] = new InvisibleBarrier(Position - new Vector2(7, 40), 8, height + 40);
            Barriers[1] = new InvisibleBarrier(TopRight - new Vector2(1, 40), 8, height + 40);
            Barriers[2] = new InvisibleBarrier(Barriers[0].TopLeft - Vector2.UnitY * 8, Barriers[1].Right - Barriers[0].Left, 8);
            scene.Add(Barriers);
            DisableBarriers();
            if (scene.GetPlayer() is Player player)
            {
                prevPlayerLightAlpha = player.Light.Alpha;
            }
            foreach (Sigil sigil in Scene.Tracker.GetEntities<Sigil>())
            {
                if (CollideCheck(sigil))
                {
                    if (sigil.Behind)
                    {
                        sigil.Depth = Depth + 1;
                    }
                    else
                    {
                        sigil.Depth = Depth - 1;
                    }
                }
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Elevator.RemoveSelf();
            Cover.RemoveSelf();
            Barriers.RemoveSelf();
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
                key.Collidable = key.Behind == HidesPlayer && InsideTower;
            }
            if (Scene.GetPlayer() is Player player)
            {
                if (InColumn)
                {
                    CoverGraphic.Alpha = Calc.Approach(CoverGraphic.Alpha, 0.5f, Engine.DeltaTime);
                    player.Light.Alpha = Calc.Approach(player.Light.Alpha, 0, Engine.DeltaTime);
                }
                else
                {
                    CoverGraphic.Alpha = Calc.Approach(CoverGraphic.Alpha, 0, Engine.DeltaTime);
                    player.Light.Alpha = Calc.Approach(player.Light.Alpha, prevPlayerLightAlpha, Engine.DeltaTime);
                }
                if (extend != null)
                {
                    extend.Alpha = CoverGraphic.Alpha;
                }
                Occlude.Visible = HidesPlayer && InsideTower;
                Talk.Enabled = InsideTower && ColumnSolved && player.Bottom <= Y;
                if (Talk2 != null)
                {
                    Talk2.Enabled = ColumnSolved && player.Bottom <= TrueBottom;
                }

                FadeAlpha = Calc.Approach(FadeAlpha, InColumn ? 1 : 0, Engine.DeltaTime);
                if (player.IsRiding(Elevator))
                {
                    if (Input.MoveX == 0)
                    {
                        float move = Input.MoveY * 50f * Engine.DeltaTime;
                        float y = Calc.Clamp(Elevator.Y + move, Parent.Y, TrueBottom);
                        float prev = Elevator.Y;
                        Elevator.MoveToY(y);
                        if (y != prev)
                        {
                            player.Hair.MoveHairBy(Vector2.UnitY * (y - prev));
                        }
                    }
                }
            }
        }

    }
}