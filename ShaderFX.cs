// PuzzleIslandHelper.PuzzleIslandHelperCommands
using Celeste;
using Celeste.Mod;
using Celeste.Mod.PuzzleIslandHelper;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

public class ShaderFX
{
    private static HashSet<Effect> effects = [];
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
    public static Effect Monument;
    public static Effect WhiteOut;

    [OnLoadContent]
    public static void LoadFx()
    {
        Monument = LoadEffect("monument");
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
        WhiteOut = LoadEffect("whiteOut");

    }
    public static void DisposeFXs()
    {
        foreach(Effect effect in effects)
        {
            effect?.Dispose();
        }
        effects.Clear();
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
        Effect effect = null;
        string name = fullPath ? $"Effects/{id}.cso" : $"Effects/PuzzleIslandHelper/Shaders/{id}.cso";
        if (Everest.Content.TryGet(name, out var effectAsset, true))
        {
            try
            {
                effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
            }
            catch (Exception ex)
            {
                throw new Exception("PuzzleIslandHelper/ShaderFX: Unable to load the Shader " + id, ex);
                /*                Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Failed to load the Shader " + ID);
                                Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Exception: \n" + ex.ToString());*/
            }
        }
        if(effect != null)
        {
            effects.Add(effect);
        }
        return effect;
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
