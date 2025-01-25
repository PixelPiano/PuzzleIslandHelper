// PuzzleIslandHelper.PuzzleIslandHelperCommands
using Celeste;
using Celeste.Mod;
using Celeste.Mod.PuzzleIslandHelper;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

public class ShaderFX
{
    public static Effect Jitter;
    public static Effect MonitorDecal;
    public static Effect Static;
    public static Effect LCD;
    public static Effect SineLines;
    public static Effect CurvedScreen;
    public static Effect FuzzyNoise;
    public static Effect FuzzyAppear;
    public static Effect Shine;
    public static Effect PlayerStatic;
    public static Effect Sway;
    public static Effect GlitchAura;
    public static Effect InvertOrb;
    public static Effect BitrailAbsorb;
    public static Effect Scroll;
    public static void LoadFx()
    {
        Scroll = LoadEffect("scroll");
        Jitter = LoadEffect("jitter");
        InvertOrb = LoadEffect("invertOrb");
        MonitorDecal = LoadEffect("monitorDecal");
        Static = LoadEffect("static");
        LCD = LoadEffect("lcd");
        SineLines = LoadEffect("sineLines");
        CurvedScreen = LoadEffect("curvedScreen");
        FuzzyNoise = LoadEffect("fuzzyNoise");
        FuzzyAppear = LoadEffect("fuzzyAppear");
        Shine = LoadEffect("shine");
        PlayerStatic = LoadEffect("playerStatic");
        Sway = LoadEffect("huskSway");
        GlitchAura = LoadEffect("glitchAura");
        BitrailAbsorb = LoadEffect("bitrailAbsorb");

    }
    public static void DisposeFXs()
    {
        Scroll?.Dispose();
        Jitter?.Dispose();
        MonitorDecal?.Dispose();
        Static?.Dispose();
        LCD?.Dispose();
        SineLines?.Dispose();
        CurvedScreen?.Dispose();
        FuzzyNoise?.Dispose();
        FuzzyAppear?.Dispose();
        Shine?.Dispose();
        PlayerStatic?.Dispose();
        Sway?.Dispose();
        GlitchAura?.Dispose();
        InvertOrb?.Dispose();
        BitrailAbsorb?.Dispose();
    }
    [OnLoad]
    public static void Load()
    {
        //LoadFx();
        Everest.Content.OnUpdate += Content_OnUpdate;
    }
    [OnUnload]
    public static void Unload()
    {
        DisposeFXs();
        Everest.Content.OnUpdate -= Content_OnUpdate;
    }
    public static Effect LoadEffect(string id, bool fullPath = false)
    {
        id = id.Replace('\\', '/');

        string name = fullPath ? $"Effects/{id}.cso" : $"Effects/PuzzleIslandHelper/Shaders/{id}.cso";
        if (Everest.Content.TryGet(name, out var effectAsset, true))
        {
            try
            {
                Effect effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
                return effect;
            }
            catch (Exception ex)
            {
                throw new Exception("PuzzleIslandHelper/ShaderFX: Unable to load the Shader " + id, ex);
                /*                Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Failed to load the Shader " + ID);
                                Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Exception: \n" + ex.ToString());*/
            }
        }
        return null;
    }
    private static void Content_OnUpdate(ModAsset from, ModAsset to)
    {
        if (to.Format == "cso" || to.Format == ".cso")
        {
            try
            {
                AssetReloadHelper.Do("Reloading Shaders", () =>
                {
                    DisposeFXs();
                    LoadFx();
                }, () =>
                {
                    (Engine.Scene as Level)?.Reload();
                });

            }
            catch (Exception e)
            {
                // there's a catch-all filter on Content.OnUpdate that completely ignores the exception,
                // would nice to actually see it though
                Logger.LogDetailed(e);
            }

        }
    }
}
