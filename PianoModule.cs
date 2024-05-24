using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using System;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities.GearEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;

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
            InputBox.Load();
            Stool.Load();
            PuzzleSpotlight.Load();
            DigitalEffect.Load();
            MovingJelly.Load();
            EscapeTimer.Load();
            LightMachineInfo.Load();
            LightsIcon.Load();
            PassThruBooster.Load();
            InvertOverlay.Load();
            BgTilesColorgrade.Load();
            DigitalOverlay.Load();
            PotionFluid.Load();
            BlockGlitch.Load();
            DeadRefill.Load();
            LabTubeLight.Load();
            StageData.Load();
            RenderHelper.Load();
            InterfaceData.Load();
            GameshowData.Load();
            ShaderOverlay.Load();
            PortraitRuiner.Load();
            AudioEffectGlobal.Load();
            AccessProgram.Load();
            AccessData.Load();
            GearHolder.GearHolderRenderer.Load();
            SceneSwitch.Load();
            ShaderFX.Load();
            PrologueBooster.Load();
            PrologueSequence.Load();
            ShiftAreaRenderer.Load();
            ProgramLoader.Load();
            MazeData.Load();
        }


        public override void Unload()
        {
            InputBox.Unload();
            ShaderFX.Unload();
            LabTubeLight.Unload();
            Stool.Unload();
            PuzzleSpotlight.Unload();
            MovingJelly.Unload();
            LightsIcon.Unload();
            PassThruBooster.Unload();
            InvertOverlay.Unload();
            BgTilesColorgrade.Unload();
            DigitalOverlay.Unload();
            FluidBottle.Unload();
            PotionFluid.Unload();
            BlockGlitch.Unload();
            DeadRefill.Unload();
            StageData.Unload();
            RenderHelper.Unload();
            LCDArea.Unload();
            InterfaceData.Unload();
            GameshowData.Unload();
            ShaderOverlay.Unload();
            PortraitRuiner.Unload();
            AudioEffectGlobal.Unload();
            AccessProgram.Unload();
            AccessData.Unload();
            GearHolder.GearHolderRenderer.Unload();
            SceneSwitch.Unload();
            BetaCube.Unload();
            PrologueBooster.Unload();
            PrologueSequence.Unload();
            PrologueBlock.Unload();
            ShiftAreaRenderer.Unload();
            GrassMazeOverlay.Unload();
            MazeData.Unload();
            GSRenderer.Unload();
        }

        public override void Initialize()
        {
            PuzzleSpotlight.Initialize();
            BetaCube.Initialize();
        }
    }
}