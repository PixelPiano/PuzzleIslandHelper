using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{

    //
    // Summary:
    //     Mark this entity as a Custom FakeTerminalProgram.
    //     This Program will be loaded when a matching ID is detected.
    //     Read More.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TerminalProgramAttribute : Attribute
    {
        //
        // Summary:
        //     A list of unique identifiers for this Program.
        public string[] IDs;

        //
        // Summary:
        //     Mark this entity as a Custom FakeTerminalProgram.
        //
        // Parameters:
        //   ids:
        //     A list of unique identifiers for this Program.
        public TerminalProgramAttribute(params string[] ids)
        {
            IDs = ids;
            
        }
    }
}