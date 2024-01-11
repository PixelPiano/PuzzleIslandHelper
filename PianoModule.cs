using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.ModIntegration;
using IL.Monocle;
using Microsoft.Xna.Framework;
using MonoMod.ModInterop;
using System;
using MonoMod.Utils;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using System.Linq;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes;
using static MonoMod.InlineRT.MonoModRule;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;

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
        public static Dictionary<string, Effect> CutsceneShaders { get; set; }
        public PianoModule()
        {
            Instance = this;
        }
        public override void PrepareMapDataProcessors(MapDataFixup context)
        {
            base.PrepareMapDataProcessors(context);

            context.Add<PianoMapDataProcessor>();
        }
        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);


            PipeSpout.StreamSpritesheet = GFX.Game["objects/PuzzleIslandHelper/waterPipes/streams"];
            PipeSpout.DissolveTextures[0] = GFX.Game["objects/PuzzleIslandHelper/waterPipes/streamDissolve00"];
            PipeSpout.DissolveTextures[1] = GFX.Game["objects/PuzzleIslandHelper/waterPipes/streamDissolve01"];
            PipeSpout.DissolveTextures[2] = GFX.Game["objects/PuzzleIslandHelper/waterPipes/streamDissolve02"];
            PipeSpout.DissolveTextures[3] = GFX.Game["objects/PuzzleIslandHelper/waterPipes/streamDissolve03"];
            if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/Tutorial", out var asset)
                && asset.TryDeserialize(out StageData myData))
            {
                StageData = myData;
                foreach (KeyValuePair<string, GeneratorStage> pair in StageData.Stages)
                {
                    StageData.Stages[pair.Key].ParseData();
                }
            }
            if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/InterfacePresets", out var asset2)
                && asset2.TryDeserialize(out InterfaceData myData2))
            {
                InterfaceData = myData2;
            }
            if(Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/GameshowQuestions", out var asset3) 
                && asset3.TryDeserialize(out GameshowData myData3))
            {
                GameshowData = myData3;
                GameshowData.ParseData();
            }
            BlockGlitch.Shader = ShaderHelper.TryGetEffect("jitter");
            MonitorDecalGroup.Shader = ShaderHelper.TryGetEffect("monitorDecal");
            ArtifactTester.Shader = (LCDParallax.Shader = ShaderHelper.TryGetEffect("static"));
            LCDParallax.Shader = ShaderHelper.TryGetEffect("lcd");
            LCDParallax.MaskShader = ShaderHelper.TryGetEffect("sineLines");
            ViewContent.Shader = ShaderHelper.TryGetEffect("curvedScreen");
            CutsceneShaders = new()
            {
                {"GrassShift",ShaderHelper.TryGetEffect("fuzzyNoise")}
            };
        }
        private void PlayerHair_Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            if (DigitalEffect.RenderCondition) orig(self);
        }
        private Backdrop Level_OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above)
        {
            if (child.Name.Equals("PuzzleIslandHelper/HairColorOverride", StringComparison.OrdinalIgnoreCase))
            {
                bool[] bools = new bool[7];
                bools[0] = child.AttrBool("zero");
                bools[1] = child.AttrBool("one");
                bools[2] = child.AttrBool("two");
                bools[3] = child.AttrBool("three");
                bools[4] = child.AttrBool("four");
                bools[5] = child.AttrBool("five");
                bools[6] = child.AttrBool("six");
                return new HairColorOverride(child.Attr("noDashes"), child.Attr("oneDash"),
                                            child.Attr("twoDashes"), child.Attr("threeDashes"),
                                            child.Attr("fourDashes"), child.Attr("fiveDashes"),
                                            child.Attr("sixDashes"), bools);
            }
            if (child.Name.Equals("PuzzleIslandHelper/DigitalOverlay", StringComparison.OrdinalIgnoreCase))
            {
                return new DigitalOverlay(child.AttrBool("leaveOutHair", true), child.AttrBool("backFlicker"), child.AttrBool("lineFlicker"));
            }
            if (child.Name.Equals("PuzzleIslandHelper/DigitalGrid", StringComparison.OrdinalIgnoreCase))
            {
                return new DigitalGrid(child.AttrFloat("verticalLineWidth", 4),
                                       child.AttrFloat("horizontalLineHeight", 2),
                                       child.AttrFloat("rateX", 4),
                                       child.AttrFloat("rateY", 4),
                                       child.AttrInt("xSpacing", 24),
                                       child.AttrInt("ySpacing", 24),
                                       child.Attr("color", "00ff00"),
                                       child.AttrBool("moving", true),
                                       child.AttrFloat("verticalLineAngle", 10),
                                       child.AttrFloat("horizontalLineAngle", 10),
                                       child.AttrFloat("Opacity", 1f),
                                       child.AttrBool("verticalLines", true),
                                       child.AttrBool("horizontalLines", true),
                                       child.AttrBool("blur", true),
                                       child.AttrBool("glitch", false));
            }
            if (child.Name.Equals("PuzzleIslandHelper/InvertOverlay", StringComparison.OrdinalIgnoreCase))
            {
                return new InvertOverlay(child.Attr("colorgradeFlag"), child.AttrFloat("timeMod"));
            }
            if (child.Name.Equals("PuzzleIslandHelper/ColorgradeOverlay", StringComparison.OrdinalIgnoreCase))
            {
                return new ColorgradeOverlay(child.Attr("colorgradeFlag"),
                                             child.Attr("colorgradeWhenTrue", "oldsite"),
                                             child.Attr("colorgradeWhenFalse", "none"),
                                             child.AttrBool("fadeOnFlagSwitch"),
                                             child.AttrFloat("timeModWhenTrue"),
                                             child.AttrFloat("timeModWhenFalse"));
            }
            if (child.Name.Equals("PuzzleIslandHelper/BgTilesColorgrade", StringComparison.OrdinalIgnoreCase))
            {
                return new BgTilesColorgrade(child.Attr("colorgrade"));
            }
            if (child.Name.Equals("PuzzleIslandHelper/ParallaxWindow", StringComparison.OrdinalIgnoreCase))
            {
                return new ParallaxWindow(child.Attr("flag"));
            }
            if (child.Name.Equals("PuzzleIslandHelper/BlockGlitch", StringComparison.OrdinalIgnoreCase))
            {
                return new BlockGlitch();
            }
            if (child.Name.Equals("PuzzleIslandHelper/LCDParallax", StringComparison.OrdinalIgnoreCase))
            {
                return new LCDParallax(child);
            }
            if (child.Name.Equals("PuzzleIslandHelper/Testtt", StringComparison.OrdinalIgnoreCase))
            {
                return new Testtt();
            }
            return null;
        }
        public override void Load()
        {
            Everest.Events.Level.OnLoadBackdrop += Level_OnLoadBackdrop;
            On.Celeste.PlayerHair.Render += PlayerHair_Render;
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
            MonitorDecalGroup.Load();
            DeadRefill.Load();
            LabTubeLight.Load();
            StageData.Load();
            RenderHelper.Load();
            LCDParallax.Load();
            InterfaceData.Load();
            GameshowData.Load();
            ViewContent.Load();
            ShaderOverlay.Load();
            PortraitRuiner.Load();
            AudioEffectGlobal.Load();
        }
        public override void Unload()
        {
            Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;
            On.Celeste.PlayerHair.Render -= PlayerHair_Render;
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
            MonitorDecalGroup.Unload();
            DeadRefill.Unload();
            StageData.Unload();
            RenderHelper.Unload();
            LCDParallax.Unload();
            LCDArea.Unload();
            InterfaceData.Unload();
            ViewContent.Unload();
            GameshowData.Unload();
            ShaderOverlay.Unload();
            PortraitRuiner.Unload();
            AudioEffectGlobal.Unload();

        }

        public override void Initialize()
        {
            PuzzleSpotlight.Initialize();
        }
    }
}