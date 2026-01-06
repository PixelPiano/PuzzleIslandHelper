using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.PianoModuleSession;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public enum Directions
    {
        Right = 0,
        Up = 1,
        Left = 2,
        Down = 3,
    }
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
    public struct MarkerData
    {
        public string ID;
        public Vector2 PositionInRoom;
        public Vector2 RoomPosition;
        public readonly Vector2 WorldPosition => RoomPosition + PositionInRoom;
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
    public class WarpData
    {
        public string Room;
        public string ID;
        public bool Lab;
        public bool Default;
        public WarpRune Rune;
        public bool HasRune => Rune != null;
    }
    public class CapsuleList
    {
        public WarpData DefaultRune;
        public HashSet<WarpData> DefaultRuneSet = [];
        public HashSet<WarpData> AllRunes = [];
        public HashSet<WarpData> All = [];
        public bool ContainsRune(WarpRune rune) => GetDataFromRune(rune) != null;
        public bool ContainsID(string id) => GetDataFromID(id) != null;
        public WarpData GetDataFromRune(WarpRune rune)
        {
            foreach (var d in AllRunes)
            {
                if (d.Rune.Match(rune))
                {
                    return d;
                }
            }
            return null;
        }
        public WarpData GetDataFromID(string id)
        {
            foreach (var d in All)
            {
                if (d.ID.Equals(id))
                {
                    return d;
                }
            }
            return null;
        }
    }

    public class CompassNodeData
    {
        public string Flag => $"CompassNode{{{FullID}}}:On";
        public bool Broken;
        public static Vector2 CenterOffset = Vector2.One * 8;
        public Directions Direction;
        public Vector2 PositionInRoom;
        public bool Empty;
        public bool CanTurnOn => !Empty && !Broken;
        public bool On
        {
            get
            {
                if (Empty) return false;
                if (Engine.Scene is Level level)
                {
                    return level.Session.GetFlag(Flag);
                }
                return false;
            }
            set
            {
                if (Engine.Scene is Level level)
                {
                    level.Session.SetFlag(Flag, value);
                }
            }
        }
        public string ParentID = "";
        public string ID = "";
        public float Distance => Vector2.Distance(WorldPosition, ParentData.WorldPosition);
        public int Index;
        public string FullID => $"{ID}+{ParentID}";
        public MapData MapData;
        public Vector2 WorldPosition => RoomOffset + PositionInRoom + CenterOffset;
        public Vector2 RoomOffset => MapData.Get(Room).Position;
        public string Room;
        public CompassData ParentData;
        public override string ToString()
        {
            return $"ID:{ID},\n" +
                $"ParentID:{ParentID},\n" +
                $"Position:{WorldPosition},\n" +
                $"Direction:{Direction},\n" +
                $"On:{On},\n" +
                $"Empty:{Empty},\n" +
                $"Flag:{Flag.GetFlag()},\n" +
                $"FlagName:{Flag},\n" +
                $"Distance:{Distance},\n" +
                $"Index:{Index}";
        }
    }
    public class CompassData
    {
        public static Vector2 CenterOffset = Vector2.One * (59f / 2);
        public Vector2 PositionInRoom;
        public string ID;
        public string Room;
        public MapData MapData;
        public Vector2 WorldPosition => RoomOffset + PositionInRoom + CenterOffset;
        public Vector2 RoomOffset => MapData.Get(Room).Position;
        public Dictionary<Directions, HashSet<CompassNodeData>> Nodes = []; //sorted by distance to node
        public CompassData(string id, Vector2 positionInRoom, string room, MapData mapData)
        {
            ID = id;
            PositionInRoom = positionInRoom;
            Room = room;
            MapData = mapData;
            foreach (var value in Enum.GetValues<Directions>())
            {
                Nodes.Add(value, []);
            }
        }
        public override string ToString()
        {
            return $"ID:{ID},\n" +
                $"PositionInRoom:{PositionInRoom},\n" +
                $"WorldPosition:{WorldPosition},\n" +
                $"RoomOffset:{RoomOffset},\n" +
                $"NodeCount:{NodeListString()}";
        }
        public string NodeListString()
        {
            string output = "Nodes:";
            foreach (var pair in Nodes)
            {
                output += "\n\t";
                output += $"{{{pair.Key}}}:{pair.Value.Count}";
            }

            return output;
        }
        public void Add(CompassNodeData node)
        {
            if (node != null)
            {
                node.ParentData = this;
                Nodes[node.Direction].Add(node);
            }
        }
        public void OrderNodes()
        {
            foreach (var pair2 in Nodes)
            {
                Nodes[pair2.Key] = [.. pair2.Value.OrderBy(item => item.Distance)];
                int index = 0;
                foreach (var n in Nodes[pair2.Key])
                {
                    n.Index = index++;
                }
            }
        }
        public Directions GetDirection(Level level)
        {
            return (Directions)level.Session.GetCounter("Compass" + ID);
        }
        public void Reset(Level level)
        {
            level.Session.SetCounter("Compass" + ID, 0);
            TrySetFirstFound(level);
        }
        public bool TrySetFirstFound(Level level)
        {
            DeactivateAll();
            if (Compass.Enabled)
            {
                Directions dir = GetDirection(level);
                foreach (var n in Nodes[dir])
                {
                    if (n.CanTurnOn)
                    {
                        n.On = true;
                        return true;
                    }
                }
                Directions oppDir = dir switch
                {
                    Directions.Right => Directions.Left,
                    Directions.Up => Directions.Down,
                    Directions.Left => Directions.Right,
                    Directions.Down => Directions.Up,
                };
                List<CompassNodeData> secondSet = [.. Nodes[oppDir]];
                for (int i = secondSet.Count - 1; i >= 0; i--)
                {
                    var n2 = secondSet[i];
                    if (n2.CanTurnOn)
                    {
                        n2.On = true;
                        return true;
                    }
                }
            }
            return false;
        }
        public void DeactivateAll()
        {
            foreach (var pair in Nodes)
            {
                foreach (var node in pair.Value)
                {
                    node.On = false;
                }
            }
        }
    }
    public class PianoMapDataProcessor : EverestMapDataProcessor
    {
        private string levelName;
        public static readonly Dictionary<string, Dictionary<string, string>> IPAddressTeleports = [];
        public static readonly Dictionary<string, List<PortalNodeData>> PortalNodes = [];
        public static readonly Dictionary<string, List<BitrailData>> Bitrails = [];
        public static readonly Dictionary<string, Dictionary<string, CalidusSpawnerData>> CalidusSpawners = [];
        public static readonly Dictionary<string, List<SecurityCamData>> SecurityCams = [];
        public static readonly Dictionary<string, CapsuleList> WarpCapsules = [];
        public static readonly Dictionary<string, Dictionary<string, List<SlotData>>> SlotData = [];
        public static readonly Dictionary<string, Dictionary<string, List<MarkerData>>> MarkerData = [];
        public static readonly Dictionary<string, List<string>> CollectableData = [];
        public static readonly Dictionary<string, HashSet<CompassNodeData>> CompassNodeData = [];
        public static readonly Dictionary<string, HashSet<CompassData>> CompassData = [];
        public static readonly Dictionary<string, Dictionary<string, Ascwiit.Controller.Data>> AscwiitCodes = [];

        public static void Reset<T>(Dictionary<string, List<T>> dict, string key)
        {
            dict.Remove(key);
            dict.Add(key, []);
        }
        public static void Reset<T>(Dictionary<string, HashSet<T>> dict, string key)
        {
            dict.Remove(key);
            dict.Add(key, []);
        }
        public static void Reset<T, T2>(Dictionary<string, Dictionary<T, T2>> dict, string key)
        {
            dict.Remove(key);
            dict.Add(key, []);
        }
        public static void Reset<T>(Dictionary<string, T> dict, string key, Func<T> create)
        {
            dict.Remove(key);
            dict.Add(key, create == null ? default : create());
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
            Reset(WarpCapsules, key, delegate { return new CapsuleList(); });
            Reset(MarkerData, key);
            Reset(CollectableData, key);
            Reset(CompassNodeData, key);
            Reset(CompassData, key);
            Reset(AscwiitCodes, key);
        }
        [Command("print_markers", "")]
        public static void PrintMarkers()
        {
            if (Engine.Scene is Level level && level.GetAreaKey() is string s && MarkerData.TryGetValue(s, out var value))
            {
                foreach (var pair in value)
                {
                    Engine.Commands.Log(string.Format("{0}: ({1})", pair.Key, pair.Value.Count));
                }
            }
        }
        public override Dictionary<string, Action<BinaryPacker.Element>> Init()
        {
            string key = AreaKey.GetFullID();
            Action<BinaryPacker.Element> compassNodeData = data =>
            {
                if (CompassNodeData[key] == null)
                {
                    CompassNodeData[key] = [];
                }

                CompassNodeData nodeData = new()
                {
                    ID = data.Attr("nodeID"),
                    ParentID = data.Attr("compassID"),
                    PositionInRoom = data.Position(),
                    Empty = data.AttrBool("startEmpty"),
                    Direction = Enum.Parse<Directions>(data.Attr("direction")),
                    Room = levelName,
                    MapData = MapData,
                };

                CompassNodeData[key].Add(nodeData);
            };
            Action<BinaryPacker.Element> ascwiitSequenceData = data =>
            {
                Ascwiit.Controller.Data sequence = new()
                {
                    Group = data.Attr("groupID"),
                    PositionInRoom = data.Position(),
                    Room = levelName
                };
                string[] steps = data.Attr("steps").Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                List<Directions> stepsList = [];
                foreach (string s in steps)
                {
                    if (Enum.TryParse(s, true, out Directions result))
                    {
                        stepsList.Add(result);
                    }
                }
                sequence.Steps = stepsList.ToArray();
                AscwiitCodes[key].TryAdd(sequence.Group, sequence);
            };
            Action<BinaryPacker.Element> compassData = data =>
            {
                if (CompassData[key] == null)
                {
                    CompassData[key] = [];
                }
                if (data.AttrBool("leader"))
                {
                    CompassData compassData = new(data.Attr("compassID"), data.Position(), levelName, MapData);
                    CompassData[key].Add(compassData);
                }
            };
            Action<BinaryPacker.Element> collectableHandler = data =>
            {
                if (CollectableData[key] == null)
                {
                    CollectableData[key] = new List<string>();
                }
                CollectableData[key].Add(data.Attr("code").Replace(" ", ""));
            };
            Action<BinaryPacker.Element> markerHandler = data =>
            {
                MarkerData marker = default;
                marker.ID = data.Attr("markerID");
                marker.PositionInRoom = data.Position();
                if (MapData != null && MapData.Get(levelName) is LevelData leveldata)
                {
                    marker.RoomPosition = leveldata.Position;
                }
                if (MarkerData[key].TryGetValue(levelName, out List<MarkerData> value))
                {
                    value.Add(marker);
                }
                else
                {
                    MarkerData[key].Add(levelName, [marker]);
                }
            };
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
            Action<BinaryPacker.Element> wipEntityHandler = (data) =>
            {
                string name = data.Attr("name");
                if (!string.IsNullOrEmpty(name))
                {
                    data.Name = name;
                }
            };
            Action<BinaryPacker.Element, bool> accessWarpHandler = (data, hasRune) =>
            {
                WarpData awData = null;

                string id = data.Attr("warpID");
                if (string.IsNullOrEmpty(id)) return;

                if (!string.IsNullOrEmpty(levelName))
                {
                    if (hasRune)
                    {
                        WarpRune rune = null;
                        rune = new(id, data.Attr("rune", ""));
                        if (rune != null && !WarpCapsules[key].ContainsRune(rune))
                        {
                            awData = new()
                            {
                                ID = id,
                                Room = levelName,
                                Rune = rune,
                                Lab = data.AttrBool("isLaboratory"),
                                Default = data.AttrBool("isDefaultRune")
                            };
                            if (awData.Default)
                            {
                                WarpCapsules[key].DefaultRune = awData;
                            }
                            if (data.AttrBool("isPartOfFirstSet"))
                            {
                                WarpCapsules[key].DefaultRuneSet.Add(awData);
                            }
                            WarpCapsules[key].AllRunes.Add(awData);
                        }
                    }
                    else
                    {
                        awData = new()
                        {
                            ID = id,
                            Room = levelName
                        };
                    }
                    if (awData != null)
                    {
                        WarpCapsules[key].All.Add(awData);
                    }
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
                    "entity:PuzzleIslandHelper/AscwiitController", sequence =>
                    {
                        ascwiitSequenceData(sequence);
                    }
                },
                {
                    "entity:PuzzleIslandHelper/Compass", compass =>
                    {
                       compassData(compass);
                    }
                },
                {
                    "entity:PuzzleIslandHelper/CompassNode", compassNode =>
                    {
                       compassNodeData(compassNode);
                    }
                },
                {
                    "entity:PuzzleIslandHelper/WipEntity", wip =>
                    {
                        wipEntityHandler(wip);
                    }
                },
                {
                    "entity:PuzzleIslandHelper/Marker",marker =>
                    {
                       markerHandler(marker);
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
                        accessWarpHandler(accessWarp, true);
                    }
                },
                {
                    "entity:PuzzleIslandHelper/WarpCapsuleBeta", betaAccessWarp =>
                    {
                       accessWarpHandler(betaAccessWarp, false);
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
        [Command("print_compass", "")]
        public static void printcompass()
        {
            var key = Engine.Scene.GetAreaKey();
            if (CompassData.TryGetValue(key, out var c))
            {
                var list = c;
                foreach (var v in list)
                {
                    Engine.Commands.Log(v.ID);
                    foreach (var v2 in v.Nodes)
                    {
                        Engine.Commands.Log(v2.Key + ":");
                        string output = "\t";
                        foreach (var v3 in v2.Value)
                        {
                            output += v3.ID + "\n\t";
                        }
                        Engine.Commands.Log(output);
                    }
                }
            }
        }
        public override void End()
        {
            levelName = null;
        }
    }
}
