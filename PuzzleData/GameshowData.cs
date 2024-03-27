using System;
using System.Collections.Generic;
using Monocle;


namespace Celeste.Mod.PuzzleIslandHelper.PuzzleData
{
    public class GameshowData
    {
        public class Question
        {
            public void ParseData()
            {
                if (PerPage <= 0)
                {
                    PerPage = 3;
                }
                ID ??= "";
                ChoiceList = new();
                for (int i = 0; i < Choices; i++)
                {
                    ChoiceList.Add(Prefix + (i + 1));
                }
            }
            public List<string> GetPage(int page)
            {
                List<string> choices = new();
                for (int i = 0; i < PerPage; i++)
                {
                    int num = page * PerPage + i;
                    if (num < Choices)
                    {
                        choices.Add(ChoiceList[num]);
                    }
                }
                if ((page + 1) * PerPage < Choices)
                {
                    choices.Add("qNext");
                }
                return choices;
            }
            public int Number;
            public int PerPage { get; set; }
            public string ID { get; set; }
            public string Prefix => "q" + Number;
            public string Q => Prefix;
            public int RandomIncorrect { get; set; }
            public int Choices { get; set; }

            public List<string> ChoiceList = new();
        }
        public List<Question> Questions { get; set; }
        public List<string> Answers { get; set; }
        public bool IsAnswer(Question q, int index)
        {
            if (index < q.ChoiceList.Count)
            {
                return Answers.Contains(q.ChoiceList[index]);
            }
            return false;
        }
        public void ParseData()
        {
            for (int i = 0; i < Questions.Count; i++)
            {
                Questions[i].Number = i + 1;
                Questions[i].ParseData();
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
                    AssetReloadHelper.Do("Reloading Gameshow Questions", () =>
                    {
                        if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/GameshowQuestions", out var asset)
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
