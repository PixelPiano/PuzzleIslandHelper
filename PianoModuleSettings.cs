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
/*        public enum HomeTransportMethods
        {
            Screen,
            Machine
        }
        [SettingName("modoptions_PuzzleIslandHelperModule_TransportMethod")]
        public HomeTransportMethods HomeTransportMethod {get; set; } = HomeTransportMethods.Machine;*/
        [SettingName("modoptions_PuzzleIslandHelperModule_DigitalHair")]
        public bool RenderDigitalHair { get; set; } = true;
        [SettingName("modoptions_PuzzleIslandHelperModule_HideCollectableIndicators")]
        public bool HideCollectableIndicators { get; set; } = false;
        [SettingSubText("modoptions_PuzzleIslandHelperModule_InvToggle_desc")]
        public StickyHoldMode ToggleSticky { get; set; } = StickyHoldMode.Click;
    }
}
