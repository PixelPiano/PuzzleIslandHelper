using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class BitrailData
    {
        public string LevelName;
        public Vector2 Offset;
        public Vector2 RoomOffset;
        public int Bounces;
        public bool IsExit;
        public string GroupID;
        public Color Color;
        public float TimeLimit;
        public BitrailNode.ControlTypes ControlType;
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
            Action<BinaryPacker.Element> bitrailNodeHandler = data =>
          {
              BitrailData raildata = new BitrailData
              {
                  MapData = MapData,
                  LevelName = levelName,
                  Offset = new(data.AttrFloat("x"), data.AttrFloat("y")),
                  Bounces = data.AttrInt("bounces"),
                  IsExit = data.AttrBool("isExit"),
                  GroupID = data.Attr("groupId"),
                  Color = Calc.HexToColor(data.Attr("color")),
                  TimeLimit = data.AttrFloat("timeLimit", -1),
                  ControlType = (BitrailNode.ControlTypes)Enum.Parse(typeof(BitrailNode.ControlTypes), data.Attr("control", "Default"))
          };
            if (raildata is not null)
            {
                Bitrails.Add(raildata);
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
