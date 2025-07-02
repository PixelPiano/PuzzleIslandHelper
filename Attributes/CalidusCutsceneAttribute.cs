using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{

    //
    // Summary:
    //     Mark this entity as a Custom Passenger Cutscene
    //     Will add a cutscene to the scene if the passenger is talked to.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CalidusCutsceneAttribute : CustomEventAttribute
    {
        //
        // Summary:
        //     A list of unique identifiers for this Cutscene.
        public string[] CustomIDs;
        //
        // Summary:
        //     Mark this entity as a Custom Passenger Cutscene.
        //
        // Parameters:
        //   ids:
        //     A list of unique identifiers for this Cutscene.
        public CalidusCutsceneAttribute(params string[] ids) : base(ids)
        {
            CustomIDs = ids;
        }
        private static string[] InsertModName(string[] ids)
        {
            string modName = "PuzzleIslandHelper/";
            for(int i = 0; i<ids.Length; i++)
            {
                if (!ids[i].StartsWith(modName))
                {
                    ids[i] = modName + ids[i];
                }
            }
            return ids;
        }
    }
}