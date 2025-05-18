using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Collections.Generic;
using TAS.EverestInterop;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    //[ConstantEntity("PuzzleIslandHelper/PixelSorter")]
    [Tracked]
    public class PixelSorter : Entity
    {
        public static bool SortingEnabled
        {
            get
            {
                return sortingEnabled && !CurrentBehavior.Empty;
            }
            set
            {
                sortingEnabled = value;
            }
        }
        private static bool sortingEnabled;
        public static Rectangle Core = new Rectangle(0, 0, 320, 180);
        public static List<Rectangle> Rectangles = [];

        public PixelSorter() : base()
        {
            Tag |= Tags.Global | Tags.TransitionUpdate;
        }
        public struct Behavior
        {
            public int ChunkWidth;
            public int ChunkHeight;
            public int XSkip;
            public int YSkip;
            public bool Empty => ChunkWidth == 0 && ChunkHeight == 0;
            public static Behavior None = new Behavior(0, 0);
            public static Behavior FullSort = new Behavior(320, 180);
            public static Behavior RowSort = new Behavior(320, 1);
            public static Behavior ColumnSort = new Behavior(1, 180);
            public static Behavior EightGrid = new Behavior(8, 8);
            public static Behavior Chunk(int size)
            {
                return new Behavior(size, size, 0, 0);
            }
            public Behavior(int chunkwidth, int chunkheight, int xSkip = 0, int ySkip = 0)
            {
                ChunkWidth = chunkwidth;
                ChunkHeight = chunkheight;
                XSkip = xSkip;
                YSkip = ySkip;
            }
            public static bool operator ==(Behavior left, Behavior right)
            {
                return left.ChunkWidth == right.ChunkWidth && left.ChunkHeight == right.ChunkHeight && left.XSkip == right.XSkip && left.YSkip == right.YSkip;
            }
            public static bool operator !=(Behavior left, Behavior right) => !(left == right);

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        public static void SetData(Texture2D texture, Rectangle rectangle, Color[] data)
        {
            texture.SetData(0, rectangle, data, 0, rectangle.Width * rectangle.Height);
        }
        public static Color[] Orig = new Color[MaxElements];
        public static VirtualRenderTarget Source => GameplayBuffers.Level;
        public const int MaxElements = 320 * 180;
        public static Behavior CurrentBehavior = Behavior.EightGrid;
        public static Behavior prevBehavior;
/*        [OnLoad]
        public static void Load()
        {
            On.Celeste.Glitch.Apply += Glitch_Apply;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Glitch.Apply -= Glitch_Apply;
        }*/
        private static void Glitch_Apply(On.Celeste.Glitch.orig_Apply orig, VirtualRenderTarget source, float timer, float seed, float amplitude)
        {
            orig(source, timer, seed, amplitude);
            if (SortingEnabled)
            {
                source.Target.GetData(Orig);
                if (CurrentBehavior != prevBehavior)
                {
                    Rectangles.Clear();
                    Rectangles = Core.Split(CurrentBehavior.ChunkWidth, CurrentBehavior.ChunkHeight, CurrentBehavior.XSkip, CurrentBehavior.YSkip);
                }
                foreach (Rectangle r in Rectangles)
                {
                    Color[] data = new Color[r.Width * r.Height];
                    for (int x = r.X; x < r.Right; x++)
                    {
                        for (int y = r.Y; y < r.Bottom; y++)
                        {
                            data[x - r.X + (y - r.Y) * r.Width] = Orig[x + y * 320];
                        }
                    }
                    Color[] ordered = [.. data.OrderBy(item => (item.R + item.G + item.B))];
                    SetData(source.Target, r, ordered);
                }
                prevBehavior = CurrentBehavior;
                /*

                                Orig = new Color[MaxElements];
                                source.Target.GetData(Orig);

                                Rectangle core = new Rectangle(0, 0, 320, 180);
                                List<Rectangle> rects = core.Split(CurrentBehavior.ChunkWidth, CurrentBehavior.ChunkHeight, CurrentBehavior.XSkip, CurrentBehavior.YSkip);
                                foreach (Rectangle r in rects)
                                {
                                    Color[] chunk = new Color[r.Width * r.Height];
                                    for (int x = r.X; x < r.Right; x++)
                                    {
                                        for (int y = r.Y; y < r.Bottom; y++)
                                        {
                                            chunk[(x - r.X) + (y - r.Y) * r.Width] = Orig[x + y * 320];
                                        }
                                    }
                                    source.Target.SetData(0, r, [.. chunk.OrderBy(item => (item.R + item.G + item.B) / 3f)], 0, r.Width * r.Height);
                                }*/
            }
        }
        public static void SetBehavior(Behavior behavior)
        {
            CurrentBehavior = behavior;
        }
        [Command("pixelsort", "enables pixel sorting")]
        public static void PixelSort()
        {
            SortingEnabled = true;
            CurrentBehavior = Behavior.FullSort;
        }
        [Command("squaresort", "enables pixel sorting")]
        public static void SquareSort(int size = 8)
        {
            SortingEnabled = true;
            CurrentBehavior = Behavior.Chunk(size);
        }
        [Command("gridsort", "enables pixel sorting")]
        public static void GridSort(int row = 320, int column = 180)
        {
            SortingEnabled = true;
            CurrentBehavior = new Behavior(row, column, 0, 0);
        }
        [Command("rowsort", "enables pixel sorting")]
        public static void RowSort()
        {
            SortingEnabled = true;
            CurrentBehavior = Behavior.RowSort;
        }
        [Command("columnsort", "enables pixel sorting")]
        public static void ColumnSort()
        {
            SortingEnabled = true;
            CurrentBehavior = Behavior.ColumnSort;
        }
        [Command("pixelsortstop", "disables pixel sorting")]
        public static void StopSort()
        {
            SortingEnabled = false;
        }
        private static Color[] test = [Color.Red, Color.Blue, Color.Green, Color.Yellow];
        /*        public override void ApplyParameters(Level level)
                {
                    base.ApplyParameters(level);
                    Effect.Parameters["array"]
                }*/
    }
}