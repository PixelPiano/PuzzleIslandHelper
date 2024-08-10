using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class BitrailData
    {
        public string LevelName;
        public Vector2 Offset;
        public Vector2 RoomOffset;
        public LevelData LevelData => MapData.Get(LevelName);
        public MapData MapData;
        public BitrailData()
        {
        }
    }
    public class PianoMapDataProcessor : EverestMapDataProcessor
    {
        private string levelName;
        public static List<BitrailData> Bitrails = new();
        public override Dictionary<string, Action<BinaryPacker.Element>> Init()
        {
            Action<BinaryPacker.Element> bitrailNodeHandler = bitrailData =>
          {
              BitrailData data = new BitrailData
              {
                  MapData = MapData,
                  LevelName = levelName,
                  Offset = new(bitrailData.AttrFloat("x"), bitrailData.AttrFloat("y"))
              };
              if (data is not null)
              {
                  Bitrails.Add(data);
                  Console.WriteLine("Bitrails.Count: " + Bitrails.Count);
              }
          };
            return new Dictionary<string, Action<BinaryPacker.Element>> {
                {
                    "level", level =>
                    {
                        // be sure to write the level name down.
                        levelName = level.Attr("name").Split(':')[0];
                        if (levelName.StartsWith("lvl_")) {
                            levelName = levelName.Substring(4);
                        }
                    }
                },
                {
                    "entity:PuzzleIslandHelper/BitrailNode", node =>
                    {
                       bitrailNodeHandler(node);
                    }
                }
            };
        }

        public override void Reset()
        {
            // reset the dictionary for the current map and mode.
            Bitrails = new List<BitrailData>();
        }

        public override void End()
        {
            levelName = null;
        }
    }
}
