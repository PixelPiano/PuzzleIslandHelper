using Celeste.Mod.PuzzleIslandHelper.Attributes;
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
    public static class CutsceneLoader
    {
        public delegate CutsceneEntity ContentLoader();
        public static readonly Dictionary<string, ContentLoader> ContentLoaders = new Dictionary<string, ContentLoader>();
        public static bool LoadCustomCutscene(string name, Level level)
        {
            var cutscene = CreateCutscene(name);
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
        public static CutsceneEntity CreateCutscene(string name)
        {
            if (ContentLoaders.TryGetValue(name, out var value))
            {
                CutsceneEntity cutscene = value();
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
                foreach (CutsceneAttribute customAttribute in type.GetCustomAttributes<CutsceneAttribute>())
                {
                    string[] iDs = customAttribute.CustomIDs;
                    foreach (string text in iDs)
                    {
                        string[] array = text.Split('=');
                        string text2;
                        if (array.Length == 1)
                        {
                            text2 = array[0];
                        }
                        else
                        {
                            if (array.Length != 2)
                            {
                                Logger.Log(LogLevel.Warn, "core", "Invalid number of custom calidus cutscene ID elements: " + text + " (" + type.FullName + ")");
                                continue;
                            }
                            text2 = array[0];
                        }
                        text2 = text2.Trim();
                        ContentLoader loader = null;
                        ConstructorInfo ctor = type.GetConstructor([]);
                        if (ctor != null)
                        {
                            loader = () => (CutsceneEntity)ctor.Invoke([]);
                        }
                        if (loader == null)
                        {
                            Logger.Log(LogLevel.Warn, "PuzzleIslandHelper", "Found Cutscene without suitable constructor");
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
