using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class QuickGlitch : Entity
    {
        public float Interval;
        public int MaxBlocks;
        public Vector2 Offset;
        public Vector2 Padding;
        private float timer;
        private float duration;
        public bool Timed;
        public List<Block> Blocks = new();
        public Entity Entity;
        public NumRange2 BlockSizeRange;
        public class Block
        {
            public Vector2 Position;
            public Vector2 origPosition;
            public float Width;
            public float Height;
            private Rectangle Bounds;
            public Color Color;
            public Block(Vector2 position, float width, float height, Color color)
            {
                origPosition = Position = position;
                Bounds = new Rectangle((int)position.X, (int)position.Y, (int)width, (int)height);
                Color = color;
            }
            public void Update()
            {
                Bounds.X = (int)Position.X;
                Bounds.Y = (int)Position.Y;
            }
            public void Render()
            {
                Draw.Rect(Bounds, Color);
            }
        }
        public QuickGlitch(Entity entity, NumRange2 blockSizeRange, Vector2 padding, float interval, int maxBlocks, float duration) : base(entity.Collider != null ? entity.Collider.AbsolutePosition : entity.Position)
        {
            BlockSizeRange = blockSizeRange;
            Entity = entity;
            Collider = new Hitbox(entity.Width, entity.Height);
            Interval = interval;
            MaxBlocks = maxBlocks;
            Padding = padding;
            Depth = -100000;
            Timed = true;
            this.duration = duration;
            Add(new Coroutine(Sequence()));
        }
        public static QuickGlitch Create(Entity entity, NumRange2 blockSizeRange, Vector2 padding, float interval, int maxBlocks, float duration)
        {
            if(entity.Scene is not null)
            {
                QuickGlitch glitch = new(entity, blockSizeRange, padding, interval, maxBlocks, duration);
                entity.Scene.Add(glitch);
                return glitch;
            }
            return null;
        }
        public override void Update()
        {
            base.Update();
            if (Entity is Calidus calidus)
            {
                foreach (Block b in Blocks)
                {
                    b.Position = b.origPosition - Vector2.UnitY * calidus.FloatTarget;
                }
            }
            foreach (Block b in Blocks)
            {
                b.Update();
            }
            if (Timed)
            {
                timer += Engine.RawDeltaTime;
                if (timer > duration)
                {
                    RemoveSelf();
                }
            }
        }
        public override void Render()
        {
            base.Render();
            foreach (Block block in Blocks)
            {
                block.Render();
            }
        }
        private IEnumerator Sequence()
        {
            Vector2 pos;
            while (true)
            {
                Blocks.Clear();
                for (int i = 0; i < MaxBlocks; i++)
                {
                    Vector2 size = BlockSizeRange.Random();
                    Color color = Calc.Random.Choose(Color.Green, Color.Black);
                    pos = Position - Padding + new Vector2(Calc.Random.Range(0, Width + Padding.X * 2), Calc.Random.Range(0, Height + Padding.Y * 2));
                    pos += Offset;
                    Blocks.Add(new Block(pos, size.X, size.Y, color));
                }
                yield return Interval;
            }
        }
    }
}