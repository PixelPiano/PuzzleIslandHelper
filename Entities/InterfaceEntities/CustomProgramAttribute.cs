using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{

    //
    // Summary:
    //     Mark this entity as a Custom WindowContent.
    //     This Program will be loaded when a matching ID is detected.
    //     Read More.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomProgram : Attribute
    {
        //
        // Summary:
        //     A list of unique identifiers for this Program.
        public string[] IDs;

        //
        // Summary:
        //     Mark this entity as a Custom Interface Program.
        //
        // Parameters:
        //   ids:
        //     A list of unique identifiers for this Program.
        public CustomProgram(params string[] ids)
        {
            IDs = ids;
            
        }
    }
}