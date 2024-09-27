using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.PuzzleData
{
    public class StageData
    {
        public int CurrentStage { get; set; }
        public bool Completed
        {
            get
            {
                return CurrentStage >= Stages.Count && CurrentStage >= StageSequence.Count;
            }
        }
        public Dictionary<string, GeneratorStage> Stages { get; set; }
        public List<string> StageSequence { get; set; }
        public void Reset()
        {
            CurrentStage = 0;
        }

        public void ParseData()
        {
            foreach (KeyValuePair<string, GeneratorStage> pair in Stages)
            {
                Stages[pair.Key].ParseData();
            }
        }
        public bool GetNextStage(out GeneratorStage stage)
        {
            CurrentStage++;
            if (!Completed)
            {
                string name = StageSequence[CurrentStage];
                stage = Stages[name];
                return true;
            }
            stage = null;
            return false;
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Content.OnUpdate += Content_OnUpdate;
        }
        [OnUnload]
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
                    AssetReloadHelper.Do("Reloading Generator Stage Data", () =>
                    {
                        if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/StageData", out var asset)
                            && asset.TryDeserialize(out StageData myData))
                        {
                            PianoModule.StageData = myData;
                            foreach (KeyValuePair<string, GeneratorStage> pair in PianoModule.StageData.Stages)
                            {
                                PianoModule.StageData.Stages[pair.Key].ParseData();
                            }
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
