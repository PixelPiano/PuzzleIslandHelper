using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Calidus;

namespace Celeste.Mod.PuzzleIslandHelper.Loaders
{
    public static class CalidusCutsceneLoader
    {
        public delegate CalidusCutscene ContentLoader(Player player, Calidus calidus, Arguments start, Arguments end);
        public static readonly Dictionary<string, ContentLoader> ContentLoaders = new Dictionary<string, ContentLoader>();
        public static bool LoadCustomCutscene(string name, Player player, Calidus calidus, string startArgs, string endArgs, Level level)
        {
            var cutscene = CreateCutscene(name, player, calidus, startArgs, endArgs);
            if (cutscene != null)
            {
                level.Add(cutscene);
            }
            return cutscene != null;
        }
        public static bool HasCutscene(string name)
        {
            return ContentLoaders.ContainsKey(name);
        }
        public static CalidusCutscene CreateCutscene(string name, Player player, Calidus calidus, string startArgs, string endArgs)
        {
            Arguments start = ParseArgs(startArgs);
            Arguments end = ParseArgs(endArgs);
            if (ContentLoaders.TryGetValue(name, out var value))
            {
                CalidusCutscene cutscene = value(player, calidus, start, end);
                cutscene.CutsceneID = name;
                return cutscene;
            }
            return null;
        }
        [OnLoad]
        public static void Load()
        {
            Assembly assembly = typeof(PianoModule).Assembly;
            Type[] types = assembly.GetTypesSafe();
            foreach (Type type in types)
            {
                foreach (CalidusCutsceneAttribute customAttribute in type.GetCustomAttributes<CalidusCutsceneAttribute>())
                {
                    string[] iDs = customAttribute.CustomIDs;
                    foreach (string text in iDs)
                    {
                        string[] array = text.Split('=');
                        string text2;
                        string text3;
                        if (array.Length == 1)
                        {
                            text2 = array[0];
                            text3 = "Load";
                        }
                        else
                        {
                            if (array.Length != 2)
                            {
                                Logger.Log(LogLevel.Warn, "core", "Invalid number of custom calidus cutscene ID elements: " + text + " (" + type.FullName + ")");
                                continue;
                            }
                            text2 = array[0];
                            text3 = array[1];
                        }
                        text2 = text2.Trim();
                        text3 = text3.Trim();
                        ContentLoader loader = null;
                        ConstructorInfo ctor = type.GetConstructor([typeof(Player), typeof(Calidus), typeof(Arguments), typeof(Arguments)]);
                        if (ctor != null)
                        {
                            loader = (player, calidus, start, end) => (CalidusCutscene)ctor.Invoke([player, calidus, start, end]);
                        }
                        if (loader == null)
                        {
                            Logger.Log(LogLevel.Warn, "PuzzleIslandHelper", "Found calidus cutscene without suitable constructor / " + text3 + "/" + text2 + " (" + type.FullName + ")");
                        }
                        else
                        {
                            if (!ContentLoaders.ContainsKey(text2))
                            {
                                ContentLoaders.Add(text2, loader);
                            }
                            else
                            {
                                ContentLoaders[text2] = loader;
                            }
                        }
                    }
                }
            }
        }
    }
}
