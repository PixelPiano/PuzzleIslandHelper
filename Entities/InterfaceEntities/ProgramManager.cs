using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;

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