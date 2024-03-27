using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/CustomFloatingBlock")]
    [Tracked]
    public class CustomFloatingBlock : Solid
    {
        private char TileType;
        private List<string> RandomList = new List<string>();

        private string filename;
        private string flag;

        public int RealWidth;
        public int RealHeight;
        private string TilesData;
        public FancyFloatySpaceBlock FancyBlock;

        public CustomFloatingBlock(Vector2 Position, Vector2 offset, string flag, char TileType, int Width, int Height, bool useRandom, int index, bool forCutscene = false)
           : base(Position + offset, Width, Height, false)
        {
            Collidable = false;
            if (!forCutscene)
            {
                filename = "ModFiles/PuzzleIslandHelper/RandomFancyBlocks";
            }
            else
            {
                filename = "ModFiles/PuzzleIslandHelper/floatingCircleBlocks";
            }
            this.flag = flag;
            this.TileType = TileType;
            AddRandom();
            int randomIndex = new Random().Range(0, RandomList.Count);
            TilesData = RandomList[useRandom ? randomIndex : Calc.Clamp(index, 0, RandomList.Count - 1)];
            EntityData BlockData = new EntityData
            {
                Name = "FancyTileEntities/FancyFloatySpaceBlock",
                Position = Position,
                Values = new()
            {
                {"randomSeed", Calc.Random.Next()},
                {"blendEdges", true },
                {"width", Width },
                {"height", Height },
                {"tileData", TilesData }
            }
            };
            FancyBlock = new FancyFloatySpaceBlock(BlockData, offset); ;
            FancyBlock.Visible = true;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[TileType];
        }
        public void Remove()
        {
            FancyBlock.RemoveSelf();
            RemoveSelf();
        }

        public CustomFloatingBlock(EntityData data, Vector2 offset)
          : this(data.Position, offset, data.Attr("flag"), data.Char("tiletype", '3'), data.Width, data.Height, data.Bool("useRandomPreset"), data.Int("blockIndex"))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            FancyBlock.Center = Center;
            scene.Add(FancyBlock);
        }
        private void AddRandom()
        {
            string content = Everest.Content.TryGet(filename, out var asset) ? ReadModAsset(asset) : null;
            string[] array = content.Split('\n');
            string toAdd = "";
            foreach (string s in array)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    if (!string.IsNullOrWhiteSpace(toAdd))
                    {
                        toAdd = toAdd.Replace('1', TileType);
                        RandomList.Add(toAdd);
                    }
                    toAdd = "";
                    continue;
                }
                RealWidth = s.Length * 8;
                toAdd += s + '\n';
            }
            RealHeight = array.Length * 8;
        }
        public static string ReadModAsset(string filename)
        {
            return Everest.Content.TryGet(filename, out var asset) ? ReadModAsset(asset) : null;
        }
        public static string ReadModAsset(ModAsset asset)
        {
            using var reader = new StreamReader(asset.Stream);

            return reader.ReadToEnd();
        }
    }
}