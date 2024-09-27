using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using System;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Celeste.Mod.PuzzleIslandHelper.MenuElements;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using IL.Monocle;
using Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities;
using FrostHelper;
using System.Reflection;

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
        public static InterfaceData InterfaceData { get; set; }
        public static GameshowData GameshowData { get; set; }
        public static AccessData AccessData { get; set; }
        public static MazeData MazeData { get; set; }
        public PianoModule()
        {
            Instance = this;
        }
        public override void PrepareMapDataProcessors(MapDataFixup context)
        {
            base.PrepareMapDataProcessors(context);
            context.Add<PianoMapDataProcessor>();

        }
        public static void LoadCustomData()
        {
            StageData = GetContent<StageData>("Tutorial");
            InterfaceData = GetContent<InterfaceData>("InterfacePresets");
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
            PipeSpout.StreamSpritesheet = GFX.Game["objects/PuzzleIslandHelper/waterPipes/streams"];
            for (int i = 0; i < 4; i++)
            {
                PipeSpout.DissolveTextures[i] = GFX.Game["objects/PuzzleIslandHelper/waterPipes/streamDissolve0" + i];
            }
            LoadCustomData();
            ShaderFX.LoadFXs();
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
        public override void Load()
        {
            InvokeAllWithAttribute(typeof(OnLoad));
        }


        public override void Unload()
        {
            InvokeAllWithAttribute(typeof(OnUnload));
        }

        public override void Initialize()
        {
            InvokeAllWithAttribute(typeof(OnInitialize));
        }

        public static void InvokeAllWithAttribute(Type attributeType)
        {
            Type attributeType2 = attributeType;
            Type[] typesSafe = typeof(PianoModule).Assembly.GetTypesSafe();
            for (int i = 0; i < typesSafe.Length; i++)
            {
                checkType(typesSafe[i]);
            }
            void checkType(Type type)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                foreach (MethodInfo method in methods)
                {
                    foreach (CustomAttributeData customAttribute in method.CustomAttributes)
                    {
                        if (customAttribute.AttributeType == attributeType2)
                        {
                            method.Invoke(null, null);
                            return;
                        }
                    }
                }
            }
        }
    }
}