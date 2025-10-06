using Celeste.Mod.PuzzleIslandHelper.Entities;
using System;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using System.Reflection;
using Monocle;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModule : EverestModule
    {
        public static PianoModule Instance { get; private set; }
        public override Type SaveDataType => typeof(PianoModuleSaveData);
        public static PianoModuleSaveData SaveData => (PianoModuleSaveData)Instance._SaveData;
        public override Type SettingsType => typeof(PianoModuleSettings);
        public static PianoModuleSettings Settings => (PianoModuleSettings)Instance._Settings;
        public override Type SessionType => typeof(PianoModuleSession);
        public static PianoModuleSession Session => (PianoModuleSession)Instance._Session;
        public static StageData StageData { get; set; }
        public static GameshowData GameshowData { get; set; }
        public static AccessData AccessData { get; set; }
        public static MazeData MazeData { get; set; }
        public static Dictionary<string, MethodInfo> PassengerSetups = [];
        public static string CalidusFreedFlag = "CalidusFreed";
        public static string MapName
        {
            get
            {
                string o = "";
                if (Engine.Scene is Level level)
                {
                    o = level.Session.MapData.Data.Name;
                }
                return o;
            }
        }
        public static bool IsFromPuzzleIsland(Level level)
        {
            return level.Session.MapData.Data.Name.StartsWith("Piano_Boy/Puzzle_Island");
        }
        public static bool IsPuzzleIsland => MapName.StartsWith("Piano_Boy/Puzzle_Island");
        public static bool IsMap1 => MapName == "Piano_Boy/Puzzle_Island/map1";
        public static bool IsMap2 => MapName == "Piano_Boy/Puzzle_Island/map2";
        public PianoModule()
        {
            Instance = this;
        }
        public override void DeserializeSaveData(int index, byte[] data)
        {
            base.DeserializeSaveData(index, data);
            SaveData.InterfaceData = GetContent<InterfaceData>("InterfacePresets");
        }
        public override void PrepareMapDataProcessors(MapDataFixup context)
        {
            base.PrepareMapDataProcessors(context);
            context.Add<PianoMapDataProcessor>();
        }
        public override void DeserializeSession(int index, byte[] data)
        {
            base.DeserializeSession(index, data);
           
        }

        [OnLoadContent]
        public static void LoadCustomData()
        {
            StageData = GetContent<StageData>("Tutorial");
            GameshowData = GetContent<GameshowData>("GameshowQuestions");
            AccessData = GetContent<AccessData>("AccessLinks");
            MazeData = GetContent<MazeData>("MazeData");
            MazeData?.ParseData();
            StageData?.ParseData();
            GameshowData?.ParseData();
            AccessData?.ParseData();
        }
        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            InvokeAllWithAttribute(typeof(OnLoadContent), (c, m) => m.Invoke(null, null));
        }
        public override void Load()
        {
            InvokeAllWithAttribute(typeof(OnLoad), (c, m) => m.Invoke(null, null));
        }


        public override void Unload()
        {
            InvokeAllWithAttribute(typeof(OnUnload), (c, m) => m.Invoke(null, null));
        }

        public override void Initialize()
        {
            InvokeAllWithAttribute(typeof(OnInitialize), (c, m) => m.Invoke(null, null));
        }

        public static void InvokeAllWithAttribute(Type attributeType, Action<CustomAttributeData, MethodInfo> action, string logPrefix = null)
        {
            bool usesLog = !string.IsNullOrEmpty(logPrefix);
            Type attributeType2 = attributeType;
            Type[] typesSafe = typeof(PianoModule).Assembly.GetTypesSafe();
            if (usesLog)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                for (int i = 0; i < typesSafe.Length; i++)
                {
                    Stopwatch localStopwatch = new Stopwatch();
                    stopwatch.Start();
                    checkType(typesSafe[i]);
                    stopwatch.Stop();
                    Logger.Info("PuzzleIslandHelper", $"{logPrefix} Type: {typesSafe[i].Name}, Time Elapsed: {localStopwatch.Elapsed}.");
                }
                stopwatch.Stop();
                Logger.Info("PuzzleIslandHelper", logPrefix + " Time Elapsed: " + stopwatch.Elapsed.ToString());
            }
            else
            {
                for (int i = 0; i < typesSafe.Length; i++)
                {
                    checkType(typesSafe[i]);
                }
            }

            void checkType(Type type)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (MethodInfo method in methods)
                {
                    foreach (CustomAttributeData customAttribute in method.CustomAttributes)
                    {
                        if (customAttribute.AttributeType == attributeType2)
                        {
                            action?.Invoke(customAttribute, method);
                            return;
                        }
                    }
                }
            }
        }
        public static T GetContent<T>(string path)
        {
            T result = default;
            if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/" + path, out ModAsset asset))
            {
                asset.TryDeserialize(out result);
            }
            return result;
        }
    }
}