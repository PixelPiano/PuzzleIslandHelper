using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{

    //
    // Summary:
    //     Mark this entity as a Custom Passenger Cutscene
    //     Will add a cutscene to the scene if the passenger is talked to.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConstantEntityAttribute : Attribute
    {
        //
        // Summary:
        //     A list of unique identifiers for this Constant Entity.
        public string[] IDs;

        //
        // Summary:
        //     Mark this entity to be added to the level on Level.LoadingThread.
        //
        // Parameters:
        //   ids:
        //     A list of unique identifiers for this Constant Entity.
        public ConstantEntityAttribute(params string[] ids) : base()
        {
            IDs = ids;
        }

        public static class ConstantEntityLoader
        {
            public delegate Entity ContentLoader();
            public static readonly Dictionary<string, ContentLoader> ContentLoaders = new Dictionary<string, ContentLoader>();
            public static bool AddConstantEntity(string name, Level level)
            {
                var entity = Create(name);
                if (entity != null)
                {
                    level.Add(entity);
                }
                return entity != null;
            }
            public static Entity Create(string name)
            {
                if (ContentLoaders.TryGetValue(name, out var value))
                {
                    var cutscene = value();
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

            private static void LevelLoader_OnLoadingThread(Level level)
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