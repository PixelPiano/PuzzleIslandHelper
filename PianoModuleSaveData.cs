using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.EscapeRoomEntities;
using IL.Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Triggers;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModuleSaveData : EverestModuleSaveData
    {
        public enum Endings
        {
            Null,
            Reset,
            Recover,
            Eject,
            Duplicate,
            Inject,
        }
        public Dictionary<Endings, bool> EndingsSeen = new()
        {
            {Endings.Null,false},
            {Endings.Reset,false},
            {Endings.Recover,false},
            {Endings.Eject, false},
            {Endings.Duplicate,false},
            {Endings.Inject,false}
        };
    }
}