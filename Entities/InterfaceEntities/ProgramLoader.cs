using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public static class ProgramLoader
    {
        public delegate WindowContent ContentLoader(Window window);
        public static readonly Dictionary<string, ContentLoader> ContentLoaders = new Dictionary<string, ContentLoader>();
        public static bool LoadCustomProgram(string name, Window window, Level level)
        {
            if (ContentLoaders.TryGetValue(name, out var value))
            {
                var program = value(window);
                if (program != null)
                {
                    program.Name = name;
                    if (window != null && window.Interface is Interface inter)
                    {
                        if (inter.GetProgram(name) is WindowContent content)
                        {
                            content.Window = inter.Window;
                            return true;
                        }
                        inter.Content.Add(program);
                        level.Add(program);
                        return true;
                    }
                    return false;
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
                foreach (CustomProgram customAttribute in type.GetCustomAttributes<CustomProgram>())
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
                        ConstructorInfo ctor = type.GetConstructor(new Type[] { typeof(Window) });
                        if (ctor != null)
                        {
                            loader = (Window window) => (WindowContent)ctor.Invoke(new object[] { window });
                        }
                        if (loader == null)
                        {
                            Logger.Log(LogLevel.Warn, "PuzzleIslandHelper", "Found custom program without suitable constructor / " + text3 + "(BetterWindow): " + text2 + " (" + type.FullName + ")");
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
