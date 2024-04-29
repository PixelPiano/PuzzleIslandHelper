using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Monocle;


namespace Celeste.Mod.PuzzleIslandHelper.PuzzleData
{
    public class MazeData
    {
        public List<string> Data { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }
        
        public string[,] Grid;
        public void ParseData()
        {
            Grid = new string[Rows, Columns];

            for (int i = 0; i < Rows; i++)
            {
                string[] data = Data[i].Split(',');
                for (int j = 0; j < data.Length; j++)
                {
                    if(j >= Columns) break;
                    string d = data[j].Replace(" ", "");
                    Grid[i,j] = d;
                }
            }
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
                    AssetReloadHelper.Do("Reloading Maze Data", () =>
                    {
                        if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/MazeData", out var asset)
                            && asset.TryDeserialize(out MazeData myData))
                        {
                            PianoModule.MazeData = myData;
                        }
                    }, () =>
                    {
                        (Engine.Scene as Level)?.Reload();
                    });

                }
                catch (Exception e)
                {
                    Logger.LogDetailed(e, "Unable to reload Maze Data");
                }
            }

        }
    }

}
