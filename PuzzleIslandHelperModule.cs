using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using System;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PuzzleIslandHelperModule : EverestModule {
        public static PuzzleIslandHelperModule Instance { get; private set; }
        public override Type SettingsType => typeof(PuzzleIslandHelperModuleSettings);
        public static PuzzleIslandHelperModuleSettings Settings => (PuzzleIslandHelperModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(PuzzleIslandHelperModuleSession);
        public static PuzzleIslandHelperModuleSession Session => (PuzzleIslandHelperModuleSession) Instance._Session;

        public PuzzleIslandHelperModule() {
            Instance = this;
        }
        private void PlayerHair_Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            if (DigitalEffect.RenderCondition) orig(self);
        }
        private Backdrop Level_OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above)
        {
            if (child.Name.Equals("PuzzleIslandHelper/DigitalGrid", StringComparison.OrdinalIgnoreCase))
            {
                return new DigitalGrid(child.AttrFloat("verticalLineWidth",4),
                                       child.AttrFloat("horizontalLineHeight",2),
                                       child.AttrFloat("rateX",4),
                                       child.AttrFloat("rateY", 4),
                                       child.AttrInt("xSpacing", 24),
                                       child.AttrInt("ySpacing", 24),
                                       child.Attr("color","00ff00"),
                                       child.AttrBool("moving",true),
                                       child.AttrFloat("verticalLineAngle",10),
                                       child.AttrFloat("horizontalLineAngle",10),
                                       child.AttrFloat("Opacity",1f),
                                       child.AttrBool("verticalLines", true),
                                       child.AttrBool("horizontalLines", true),
                                       child.AttrBool("blur", true),
                                       child.AttrBool("glitch", false));
            }
            return null;
        }
        public override void Load() {
            Stool.Load();
            PuzzleSpotlight.Load();
            WaveformRenderer.Load();
            DigitalEffect.Load();
            Everest.Events.Level.OnLoadBackdrop += Level_OnLoadBackdrop;
            On.Celeste.PlayerHair.Render += PlayerHair_Render;
            MovingJelly.Load();
            EscapeTimer.Load();
            // TODO: apply any hooks that should always be active
        }

        public override void Unload() {
            Stool.Unload();
            //SpookySpotlightRenderer.Unload();
            PuzzleSpotlight.Unload();
            WaveformRenderer.Unload();
            Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;
            On.Celeste.PlayerHair.Render -= PlayerHair_Render;
            MovingJelly.Unload();
            //Grapher.Unload();
            // TODO: unapply any hooks applied in Load()
        }
        
        public override void Initialize(){
            PuzzleSpotlight.Initialize();
            WaveformRenderer.Initialize();
            //Grapher.Initialize();
        }
    }
}