﻿// PuzzleIslandHelper.PuzzleIslandHelperCommands
using Celeste;
using Celeste.Mod;
using Celeste.Mod.PuzzleIslandHelper;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

    public static void LoadFXs()
    {
        Jitter = LoadFx("jitter");
        MonitorDecal = LoadFx("monitorDecal");
        Static = LoadFx("static");
        LCD = LoadFx("lcd");
        SineLines = LoadFx("sineLines");
        CurvedScreen = LoadFx("curvedScreen");
        FuzzyNoise = LoadFx("fuzzyNoise");
        FuzzyAppear = LoadFx("fuzzyAppear");
    }
    public static void DisposeFXs()
    {
        Jitter?.Dispose();
        MonitorDecal?.Dispose();
        Static?.Dispose();
        LCD?.Dispose();
        SineLines?.Dispose();
        CurvedScreen?.Dispose();
        FuzzyNoise?.Dispose();
        FuzzyAppear?.Dispose();
    }
    public static void Load()
    {
        Everest.Content.OnUpdate += Content_OnUpdate;
    }
    public static void Unload()
    {
        DisposeFXs();
        Everest.Content.OnUpdate -= Content_OnUpdate;
    }
    public static Effect LoadFx(string id, bool fullPath = false)
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
                Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Failed to load the shader " + id);
                Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Exception: \n" + ex.ToString());
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
                    LoadFXs();
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