using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PassengerSetup : Attribute
    {
        public string[] CustomIDs;
        public PassengerSetup(params string[] ids)
        {
            CustomIDs = ids;
        }
    }
}