using Celeste.Mod.PuzzleIslandHelper.Entities;
using System;
using System.Reflection;
using Monocle;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;

namespace Celeste.Mod.PuzzleIslandHelper
{
    //work in progress - might not make it in
    public class Inventory
    {
        public struct TowerKey
        {
            public string ID;
            public bool Decayed;
            public Color Color;
            public bool Used;
        }
        public struct Document
        {
            public enum Formats
            {
                Dialogue,
                Cutscene
            }
            public HashSet<string> Tags;
            public string ID;
        }
        public HashSet<TowerKey> TowerKeys = [];
        public HashSet<Document> Documents = [];
        


    }
}