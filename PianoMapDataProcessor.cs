using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using static MonoMod.InlineRT.MonoModRule;

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
    public class PortalNodeData
    {
        public string LevelName;
        public Vector2 Center => LevelData.Position + Offset;
        public Vector2 PortalNodePosition;
        public float Thickness;
        public string Flag;
        public Vector2 Offset;
        public Vector2 RoomOffset;
        public MapData MapData;
        public Vector2? Start;
        public Vector2? End;
        public LevelData LevelData => MapData.Get(LevelName);
        public PortalNodeData()
        {

        }
    }
    public struct CalidusSpawnerData
    {
        public string EyeFlag;
        public string HeadFlag;
        public string LeftArmFlag;
        public string RightArmFlag;
    }
    public struct TileLayoutData
    {
        public string CopyFrom;
        public string CopyTo;
        public Point Offset;
    }
    public struct WarpCapsuleData
    {
        public string Room;
        public Vector2 Position;
        public float Delay;
    }
    public class PianoMapDataProcessor : EverestMapDataProcessor
    {
        private string levelName;
        public static List<BitrailData> Bitrails = new();
        public static List<PortalNodeData> PortalNodes = new();
        public static List<TileLayoutData> CopiedTileseedData = new();
        public static Dictionary<string, WarpCapsuleData> WarpLinks = new();
        public static WarpCapsuleData PrimaryWarpData;
        public static Dictionary<string, CalidusSpawnerData> CalidusSpawners = new();
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
            Action<BinaryPacker.Element> portalNodeHandler = data =>
            {
                PortalNodeData seeker = new PortalNodeData
                {
                    MapData = MapData,
                    LevelName = levelName,
                    Offset = new(data.AttrFloat("x"), data.AttrFloat("y")),
                    Flag = data.Attr("flag"),
                    Thickness = data.AttrFloat("thickness", 16)
                };
                if (seeker != null)
                {
                    PortalNodes.Add(seeker);
                }
            };
            Action<BinaryPacker.Element> calidusSpawnerHandler = data =>
            {
                CalidusSpawnerData cData = new()
                {
                    EyeFlag = data.Attr("eyeFlag"),
                    HeadFlag = data.Attr("headFlag"),
                    LeftArmFlag = data.Attr("leftArmFlag"),
                    RightArmFlag = data.Attr("rightArmFlag"),
                };
                if (!CalidusSpawners.ContainsKey(levelName))
                {
                    CalidusSpawners.Add(levelName, cData);
                }
            };
            /*            Action<BinaryPacker.Element> tileLayoutControllerHandler = data =>
                        {
                            TileLayoutData tData = new()
                            {
                                CopyFrom = data.Attr("copyFromRoom"),
                                CopyTo = levelName,

                            };
                            CopiedTileseedData.Add(tData);
                        };*/
            Action<BinaryPacker.Element> accessWarpHandler = data =>
            {
                WarpCapsuleData awData = default;
                string id = data.Attr("warpID");
                if (!string.IsNullOrEmpty(levelName) && !string.IsNullOrEmpty(id) && !WarpLinks.ContainsKey(id))
                {
                    awData = new()
                    {
                        Room = levelName,
                        Delay = data.AttrFloat("warpDelay"),
                        //Position = MapData.Get(levelName).Position + new Vector2(data.AttrFloat("x"),data.AttrFloat("y"))
                    };
                    WarpLinks.Add(id, awData);
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
                },
                {
                    "entity:PuzzleIslandHelper/CalidusSpawner", spawner =>
                    {
                        calidusSpawnerHandler(spawner);
                    }
                },
                {
                    "entity:PuzzleIslandHelper/PortalNode", portalNode =>
                    {
                        portalNodeHandler(portalNode);
                    }
                },
                {
                    "entity:PuzzleIslandHelper/DigiWarpReceiver", accessWarp =>
                    {
                        accessWarpHandler(accessWarp);
                    }
                },
/*                {
                    "entity:PuzzleIslandHelper/TileLayoutController", layout =>
                    {
                        tileLayoutControllerHandler(layout);
                    }
                },*/
            };
        }

        public override void Reset()
        {
            // reset the dictionary for the current map and mode.
            WarpLinks = new();
            PortalNodes = new();
            Bitrails = new();
            CalidusSpawners = new();
            CopiedTileseedData = new();
        }

        public override void End()
        {
            levelName = null;
        }
    }
}
