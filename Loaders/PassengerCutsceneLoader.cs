using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Loaders
{
    public static class PassengerCutsceneLoader
    {
        public delegate PassengerCutscene ContentLoader(Passenger passenger, Player player);
        public static readonly Dictionary<string, ContentLoader> ContentLoaders = new Dictionary<string, ContentLoader>();
        public static bool LoadCustomCutscene(string name, Passenger passenger, Player player, Level level)
        {
            var cutscene = CreateCutscene(name, passenger, player);
            if(cutscene != null)
            {
                level.Add(cutscene);
            }
            return cutscene != null;
        }
        public static bool HasCutscene(string name)
        {
            return ContentLoaders.ContainsKey(name);
        }
        public static PassengerCutscene CreateCutscene(string name, Passenger passenger, Player player)
        {
            if (ContentLoaders.TryGetValue(name, out var value))
            {
                var cutscene = value(passenger, player);
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
                foreach (CustomPassengerCutsceneAttribute customAttribute in type.GetCustomAttributes<CustomPassengerCutsceneAttribute>())
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
                                Logger.Log(LogLevel.Warn, "core", "Invalid number of custom passenger cutscene ID elements: " + text + " (" + type.FullName + ")");
                                continue;
                            }
                            text2 = array[0];
                            text3 = array[1];
                        }
                        text2 = text2.Trim();
                        text3 = text3.Trim();
                        ContentLoader loader = null;
                        ConstructorInfo ctor = type.GetConstructor(new Type[] { typeof(Passenger), typeof(Player) });
                        if (ctor != null)
                        {
                            loader = (passenger, player) => (PassengerCutscene)ctor.Invoke(new object[] { passenger, player });
                        }
                        if (loader == null)
                        {
                            Logger.Log(LogLevel.Warn, "PuzzleIslandHelper", "Found passenger cutscene without suitable constructor / " + text3 + "(PassengerWIP): " + text2 + " (" + type.FullName + ")");
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
