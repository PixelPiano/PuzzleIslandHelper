using System;
using static Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities.LabGeneratorPuzzle.LGPOverlay;
using System.Collections.Generic;
using System.Linq;
using Monocle;
using FMOD;

namespace Celeste.Mod.PuzzleIslandHelper.PuzzleData
{
    public class InterfaceData
    {
        public class Presets
        {
            public class IconText
            {
                public void ParseData()
                {
                    if(ID == null)
                    {
                        ID = "";
                    }
                    if(Tab == null)
                    {
                        Tab = "";
                    }
                    if(Window == null)
                    {
                        Window = "";
                    }
                }
                public string ID { get; set; }
                public string Tab { get; set; }
                public string Window { get; set; }
            }
            public List<IconText> Icons { get; set;}
    }

    public Dictionary<string, Presets> Layouts { get; set; }
    public bool IsValid(string id)
    {
        if (Layouts == null || Layouts.Count == 0)
        {
            Console.WriteLine("Layouts is null!");
            return false;
        }
        if (!Layouts.ContainsKey(id))
        {
            Console.WriteLine("InterfaceData with key \"" + id + "\" does not exist");
            return false;
        }
        return Layouts.ContainsKey(id);
    }
    public Presets GetPreset(string id)
    {
        if (!IsValid(id))
        {
            return null;
        }
        else return Layouts[id];
    }
    public static void Load()
    {
        Everest.Content.OnUpdate += Content_OnUpdate;
    }
    public static void Unload()
    {
        Everest.Content.OnUpdate -= Content_OnUpdate;
    }
    private static void Content_OnUpdate(ModAsset from, ModAsset to)
    {
        if (to.Format == "yml" || to.Format == ".yml")
        {
            try
            {
                AssetReloadHelper.Do("Reloading Interface Presets", () =>
                {
                    if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/InterfacePresets", out var asset)
                        && asset.TryDeserialize(out InterfaceData myData))
                    {
                        PianoModule.InterfaceData = myData;
                    }
                }, () =>
                {
                    (Engine.Scene as Level)?.Reload();
                });

            }
            catch (Exception e)
            {
                Logger.LogDetailed(e);
            }
        }

    }
}

}
