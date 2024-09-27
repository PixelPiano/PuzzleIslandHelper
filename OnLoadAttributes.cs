using System;


namespace Celeste.Mod.PuzzleIslandHelper
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class OnLoad : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class OnUnload : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class OnLoadContent : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class OnInitialize : Attribute
    {
    }
}
