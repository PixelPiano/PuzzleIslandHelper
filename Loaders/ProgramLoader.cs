using Celeste.Mod.PuzzleIslandHelper.Attributes;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Loaders
{
    public static class TerminalProgramLoader
    {
        public delegate TerminalProgram ContentLoader(FakeTerminal terminal);
        public static readonly Dictionary<string, ContentLoader> ContentLoaders = new Dictionary<string, ContentLoader>();
        public static bool LoadCustomProgram(string name, FakeTerminal terminal, Level level)
        {
            if (ContentLoaders.TryGetValue(name, out var value))
            {
                var program = value(terminal);
                if (program != null)
                {
                    program.Name = name;
                    level.Add(program);
                    return true;
                }
            }
            return false;
        }
        [OnLoad]
        public static void Load()
        {
            Assembly assembly = typeof(PianoModule).Assembly;
            Type[] types = assembly.GetTypesSafe();
            foreach (Type type in types)
            {
                foreach (TerminalProgramAttribute customAttribute in type.GetCustomAttributes<TerminalProgramAttribute>())
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
                                Logger.Log(LogLevel.Warn, "core", "Invalid number of custom program ID elements: " + text + " (" + type.FullName + ")");
                                continue;
                            }
                            text2 = array[0];
                            text3 = array[1];
                        }
                        text2 = text2.Trim();
                        text3 = text3.Trim();
                        ContentLoader loader = null;
                        ConstructorInfo ctor = type.GetConstructor(new Type[] { typeof(FakeTerminal) });
                        if (ctor != null)
                        {
                            loader = (terminal) => (TerminalProgram)ctor.Invoke(new object[] { terminal });
                        }
                        if (loader == null)
                        {
                            Logger.Log(LogLevel.Warn, "PuzzleIslandHelper", "Found custom terminal program without suitable constructor / " + text3 + "(FakeTerminal): " + text2 + " (" + type.FullName + ")");
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
