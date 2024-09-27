using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.MenuElements
{
    public class OuiFileFader
    {
        public static float FadeAmount;
        private static ILHook PortraitRenderHook;
        private static string Name = "Piano_Boy/Puzzle_Island";
        private static List<RandomizedCharacter> randoms = new();
        public static char[] chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&".ToCharArray();
        public class RandomizedCharacter
        {
            public char Character;
            private float timer;
            private float limit;
            public int Index;
            private int length;
            public RandomizedCharacter(int length)
            {
                this.length = length;
                Calc.PushRandom();
                limit = Calc.Random.Range(0.2f, 0.8f);
                timer = Calc.Random.Range(0, limit);
                Calc.PopRandom();
                randomize();

            }
            private void randomize()
            {
                Calc.PushRandom();
                Index = Calc.Random.Range(0, length);
                Character = chars[Calc.Random.Range(0, chars.Length)];
                Calc.PopRandom();
            }
            public void Update()
            {
                timer += Engine.DeltaTime;
                if (timer > limit)
                {
                    timer = 0;
                    randomize();
                }
            }
        }
        [OnLoad]
        public static void Load()
        {
            randoms.Clear();
            PortraitRenderHook = new ILHook(typeof(OuiFileSelectSlot).GetMethod("orig_Render", BindingFlags.Instance | BindingFlags.Public), PortraitRender);
            On.Celeste.OuiFileSelectSlot.Update += OuiFileSelectSlot_Update;
            On.Celeste.OuiFileSelectSlot.OnDeleteSelected += OuiFileSelectSlot_OnDeleteSelected;
        }

        private static void OuiFileSelectSlot_OnDeleteSelected(On.Celeste.OuiFileSelectSlot.orig_OnDeleteSelected orig, OuiFileSelectSlot self)
        {
            GlobalTimer.Time = 0;
            FadeAmount = 0;
        }

        private static void PortraitRender(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(
                    MoveType.After,
                    instr => instr.MatchLdfld<OuiFileSelectSlot>("Portrait"),
                    instr => instr.MatchCall<Color>("get_White")
                    ))
            {
                cursor.Index++;
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(GetAlpha);
                cursor.Emit(OpCodes.Mul);
            }
            if (cursor.TryGotoNext(
                    MoveType.After,
                    instr => instr.MatchLdfld<OuiFileSelectSlot>("Name")
                    ))
            {
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(GetName);
            }
        }
        private static float GetAlpha(OuiFileSelectSlot current)
        {
            if (current.SaveData?.LevelSet == Name)
            {
                return Calc.LerpClamp(1, 0.5f, FadeAmount);
            }
            else return 1f;
        }

        private static string GetName(OuiFileSelectSlot current)
        {
            if (current.SaveData?.LevelSet == Name && FadeAmount > 0)
            {
                char[] chars = current.Name.ToCharArray();
                int num = (int)Calc.Min(chars.Length, chars.Length * FadeAmount);
                if (randoms.Count == 0)
                {
                    for (int i = 0; i < num; i++)
                    {
                        randoms.Add(new RandomizedCharacter(chars.Length));
                    }
                }
                for (int i = 0; i < randoms.Count; i++)
                {
                    randoms[i].Update();
                    chars[randoms[i].Index] = randoms[i].Character;
                }
                string newName = "";
                for (int i = 0; i < chars.Length; i++)
                {
                    newName += chars[i];
                }
                return newName;
            }
            return current.Name;
        }
        [OnUnload]
        public static void Unload()
        {
            PortraitRenderHook?.Dispose();
            PortraitRenderHook = null;
            On.Celeste.OuiFileSelectSlot.Update -= OuiFileSelectSlot_Update;
            On.Celeste.OuiFileSelectSlot.OnDeleteSelected -= OuiFileSelectSlot_OnDeleteSelected;
        }
        private static void OuiFileSelectSlot_Update(On.Celeste.OuiFileSelectSlot.orig_Update orig, OuiFileSelectSlot self)
        {
            orig(self);
        }
    }
}
