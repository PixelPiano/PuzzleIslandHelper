namespace Celeste.Mod.PuzzleIslandHelper
{
    [SettingName("modoptions_PuzzleIslandHelperModule")]
    public class PianoModuleSettings : EverestModuleSettings
    {

        [SettingName("modoptions_PuzzleIslandHelperModule_InvState")]
        //[SettingIgnore]
        public bool InvertAbility {get; set;} = false;


        [SettingName("modoptions_PuzzleIslandHelperModule_InvIntensity")]
        [SettingSubText("modoptions_PuzzleIslandHelperModule_InvIntensity_desc")]
        [SettingRange(1, 5)]
        public int InvertEffectIntensity { get; set; } = 5;
    }
}
