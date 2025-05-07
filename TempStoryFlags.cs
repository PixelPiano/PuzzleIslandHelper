using Celeste.Mod.PuzzleIslandHelper.Entities;
using System;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using System.Reflection;
using Monocle;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public static class TempStoryFlags
    {
        public const string Prefix = "StoryFlags:";
        public static bool PowerLabFirstAndOneMeeting => getFlag("powerFirst");
        private static bool getFlag(string flag) => Engine.Scene is Level level && level.Session.GetFlag(Prefix + flag);
        [Command("story","set story flag")]
        public static void SetStoryFlag(string flag, bool value = true)
        {
            if(Engine.Scene is Level level)
            {
                level.Session.SetFlag(Prefix + flag, value);
            }
        }
    }
}