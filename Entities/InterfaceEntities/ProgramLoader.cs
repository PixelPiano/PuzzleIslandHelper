using Celeste.Mod.Core;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
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
        public static void Load()
        {
            Assembly assembly = typeof(CoreModule).Assembly;
            Type[] types = assembly.GetTypesSafe();
            bool flag = false;
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
                        MethodInfo gen3 = type.GetMethod(text3, new Type[1]
                        {
                            typeof(BetterWindow),
                        });
                        if (gen3 != null && gen3.ReturnType.IsCompatible(typeof(WindowContent)))
                        {
                            loader = (BetterWindow window) => (WindowContent)gen3.Invoke(null, new object[1] { window });
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
