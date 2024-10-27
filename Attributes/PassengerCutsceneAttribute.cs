using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{

    //
    // Summary:
    //     Mark this entity as a Custom Passenger Cutscene
    //     Will add a cutscene to the scene if the passenger is talked to.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomPassengerCutsceneAttribute : Attribute
    {
        //
        // Summary:
        //     A list of unique identifiers for this Cutscene.
        public string[] IDs;

        //
        // Summary:
        //     Mark this entity as a Custom Passenger Cutscene
        //
        // Parameters:
        //   ids:
        //     A list of unique identifiers for this Cutscene.
        public CustomPassengerCutsceneAttribute(params string[] ids)
        {
            IDs = ids;

        }
    }
}