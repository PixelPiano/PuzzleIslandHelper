using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConstantEntityAttribute : Attribute
    {
        public string[] IDs;
        public string[] Maps;
        //
        // Summary:
        //     Mark this entity to be added to the level on Level.LoadingThread.
        //
        public ConstantEntityAttribute(params string[] ids) : base()
        {
            IDs = ids;
        }
        public ConstantEntityAttribute(string[] maps, params string[] ids) : base()
        {
            Maps = maps;
            IDs = ids;
        }
        public static class ConstantEntityLoader
        {
            public delegate Entity ContentLoader();
            public static readonly Dictionary<string, ContentLoader> ContentLoaders = new Dictionary<string, ContentLoader>();
            [OnLoad]
            public static void Load()
            {
                Assembly assembly = typeof(PianoModule).Assembly;
                Type[] types = assembly.GetTypesSafe();
                foreach (Type type in types)
                {
                    foreach (ConstantEntityAttribute customAttribute in type.GetCustomAttributes<ConstantEntityAttribute>())
                    {
                        string[] iDs = customAttribute.IDs;
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
                                    Logger.Log(LogLevel.Warn, "core", "Invalid number of constant entity ID elements: " + text + " (" + type.FullName + ")");
                                    continue;
                                }
                                text2 = array[0];
                                text3 = array[1];
                            }
                            text2 = text2.Trim();
                            text3 = text3.Trim();
                            ContentLoader loader = null;
                            ConstructorInfo ctor = type.GetConstructor(new Type[] { });
                            if (ctor != null)
                            {
                                loader = () => (Entity)ctor.Invoke(new object[] { });
                            }
                            if (loader == null)
                            {
                                Logger.Log(LogLevel.Warn, "PuzzleIslandHelper", "Found constant entity without suitable constructor / " + "It should contain a constructor with no parameters!");
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
                Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
            }
            [OnUnload]
            public static void Unload()
            {
                Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
            }
            [Command("isPuzzleIsland", "as")]
            public static void IsPuzzleIsland()
            {
                Engine.Commands.Log(PianoModule.IsPuzzleIsland);
                Engine.Commands.Log(PianoModule.MapName);
            }
            private static void LevelLoader_OnLoadingThread(Level level)
            {
                if (PianoModule.IsFromPuzzleIsland(level))
                {
                    foreach (var a in ContentLoaders)
                    {
                        ContentLoaders.TryGetValue(a.Key, out var value);
                        level.Add(value());
                    }
                }
            }
        }
    }
}