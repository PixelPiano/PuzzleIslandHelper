using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using PrismaticHelper.Entities.Panels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using static Celeste.Mod.PuzzleIslandHelper.Cutscenes.Gameshow;

namespace Celeste.Mod.PuzzleIslandHelper
{
    /*    public class AreaDataDict<TKey, TValue> : Dictionary<AreaKey, Dictionary<TKey, TValue>>
        {
            public AreaDataDict() { }
            public bool HasArea(Scene scene)
            {
                return scene is Level level && ContainsKey(level.Session.Area);
            }
            public void Reset(AreaKey key)
            {
                if (!ContainsKey(key))
                {
                    Add(key, []);
                }
                else
                {
                    base[key] = [];
                }
            }
        }
        public class AreaDataList<TValue> : Dictionary<AreaKey, List<TValue>>
        {
            public AreaDataList() { }
            public bool HasArea(Scene scene)
            {
                return scene is Level level && ContainsKey(level.Session.Area);
            }
            public void Reset(AreaKey key)
            {
                if (!ContainsKey(key))
                {
                    Add(key, []);
                }
                else
                {
                    base[key] = [];
                }
            }
        }
        public class AreaData<TValue> : Dictionary<AreaKey, TValue>
        {
            public AreaData() { }
            public bool HasArea(Scene scene)
            {
                return scene is Level level && ContainsKey(level.Session.Area);
            }
            public void Reset(AreaKey key, Func<TValue> create)
            {
                if (!ContainsKey(key))
                {
                    Add(key, create());
                }
                else
                {
                    base[key] = create();
                }
            }
        }*/
    public struct SecurityCamData
    {
        public string Name;
        public string Room;
        public Vector2 RoomPosition;
        public override string ToString()
        {
            return string.Format("{0},{1},{2}", Name, Room, RoomPosition);
        }

    }
    public class SlotData
    {
        public string Room;
        public Vector2 Offset;
        public string Flag;
        public int Index;
    }
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
    public class WarpCapsuleData
    {
        public string Name;
        public bool Lab;
        public string Room;
        public Vector2 Position;
        public WarpCapsule.Rune Rune;
    }
    public class RuneList
    {
        public WarpCapsuleData Default;
        public List<WarpCapsuleData> DefaultSet = [];
        public List<WarpCapsuleData> All = [];
        public bool Contains(WarpCapsule.Rune rune)
        {
            return All.Find(item => item.Rune.Match(rune)) != null;
        }
        public WarpCapsuleData GetDataFromRune(WarpCapsule.Rune rune)
        {
            return All.Find(item => item.Rune.Match(rune));
        }
    }
    public class PianoMapDataProcessor : EverestMapDataProcessor
    {
        private string levelName;
        /*        public static readonly AreaDataDict<string, string> IPAddressTeleports = [];
                public static readonly AreaDataList<PortalNodeData> PortalNodes = [];
                public static readonly AreaDataList<BitrailData> Bitrails = [];
                public static readonly AreaDataDict<string, CalidusSpawnerData> CalidusSpawners = [];
                public static readonly AreaDataList<SecurityCamData> SecurityCams = [];
                public static readonly AreaData<RuneList> WarpRunes = [];
                public static readonly AreaDataDict<string, List<SlotData>> SlotData = [];*/
        public static readonly Dictionary<string, Dictionary<string, string>> IPAddressTeleports = [];
        public static readonly Dictionary<string, List<PortalNodeData>> PortalNodes = [];
        public static readonly Dictionary<string, List<BitrailData>> Bitrails = [];
        public static readonly Dictionary<string, Dictionary<string, CalidusSpawnerData>> CalidusSpawners = [];
        public static readonly Dictionary<string, List<SecurityCamData>> SecurityCams = [];
        public static readonly Dictionary<string, RuneList> WarpRunes = [];
        public static readonly Dictionary<string, Dictionary<string, List<SlotData>>> SlotData = [];
        public void Reset<T>(Dictionary<string, List<T>> dict, string key)
        {
            dict.Remove(key);
            dict.Add(key, []);
        }
        public void Reset<T, T2>(Dictionary<string, Dictionary<T, T2>> dict, string key)
        {
            dict.Remove(key);
            dict.Add(key, []);
        }
        public void Reset<T>(Dictionary<string, T> dict, string key, Func<T> create)
        {
            dict.Remove(key);
            dict.Add(key, create());
        }
        public override void Reset()
        {
            string key = AreaKey.GetFullID();
            Reset(SlotData, key);
            Reset(IPAddressTeleports, key);
            Reset(PortalNodes, key);
            Reset(Bitrails, key);
            Reset(CalidusSpawners, key);
            Reset(SecurityCams, key);
            Reset(WarpRunes, key, delegate { return new RuneList(); });
        }
        public override Dictionary<string, Action<BinaryPacker.Element>> Init()
        {
            string key = AreaKey.GetFullID();
            Action<BinaryPacker.Element> memoryBlockadeHandler = data =>
            {
                Vector2 pos = new Vector2(data.AttrFloat("x"), data.AttrFloat("y"));
                List<string> flags = MemoryBlockade.GetSlotFlagData(data.Attr("flags"));
                for (int i = 0; i < flags.Count; i++)
                {
                    string f = flags[i];
                    SlotData slotdata = new SlotData
                    {
                        Room = levelName,
                        Offset = pos,
                        Flag = f,
                        Index = i
                    };

                    if (!SlotData[key].TryGetValue(f, out List<SlotData> value))
                    {
                        value = new List<SlotData>();
                        SlotData[key].Add(f, value);
                    }
                    value.Add(slotdata);
                }
            };
            Action<BinaryPacker.Element> securityCamHandler = data =>
            {
                SecurityCamData cam;
                cam.Name = data.Attr("name");
                cam.Room = levelName;
                cam.RoomPosition = new Vector2(data.AttrFloat("x"), data.AttrFloat("y"));
                SecurityCams[key].Add(cam);
            };
            Action<BinaryPacker.Element> passengerDummyHandler = data =>
            {
                string type = data.Attr("passengerType", "Civilian");
                data.Name = "PuzzleIslandHelper/Passengers/" + type;
            };
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
                    Bitrails[key].Add(raildata);
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
                    PortalNodes[key].Add(seeker);
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
                if (!CalidusSpawners[key].ContainsKey(levelName))
                {
                    CalidusSpawners[key].Add(levelName, cData);
                }
            };
            Action<BinaryPacker.Element> ipTeleportHandler = data =>
            {
                string room = levelName;
                if (!IPAddressTeleports[key].ContainsValue(room))
                {
                    string address = IPTeleport.GetAddress(data.Attr("first", "000"), data.Attr("second", "00"), data.Attr("third", "00"), data.Attr("last", "0"));
                    IPAddressTeleports[key].Add(address, room);
                }
            };

            Action<BinaryPacker.Element> accessWarpHandler = data =>
            {
                WarpCapsuleData awData = default;

                string id = data.Attr("warpID");

                if (string.IsNullOrEmpty(id)) return;
                WarpCapsule.Rune rune = new(id, data.Attr("rune"));
                if (!string.IsNullOrEmpty(levelName) && rune != null && !WarpRunes[key].Contains(rune))
                {
                    awData = new()
                    {
                        Name = id,
                        Room = levelName,
                        Rune = rune,
                        Lab = data.AttrBool("isLaboratory")
                    };
                    if (data.AttrBool("isDefaultRune"))
                    {
                        WarpRunes[key].Default = awData;
                    }
                    if (data.AttrBool("isPartOfFirstSet"))
                    {
                        WarpRunes[key].DefaultSet.Add(awData);
                    }
                    WarpRunes[key].All.Add(awData);
                }

            };
            return new Dictionary<string, Action<BinaryPacker.Element>>
            {
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
                    "entity:PuzzleIslandHelper/SecurityCam",cam =>
                    {
                        securityCamHandler(cam);
                    }
                },
                {
                    "entity:PuzzleIslandHelper/IPTeleport", ip =>
                    {
                        ipTeleportHandler(ip);
                    }
                },
                {
                    "entity:PuzzleIslandHelper/MemoryBlockade", block =>
                    {
                        memoryBlockadeHandler(block);
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
                    "entity:PuzzleIslandHelper/WarpCapsule", accessWarp =>
                    {
                        accessWarpHandler(accessWarp);
                    }
                },
                {
                    "entity:PuzzleIslandHelper/PassengerMapProcessorDummy", dummyPassenger =>
                    {
                        passengerDummyHandler(dummyPassenger);
                    }
                },
            };
        }
        public override void End()
        {
            levelName = null;
        }
    }
}
