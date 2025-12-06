using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;
using static Celeste.Autotiler;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/WaveBlock")]
    [Tracked]
    public class WaveBlock : Solid
    {
        public TileGrid TileGrid;
        public AnimatedTiles AnimatedTiles;
        public bool ForceUncollidable;
        private float alpha = 0;
        private float extendOffset;
        private VirtualRenderTarget target;
        private float flash = 0;
        private float heightLerp = 1;
        private VertexPositionColor[] vertices;
        private int[] indices = [0, 1, 2, 2, 3, 1];
        private bool verticesEnabled;
        private Color[] colors = new Color[4];
        public bool started;
        private float verticeExtendHeightMult = 0.3f;
        private float vertexAlpha = 1;
        private bool inPosition;
        private FlagList Flag, FlagOnEnd;
        public Vector2 Target;
        public Vector2 Node;
        public char Tile;
        private bool fromAbove;
        public bool BlendIn;
        public float Delay;
        public float Duration;
        public bool Managed = true;
        public bool AtTarget;
        public Vector2 RenderOffset;
        public WaveBlock(EntityData data, Vector2 offset) : this(
            data.Position + offset, data.NodesOffset(offset)[0],
            data.Width, data.Height, data.Char("tiletype"), data.Attr("flag"), data.Attr("flagOnEnd"),
            data.Float("overshoot"), data.Bool("blendIn"), data.Bool("fromAbove"),
            data.HexColor("colorA"), data.HexColor("colorB"), data.Float("delay"), data.Float("duration", 1))
        {
            Managed = false;
        }
        public WaveBlock(Vector2 position, Vector2 node, float width, float height, char tile, string flag, string flagOnEnd, float extendOffset, bool blendIn, bool fromAbove, Color colorA, Color colorB, float delay = 0, float duration = 1) : base(position, width, height, false)
        {
            Duration = duration;
            Flag = new FlagList(flag);
            FlagOnEnd = new FlagList(flagOnEnd);
            Delay = delay;
            BlendIn = blendIn;
            Tile = tile;
            Target = position;
            Node = node;
            this.fromAbove = fromAbove;
            vertices = new VertexPositionColor[4];
            if (fromAbove)
            {
                colors[2] = colors[3] = Color.Transparent;
                colors[0] = colorA;
                colors[1] = colorB;
            }
            else
            {
                colors[0] = colors[1] = Color.Transparent;
                colors[2] = colorA;
                colors[3] = colorB;
            }
            vertices[0] = new VertexPositionColor();
            vertices[1] = new VertexPositionColor();
            vertices[2] = new VertexPositionColor();
            vertices[3] = new VertexPositionColor();
            for (int i = 0; i < colors.Length; i++)
            {
                vertices[i].Color = colors[i];
            }
            Add(new EffectCutout());
            Depth = -13000;
            Tag |= Tags.TransitionUpdate;
            target = VirtualContent.CreateRenderTarget("bridge slice", (int)(Width + 16), (int)(Height + 16));
            Add(new BeforeRenderHook(() =>
            {
                target.SetAsTarget(true);
                Draw.SpriteBatch.StandardBegin();

                RenderAt(RenderOffset);

                if (flash > 0 && heightLerp > 0)
                {
                    float x = 0;
                    float y = 0;
                    if (!fromAbove)
                    {
                        y += Height - Height * heightLerp;
                    }
                    Draw.Rect(new Vector2(x, y), Width, Height * heightLerp, Color.White * flash);
                }
                Draw.SpriteBatch.End();
            }));
            this.extendOffset = extendOffset;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            target?.Dispose();
            target = null;
            if (started && !AtTarget)
            {
                FlagOnEnd.State = true;
            }
        }
        public void Start(float duration)
        {
            if (started) return;
            AtTarget = false;
            alpha = 0;
            Collidable = false;
            started = true;
            Flag.State = true;
            Add(new Coroutine(routine(duration, Delay)));
        }
        public void Snap()
        {
            Components.RemoveAll<Coroutine>();
            FlagOnEnd.State = true;
            started = true;
            AtTarget = true;
            alpha = 1;
            inPosition = true;
            flash = 0;
            heightLerp = 0;
            Collidable = false;
            verticesEnabled = true;
            verticeExtendHeightMult = 0.5f;
            MoveTo(Target);
            Collidable = true;
            foreach (WaveBlock block in Scene.Tracker.GetEntities<WaveBlock>())
            {
                if (block != this && !block.AtTarget && block.Flag)
                {
                    block.Snap();
                }
            }
        }
        private IEnumerator positionLerp(float duration)
        {
            Vector2 extendVector = -Vector2.Normalize(Node - Target);
            Vector2 from = Node;
            Vector2 to = Target;
            Vector2 extended = Target + extendVector * extendOffset;
            for (float i = 0; i < 1; i += Engine.DeltaTime / (duration * 0.7f))
            {
                MoveTo(Vector2.Lerp(from, extended, Ease.CubeOut(i)));
                alpha = Ease.CubeIn(i);
                yield return null;
            }
            alpha = 1;
            inPosition = true;
            from = extended;
            to = Target;
            for (float i = 0; i < 1; i += Engine.DeltaTime / (duration * 0.3f))
            {
                float eased = Ease.SineInOut(i);
                MoveTo(Vector2.Lerp(from, to, eased));
                yield return null;
            }
            MoveTo(to);

        }
        private IEnumerator routine(float duration, float delay)
        {
            MoveTo(Node);
            alpha = 0;
            Collidable = false;
            if (delay > 0)
            {
                yield return delay;
            }
            Add(new Coroutine(positionLerp(duration)));
            while (!inPosition)
            {
                yield return null;
            }
            AtTarget = true;
            FlagOnEnd.State = true;
            alpha = 1;
            Collidable = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.4f)
            {
                flash = i;
                yield return null;
            }
            verticesEnabled = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.7f)
            {
                heightLerp = 1 - Ease.CubeOut(i);
                yield return null;
            }
            flash = 0;
            heightLerp = 0;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            if (BlendIn)
            {
                tilesX += 2;
                RenderOffset = -Vector2.UnitX * 8;
            }
            Generated gen = GFX.FGAutotiler.GenerateBox(Tile, tilesX, tilesY);
            Add(TileGrid = gen.TileGrid);

            AnimatedTiles = gen.SpriteOverlay;
            if (AnimatedTiles != null)
            {
                Add(AnimatedTiles);
            }
            Add(new TileInterceptor(TileGrid, highPriority: false));
            updateVertices();
            Collidable = Flag;
        }
        private bool prevState;
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Flag && !AtTarget)
            {
                Snap();
            }
        }
        public override void Update()
        {
            bool flag = Flag;
            if (flag)
            {
                if (!AtTarget && !Managed && flag && !prevState)
                {
                    Start(Duration);
                }
                base.Update();
            }
            updateVertices();
            prevState = flag;
        }
        private Color[] debugcolors = [Color.Red,Color.Green,Color.Blue,Color.Yellow];
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            for (int i = 0; i < vertices.Length; i++)
            {
                Draw.Point(vertices[i].Position.XY(),debugcolors[i]);
            }
        }
        private void updateVertices()
        {
            if (!fromAbove)
            {
                vertices[0].Position.Y = vertices[1].Position.Y = Bottom - (Height - Height * heightLerp) * verticeExtendHeightMult;
                vertices[2].Position.Y = vertices[3].Position.Y = Top;
            }
            else
            {
                vertices[0].Position.Y = vertices[1].Position.Y = Bottom;
                vertices[2].Position.Y = vertices[3].Position.Y = Top + (Height - Height * heightLerp) * verticeExtendHeightMult;
            }
            vertices[0].Position.X = vertices[2].Position.X = X;
            vertices[1].Position.X = vertices[3].Position.X = X + Width;
            for (int i = 0; i < 4; i++)
            {
                vertices[i].Color = colors[i] * vertexAlpha;
            }
            if (verticesEnabled)
            {
                if (verticeExtendHeightMult > 0.5f) verticeExtendHeightMult = Calc.Approach(verticeExtendHeightMult, 0.5f, Engine.DeltaTime);
                if (vertexAlpha > 0) vertexAlpha = Calc.Approach(vertexAlpha, 0.8f, Engine.DeltaTime);
            }
        }
        public override void Render()
        {
            if (!Flag) return;

            Draw.SpriteBatch.Draw(target, Position, Color.White * alpha);
            if (verticesEnabled && vertexAlpha > 0)
            {
                Draw.SpriteBatch.End();
                Level level = SceneAs<Level>();
                GFX.DrawIndexedVertices(level.Camera.Matrix, vertices, 4, indices, 2,null,BlendState.Additive);
                GameplayRenderer.Begin();
            }
        }
        public void RenderAt(Vector2 position)
        {
            if (BlendIn)
            {
                Level level = Scene as Level;
                if (level.ShakeVector.X < 0f && level.Camera.X <= (float)level.Bounds.Left && base.X <= (float)level.Bounds.Left)
                {
                    TileGrid.RenderAt(position + new Vector2(-3f, 0f), 1, 0);
                    AnimatedTiles?.RenderAt(position + new Vector2(-3f, 0f), 1, 0);
                }

                if (level.ShakeVector.X > 0f && level.Camera.X + 320f >= (float)level.Bounds.Right && base.X + base.Width >= (float)level.Bounds.Right)
                {
                    TileGrid.RenderAt(position + new Vector2(3f, 0f), 1, 0);
                    AnimatedTiles?.RenderAt(position + new Vector2(3f, 0f), 1, 0);
                }

                if (level.ShakeVector.Y < 0f && level.Camera.Y <= (float)level.Bounds.Top && base.Y <= (float)level.Bounds.Top)
                {
                    TileGrid.RenderAt(position + new Vector2(0f, -3f), 1, 0);
                    AnimatedTiles?.RenderAt(position + new Vector2(0f, -3f), 1, 0);
                }

                if (level.ShakeVector.Y > 0f && level.Camera.Y + 180f >= (float)level.Bounds.Bottom && base.Y + base.Height >= (float)level.Bounds.Bottom)
                {
                    TileGrid.RenderAt(position + new Vector2(0f, 3f), 1, 0);
                    AnimatedTiles?.RenderAt(position + new Vector2(0f, 3f), 1, 0);
                }
                TileGrid.RenderAt(position, 1, 0);
                AnimatedTiles.RenderAt(position, 1, 0);
            }
            else
            {
                TileGrid.RenderAt(position, 0, 0);
                AnimatedTiles.RenderAt(position, 0, 0);
            }
        }
    }
}