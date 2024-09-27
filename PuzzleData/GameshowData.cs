using System;
using System.Collections.Generic;
using FrostHelper;
using Monocle;


namespace Celeste.Mod.PuzzleIslandHelper.PuzzleData
{
    public class GameshowData
    {
        public class Question
        {
            public void ParseData(int roomNumber)
            {
                Data ??= "";
                string[] data = Data.Replace(" ", "").Split(',');
                RoomNumber = roomNumber;
                QuestionNumber = data.Length > 0 ? data[0].ToInt() : 1;
                Answer = data.Length > 1 ? data[1].ToInt() : 1;
                Choices = data.Length > 2 ? data[2].ToInt() : 4;
                PerPage = data.Length > 3 ? data[3].ToInt() : 4;
                RandomIncorrect = data.Length > 4 ? data[4].ToInt() : 0;

                ChoiceList = new();
                for (int i = 0; i < Choices; i++)
                {
                    ChoiceList.Add(Dialog + "c" + (i + 1));
                }
                Logger.Log(LogLevel.Info, "PuzzleIslandHelper/GameshowData", $"Parsed Data ->\n\tRoomNumber = {RoomNumber},\n\tQuestionNumber = {QuestionNumber},\n\tAnswer = {Answer},\n\tChoices = {Choices},\n\tChoicesPerPage = {PerPage},\n\tRandomIncorrect = {RandomIncorrect}");
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
                return choices;
            }
            public int RoomNumber;
            public int QuestionNumber;
            public string Data { get; set; }
            public int Choices;
            public int Answer;
            public int PerPage;
            public int RandomIncorrect;
            public string Dialog => "r" + RoomNumber + "q" + QuestionNumber;
            public string Q => Dialog;

            public List<string> ChoiceList = new();
        }
        public class QuestionBunch
        {
            public List<Question> Options { get; set; }
            public Question GetRandom()
            {
                if (Options.Count == 0) return null;

                Calc.PushRandom();
                int num = Calc.Random.Range(0, Options.Count);
                Calc.PopRandom();
                return Options[num];
            }
        }
        public List<QuestionBunch> QuestionSets { get; set; }
        public bool IsAnswer(Question q, int index)
        {
            if (index < q.ChoiceList.Count)
            {
                return q.Answer == index + 1;
            }
            return false;
        }
        public void ParseData()
        {
            int count = 1;
            foreach (QuestionBunch bunch in QuestionSets)
            {
                foreach (Question q in bunch.Options)
                {
                    q.ParseData(count);
                }
                count++;
            }
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
