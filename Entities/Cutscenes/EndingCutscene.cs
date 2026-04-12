using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [Tracked]
    public class EndingCutscene : Textscene
    {
        [Command("pi_ending", "plays an ending")]
        public static void PlayEnding(string ending)
        {
            Endings e = Endings.Early;
            if (Enum.TryParse(ending, true, out Endings e2))
            {
                e = e2;
            }
            Engine.Scene?.Add(new EndingCutscene(ending));
        }
        public enum Endings
        {
            Early,
            Normal,
            Pendant,
            TrueTower
        }
        public Endings Ending;
        public string DialogID;

        public EndingCutscene(Endings type, bool endsChapter = true) : base(-1, -1, "EndingEarly")// GetDialogID(type))
        {
            EndingChapterAfter = endsChapter;
            Ending = type;
        }
        public EndingCutscene(string dialog) : base(-1, -1, dialog) { }

        public override void OnCue(string[] args)
        {
            base.OnCue(args);
            CuedRoutines.Add((string[] args2) => { return WaitForButton(); });
        }
        public static string EndingFlagString(Endings ending)
        {
            return "PuzzleIslandHelper - Ending " + ending;
        }
        public int TimesSeen(Level level, Endings ending)
        {
            return level.Session.GetCounter(EndingFlagString(ending));
        }
        public override void OnBegin(Level level)
        {
            level.Session.IncrementCounter(EndingFlagString(Ending));
            base.OnBegin(level);

        }
        public static string GetDialogID(Endings ending)
        {
            string name = "Ending" + ending.ToString();
            int times = (Engine.Scene as Level).Session.GetCounter(EndingFlagString(ending));
            for (int i = times; i >= 0; i--)
            {
                if (Dialog.Has(name + (i + 1)))
                {
                    return name + (i + 1);
                }
            }
            return name;
        }
        public override void OnEnd(Level level)
        {
            base.OnEnd(level);
        }
    }
}
