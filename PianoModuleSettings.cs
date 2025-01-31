using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.PuzzleIslandHelper
{
    [SettingName("modoptions_PuzzleIslandHelperModule")]
    public class PianoModuleSettings : EverestModuleSettings
    {
        [SettingName("modoptions_PuzzleIslandHelperModule_DigitalHair")]
        public bool RenderDigitalHair { get; set; } = true;

        [SettingName("modoptions_PuzzleIslandHelperModule_InvState")]
        public bool InvertAbility { get; set; } = false;
        [SettingName("modoptions_PuzzleIslandHelperModule_HideCollectableIndicators")]
        public bool HideCollectableIndicators { get; set; } = false;

        [SettingName("modoptions_PuzzleIslandHelperModule_InvIntensity")]
        [SettingSubText("modoptions_PuzzleIslandHelperModule_InvIntensity_desc")]
        [SettingRange(1, 5)]
        public int InvertEffectIntensity { get; set; } = 5;

        [DefaultButtonBinding(Buttons.RightStick, Keys.F)]
        public ButtonBinding InvertAbilityBinding { get; set; }
        public enum InvertActivationModes
        {
            Toggle,
            Hold
        }
        public enum StickyHoldMode
        {
            Click,
            Hold
        }

        [SettingSubText("modoptions_PuzzleIslandHelperModule_InvToggle_desc")]
        public InvertActivationModes ToggleInvert { get; set; } = InvertActivationModes.Hold;

        [SettingSubText("modoptions_PuzzleIslandHelperModule_InvToggle_desc")]
        public StickyHoldMode ToggleSticky { get; set; } = StickyHoldMode.Click;
    }
}
