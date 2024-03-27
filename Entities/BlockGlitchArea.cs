using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BlockGlitchArea")]
    [Tracked]
    public class BlockGlitchArea : Entity
    {
        
        public Rectangle Bounds;
        private float MaxTime;
        private Vector2 JitterOffset;
        private float Ellapsed;
        private Random Random;
        public static float MaxWidth = 70;
        public static float MaxHeight = 70;
        private Level level;
        public bool InBackground;
        public Color Color;
        public BlockGlitchArea(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
        }
        public BlockGlitchArea(Vector2 Position, Rectangle Bounds)
        {
            this.Position = Position;
            this.Bounds = Bounds;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Random = new Random();
            MaxTime = Random.Range(0.5f, 4f);
            Ellapsed = 0;
            Randomize();
        }
        private Color RandomColor()
        {
            return Calc.Random.Range(0, 20) switch
            {
                1 => Color.Red,
                2 => Color.Green,
                3 => Color.Blue,
                _ => Color.White
            };
        }
        public BlockGlitchArea()
        {
        }
        public void Randomize()
        {
            InBackground = Random.Chance(0.5f);
            Position.X = Random.Range(0, 300);
            Position.Y = Random.Range(0, 160);
            Bounds = new Rectangle(Random.Range(20, 300),
                                    Random.Range(20, 160),
                                    (int)Random.Range(10, Calc.Min(MaxWidth, 320 - Position.X)),
                                    (int)Random.Range(10, Calc.Min(MaxHeight, 180 - Position.Y)));
            Color = RandomColor();
            MaxTime = Random.Range(0.5f, 2f);
            Ellapsed = 0;
        }
        public override void Update()
        {
            base.Update();
            // level = SceneAs<Level>();
            /*            if (level is not null)
                        {
                            if (level.OnInterval(120 / level.Tracker.GetEntities<BlockGlitchArea>().Count))
                            {
                                Random = new Random((int)level.TimeActive);
                            }
                        }*/
            Ellapsed += Engine.DeltaTime;
            if (Ellapsed > MaxTime)
            {
                Randomize();
            }
        }
    }
}

