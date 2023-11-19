using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using VivHelper.Effects;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TileseedHelper")]
    public class TileseedHelper : Entity
    {
        public static Vector2 TilePosition = Vector2.Zero;
        public static List<Vector2> Points = new();
        public static bool BeenAdded;
        private static Vector2 Largest;
        public static LevelData LastData;
        public static int[,] Tex;
        public static Grid Grid;
        public static int DefaultSeed
        {
            get
            {
                int result = 0;
                if (PianoMapDataProcessor.TileseedAreas.Count <= 0)
                {
                    return 0;
                }
                foreach (char c in PianoMapDataProcessor.TileseedAreas[0].LevelName)
                {
                    result += c;
                }
                return result;
            }
        }
        public TileseedHelper() : base(Vector2.Zero) { }
        public static void Load()
        {
            if (PianoMapDataProcessor.TileseedAreas.Count > 0)
            {
                foreach (TileseedAreaData d in PianoMapDataProcessor.TileseedAreas)
                {
                    Largest.X = Calc.Max(Largest.X, d.LevelData.Bounds.Width);
                    Largest.Y = Calc.Max(Largest.Y, d.LevelData.Bounds.Height);
                }
            }
            else
            {
                Largest = Vector2.Zero;
            }
            //SetArray(PianoMapDataProcessor.TileseedAreas);
            IL.Celeste.Autotiler.Generate += Autotiler_Generate;
            On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
        }
        private static void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);
            if (!BeenAdded)
            {
                self.Level.Add(new TileseedHelper());
                BeenAdded = true;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (LevelData data in (scene as Level).Session.MapData.Levels)
            {
                Console.WriteLine($"{data.Name}: {data.Position}");
            }

        }
        private static void Autotiler_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            FieldDefinition f_Tiles_Textures = il.Body.Variables.First(v => v.VariableType.Name == "Tiles").VariableType.Resolve().FindField("Textures");
            if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Call && instr.Operand is MethodReference mr && mr.DeclaringType.FullName == "Monocle.Calc" && mr.Name == "Choose"))
            {

                cursor.Emit(OpCodes.Ldloc, 5);
                cursor.Emit(OpCodes.Ldloc, 7);
                cursor.Emit(OpCodes.Ldloc, 9);
                cursor.Emit(OpCodes.Ldfld, f_Tiles_Textures);
                cursor.EmitDelegate<Func<MTexture, int, int, List<MTexture>, MTexture>>((orig, x, y, textures) =>
                {
                    int count = 1;
                    foreach (TileseedAreaData a in PianoMapDataProcessor.TileseedAreas)
                    {
                        count++;
                        Vector2 coords = new Vector2(x, y);
                        coords += new Vector2(a.MapData.TileBounds.X, a.MapData.TileBounds.Y);
                        coords *= 8;
                        LevelData xyData = a.MapData.GetAt(coords);

                        if (xyData is null)
                        {
                            continue;
                        }

                        if (a.LevelData.Check(coords))
                        {
                            int x2 = (int)coords.X - (int)xyData.Position.X;
                            int y2 = (int)coords.Y - (int)xyData.Position.Y;
                            float q1mult = 0.8f;
                            float medianmult = 0.2f;
                            float q3mult = 0.4f;
                            int adjust = DefaultSeed * (Math.Abs(x2) + 1) * (Math.Abs(y2) + 1);

                            if (x2 > xyData.Bounds.Width / 4)
                            {
                                if (x2 > xyData.Bounds.Width / 2)
                                {
                                    if (x2 > xyData.Bounds.Width / 4 * 3)
                                    {
                                        adjust = (int)(adjust * q3mult);
                                    }
                                    else
                                    {
                                        adjust = (int)(adjust * medianmult);
                                    }
                                }
                                else
                                {
                                    adjust = (int)(adjust * q1mult);
                                }
                            }
                            return textures[hash((x2 / 8) + (y2 / 8) + adjust, (int)Math.Abs(a.Seed) + 1, 1) % textures.Count];
                        }
                    }
                    return orig;
                });
            }
        }

        private static int hash(long x, long seed, int n)
        {
            long x1 = (x + seed);
            int x2 = (int)((x1 >>> 32) ^ x1 * n);
            //int x3 = ((x2 >>> 16) ^ x2) * n/*0x45d9f3b*/;
            //int x4 = (int)(((x3 & 0xffffffffL)) * n >>> 32);
            //Console.WriteLine($"x1: {x1}, x2: {x2}, x3: {x3}, x4: {x4}.");
            return Math.Abs(x2);
        }

        public static void Unload()
        {
            Points.Clear();
            IL.Celeste.Autotiler.Generate -= Autotiler_Generate;
            On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
        }
    }
}