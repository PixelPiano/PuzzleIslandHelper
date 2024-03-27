using System;
using System.Collections.Generic;
using Monocle;


namespace Celeste.Mod.PuzzleIslandHelper.PuzzleData
{
    public class AccessData
    {
        public class Link
        {
            public void ParseData()
            {
                if (ID == null)
                {
                    ID = "";
                }
                if (Room == null)
                {
                    Room = "";
                }
            }
            public string ID { get; set; }
            public string Room { get; set; }
            public float Wait { get; set; }
        }

        public List<Link> Links { get; set; }
        public bool HasID(string id)
        {
            if (Links is null || Links.Count == 0) return false;
            return Links.Find(item => item.ID == id) != null;
        }
        public void ParseData()
        {
            for(int i = 0;i < Links.Count; i++)
            {
                Console.WriteLine("Parsing links index "+i);
                Links[i].ParseData();
            }
        }
        public Link GetLink(string id)
        {
            for(int i = 0; i < Links.Count; i++)
            {
                Console.WriteLine($"LinkId: {Links[i].ID}, input: {id}");
                if(Links[i].ID == id) return Links[i];
            }
            return null;
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
                    AssetReloadHelper.Do("Reloading Access Data", () =>
                    {
                        if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/AccessLinks", out var asset)
                            && asset.TryDeserialize(out AccessData myData))
                        {
                            PianoModule.AccessData = myData;
                        }
                    }, () =>
                    {
                        (Engine.Scene as Level)?.Reload();
                    });

                }
                catch (Exception e)
                {
                    Logger.LogDetailed(e, "MAJOR FUCK UP");
                }
            }

        }
    }

}
