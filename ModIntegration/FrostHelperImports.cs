using MonoMod.ModInterop;

namespace Celeste.Mod.PuzzleIslandHelper.ModIntegration;

[ModImportName("FrostHelper")]
public class FrostHelperImports
{
    public static bool LoadIfNeeded()
    {
        if (Loaded)
            return true;

        typeof(FrostHelperImports).ModInterop();

        Loaded = true;

        return true;
    }

    public static bool Loaded { get; private set; }
}