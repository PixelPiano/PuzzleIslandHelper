using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    public static class WARPData
    {
        public static WarpRune.RuneNodeInventory Inv => PianoModule.SaveData.RuneNodeInventory;
        public static HashSet<WarpRune> AllRunes = [];
        public static HashSet<WarpRune> DefaultRunes = [];
        public static HashSet<WarpRune> ObtainedRunes = [];
        public const int XOffset = 10;
        public static string DefaultPath = "objects/PuzzleIslandHelper/digiWarpReceiver/";
        public static Dictionary<string, CapsuleList> Data => PianoMapDataProcessor.WarpCapsules;
        public static bool RuneExists(WarpRune input, out WarpData warpData)
        {
            warpData = null;
            if (input == null) return false;
            if (PianoUtils.TryGetAreaKey(out AreaKey key))
            {
                if (Data.TryGetValue(key.GetFullID(), out CapsuleList list))
                {
                    foreach (var a in list.AllRunes)
                    {
                        if (input.Match(a.Rune))
                        {
                            warpData = a;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static bool RuneExists(WarpRune input) => RuneExists(input, out _);

        public static WarpData GetWarpData(WarpRune rune) => rune == null || Engine.Scene is not Level level ? null : Data[level.GetAreaKey()].GetDataFromRune(rune);
        public static Vector2 TargetScale = new Vector2(0.4f, 2f);
        public static Vector2 Scale = Vector2.One;
        public enum NodeTypes
        {
            TL, TM, TR, MLL, ML, MR, MRR, BL, BM, BR
        }
        public struct Fragment(string id, NodeTypes a, NodeTypes b)
        {
            public string ID = id;
            public NodeTypes NodeA = a;
            public NodeTypes NodeB = b;
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Player.Render += Player_Render;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Player.Render -= Player_Render;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
        }
        private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
        {
            Vector2 prevScale = self.Sprite.Scale;
            self.Sprite.Scale *= Scale;
            orig(self);
            self.Sprite.Scale = prevScale;
        }
        private static void Player_OnSpawn(Player obj)
        {
            Scale = Vector2.One;
        }
    }
}
