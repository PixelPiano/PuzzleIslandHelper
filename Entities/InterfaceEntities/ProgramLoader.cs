using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Core;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public static class ProgramLoader
    {
        public delegate WindowContent ContentLoader(BetterWindow window);
        public static readonly Dictionary<string, ContentLoader> ContentLoaders = new Dictionary<string, ContentLoader>();
        public static bool LoadCustomProgram(string name, BetterWindow window, Level level)
        {
            if (ContentLoaders.TryGetValue(name, out var value))
            {
                var program = value(window);
                if (program != null)
                {
                    program.Name = name;
                    if (PianoModule.Session.Interface is Interface inter)
                    {
                        if (inter.GetProgram(name) is WindowContent content)
                        {
                            content.Window = inter.Window;
                            return true;
                        }
                        inter.Content.Add(program);
                    }
                    level.Add(program);
                    return true;
                }
            }
            return false;
        }
        public static void Load()
        {
            Assembly assembly = typeof(PianoModule).Assembly;
            Type[] types = assembly.GetTypesSafe();
            foreach (Type type in types)
            {
                foreach (CustomProgramAttribute customAttribute in type.GetCustomAttributes<CustomProgramAttribute>())
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
                        ConstructorInfo ctor = type.GetConstructor(new Type[] { typeof(BetterWindow) });
                        if (ctor != null)
                        {
                            loader = (BetterWindow window) => (WindowContent)ctor.Invoke(new object[] { window });
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
