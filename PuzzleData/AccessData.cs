using System;
using System.Collections.Generic;
using System.Linq;
using Monocle;


namespace Celeste.Mod.PuzzleIslandHelper.PuzzleData
{
    public class AccessData
    {
        public class Link
        {
            public string ID { get; set; }
            public string Room { get; set; }
            public bool Wait { get; set;}
        }
        public List<Link> Links { get; set; }
        public bool HasID(string id)
        {
            return Links.Find(item=> item.ID == id) != null;
        }
        public string GetRoom(string id)
        {
            return Links.Find(item=> item.ID == id).Room;
        }
        public Link GetLink(string id)
        {
            return Links.Find(item=> item.ID == id);
        }
        public void ParseData()
        {
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
                    AssetReloadHelper.Do("Reloading Gameshow Questions", () =>
                    {
                        if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/AccessLinks", out var asset)
                            && asset.TryDeserialize(out GameshowData myData))
                        {
                            PianoModule.GameshowData = myData;
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
