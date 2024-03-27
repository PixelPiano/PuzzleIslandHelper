using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// PuzzleIslandHelper.LabFallingBlock
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LFBController")]
    [Tracked]

    public class LFBController : Entity
    {
        private Level level;
        private LabFallingBlock Block;
        public static List<Vector2> InvalidTiles = new();
        private Vector2 offset;
        private const int MaxBlocks = 50;
        public static int CurrentBlockCount;
        private const float MaxWaitTime = 2f;
        public static List<Rectangle> OffLimitZones = new();
        public static List<Rectangle> BlockBounds = new();
        private readonly string flag;
        private bool FlagState
        {
            get
            {
                if (string.IsNullOrEmpty(flag))
                {
                    return true;
                }
                if (Inverted)
                {
                    return !SceneAs<Level>().Session.GetFlag(flag);
                }
                else
                {
                    return SceneAs<Level>().Session.GetFlag(flag);
                }
            }
        }
        private bool Inverted;
        private bool Enabled
        {
            get
            {
                return FlagState && CurrentBlockCount < MaxBlocks;
            }
        }
        public override void Update()
        {
            base.Update();
        }
        private IEnumerator BlockAdder()
        {
            while (true)
            {
                if (Enabled)
                {
                    float y = -40;
                    float x = Calc.Random.Range(0, 320);
                    Block = new LabFallingBlock(new Vector2(x,y), new Vector2(level.Camera.Position.X, level.Bounds.Top), "labFallingBlockFlag", 'J', 1, 1, true);
                    level.Add(Block);
                }
                yield return Calc.Random.Range(0.2f, MaxWaitTime);
            }
        }
        public LFBController(EntityData data, Vector2 offset)
          : base(data.Position + offset)
        {
            this.offset = offset;
            flag = data.Attr("flag");
            Inverted = data.Bool("invertFlag");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            Grid grid = level.SolidTiles.Grid;
            CurrentBlockCount = 0;
            for (int i = 0; i < level.Bounds.Width; i += 8)
            {
                Vector2 point = new Vector2(level.Bounds.Left + i, level.Bounds.Top);
                if (grid.Collide(point))
                {
                    InvalidTiles.Add(point);
                }
            }
            if (InvalidTiles.Count > 0)
            {
                CombinePoints();
            }
            Add(new Coroutine(BlockAdder()));
        }
        private void CombinePoints()
        {
            int Width = 0;
            int Height = 50;
            Vector2 startPoint = InvalidTiles.First();
            int index = 0;
            int max = InvalidTiles.Count;
            while (InvalidTiles.IsInRange(index))
            {
                if (Width == 0)
                {
                    startPoint = InvalidTiles[index];
                }

                if (InvalidTiles.Contains(InvalidTiles[index] + Vector2.UnitX * 8))
                {
                    Width += 8;
                }
                else
                {
                    OffLimitZones.Add(new Rectangle((int)startPoint.X - 8, (int)startPoint.Y - Height, Width + 24, Height + 8));
                    Width = 0;
                    if (InvalidTiles.IsInRange(index + 1))
                    {
                        startPoint = InvalidTiles[index + 1];
                    }
                }
                index++;
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            foreach (Vector2 v in InvalidTiles)
            {
                Draw.HollowRect(v, 8, 8, Color.Blue);
            }
        }


    }
}