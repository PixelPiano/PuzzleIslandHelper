using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Attributes
{


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CutsceneAttribute : CustomEventAttribute
    {
        //
        // Summary:
        //     A list of unique identifiers for this Cutscene.
        public string[] CustomIDs;
        // Parameters:
        //   ids:
        //     A list of unique identifiers for this Cutscene.
        public CutsceneAttribute(params string[] ids) : base(ids)
        {
            CustomIDs = ids;
        }
    }
}