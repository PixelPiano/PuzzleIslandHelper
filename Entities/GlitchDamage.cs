using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/GlitchDamage")]
    [Tracked]
    public class GlitchDamage : Entity
    {
        public FlagList Flag;
        public Color[] AllowedColors;
        public float MaxSize, MinSize;
        public float MaxInterval, MinInterval;
        public float OffsetChance;
        public float MaxOffset = 1;
        public float MinOffset;
        public float XOffsetMult = 1;
        public float YOffsetMult = 1;
        public float SpawnChance = 1;
        public int CellSize;
        public VirtualMap<Block> Blocks;
        public float VisualLeft, VisualTop, VisualRight, VisualBottom;
        public bool OnScreen;
        public class Block : GraphicsComponent
        {
            public float Width, Height, Timer;
            private int row, column;
            private BlendState next;
            private float blendStateTimer;
            public Block(Vector2 position, float width, float height, float duration, Color color, int row, int column) : base(true)
            {
                Position = position;
                Width = width;
                Height = height;
                Timer = duration;
                Color = color;
                this.row = row;
                this.column = column;
            }
            public override void Update()
            {
                base.Update();
                if (blendStateTimer <= 0 && Calc.Random.Chance(0.2f))
                {
                    next = Calc.Random.Choose(BlendState.AlphaBlend, BlendState.Additive, BlendState.Opaque, BlendState.NonPremultiplied);
                    blendStateTimer = Calc.Random.Range(0.05f, 1);
                }
                if (Timer > 0)
                {
                    Timer -= Engine.DeltaTime;
                    if (Timer <= 0)
                    {
                        NotifyAndRemoveSelf();
                    }
                }
                if (blendStateTimer < 0)
                {
                    blendStateTimer -= Engine.DeltaTime;
                    if (blendStateTimer <= 0)
                    {
                        next = null;
                    }
                }
            }
            public void NotifyAndRemoveSelf()
            {
                if (Entity is GlitchDamage entity)
                {
                    entity.RemoveBlockFromMap(row, column);
                }
                RemoveSelf();
            }
            public override void Render()
            {
                base.Render();
                if (next != null)
                {
                    BlendState prev = Engine.Graphics.GraphicsDevice.BlendState;
                    Engine.Graphics.GraphicsDevice.BlendState = next;
                    Draw.Rect(RenderPosition, Width, Height, Color);
                    Engine.Graphics.GraphicsDevice.BlendState = prev;
                }
                else
                {
                    Draw.Rect(RenderPosition, Width, Height, Color);
                }
            }
        }
        public GlitchDamage(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = -5;
            Collider = new Hitbox(data.Width, data.Height);
            Flag = data.FlagList("flag");
            MaxSize = data.Float("maxSize");
            MinSize = data.Float("minSize");
            MaxInterval = data.Float("maxInterval");
            MinInterval = data.Float("minInterval");
            OffsetChance = data.Float("offsetChance");
            MaxOffset = data.Float("maxOffset");
            MinOffset = data.Float("minOffset");
            XOffsetMult = data.Float("offsetMultX", 1);
            YOffsetMult = data.Float("offsetMultY", 1);
            string c = data.Attr("colors", "FFFFFFFF");
            string[] array = c.Replace(" ", "").Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
            List<Color> validColors = [];
            if (array.Length > 0)
            {
                AllowedColors = new Color[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Length == 6)
                    {
                        AllowedColors[i] = Calc.HexToColor(array[i]);
                    }
                    else
                    {
                        AllowedColors[i] = Calc.HexToColorWithAlpha(array[i]);
                    }
                }
            }
            else
            {
                AllowedColors = [Color.White];
            }
            CellSize = (int)(Math.Min(data.Width, data.Height) / MinSize);
            Blocks = new VirtualMap<Block>((int)((float)data.Width / CellSize), (int)((float)data.Height / CellSize));

            Add(new PlayerCollider(p =>
            {
                if (Flag && !p.Dead) p.Die(Vector2.Zero);
            }));
        }
        public void RemoveBlockFromMap(int row, int column)
        {
            Blocks[column, row] = null;
        }
        public void Populate()
        {
            VisualLeft = Left;
            VisualTop = Top;
            VisualRight = Right;
            VisualBottom = Bottom;
            for (int i = 0; i < Blocks.Rows; i++)
            {
                for (int j = 0; j < Blocks.Columns; j++)
                {
                    if (Blocks[i, j] == null)
                    {
                        if (SpawnChance >= 1 || Calc.Random.Chance(SpawnChance))
                        {
                            float width = Calc.Random.Range(MinSize, MaxSize);
                            float height = Calc.Random.Range(MinSize, MaxSize);
                            Vector2 position = new Vector2(
                                j * CellSize + (CellSize / 2f) - width / 2,
                                i * CellSize + (CellSize / 2f) - height / 2);
                            if (Calc.Random.Chance(OffsetChance))
                            {
                                Vector2 random = Calc.Random.ShakeVector() * Calc.Random.Range(MinOffset, MaxOffset);
                                random.X *= XOffsetMult;
                                random.Y *= YOffsetMult;
                                position += random;
                            }
                            Block block = new Block(
                                position,
                                width,
                                height,
                                Calc.Random.Range(MinInterval, MaxInterval),
                                Calc.Random.Choose(AllowedColors),
                                i, j);
                            Blocks[i, j] = block;
                            Add(block);
                        }
                        TryExtendVisualBounds(Blocks[i, j]);
                    }
                }
            }
        }
        public void TryExtendVisualBounds(Block block)
        {
            VisualLeft = Math.Min(VisualLeft, X + block.X);
            VisualTop = Math.Min(VisualTop, Y + block.Y);
            VisualRight = Math.Max(VisualRight, X + block.X + block.Width);
            VisualBottom = Math.Max(VisualBottom, Y + block.Y + block.Height);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Populate();
        }
        public override void Update()
        {
            Camera cam = SceneAs<Level>().Camera;
            OnScreen = CullHelper.IsRectangleVisible(VisualLeft, VisualTop, VisualRight - VisualLeft, VisualBottom - VisualTop, 4, cam);
            if (OnScreen)
            {
                base.Update();
                Populate();
            }
        }
        public override void Render()
        {
            if (OnScreen)
            {
                base.Render();
            }
        }
    }
}