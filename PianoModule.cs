using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.ModIntegration;
using IL.Monocle;
using Microsoft.Xna.Framework;
using MonoMod.ModInterop;
using System;
using MonoMod.Utils;
using System.Collections.Generic;

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

        public PianoModule()
        {
            Instance = this;
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
            FluidMachine.Load();
            PotionFluid.Load();
            //FrostHelperImports.LoadIfNeeded();
        }
        public override void Unload()
        {
            Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;
            On.Celeste.PlayerHair.Render -= PlayerHair_Render;
            Stool.Unload();
            PuzzleSpotlight.Unload();
            MovingJelly.Unload();
            LightsIcon.Unload();
            PassThruBooster.Unload();
            InvertOverlay.Unload();
            BgTilesColorgrade.Unload();
            DigitalOverlay.Unload();
            FluidMachine.Unload();
            FluidBottle.Unload();
            PotionFluid.Unload();
        }

        public override void Initialize()
        {
            PuzzleSpotlight.Initialize();
        }
    }
}