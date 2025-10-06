using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.PuzzleIslandHelper
{
    [SettingName("modoptions_PuzzleIslandHelperModule")]
    public class PianoModuleSettings : EverestModuleSettings
    {
        public enum StickyHoldMode
        {
            Click,
            Hold
        }
        [SettingName("modoptions_PuzzleIslandHelperModule_DigitalHair")]
        public bool RenderDigitalHair { get; set; } = true;
        [SettingName("modoptions_PuzzleIslandHelperModule_HideCollectableIndicators")]
        public bool HideCollectableIndicators { get; set; } = true;
        [SettingSubText("modoptions_PuzzleIslandHelperModule_InvToggle_desc")]
        public StickyHoldMode ToggleSticky { get; set; } = StickyHoldMode.Click;
        public string DebugShaderFolder { get; set; } = "PuzzleIslandHelper";
        public enum WCDM
        {
            Hidden,
            Lined 
        }
        public WCDM WarpCapsuleDisplayMode {get; set;} = WCDM.Lined;
    }
}
