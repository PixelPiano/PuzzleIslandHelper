using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public class ProgramManager
    {
        public List<string> ProgramNames = new();
        public AccessProgram Access;
        public PipeProgram Pipe;
        public GameOfLifeProgram Life;
        public FountainProgram Fountain;
        public FreqProgram Freq;
        public ChatLogProgram Chat;
        public List<WindowContent> Content = new();
        public ProgramManager(List<string> ids)
        {
            ProgramNames = ids;
        }
        public void Initialize(Scene scene)
        {
        }
        public static Type GetProgramType(string id)
        {

            Type t = Type.GetType(id + "Program");
            return t;
        }
    }
}