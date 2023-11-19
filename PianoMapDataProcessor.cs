using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class TileseedAreaData
    {
        public string LevelName;
        public Vector2 RoomOffset;
        public LevelData LevelData => MapData.Get(LevelName);
        public MapData MapData;
        public float Seed;
        public TileseedAreaData()
        {
        }
    }
    public class PianoMapDataProcessor : EverestMapDataProcessor
    {
        private string levelName;
        public static List<TileseedAreaData> TileseedAreas = new();
        public override Dictionary<string, Action<BinaryPacker.Element>> Init()
        {
            Action<BinaryPacker.Element> tileseedAreaHandler = tileseedArea =>
            {
                TileseedAreaData data = new TileseedAreaData
                {
                    MapData = MapData,
                    LevelName = levelName,
                    Seed = tileseedArea.AttrFloat("seed"),
                    RoomOffset = new(tileseedArea.AttrFloat("offsetX"), tileseedArea.AttrFloat("offsetY"))
                };

                if (data is not null)
                {
                    TileseedAreas.Add(data);
                    Console.WriteLine("TileseedAreas.Count: " + TileseedAreas.Count);
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
                    "entity:PuzzleIslandHelper/TileseedArea", area => {
                        tileseedAreaHandler(area);
                    }
                },
            };
        }

        public override void Reset()
        {
            // reset the dictionary for the current map and mode.
            TileseedAreas = new List<TileseedAreaData>();
        }

        public override void End()
        {

            //PianoModule.TileseedAreas = TileseedAreas;
            //TileseedAreas.Clear();
            levelName = null;
        }
    }
}
